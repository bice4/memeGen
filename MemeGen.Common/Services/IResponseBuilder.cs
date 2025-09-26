using MemeGen.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MemeGen.Common.Services;

/// <summary>
/// Service for building standardized HTTP responses, especially for error handling.
/// </summary>
public interface IResponseBuilder
{
    /// <summary>
    /// Handles a general exception and returns a standardized error response.
    /// </summary>
    /// <param name="ex">The exception to handle.</param>
    /// <param name="statusCode">The HTTP status code to return (default is 500).</param>
    /// <returns>A standardized <see cref="IActionResult"/> representing the error response.</returns>
    IActionResult HandleException(Exception ex, int statusCode = 500);

    /// <summary>
    /// Handles a domain-specific exception and returns a standardized error response.
    /// </summary>
    /// <param name="ex">The domain exception to handle.</param>
    /// <returns>A standardized <see cref="IActionResult"/> representing the error response.</returns>
    IActionResult HandleDomainException(DomainException ex);
}

///<inheritdoc cref="IResponseBuilder"/>
public class ResponseBuilder(ILogger<ResponseBuilder> logger) : IResponseBuilder
{
    private const string ErrorMessage = "Something went wrong. Please try again later.";

    ///<inheritdoc />
    public IActionResult HandleException(Exception ex, int statusCode = 500)
    {
        logger.LogError(ex, "An error occurred while processing the request. See exception for details.");

        return new ObjectResult(ErrorMessage)
        {
            StatusCode = statusCode
        };
    }

    ///<inheritdoc />
    public IActionResult HandleDomainException(DomainException ex)
    {
        logger.LogError(ex, "An error occurred while processing the request. See exception for details.");

        return new ObjectResult(ex.ToResponseMessage())
        {
            StatusCode = ex.HttpStatusCode
        };
    }
}