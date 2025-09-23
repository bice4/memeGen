using MemeGen.ApiService.Persistent;
using MemeGen.Common.Exceptions;
using MemeGen.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using InvalidDataException = MemeGen.Common.Exceptions.InvalidDataException;

namespace MemeGen.ApiService.Services;

public interface IQuoteService
{
    Task<int> CreateAsync(string quoteContent, int personId, CancellationToken cancellationToken);

    Task UpdateAsync(int quoteId, string quoteContent, int personId, CancellationToken cancellationToken);
    Task DeleteByIdAsync(int quoteId, CancellationToken cancellationToken);

    Task<QuoteItem> GetByIdAsync(int quoteId, CancellationToken cancellationToken);
    Task<List<QuoteItem>> GetByPersonIdAsync(int personId, CancellationToken cancellationToken);

    Task<List<QuoteItem>> GetAllAsync(CancellationToken cancellationToken);

    Task ImportFromFileForPersonAsync(Stream stream, int personId, CancellationToken cancellationToken);
}

public class QuoteService(ILogger<QuoteService> logger, AppDbContext appDbContext) : IQuoteService
{
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

    public async Task UpdateAsync(int quoteId, string quoteContent, int personId, CancellationToken cancellationToken)
    {
        var quote = await GetByIdAsync(quoteId, cancellationToken);

        quote.Update(quoteContent, personId);
        await appDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Quote {Id} has been updated.", quoteId);
    }

    public async Task DeleteByIdAsync(int quoteId, CancellationToken cancellationToken)
    {
        var quote = await GetByIdAsync(quoteId, cancellationToken);

        appDbContext.Quotes.Remove(quote);
        await appDbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Quote {Id} has been deleted.", quoteId);
    }

    public async Task<QuoteItem> GetByIdAsync(int quoteId, CancellationToken cancellationToken)
    {
        var quote = await appDbContext.Quotes.FirstOrDefaultAsync(x => x.Id == quoteId, cancellationToken);
        return quote ?? throw new NotFoundException("Quote", quoteId);
    }

    public Task<List<QuoteItem>> GetByPersonIdAsync(int personId, CancellationToken cancellationToken) =>
        appDbContext.Quotes.Where(x => x.PersonId == personId).ToListAsync(cancellationToken);

    public Task<List<QuoteItem>> GetAllAsync(CancellationToken cancellationToken) =>
        appDbContext.Quotes.ToListAsync(cancellationToken);

    public async Task ImportFromFileForPersonAsync(Stream stream, int personId, CancellationToken cancellationToken)
    {
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