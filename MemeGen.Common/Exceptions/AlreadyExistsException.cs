namespace MemeGen.Common.Exceptions;

public class AlreadyExistsException : DomainException
{
    public AlreadyExistsException(string entityName, string? entityId)
    {
        EntityName = entityName;
        EntityId = entityId;
        HttpStatusCode = 409;
    }

    private string EntityName { get; }
    private string? EntityId { get; }
    
    public override string ToResponseMessage() => $"{EntityName} already exists. Id: {EntityId}";
}