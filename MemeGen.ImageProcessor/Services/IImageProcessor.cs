using System.Diagnostics;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using MemeGen.Common.Constants;
using MemeGen.Contracts.Messaging.V1.Requests;
using MemeGen.Domain.Entities;
using MemeGen.MongoDbService.Repositories;
using MongoDB.Bson;

namespace MemeGen.ImageProcessor.Services;

public interface IImageProcessor
{
    Task ProcessImageAsync(ImageProcessingRequest request, CancellationToken cancellationToken);
}

public class ImageProcessor(
    ILogger<ImageProcessor> logger,
    ITemplateRepository templateRepository,
    BlobServiceClient blobServiceClient,
    IImageGenerationRepository imageGenerationRepository) : IImageProcessor
{
    public async Task ProcessImageAsync(ImageProcessingRequest request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing image request: {CorrelationId}, templateId: {TemplateId}, quote: {Quote}",
            request.CorrelationId, request.RandomTemplateId, request.RandomQuote);

        var stopwatch = Stopwatch.StartNew();

        var imageGeneration =
            await imageGenerationRepository.GetByCorrelationIdAsync(request.CorrelationId, cancellationToken);

        if (imageGeneration == null)
        {
            stopwatch.Stop();
            throw new NullReferenceException("Image generation not found");
        }

        if (!ObjectId.TryParse(request.RandomTemplateId, out var templateId))
        {
            stopwatch.Stop();
            await FailImageProcessing(imageGeneration, "Invalid template id", cancellationToken);
            return;
        }

        var template = await templateRepository.GetByIdAsync(templateId, cancellationToken);
        if (template is null)
        {
            stopwatch.Stop();
            await FailImageProcessing(imageGeneration, "Template not found", cancellationToken);
            return;
        }

        var tempFileName = Path.GetTempFileName();

        try
        {
            var blobContainerClient = blobServiceClient.GetBlobContainerClient(AzureBlobConstants.PhotoContainerName);
            var blob = blobContainerClient.GetBlobClient(template.PhotoBlobFileName);

            var blobContent = await blob.DownloadContentAsync(cancellationToken);
            if (!blobContent.HasValue)
            {
                stopwatch.Stop();
                await FailImageProcessing(imageGeneration, "Image not found at blob", cancellationToken);
                return;
            }

            var contentBinary = blobContent.Value.Content;

            await File.WriteAllBytesAsync(tempFileName, contentBinary, cancellationToken);

            await ImageTextDrawer.DrawTextOnImage(request.RandomQuote, tempFileName, request.ImageProcessingConfig);

            // processing
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
        imageGeneration.Update(ImageGenerationStatus.Completed, additionalMessage, blobName);
        await imageGenerationRepository.UpdateAsync(imageGeneration, cancellationToken);
        logger.LogInformation("Image generation completed");
    }

    private async Task FailImageProcessing(ImageGeneration imageGeneration, string errorMessage,
        CancellationToken cancellationToken)
    {
        imageGeneration.Update(ImageGenerationStatus.Failed, errorMessage);
        await imageGenerationRepository.UpdateAsync(imageGeneration, cancellationToken);
        logger.LogInformation("Image generation failed, reason: {Reason}", errorMessage);
    }
}