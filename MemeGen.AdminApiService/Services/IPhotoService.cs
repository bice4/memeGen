using Azure.Storage.Blobs;
using MemeGen.ApiService.Persistent;
using MemeGen.Common.Constants;
using MemeGen.Common.Exceptions;
using MemeGen.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MemeGen.ApiService.Services;

public interface IPhotoService
{
    Task<int> CreateAsync(string title, int personId, string contentBase64, CancellationToken cancellationToken);
    Task DeleteByIdAsync(int photoId, CancellationToken cancellationToken);
    Task<string> GetPhotoItemContentBase64Async(int photoId, CancellationToken cancellationToken);
    Task<List<Photo>> GetAllAsync(CancellationToken cancellationToken);
    Task<List<Photo>> GetAllByPersonIdAsync(int personId, CancellationToken cancellationToken);
}

public class PhotoService(
    ILogger<PhotoService> logger,
    AppDbContext appDbContext,
    BlobServiceClient blobServiceClient)
    : IPhotoService
{
    public async Task<int> CreateAsync(string title, int personId, string contentBase64,
        CancellationToken cancellationToken)
    {
        var existingPhoto = appDbContext.Photos.FirstOrDefault(p => p.Title == title);
        if (existingPhoto != null)
            throw new AlreadyExistsException("Photo", existingPhoto.Id);

        var personExists =
            await appDbContext.Persons.AnyAsync(p => p.Id == personId, cancellationToken: cancellationToken);
        if (!personExists)
            throw new NotFoundException("Person", personId);


        var blobContainerClient = blobServiceClient.GetBlobContainerClient(AzureBlobConstants.PhotoContainerName);
        await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        var blobName = Guid.NewGuid().ToString("N");

        await using var transaction = await appDbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var blobClient = blobContainerClient.GetBlobClient(blobName);
            var contentBytes = Convert.FromBase64String(contentBase64);
            await blobClient.UploadAsync(new BinaryData(contentBytes), cancellationToken);
            
            var photo = new Photo(title, blobName, personId);

            appDbContext.Photos.Add(photo);
            await appDbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Photo with id: {Id} and blob with name: {BlobFileName} has been created.",
                photo.Id, photo.BlobFileName);

            await transaction.CommitAsync(cancellationToken);

            return photo.Id;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task DeleteByIdAsync(int photoId, CancellationToken cancellationToken)
    {
        var photo = await appDbContext.Photos.FirstOrDefaultAsync(x => x.Id == photoId, cancellationToken);
        if (photo == null)
            throw new NotFoundException("Photo", photoId);

        var blobContainerClient = blobServiceClient.GetBlobContainerClient(AzureBlobConstants.PhotoContainerName);
        var blob = blobContainerClient.GetBlobClient(photo.BlobFileName);

        await using var transaction = await appDbContext.Database.BeginTransactionAsync(cancellationToken);

        try
        {
            await blob.DeleteIfExistsAsync(cancellationToken: cancellationToken);

            appDbContext.Photos.Remove(photo);
            await appDbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Photo with id: {Id} and blob with name: {BlobFileName} has been deleted.",
                photo.Id, photo.BlobFileName);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
    
    public async Task<string> GetPhotoItemContentBase64Async(int photoId, CancellationToken cancellationToken)
    {
        var photo = await appDbContext.Photos.FirstOrDefaultAsync(x => x.Id == photoId, cancellationToken);

        if (photo == null)
            throw new NotFoundException("Photo", photoId);

        var blobContainerClient = blobServiceClient.GetBlobContainerClient(AzureBlobConstants.PhotoContainerName);
        var blob = blobContainerClient.GetBlobClient(photo.BlobFileName);

        var blobContent = await blob.DownloadContentAsync(cancellationToken);
        if (!blobContent.HasValue)
            throw new NotFoundException("Photo", photoId);

        var contentBinary = blobContent.Value.Content;
        return Convert.ToBase64String(contentBinary);
    }

    public Task<List<Photo>> GetAllAsync(CancellationToken cancellationToken)
        => appDbContext.Photos.ToListAsync(cancellationToken);

    public Task<List<Photo>> GetAllByPersonIdAsync(int personId, CancellationToken cancellationToken)
        => appDbContext.Photos.Where(x => x.PersonId == personId).ToListAsync(cancellationToken);
}