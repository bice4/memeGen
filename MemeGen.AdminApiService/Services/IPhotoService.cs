using Azure.Storage.Blobs;
using MemeGen.ApiService.Persistent;
using MemeGen.Common.Constants;
using MemeGen.Common.Exceptions;
using MemeGen.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace MemeGen.ApiService.Services;

/// <summary>
/// Service for managing photo entries and their associated blobs in Azure Blob Storage.
/// </summary>
public interface IPhotoService
{
    /// <summary>
    /// Creates a new photo entry and uploads the photo content to Azure Blob Storage.
    /// </summary>
    /// <param name="title"><see cref="string"/> title of the photo</param>
    /// <param name="personId"><see cref="int"/> id of the associated person</param>
    /// <param name="contentBase64">Base64 encoded <see cref="string"/> content of the photo</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns><see cref="int"/> id of the created photo entry</returns>
    /// <exception cref="AlreadyExistsException">thrown if a photo with the same title already exists</exception>
    /// <exception cref="NotFoundException">thrown if the associated person does not exist</exception>
    Task<int> CreateAsync(string title, int personId, string contentBase64, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a photo entry and its associated blob from Azure Blob Storage.
    /// </summary>
    /// <param name="photoId"><see cref="int"/> id of the photo to delete</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns></returns>
    /// <exception cref="NotFoundException">thrown if the photo does not exist</exception>
    Task DeleteByIdAsync(int photoId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves the Base64 encoded content of a photo from Azure Blob Storage.
    /// </summary>
    /// <param name="photoId"><see cref="int"/> id of the photo to delete</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>base64 encoded <see cref="string"/> content of the photo</returns>
    /// <exception cref="NotFoundException">thrown if the photo does not exist</exception>
    Task<string> GetPhotoItemContentBase64Async(int photoId, CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all photo entries from the database.
    /// </summary>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>list of <see cref="Photo"/></returns>
    Task<List<Photo>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Retrieves all photo entries associated with a specific personId from the database.
    /// </summary>
    /// <param name="personId"><see cref="int"/> id of the associated person</param>
    /// <param name="cancellationToken"><see cref="CancellationToken"/></param>
    /// <returns>list of <see cref="Photo"/></returns>
    Task<List<Photo>> GetAllByPersonIdAsync(int personId, CancellationToken cancellationToken);
}

/// <inheritdoc cref="IPhotoService"/>
public class PhotoService(
    ILogger<PhotoService> logger,
    AppDbContext appDbContext,
    BlobServiceClient blobServiceClient)
    : IPhotoService
{
    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    public Task<List<Photo>> GetAllAsync(CancellationToken cancellationToken)
        => appDbContext.Photos.ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task<List<Photo>> GetAllByPersonIdAsync(int personId, CancellationToken cancellationToken)
        => appDbContext.Photos.Where(x => x.PersonId == personId).ToListAsync(cancellationToken);
}