namespace MemeGen.Common.Exceptions;

public class AlreadyExistsException : DomainException
{
    public AlreadyExistsException(string entityName, int? entityId)
    {
        EntityName = entityName;
        EntityId = entityId;
        HttpStatusCode = 409;
    }

    private string EntityName { get; }
    private int? EntityId { get; }
    
    public override string ToResponseMessage() => $"{EntityName} already exists. Id: {EntityId}";
}