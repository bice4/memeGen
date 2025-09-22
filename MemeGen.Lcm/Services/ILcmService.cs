using System.Diagnostics;
using Azure.Storage.Blobs;
using MemeGen.Common.Constants;
using MemeGen.Lcm.Persistent.MongoDb;
using StackExchange.Redis;

namespace MemeGen.Lcm.Services;

public interface ILcmService
{
    Task CleanAsync(CancellationToken cancellationToken);
}

public class LcmService(
    ILogger<LcmService> logger,
    BlobServiceClient blobServiceClient,
    IConnectionMultiplexer mux,
    IImageGenerationRepository imageGenerationRepository) : ILcmService
{
    private readonly TimeSpan _ttl = TimeSpan.FromHours(2);

    public async Task CleanAsync(CancellationToken cancellationToken)
    {
        var st = Stopwatch.StartNew();
        logger.LogInformation("Cleaning up lcm storage...");
        var generationResults = await imageGenerationRepository.GetAllAsync(cancellationToken);

        var blobContainerClient = blobServiceClient.GetBlobContainerClient(AzureBlobConstants.PhotoContainerName);
        var redis = mux.GetDatabase();
        var expiredGenerationResults = generationResults.Where(generationResult =>
            generationResult.UpdatedAt.Add(_ttl) < DateTime.UtcNow).ToList();

        logger.LogInformation("Expired generation results: {Count}", expiredGenerationResults.Count);

        foreach (var generationResult in expiredGenerationResults)
        {
            try
            {
                var redisValue = await redis.StringGetAsync($"{generationResult.TemplateId}_{generationResult.Quote}");
                if (redisValue.IsNullOrEmpty)
                {
                    var blobClient = blobContainerClient.GetBlobClient(generationResult.BlobFileName);
                    await blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
                    await imageGenerationRepository.DeleteAsync(generationResult, cancellationToken);
                    logger.LogInformation("Deleted {GenerationResultTemplateId}_{GenerationResultQuote}",
                        generationResult.TemplateId, generationResult.Quote);
                }
                else
                {
                    logger.LogInformation("Redis value found for {GenerationResultTemplateId}_{GenerationResultQuote}",
                        generationResult.TemplateId, generationResult.Quote);
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to delete image file {blobName}", generationResult.BlobFileName);
            }
        }

        st.Stop();
        logger.LogInformation("Cleaned up lcm storage in {Elapsed}", st.Elapsed);
    }
}