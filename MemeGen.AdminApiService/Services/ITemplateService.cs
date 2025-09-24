using Azure.Storage.Blobs;
using MemeGen.ApiService.Persistent;
using MemeGen.ApiService.Translators;
using MemeGen.Common.Constants;
using MemeGen.Common.Exceptions;
using MemeGen.Contracts.Http.v1.Requests;
using MemeGen.Domain.Entities;
using MemeGen.MongoDbService.Repositories;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using InvalidDataException = MemeGen.Common.Exceptions.InvalidDataException;

namespace MemeGen.ApiService.Services;

public interface ITemplateService
{
    Task CreateAsync(CreateTemplateRequest createTemplateRequest, CancellationToken cancellationToken);
    Task UpdateAsync(UpdateTemplateRequest updateTemplateRequest, CancellationToken cancellationToken);

    Task<List<Template>> GetAllAsync(CancellationToken cancellationToken);

    Task<List<Template>> GetAllByPersonIdAsync(int personId, CancellationToken cancellationToken);

    Task DeleteByIdAsync(ObjectId id, CancellationToken cancellationToken);

    Task<Template?> GetByIdAsync(ObjectId id, CancellationToken cancellationToken);

    Task<List<string>> GetAllImageContentAsync(int personId, CancellationToken cancellationToken);
}

public class TemplateService(
    ILogger<TemplateService> logger,
    ITemplateRepository templateRepository,
    AppDbContext appDbContext,
    IImageGenerationRepository imageGenerationRepository,
    BlobServiceClient blobServiceClient)
    : ITemplateService
{
    public async Task CreateAsync(CreateTemplateRequest createTemplateRequest,
        CancellationToken cancellationToken)
    {
        var personExists =
            await appDbContext.Persons.AnyAsync(p => p.Id == createTemplateRequest.PersonId,
                cancellationToken: cancellationToken);
        if (!personExists)
            throw new NotFoundException("Person", createTemplateRequest.PersonId);

        var photo = await appDbContext.Photos.FirstOrDefaultAsync(x => x.Id == createTemplateRequest.PhotoId,
            cancellationToken);
        if (photo == null)
            throw new NotFoundException("Photo", createTemplateRequest.PhotoId);

        if (createTemplateRequest.Quotes.Count == 0)
            throw new InvalidDataException("Quotes cannot be empty");

        var template = createTemplateRequest.ToDomain(photo.BlobFileName);

        await templateRepository.CreateAsync(template, cancellationToken);

        logger.LogInformation("Template {Id} has been created.", template.Id);
    }

    public async Task UpdateAsync(UpdateTemplateRequest updateTemplateRequest, CancellationToken cancellationToken)
    {
        if (updateTemplateRequest.Quotes.Count == 0)
            throw new InvalidDataException("Quotes cannot be empty");

        if (!ObjectId.TryParse(updateTemplateRequest.Id, out var objectId))
            throw new InvalidDataException("Invalid template ID format");

        var template = await templateRepository.GetByIdAsync(objectId, cancellationToken);
        if (template == null)
            throw new NotFoundException("Template", 0);

        template.Update(updateTemplateRequest.Name, updateTemplateRequest.Quotes);

        await templateRepository.UpdateAsync(template, cancellationToken);

        logger.LogInformation("Template {Id} has been updated.", template.Id);
    }

    public Task<List<Template>> GetAllAsync(CancellationToken cancellationToken)
        => templateRepository.GetAllAsync(cancellationToken);

    public Task<List<Template>> GetAllByPersonIdAsync(int personId, CancellationToken cancellationToken)
        => templateRepository.GetByPersonIdAsync(personId, cancellationToken);

    public async Task DeleteByIdAsync(ObjectId id, CancellationToken cancellationToken)
    {
        await templateRepository.DeleteAsync(id, cancellationToken);
        logger.LogInformation("Template {Id} has been deleted.", id);
    }

    public Task<Template?> GetByIdAsync(ObjectId id, CancellationToken cancellationToken)
        => templateRepository.GetByIdAsync(id, cancellationToken);

    public async Task<List<string>> GetAllImageContentAsync(int personId, CancellationToken cancellationToken)
    {
        var allResults = await imageGenerationRepository.GetByPersonIdAsync(personId, cancellationToken);
        var notNullResults = allResults.Where(x => x != null).ToList();

        var base64Images = new List<string>(notNullResults.Count);
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(AzureBlobConstants.PhotoContainerName);

        foreach (var result in notNullResults)
        {
            var base64Image = await GetImageContentBase64Async(result!, blobContainerClient, cancellationToken);
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