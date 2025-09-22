using MemeGen.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MemeGen.Common.Services;

public interface IResponseBuilder
{
    IActionResult HandleException(Exception ex, int statusCode = 500);

    IActionResult HandleDomainException(DomainException ex, int statusCode = 500);
}

public class ResponseBuilder(ILogger<ResponseBuilder> logger) : IResponseBuilder
{
    private const string ErrorMessage = "Something went wrong. Please try again later.";

    public IActionResult HandleException(Exception ex, int statusCode = 500)
    {
        logger.LogError(ex, "An error occurred while processing the request. See exception for details.");

        return new ObjectResult(ErrorMessage)
        {
            StatusCode = statusCode
        };
    }

    public IActionResult HandleDomainException(DomainException ex, int statusCode = 500)
    {
        logger.LogError(ex, "An error occurred while processing the request. See exception for details.");
        
        return new ObjectResult(ex.ToResponseMessage())
        {
            StatusCode = ex.HttpStatusCode
        };
    }
}