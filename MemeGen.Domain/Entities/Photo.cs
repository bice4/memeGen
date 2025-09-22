namespace MemeGen.Domain.Entities;

public class Photo(string title, string blobFileName, int personId)
{
    public int Id { get; private set; }
    
    public string Title { get; private set; } = title;
    
    public string BlobFileName { get; private set; } = blobFileName;
    
    public int PersonId { get; private set; } = personId;
}