using Azure.Storage.Blobs;
using MemeGen.ApiService.Persistent;
using MemeGen.ApiService.Translators;
using MemeGen.Common.Constants;
using MemeGen.Common.Exceptions;
using MemeGen.Contracts.Http.v1.Models;
using MemeGen.MongoDbService.Repositories;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using InvalidDataException = System.IO.InvalidDataException;

namespace MemeGen.ApiService.Services;

/// <summary>
/// Service for retrieving information needed to create or update templates.
/// </summary>
public interface ITemplateUpdateService
{
    /// <summary>
    /// Gets the information needed to update an existing template.
    /// </summary>
    /// <param name="templateId"><see cref="string"/> id of the template to update</param>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <exception cref="NotFoundException"> thrown if the template does not exist</exception>
    /// <exception cref="InvalidDataException"> thrown if the template ID format is invalid</exception>
    /// <returns><see cref="TemplateUpdateInformation"/> containing template details and available quotes</returns>
    Task<TemplateUpdateInformation> GetUpdateInformationAsync(string templateId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the information needed to create a new template.
    /// </summary>
    /// <param name="photoId"><see cref="int"/> id of the associated photo</param>
    /// <param name="personId"><see cref="int"/> id of the associated person</param>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <exception cref="NotFoundException"> thrown if the associated person or photo does not exist</exception>
    /// <returns><see cref="TemplateCreateInformation"/> containing available quotes and photo details</returns>
    Task<TemplateCreateInformation> GetCreateInformationAsync(int photoId, int personId,
        CancellationToken cancellationToken);
}

/// <inheritdoc/>
public class TemplateUpdateService(
    ITemplateRepository templateRepository,
    AppDbContext appDbContext,
    BlobServiceClient blobServiceClient) : ITemplateUpdateService
{
    /// <inheritdoc />
    public async Task<TemplateUpdateInformation> GetUpdateInformationAsync(string templateId,
        CancellationToken cancellationToken)
    {
        if (!ObjectId.TryParse(templateId, out var objectId))
            throw new InvalidDataException("Invalid template ID format");

        var template = await templateRepository.GetByIdAsync(objectId, cancellationToken);

        if (template == null)
            throw new NotFoundException("Template", 0);

        var templateQuotes = template.Quotes ?? [];
        var allQuotes = await appDbContext.Quotes.Where(x => x.PersonId == template.PersonId)
            .ToListAsync(cancellationToken: cancellationToken);

        foreach (var quoteItem in templateQuotes.Where(quoteItem => allQuotes.Any(x => x.Quote == quoteItem)))
        {
            allQuotes.RemoveAll(x => x.Quote == quoteItem);
        }

        var templateQuotesDtos = new List<QuoteShortDto>();
        var i = 1;

        foreach (var quote in templateQuotes)
        {
            templateQuotesDtos.Add(new QuoteShortDto(quote, i));
            i++;
        }

        var base64Image = await GetBlobImageBase64(template.PhotoBlobFileName, cancellationToken);

        return new TemplateUpdateInformation(templateId, template.Name,
            templateQuotesDtos,
            allQuotes.Select(x => x.ToShortDto()),
            template.PhotoTitle,
            base64Image);
    }

    /// <inheritdoc />
    public async Task<TemplateCreateInformation> GetCreateInformationAsync(int photoId, int personId,
        CancellationToken cancellationToken)
    {
        var personExists =
            await appDbContext.Persons.AnyAsync(p => p.Id == personId,
                cancellationToken: cancellationToken);
        if (!personExists)
            throw new NotFoundException("Person", personId);

        var photo = await appDbContext.Photos.FirstOrDefaultAsync(x => x.Id == photoId,
            cancellationToken);
        if (photo == null)
            throw new NotFoundException("Photo", photoId);

        var quotes = await appDbContext.Quotes
            .Where(x => x.PersonId == personId)
            .ToListAsync(cancellationToken);

        var base64Image = await GetBlobImageBase64(photo.BlobFileName, cancellationToken);

        return new TemplateCreateInformation(quotes.Select(x => x.ToShortDto()), photo.Title, base64Image);
    }

    private async Task<string> GetBlobImageBase64(string blobName, CancellationToken cancellationToken)
    {
        var blobContainerClient = blobServiceClient.GetBlobContainerClient(AzureBlobConstants.PhotoContainerName);
        var blobClient = blobContainerClient.GetBlobClient(blobName);
        var exists = await blobClient.ExistsAsync(cancellationToken);
        if (!exists)
            throw new BlobNotFoundException(blobName);

        var blobContent = await blobClient.DownloadContentAsync(cancellationToken);
        if (!blobContent.HasValue)
            throw new BlobHasNoContentException(blobName);

        var contentBinary = blobContent.Value.Content;
        var base64Image = Convert.ToBase64String(contentBinary);
        return base64Image;
    }
}