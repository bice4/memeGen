using System.ComponentModel.DataAnnotations;

namespace MemeGen.Contracts.Http.v1.Requests;

public class CreatePhotoRequest(string title, string contentBase64, int personId)
{
    [Required(ErrorMessage = "Title is required")]
    [MaxLength(100, ErrorMessage = "Title must be less than 100 characters")]
    public string Title { get; set; } = title;

    [Required(ErrorMessage = "Content is required")]
    public string ContentBase64 { get; set; } = contentBase64;
    
    public int PersonId { get; set; } = personId;

}