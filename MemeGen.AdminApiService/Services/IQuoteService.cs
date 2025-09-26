using MemeGen.ApiService.Persistent;
using MemeGen.Common.Exceptions;
using MemeGen.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using InvalidDataException = MemeGen.Common.Exceptions.InvalidDataException;

namespace MemeGen.ApiService.Services;

/// <summary>
/// Service for managing quotes and their associations with persons.
/// </summary>
public interface IQuoteService
{
    /// <summary>
    /// Creates a new quote for a specific person.
    /// </summary>
    /// <param name="quoteContent"><see cref="string"/> content of the quote</param>
    /// <param name="personId"><see cref="int"/> id of the associated person</param>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <exception cref="NotFoundException">thrown if the associated person does not exist</exception>
    /// <returns><see cref="int"/> id of the created quote</returns>
    Task<int> CreateAsync(string quoteContent, int personId, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a quote by its ID.
    /// </summary>
    /// <param name="quoteId"><see cref="int"/> id of the quote to delete</param>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <exception cref="NotFoundException">thrown if the quote does not exist</exception>
    /// <returns></returns>
    Task DeleteByIdAsync(int quoteId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a quote by its ID.
    /// </summary>
    /// <param name="quoteId"><see cref="int"/> id of the quote to retrieve</param>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <exception cref="NotFoundException">thrown if the quote does not exist</exception>
    /// <returns><see cref="QuoteItem"/></returns>
    Task<QuoteItem> GetByIdAsync(int quoteId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all quotes associated with a specific personId.
    /// </summary>
    /// <param name="personId"><see cref="int"/> id of the associated person</param>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <returns>list of <see cref="QuoteItem"/></returns>
    Task<List<QuoteItem>> GetByPersonIdAsync(int personId, CancellationToken cancellationToken);

    /// <summary>
    /// Gets all quotes from the database.
    /// </summary>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <returns>list of <see cref="QuoteItem"/></returns>
    Task<List<QuoteItem>> GetAllAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Imports quotes from a text file for a specific person. Each line in the file represents a separate quote.
    /// Duplicate quotes within the file are ignored.
    /// </summary>
    /// <param name="stream"><see cref="Stream"/> containing the text file data</param>
    /// <param name="personId"><see cref="int"/> id of the associated person</param>
    /// <param name="cancellationToken">><see cref="CancellationToken"/></param>
    /// <exception cref="NotFoundException">thrown if the associated person does not exist</exception>
    /// <exception cref="InvalidDataException">thrown if the file is empty</exception
    /// <returns></returns>
    Task ImportFromFileForPersonAsync(Stream stream, int personId, CancellationToken cancellationToken);
}

/// <inheritdoc cref="IQuoteService"/>
public class QuoteService(ILogger<QuoteService> logger, AppDbContext appDbContext) : IQuoteService
{
    /// <inheritdoc />
    public async Task<int> CreateAsync(string quoteContent, int personId, CancellationToken cancellationToken)
    {
        var personExists =
            await appDbContext.Persons.AnyAsync(p => p.Id == personId, cancellationToken: cancellationToken);
        if (!personExists)
            throw new NotFoundException("Person", personId);

        var quote = new QuoteItem(quoteContent, personId);
        await appDbContext.Quotes.AddAsync(quote, cancellationToken);
        await appDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Quote {Id} has been created.", quote.Id);

        return quote.Id;
    }

    /// <inheritdoc />
    public async Task DeleteByIdAsync(int quoteId, CancellationToken cancellationToken)
    {
        var quote = await GetByIdAsync(quoteId, cancellationToken);

        appDbContext.Quotes.Remove(quote);
        await appDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Quote {Id} has been deleted.", quoteId);
    }

    /// <inheritdoc />
    public async Task<QuoteItem> GetByIdAsync(int quoteId, CancellationToken cancellationToken)
    {
        var quote = await appDbContext.Quotes.FirstOrDefaultAsync(x => x.Id == quoteId, cancellationToken);
        return quote ?? throw new NotFoundException("Quote", quoteId);
    }

    /// <inheritdoc />
    public Task<List<QuoteItem>> GetByPersonIdAsync(int personId, CancellationToken cancellationToken) =>
        appDbContext.Quotes.Where(x => x.PersonId == personId).ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task<List<QuoteItem>> GetAllAsync(CancellationToken cancellationToken) =>
        appDbContext.Quotes.ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task ImportFromFileForPersonAsync(Stream stream, int personId, CancellationToken cancellationToken)
    {
        var personExists =
            await appDbContext.Persons.AnyAsync(p => p.Id == personId, cancellationToken: cancellationToken);
        
        if (!personExists)
            throw new NotFoundException("Person", personId);

        using var reader = new StreamReader(stream);
        var quotes = new List<QuoteItem>();
        while (!reader.EndOfStream)
        {
            var line = await reader.ReadLineAsync(cancellationToken);
            if (string.IsNullOrWhiteSpace(line)) continue;

            if (quotes.Any(x => x.Quote == line))
                continue;

            var quote = new QuoteItem(line, personId);
            quotes.Add(quote);
        }

        if (quotes.Count == 0)
            throw new InvalidDataException("File is empty");

        await appDbContext.Quotes.AddRangeAsync(quotes, cancellationToken);
        await appDbContext.SaveChangesAsync(cancellationToken);
    }
}