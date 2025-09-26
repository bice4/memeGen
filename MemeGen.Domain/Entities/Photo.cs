namespace MemeGen.Domain.Entities;

/// <summary>
/// Entity representing a photo associated with a person.
/// </summary>
/// <param name="title">title of the photo</param>
/// <param name="blobFileName">name of the blob file in Azure Blob Storage</param>
/// <param name="personId">id of the associated person</param>
public class Photo(string title, string blobFileName, int personId)
{
    public int Id { get; private set; }
    
    public string Title { get; private set; } = title;
    
    public string BlobFileName { get; private set; } = blobFileName;
    
    public int PersonId { get; private set; } = personId;
}