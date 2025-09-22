using System.ComponentModel.DataAnnotations;

namespace MemeGen.Contracts.Http.v1.Requests;

public class CreateTemplateRequest(
    string name,
    int photoId,
    List<string> quotes,
    int personId,
    string photoTitle,
    string personName)
{
    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = name;

    public int PhotoId { get; set; } = photoId;

    [Required(ErrorMessage = "Photo title is required")]
    public string PhotoTitle { get; set; } = photoTitle;

    public int PersonId { get; set; } = personId;

    [Required(ErrorMessage = "Person name is required")]
    public string PersonName { get; set; } = personName;

    public List<string> Quotes { get; set; } = quotes;
}