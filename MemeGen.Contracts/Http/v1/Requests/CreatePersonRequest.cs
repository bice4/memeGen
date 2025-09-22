using System.ComponentModel.DataAnnotations;

namespace MemeGen.Contracts.Http.v1.Requests;

public class CreatePersonRequest(string name)
{
    [Required(ErrorMessage = "Name is required")]
    [MaxLength(100, ErrorMessage = "Name must be less than 100 characters")]
    public string Name { get; set; } = name;


}