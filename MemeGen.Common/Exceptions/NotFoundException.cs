namespace MemeGen.Common.Exceptions;

public class NotFoundException : DomainException
{
    public NotFoundException(string entityName, int? entityId)
    {
        EntityName = entityName;
        EntityId = entityId;
        HttpStatusCode = 404;
    }

    private string EntityName { get; }
    private int? EntityId { get; }

    public override string ToResponseMessage() => $"{EntityName} with id: {EntityId} not found";
}