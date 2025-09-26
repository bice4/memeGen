using MongoDB.Bson;

namespace MemeGen.Domain.Entities;

/// <summary>
/// Entity representing a meme template, including associated photo and quotes. Contains metadata such as usage count.
/// </summary>
/// <param name="name">name of the template</param>
/// <param name="photoId">id of the associated photo</param>
/// <param name="photoBlobFileName">blob file name of the associated photo</param>
/// <param name="quotes">list of quotes associated with the template</param>
/// <param name="personId">id of the associated person</param>
/// <param name="personName">name of the associated person</param>
/// <param name="photoTitle">title of the associated photo</param>
/// <param name="usages">number of times the template has been used, default is 0</param>
public class Template(
    string name,
    int photoId,
    string photoBlobFileName,
    List<string> quotes,
    int personId,
    string personName,
    string photoTitle,
    int usages = 0)
{
    public ObjectId Id { get; private set; } = ObjectId.GenerateNewId();

    public string Name { get; private set; } = name;

    public int PhotoId { get; private set; } = photoId;

    public string PhotoTitle { get; set; } = photoTitle;

    public string PhotoBlobFileName { get; private set; } = photoBlobFileName;

    public List<string> Quotes { get; private set; } = quotes;

    public int PersonId { get; private set; } = personId;

    public string PersonName { get; set; } = personName;

    public int Usages { get; private set; } = usages;

    public void Update(string name, List<string> quotes)
    {
        Name = name;
        Quotes = quotes;
    }
    
    public void IncreaseUsage()
    {
        Usages++;
    }
}