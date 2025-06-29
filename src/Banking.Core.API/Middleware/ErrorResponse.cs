namespace BankingProject.API.Middleware;

/// <summary>
/// Represents a standardized error response following Microsoft API guidelines
/// </summary>
public sealed class ErrorResponse
{
    /// <summary>
    /// Gets or sets the HTTP status code
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the error message for display to users
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets detailed error information for debugging
    /// </summary>
    public string Details { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the error type/category
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the timestamp when the error occurred
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the trace identifier for correlation
    /// </summary>
    public string? TraceId { get; set; }
}
