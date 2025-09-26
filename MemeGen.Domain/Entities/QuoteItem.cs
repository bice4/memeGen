namespace MemeGen.Domain.Entities;

/// <summary>
/// Entity representing a quote associated with a person.
/// </summary>
/// <param name="quote">content of the quote</param>
/// <param name="personId">id of the associated person</param>
public class QuoteItem(string quote, int personId)
{
    public int Id { get; private set; }

    public string Quote { get; private set; } = quote;

    public int PersonId { get; private set; } = personId;
}