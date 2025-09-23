using System.Text.Json;
using Azure.Storage.Blobs;
using MemeGen.ClientApiService.Models;
using MemeGen.ClientApiService.Persistent.MongoDb;
using MemeGen.ClientApiService.Translators;
using MemeGen.Common.Constants;
using MemeGen.Common.Exceptions;
using MemeGen.ConfigurationService;
using MemeGen.Contracts.Messaging.V1;
using MemeGen.Contracts.Messaging.V1.Requests;
using MemeGen.Domain.Entities;
using RabbitMQ.Client;

namespace MemeGen.ClientApiService.Services;

public interface IImageService
{
    Task<PersonImage> CreateImageForPerson(int personId, CancellationToken cancellationToken);

    Task<ImageGenerationResult> GetImageForPersonByCorrelationId(string correlationId,
        CancellationToken cancellationToken);
}

public class ImageService(
    ILogger<ImageService> logger,
    ITemplateRepository templateRepository,
    IImageGenerationRepository imageGenerationRepository,
    IImageCache imageCache,
    IConnection rmqConnection,
    BlobServiceClient blobServiceClient,
    IConfigurationService configurationService) : IImageService
{
    public async Task<PersonImage> CreateImageForPerson(int personId, CancellationToken cancellationToken)
    {
        // Check if a person has templates
        var personTemplates = await templateRepository.GetByPersonIdAsync(personId, cancellationToken);
        if (personTemplates == null || personTemplates.Count == 0)
            throw new NotFoundException("Template", personId);

        // Create correlationId will be used for polling and link between services
        var correlationId = Guid.NewGuid().ToString("N");

        // Select a random template and quote
        var (randomTemplate, randomQuote) = imageCache.GetRandomTemplateAndQuote(personTemplates);

        var configuration = await configurationService.GetConfigurationAsync<ImageGenerationConfiguration>(
            ImageGenerationConfiguration.DefaultRowKey, cancellationToken);

        if (configuration == null)
            throw new NotFoundException("Configuration", 0);

        var cachedImage =
            await imageCache.GetImageFromCacheAsync(
                [randomTemplate.Id.ToString(), randomQuote, configuration.GetThumbprint()],
                cancellationToken);

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
            configuration.GetThumbprint(), personId);

        // Create RMQ message
        var imageProcessingRequest = new ImageProcessingRequest(
            correlationId, randomQuote, randomTemplate.Id.ToString(), configuration.ToMessagingConfig());

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

    public async Task<ImageGenerationResult> GetImageForPersonByCorrelationId(string correlationId,
        CancellationToken cancellationToken)
    {
        // Check if correlationId exists in MongoDb
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

        var blobResult = await GetImageFromBlob(imageGeneration.BlobFileName, cancellationToken);

        if (!string.IsNullOrWhiteSpace(blobResult.errorMessage))
        {
            return new ImageGenerationResult(ImageGenerationStatus.Failed, correlationId, blobResult.errorMessage);
        }

        await imageCache.AddImageToCacheAsync(
            [imageGeneration.TemplateId, imageGeneration.Quote, imageGeneration.ConfigurationThumbprint],
            imageGeneration.BlobFileName, cancellationToken);

        return new ImageGenerationResult(ImageGenerationStatus.Completed, correlationId,
            imageGeneration.AdditionalMessage, blobResult.base64Content);
    }

    private async Task<(string? base64Content, string? errorMessage)> GetImageFromBlob(string blobName,
        CancellationToken cancellationToken)
    {
        // Check if a blob file exists
        var blobClient = blobServiceClient.GetBlobContainerClient(AzureBlobConstants.PhotoContainerName)
            .GetBlobClient(blobName);

        var blobExists = await blobClient.ExistsAsync(cancellationToken);
        if (!blobExists)
            return (null!, "Blob not found");

        // Check if blob content is valid and exists
        var blobContent = await blobClient.DownloadContentAsync(cancellationToken);
        if (!blobContent.HasValue)
            return (null!, "Fail to download image");

        // Convert to Base64
        var contentBinary = blobContent.Value.Content;
        var base64 = Convert.ToBase64String(contentBinary);

        return (base64, null!);
    }
}