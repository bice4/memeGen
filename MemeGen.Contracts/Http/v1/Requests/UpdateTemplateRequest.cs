using System.ComponentModel.DataAnnotations;

namespace MemeGen.Contracts.Http.v1.Requests;

public class UpdateTemplateRequest(
    string id,
    string name,
    List<string> quotes)
{
    public string Id { get; set; } = id;

    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; } = name;
    
    public List<string> Quotes { get; set; } = quotes;
}