using System.Diagnostics;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MemeGen.Common.Constants;
using MemeGen.Contracts.Messaging.V1.Requests;
using MemeGen.Domain.Entities;
using MemeGen.MongoDbService.Repositories;
using MongoDB.Bson;

namespace MemeGen.ImageProcessor.Services;

/// <summary>
/// Service for processing messages from the image processing queue and generating images.
/// </summary>
public interface IImageProcessor
{
    /// <summary>
    /// Processes an image by drawing text on it based on the provided request details.
    /// </summary>
    /// <param name="request"><see cref="ImageProcessingRequest"/> request containing image processing details</param>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <returns></returns>
    Task ProcessImageAsync(ImageProcessingRequest request, CancellationToken cancellationToken);
}

/// <inheritdoc/>
public class ImageProcessor(
    ILogger<ImageProcessor> logger,
    ITemplateRepository templateRepository,
    BlobServiceClient blobServiceClient,
    IImageGenerationRepository imageGenerationRepository) : IImageProcessor
{
    /// <inheritdoc/>
    public async Task ProcessImageAsync(ImageProcessingRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing image request: {CorrelationId}, templateId: {TemplateId}, quote: {Quote}",
            request.CorrelationId, request.RandomTemplateId, request.RandomQuote);

        var stopwatch = Stopwatch.StartNew();

        // Get image generation entry
        var imageGeneration =
            await imageGenerationRepository.GetByCorrelationIdAsync(request.CorrelationId, cancellationToken);
        
        if (imageGeneration == null)
        {
            stopwatch.Stop();
            throw new NullReferenceException("Image generation not found");
        }

        // Get template by id
        if (!ObjectId.TryParse(request.RandomTemplateId, out var templateId))
        {
            stopwatch.Stop();
            // Fail image generation, with invalid template id
            await FailImageProcessing(imageGeneration, "Invalid template id", cancellationToken);
            return;
        }

        // Fetch template from repository
        var template = await templateRepository.GetByIdAsync(templateId, cancellationToken);
        if (template is null)
        {
            stopwatch.Stop();
            await FailImageProcessing(imageGeneration, "Template not found", cancellationToken);
            return;
        }

        // Create a temp file to store the image
        // TODO:: Use in-memory processing to avoid file I/O overhead
        var tempFileName = Path.GetTempFileName();

        try
        {
            // Download image from blob storage
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(AzureBlobConstants.PhotoContainerName);
            var blob = blobContainerClient.GetBlobClient(template.PhotoBlobFileName);

            var blobContent = await blob.DownloadContentAsync(cancellationToken);
            
            // If blob content is null, fail the image generation
            if (!blobContent.HasValue)
            {
                stopwatch.Stop();
                await FailImageProcessing(imageGeneration, "Image not found at blob", cancellationToken);
                return;
            }

            // Save blob content to temp file
            // Draw text on image
            var contentBinary = blobContent.Value.Content;
            await File.WriteAllBytesAsync(tempFileName, contentBinary, cancellationToken);
            await ImageTextDrawer.DrawTextOnImage(request.RandomQuote, tempFileName, request.ImageProcessingConfig);

            // Upload the processed image back to blob storage
            // Use correlation id as blob name
            // Set content type to image/jpeg
            // Update image generation entry with blob name and status completed
            // If any of these steps fail, update image generation entry with status failed and error message
            var blobName = request.CorrelationId;
            var blobClient = blobContainerClient.GetBlobClient(blobName);

            await blobClient.UploadAsync(tempFileName, cancellationToken);
            await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders
            {
                ContentType = "image/jpeg"
            }, cancellationToken: cancellationToken);

            stopwatch.Stop();
            await CompleteImageProcessing(imageGeneration, blobName, $"Elapsed: {stopwatch.ElapsedMilliseconds} ms",
                cancellationToken);
        }
        catch (Exception e)
        {
            stopwatch.Stop();
            logger.LogError(e, "Error drawing on image");
            await FailImageProcessing(imageGeneration, "Image processing error", cancellationToken);
            if (File.Exists(tempFileName))
                File.Delete(tempFileName);
        }
        finally
        {
            if (File.Exists(tempFileName))
                File.Delete(tempFileName);
        }
    }

    private async Task CompleteImageProcessing(ImageGeneration imageGeneration, string blobName,
        string? additionalMessage = null, CancellationToken cancellationToken = default)
    {
        // Update image generation status to completed with blob name
        imageGeneration.Update(ImageGenerationStatus.Completed, additionalMessage, blobName);
        await imageGenerationRepository.UpdateAsync(imageGeneration, cancellationToken);
        logger.LogInformation("Image generation completed");
    }

    private async Task FailImageProcessing(ImageGeneration imageGeneration, string errorMessage,
        CancellationToken cancellationToken)
    {
        // Update image generation status to failed with error message
        imageGeneration.Update(ImageGenerationStatus.Failed, errorMessage);
        await imageGenerationRepository.UpdateAsync(imageGeneration, cancellationToken);
        logger.LogInformation("Image generation failed, reason: {Reason}", errorMessage);
    }
}