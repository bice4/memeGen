using Azure.Storage.Blobs;
using Microsoft.Extensions.Logging;

namespace MemeGen.AzureBlobServices;

public interface IAzureBlobService
{
    Task DeleteIfExistsAsync(string blobName, CancellationToken cancellationToken);

    Task UploadContentInBase64Async(string blobName, string base64Content, CancellationToken cancellationToken);

    Task<string?> GetContentInBase64Async(string blobName, CancellationToken cancellationToken);

    Task<BinaryData?> GetContentAsBinaryDataAsync(string blobName, CancellationToken cancellationToken);

    Task UploadContentFromFileAsync(string blobName, string filePath, string contentType,
        CancellationToken cancellationToken);
}

public class AzureBlobService(ILogger<AzureBlobService> logger, BlobServiceClient blobServiceClient) : IAzureBlobService
{
    public Task DeleteIfExistsAsync(string blobName, CancellationToken cancellationToken)
    {
        var blobClient = blobServiceClient
            .GetBlobContainerClient(AzureBlobConstants.PhotoContainerName)
            .GetBlobClient(blobName);

        logger.LogInformation("Deleting blob {BlobName}.", blobName);

        return blobClient.DeleteIfExistsAsync(cancellationToken: cancellationToken);
    }

    public Task UploadContentInBase64Async(string blobName, string base64Content, CancellationToken cancellationToken)
    {
        var contentBytes = Convert.FromBase64String(base64Content);
        var blobClient = blobServiceClient
            .GetBlobContainerClient(AzureBlobConstants.PhotoContainerName)
            .GetBlobClient(blobName);

        logger.LogInformation("Uploading data to blob {BlobName}, size {Size}bytes.", blobName, contentBytes.Length);

        return blobClient.UploadAsync(new BinaryData(contentBytes), cancellationToken);
    }

    public async Task<string?> GetContentInBase64Async(string blobName, CancellationToken cancellationToken)
    {
        var blobClient = blobServiceClient
            .GetBlobContainerClient(AzureBlobConstants.PhotoContainerName)
            .GetBlobClient(blobName);

        var response = await blobClient.DownloadContentAsync(cancellationToken);

        if (!response.HasValue) return null;

        var contentBytes = response.Value.Content;
        return Convert.ToBase64String(contentBytes);
    }

    public async Task<BinaryData?> GetContentAsBinaryDataAsync(string blobName, CancellationToken cancellationToken)
    {
        var blobClient = blobServiceClient.GetBlobContainerClient(AzureBlobConstants.PhotoContainerName)
            .GetBlobClient(blobName);

        var response = await blobClient.DownloadContentAsync(cancellationToken);
        return response.HasValue ? response.Value.Content : null;
    }

    public Task UploadContentFromFileAsync(string blobName, string filePath, string contentType,
        CancellationToken cancellationToken)
    {
        var blobClient = blobServiceClient
            .GetBlobContainerClient(AzureBlobConstants.PhotoContainerName)
            .GetBlobClient(blobName);

        logger.LogInformation("Uploading file {FilePath} to blob {BlobName}.", filePath, blobName);

        return blobClient.UploadAsync(filePath, new Azure.Storage.Blobs.Models.BlobHttpHeaders
        {
            ContentType = contentType
        }, cancellationToken: cancellationToken);
    }
}