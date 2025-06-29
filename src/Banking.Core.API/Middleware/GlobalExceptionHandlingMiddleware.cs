using System.Diagnostics;
using System.Net;
using System.Text.Json;

namespace BankingProject.API.Middleware;

/// <summary>
/// Middleware for global exception handling following Microsoft standards
/// </summary>
public sealed class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to handle exceptions globally
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <returns>Task representing the asynchronous operation</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    /// <summary>
    /// Handles exceptions and returns appropriate HTTP responses
    /// </summary>
    /// <param name="context">The HTTP context</param>
    /// <param name="exception">The exception to handle</param>
    /// <returns>Task representing the asynchronous operation</returns>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var activity = Activity.Current;
        
        // Log the exception with appropriate level
        _logger.LogError(exception, "An unhandled exception occurred: {ExceptionMessage}", exception.Message);

        // Set activity status if available
        activity?.SetStatus(ActivityStatusCode.Error, exception.Message);
        activity?.AddException(exception);

        var errorResponse = CreateErrorResponse(exception);
        errorResponse.TraceId = context.TraceIdentifier;
        
        var jsonResponse = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = errorResponse.StatusCode;

        await context.Response.WriteAsync(jsonResponse);
    }

    /// <summary>
    /// Creates an appropriate error response based on the exception type
    /// </summary>
    /// <param name="exception">The exception to process</param>
    /// <returns>ErrorResponse object with appropriate status code and message</returns>
    private static ErrorResponse CreateErrorResponse(Exception exception)
    {
        return exception switch
        {
            ArgumentNullException argumentNullException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = "Invalid request data",
                Details = $"Parameter '{argumentNullException.ParamName}' cannot be null",
                Type = "ValidationError"
            },
            ArgumentOutOfRangeException argumentOutOfRangeException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = "Invalid parameter value",
                Details = argumentOutOfRangeException.Message,
                Type = "ValidationError"
            },
            InvalidOperationException invalidOperationException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Message = "Operation cannot be completed",
                Details = invalidOperationException.Message,
                Type = "BusinessLogicError"
            },
            UnauthorizedAccessException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.Unauthorized,
                Message = "Access denied",
                Details = "You are not authorized to perform this operation",
                Type = "AuthorizationError"
            },
            FileNotFoundException fileNotFoundException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Message = "Resource not found",
                Details = fileNotFoundException.Message,
                Type = "NotFoundError"
            },
            TimeoutException => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.RequestTimeout,
                Message = "Request timeout",
                Details = "The operation took too long to complete",
                Type = "TimeoutError"
            },
            _ => new ErrorResponse
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Message = "An unexpected error occurred",
                Details = "Please try again later or contact support if the problem persists",
                Type = "InternalServerError"
            }
        };
    }
}
