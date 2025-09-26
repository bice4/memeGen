namespace MemeGen.Common.Exceptions;

public class BlobNotFoundException : DomainException
{
    private readonly string _blobName;

    public BlobNotFoundException(string blobName)
    {
        _blobName = blobName;
        HttpStatusCode = 404;
    }

    public override string ToResponseMessage() => $"Blob with name: {_blobName} not found";
}