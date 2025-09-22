using MongoDB.Bson;

namespace MemeGen.Domain.Entities;

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

    public void IncreaseUsage()
    {
        Usages++;
    }
}