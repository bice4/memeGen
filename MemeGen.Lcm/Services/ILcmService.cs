using System.Diagnostics;
using MemeGen.AzureBlobServices;
using MemeGen.ConfigurationService;
using MemeGen.Domain.Entities.Configuration;
using MemeGen.MongoDbService.Repositories;
using MemeGen.RedisService;

namespace MemeGen.Lcm.Services;

/// <summary>
/// Service for lifecycle management (LCM) of image generation results and their associated blobs in Azure Blob Storage.
/// </summary>
public interface ILcmService
{
    /// <summary>
    /// Cleans up old image generation results and their associated blobs from Azure Blob Storage based on retention policy.
    /// </summary>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <returns></returns>
    Task CleanAsync(CancellationToken cancellationToken);
}

/// <inheritdoc/>
public class LcmService(
    ILogger<LcmService> logger,
    IAzureBlobService azureBlobService,
    IRedisRepository redisRepository,
    IImageGenerationRepository imageGenerationRepository,
    IConfigurationService configurationService) : ILcmService
{
    // Default retention period of 120 minutes if configuration is missing
    private readonly TimeSpan _defaultImageRetentionInMinutes = TimeSpan.FromMinutes(120);

    /// <inheritdoc/>
    public async Task CleanAsync(CancellationToken cancellationToken)
    {
        var st = Stopwatch.StartNew();

        logger.LogInformation("Cleaning up storage...");

        // Get all image generation results
        var generationResults = await imageGenerationRepository.GetAllAsync(cancellationToken);

        // Get image caching configuration
        var imageCachingConfiguration = await configurationService.GetConfigurationAsync<ImageCachingConfiguration>(
            ImageCachingConfiguration.DefaultRowKey, cancellationToken);

        // Determine retention period based on configuration or use default 
        var retention = imageCachingConfiguration == null
            ? _defaultImageRetentionInMinutes
            : TimeSpan.FromMinutes(imageCachingConfiguration.ImageRetentionInMinutes);

        // Identify expired generation results based on retention policy
        var expiredGenerationResults = generationResults.Where(generationResult =>
            generationResult.UpdatedAt.Add(retention) < DateTime.UtcNow).ToList();

        logger.LogInformation("Expired generation results: {Count}", expiredGenerationResults.Count);

        foreach (var generationResult in expiredGenerationResults)
        {
            try
            {
                // Check if the image is still cached in Redis
                // If not cached, delete the blob and the database record
                // If cached, skip deletion
                var key = generationResult.KeyFromImageGeneration();
                var redisValue = await redisRepository.GetValueAsync(key);
                if (string.IsNullOrWhiteSpace(redisValue))
                {
                    // If blob file name is present, delete the blob from Azure Blob Storage
                    if (!string.IsNullOrWhiteSpace(generationResult.BlobFileName))
                    {
                        await azureBlobService.DeleteIfExistsAsync(generationResult.BlobFileName, cancellationToken);
                    }

                    await imageGenerationRepository.DeleteAsync(generationResult, cancellationToken);
                    logger.LogInformation("Deleted {GenerationResultTemplateId} {GenerationResultQuote}",
                        generationResult.TemplateId, generationResult.Quote);
                }
                else
                {
                    logger.LogInformation(
                        "Redis value found for {Key} exists, skipping deletion of {GenerationResultTemplateId} {GenerationResultQuote}",
                        key, generationResult.TemplateId, generationResult.Quote);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to delete image file {BlobName} with generation id {GenerationId}",
                    generationResult.BlobFileName, generationResult.Id);
            }
        }

        st.Stop();
        logger.LogInformation("Cleaned up storage in {Elapsed}", st.Elapsed);
    }
}