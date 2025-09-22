namespace MemeGen.Common.Exceptions;

public class InvalidDataException : DomainException
{
    public InvalidDataException(string validationMessage)
    {
        _validationMessage = validationMessage;
        HttpStatusCode = 400;
    }

    private readonly string _validationMessage;

    public override string ToResponseMessage() => _validationMessage;
}