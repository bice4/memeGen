using Azure.Storage.Blobs;
using MemeGen.ApiService.Persistent;
using MemeGen.ApiService.Translators;
using MemeGen.AzureBlobServices;
using MemeGen.Common.Exceptions;
using MemeGen.Contracts.Http.v1.Requests;
using MemeGen.Domain.Entities;
using MemeGen.MongoDbService.Repositories;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using InvalidDataException = MemeGen.Common.Exceptions.InvalidDataException;

namespace MemeGen.ApiService.Services;

/// <summary>
/// Service for managing template entries and their associated image contents.
/// </summary>
public interface ITemplateService
{
    /// <summary>
    /// Creates a new template in the database.
    /// </summary>
    /// <param name="createTemplateRequest"><see cref="CreateTemplateRequest"/> request containing template details</param>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <exception cref="NotFoundException"> thrown if the associated person or photo does not exist</exception>
    /// <exception cref="InvalidDataException"> thrown if the quotes list is empty</exception>
    /// <returns></returns>
    Task CreateAsync(CreateTemplateRequest createTemplateRequest, CancellationToken cancellationToken);

    /// <summary>
    /// Updates an existing template in the database.
    /// </summary>
    /// <param name="updateTemplateRequest"><see cref="UpdateTemplateRequest"/> request containing updated template details</param>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <exception cref="NotFoundException"> thrown if the template does not exist</exception>
    /// <exception cref="InvalidDataException"> thrown if the quotes list is empty or if the template ID format is invalid</exception>
    /// <returns></returns>
    Task UpdateAsync(UpdateTemplateRequest updateTemplateRequest, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all templates from the database.
    /// </summary>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <returns>list of <see cref="Template"/></returns>
    Task<List<Template>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets all templates associated with a specific personId from the database.
    /// </summary>
    /// <param name="personId"><see cref="int"/> id of the associated person</param>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <returns>list of <see cref="Template"/></returns>
    Task<List<Template>> GetAllByPersonIdAsync(int personId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a template by its ID from the database.
    /// </summary>
    /// <param name="id"><see cref="ObjectId"/> id of the template to delete</param>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <returns></returns>
    Task DeleteByIdAsync(ObjectId id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a template by its ID from the database.
    /// </summary>
    /// <param name="id">><see cref="ObjectId"/> id of the template to retrieve</param>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <returns><see cref="Template"/> or null if not found</returns>
    Task<Template?> GetByIdAsync(ObjectId id, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all image contents associated with a specific personId from Azure Blob Storage as Base64 encoded strings.
    /// </summary>
    /// <param name="personId">><see cref="int"/> id of the associated person</param>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <returns>list of Base64 encoded <see cref="string"/> image contents</returns>
    Task<List<string>> GetAllImageContentAsync(int personId, CancellationToken cancellationToken);
}

/// <inheritdoc cref="ITemplateService"/>
public class TemplateService(
    ILogger<TemplateService> logger,
    ITemplateRepository templateRepository,
    AppDbContext appDbContext,
    IImageGenerationRepository imageGenerationRepository,
    IAzureBlobService azureBlobService)
    : ITemplateService
{
    /// <inheritdoc />
    public async Task CreateAsync(CreateTemplateRequest createTemplateRequest,
        CancellationToken cancellationToken)
    {
        var personExists =
            await appDbContext.Persons.AnyAsync(p => p.Id == createTemplateRequest.PersonId,
                cancellationToken: cancellationToken);
        if (!personExists)
            throw new NotFoundException("Person", createTemplateRequest.PersonId.ToString());

        var photo = await appDbContext.Photos.FirstOrDefaultAsync(x => x.Id == createTemplateRequest.PhotoId,
            cancellationToken);
        if (photo == null)
            throw new NotFoundException("Photo", createTemplateRequest.PhotoId.ToString());

        if (createTemplateRequest.Quotes.Count == 0)
            throw new InvalidDataException("Quotes cannot be empty");

        var template = createTemplateRequest.ToDomain(photo.BlobFileName);

        await templateRepository.CreateAsync(template, cancellationToken);

        logger.LogInformation("Template {Id} has been created.", template.Id);
    }

    /// <inheritdoc />
    public async Task UpdateAsync(UpdateTemplateRequest updateTemplateRequest, CancellationToken cancellationToken)
    {
        if (updateTemplateRequest.Quotes.Count == 0)
            throw new InvalidDataException("Quotes cannot be empty");

        if (!ObjectId.TryParse(updateTemplateRequest.Id, out var objectId))
            throw new InvalidDataException("Invalid template ID format");

        var template = await templateRepository.GetByIdAsync(objectId, cancellationToken);
        if (template == null)
            throw new NotFoundException("Template", updateTemplateRequest.Id);

        template.Update(updateTemplateRequest.Name, updateTemplateRequest.Quotes);

        await templateRepository.UpdateAsync(template, cancellationToken);

        logger.LogInformation("Template {Id} has been updated.", template.Id);
    }

    /// <inheritdoc />
    public Task<List<Template>> GetAllAsync(CancellationToken cancellationToken)
        => templateRepository.GetAllAsync(cancellationToken);

    /// <inheritdoc />
    public Task<List<Template>> GetAllByPersonIdAsync(int personId, CancellationToken cancellationToken)
        => templateRepository.GetByPersonIdAsync(personId, cancellationToken);

    /// <inheritdoc />
    public async Task DeleteByIdAsync(ObjectId id, CancellationToken cancellationToken)
    {
        await templateRepository.DeleteAsync(id, cancellationToken);
        logger.LogInformation("Template {Id} has been deleted.", id);
    }

    /// <inheritdoc />
    public Task<Template?> GetByIdAsync(ObjectId id, CancellationToken cancellationToken)
        => templateRepository.GetByIdAsync(id, cancellationToken);

    /// <inheritdoc />
    public async Task<List<string>> GetAllImageContentAsync(int personId, CancellationToken cancellationToken)
    {
        var allResults = await imageGenerationRepository.GetBlobNamesByPersonIdAsync(personId, cancellationToken);
        var notNullResults = allResults.Where(x => x != null).ToList();

        var base64Images = new List<string>(notNullResults.Count);

        foreach (var result in notNullResults)
        {
            var base64Image = await azureBlobService.GetContentInBase64Async(result!, cancellationToken);
            if (base64Image != null)
                base64Images.Add(base64Image);
        }

        return base64Images;
    }

    private async Task<string?> GetImageContentBase64Async(string blobFileName, BlobContainerClient client,
        CancellationToken cancellationToken)
    {
        try
        {
            var blob = client.GetBlobClient(blobFileName);

            var blobContent = await blob.DownloadContentAsync(cancellationToken);
            if (!blobContent.HasValue)
                return null!;

            // Convert to Base64
            var contentBinary = blobContent.Value.Content;
            return Convert.ToBase64String(contentBinary);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Failed to get image {BlobFileName} content from Blob storage.", blobFileName);
            return null;
        }
    }
}