using BankingProject.API.Middleware;

namespace BankingProject.API.Extensions;

/// <summary>
/// Extension methods for registering middleware components following Microsoft standards
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Adds global exception handling middleware to the application pipeline
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder for method chaining</returns>
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app, nameof(app));
        
        return app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
    }
}
