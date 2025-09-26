using System.Text.Json;
using MemeGen.AzureBlobServices;
using MemeGen.ClientApiService.Models;
using MemeGen.ClientApiService.Translators;
using MemeGen.Common.Exceptions;
using MemeGen.ConfigurationService;
using MemeGen.Contracts.Messaging.V1;
using MemeGen.Contracts.Messaging.V1.Requests;
using MemeGen.Domain.Entities;
using MemeGen.Domain.Entities.Configuration;
using MemeGen.MongoDbService.Repositories;
using RabbitMQ.Client;

namespace MemeGen.ClientApiService.Services;

/// <summary>
/// Service for managing image generation requests and retrievals. Responsible for creating image generation tasks and fetching their results.
/// </summary>
public interface IImageService
{
    /// <summary>
    /// Creates an image for a person by selecting a random template and quote, then initiating the image generation process.
    /// </summary>
    /// <param name="personId"><see cref="int"/> id of the person</param>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <returns>><see cref="PersonImage"/> containing correlationId and image status</returns>
    /// <exception cref="NotFoundException"> thrown if the person has no templates or if the configuration is missing</exception>
    Task<PersonImage> CreateImageForPerson(int personId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the status and result of an image generation process by its correlation ID.
    /// </summary>
    /// <param name="correlationId">><see cref="string"/> correlation ID of the image generation process</param>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <returns>><see cref="ImageGenerationResult"/> containing status, correlationId, additional message, and base64 image content if completed</returns>
    Task<ImageGenerationResult> GetImageForPersonByCorrelationId(string correlationId,
        CancellationToken cancellationToken);
}

/// <inheritdoc/>
public class ImageService(
    ILogger<ImageService> logger,
    ITemplateRepository templateRepository,
    IImageGenerationRepository imageGenerationRepository,
    IImageCache imageCache,
    IConnection rmqConnection,
    IAzureBlobService azureBlobService,
    IConfigurationService configurationService) : IImageService
{
    /// <inheritdoc/>
    public async Task<PersonImage> CreateImageForPerson(int personId, CancellationToken cancellationToken)
    {
        // Check if a person has templates
        var personTemplates = await templateRepository.GetByPersonIdAsync(personId, cancellationToken);
        if (personTemplates == null || personTemplates.Count == 0)
            throw new NotFoundException("Template for the person", personId.ToString());

        // Create correlationId will be used for polling and link between services
        var correlationId = Guid.NewGuid().ToString("N");

        // Select a random template and quote
        var (randomTemplate, randomQuote) = imageCache.GetRandomTemplateAndQuote(personTemplates);

        // Get image generation configuration
        var imageGenerationConfiguration =
            await configurationService.GetConfigurationAsync<ImageGenerationConfiguration>(
                ImageGenerationConfiguration.DefaultRowKey, cancellationToken);

        // If configuration is missing, use default configuration
        if (imageGenerationConfiguration == null)
            imageGenerationConfiguration = ImageGenerationConfiguration.CreateDefault(AzureTablesConstants
                .DefaultPartitionKey);

        // Check Redis cache for existing image with the same template, quote, and configuration
        // If found, return the cached image
        var cachedImage =
            await imageCache.GetImageFromRedisCache(
                [randomTemplate.Id.ToString(), randomQuote, imageGenerationConfiguration.GetThumbprint()]);

        if (cachedImage != null)
        {
            logger.LogInformation("Image found in cache for {PersonId} with {TemplateName} and {Quote}", personId,
                randomTemplate.Name, randomQuote);

            var blobResult = await GetImageFromBlob(cachedImage, cancellationToken);
            if (!string.IsNullOrWhiteSpace(blobResult.errorMessage))
            {
                logger.LogWarning("Failed to get blob content: {ErrorMessage}. Continue to generate new image",
                    blobResult.errorMessage);
            }
            else
            {
                return new PersonImage(correlationId, true, blobResult.base64Content);
            }
        }

        logger.LogInformation("Generating new image for {PersonId} with {TemplateName} and {Quote}", personId,
            randomTemplate.Name, randomQuote);

        var imageGeneration = new ImageGeneration(correlationId, randomQuote, randomTemplate.Id.ToString(),
            imageGenerationConfiguration.GetThumbprint(), personId);

        // Create RMQ message
        var imageProcessingRequest = new ImageProcessingRequest(
            correlationId, randomQuote, randomTemplate.Id.ToString(), imageGenerationConfiguration.ToMessagingConfig());

        // Publish a message to a queue
        using var channel = rmqConnection.CreateModel();
        var bytes = JsonSerializer.SerializeToUtf8Bytes(imageProcessingRequest);
        channel.QueueDeclare(MessagingContractConstants.ContentProcessingQueueName, durable: true, exclusive: false,
            autoDelete: false);
        channel.BasicPublish(exchange: string.Empty, routingKey: MessagingContractConstants.ContentProcessingQueueName,
            body: bytes);

        await imageGenerationRepository.CreateAsync(imageGeneration, cancellationToken);
        await templateRepository.IncreaseTemplateUsageAsync(randomTemplate.Id, cancellationToken);

        return new PersonImage(correlationId, false);
    }

    /// <inheritdoc/>
    public async Task<ImageGenerationResult> GetImageForPersonByCorrelationId(string correlationId,
        CancellationToken cancellationToken)
    {
        // Check if correlationId exists in MongoDb and get the image generation record
        ImageGeneration? imageGeneration;
        try
        {
            imageGeneration = await imageGenerationRepository.GetByCorrelationIdAsync(correlationId, cancellationToken);

            if (imageGeneration == null)
                return new ImageGenerationResult(ImageGenerationStatus.Failed, correlationId, "Image not found");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get image for {CorrelationId}", correlationId);
            return new ImageGenerationResult(ImageGenerationStatus.Failed, correlationId, "Image not found");
        }

        // Check if image generation is completed and has a valid blob file name
        if (imageGeneration.Status != ImageGenerationStatus.Completed || imageGeneration.BlobFileName == null)
            return new ImageGenerationResult(imageGeneration!.Status, imageGeneration.CorrelationId,
                imageGeneration.AdditionalMessage);

        // Get image content from Blob storage and convert to Base64
        // If fails, return an error message
        // If succeeds, cache the image in Redis and return the Base64 content
        var blobResult = await GetImageFromBlob(imageGeneration.BlobFileName, cancellationToken);

        if (!string.IsNullOrWhiteSpace(blobResult.errorMessage))
        {
            return new ImageGenerationResult(ImageGenerationStatus.Failed, correlationId, blobResult.errorMessage);
        }

        // Cache the image in Redis
        // Get image caching configuration for cache duration
        // If configuration is missing, use default cache duration of 120 minutes
        var imageCachingConfiguration =
            await configurationService.GetConfigurationAsync<ImageCachingConfiguration>(
                ImageCachingConfiguration.DefaultRowKey, cancellationToken);

        TimeSpan? expiration = null;

        if (imageCachingConfiguration != null)
            expiration = TimeSpan.FromMinutes(imageCachingConfiguration.CacheDurationInMinutes);

        await imageCache.AddImageToRedisCache(imageGeneration, expiration);

        return new ImageGenerationResult(ImageGenerationStatus.Completed, correlationId,
            imageGeneration.AdditionalMessage, blobResult.base64Content);
    }

    private async Task<(string? base64Content, string? errorMessage)> GetImageFromBlob(string blobName,
        CancellationToken cancellationToken)
    {
        var base64 = await azureBlobService.GetContentInBase64Async(blobName, cancellationToken);

        return string.IsNullOrWhiteSpace(base64)
            ? (null!, "Blob not found or empty content")
            : (base64, null!);
    }
}