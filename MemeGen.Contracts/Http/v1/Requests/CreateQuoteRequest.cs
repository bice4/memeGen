using System.ComponentModel.DataAnnotations;

namespace MemeGen.Contracts.Http.v1.Requests;

public class CreateQuoteRequest(string quote, int personId)
{
    [Required(ErrorMessage = "Quote is required")]
    [MaxLength(2048, ErrorMessage = "Quote must be less than 2048 characters")]
    public string Quote { get; set; } = quote;

    public int PersonId { get; set; } = personId;
}