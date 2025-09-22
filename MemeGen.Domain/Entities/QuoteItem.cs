namespace MemeGen.Domain.Entities;

public class QuoteItem(string quote, int personId)
{
    public int Id { get; private set; }

    public string Quote { get; private set; } = quote;

    public int PersonId { get; private set; } = personId;

    public void Update(string quote, int personId)
    {
        Quote = quote;
        PersonId = personId;
    }
}