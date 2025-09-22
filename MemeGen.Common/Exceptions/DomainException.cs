namespace MemeGen.Common.Exceptions;

public abstract class DomainException : Exception
{
    public int HttpStatusCode { get; protected init; } = 500;
    
    public abstract string ToResponseMessage();
}