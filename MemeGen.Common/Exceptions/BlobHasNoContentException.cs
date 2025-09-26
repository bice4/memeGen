namespace MemeGen.Common.Exceptions;

public class BlobHasNoContentException : DomainException
{
    private readonly string _blobName;

    public BlobHasNoContentException(string blobName)
    {
        _blobName = blobName;
        HttpStatusCode = 410;
    }

    public override string ToResponseMessage() => $"Blob with name: {_blobName} has no content";
}