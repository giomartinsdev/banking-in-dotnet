using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.Events;

namespace BankingProject.Infrastructure.Instrumentation.Tracing;

/// <summary>
/// Service responsible for configuring MongoDB tracing and telemetry
/// </summary>
public sealed class MongoDbTracingService
{
    private readonly ActivitySource _activitySource;
    private readonly ConcurrentDictionary<int, Activity> _activityTracker;
    private readonly ILogger<MongoDbTracingService> _logger;

    /// <summary>
    /// Initializes a new instance of the MongoDbTracingService class
    /// </summary>
    /// <param name="activitySource">The activity source for creating traces</param>
    /// <param name="activityTracker">Dictionary to track activities by request ID</param>
    /// <param name="logger">Logger for debugging and monitoring</param>
    public MongoDbTracingService(
        ActivitySource activitySource,
        ConcurrentDictionary<int, Activity> activityTracker,
        ILogger<MongoDbTracingService> logger)
    {
        _activitySource = activitySource ?? throw new ArgumentNullException(nameof(activitySource));
        _activityTracker = activityTracker ?? throw new ArgumentNullException(nameof(activityTracker));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Configures MongoDB cluster with tracing capabilities
    /// </summary>
    /// <param name="clusterBuilder">The MongoDB cluster builder</param>
    public void ConfigureTracing(ClusterBuilder clusterBuilder)
    {
        ArgumentNullException.ThrowIfNull(clusterBuilder);

        clusterBuilder.Subscribe<CommandStartedEvent>(HandleCommandStarted);
        clusterBuilder.Subscribe<CommandSucceededEvent>(HandleCommandSucceeded);
        clusterBuilder.Subscribe<CommandFailedEvent>(HandleCommandFailed);
    }

    /// <summary>
    /// Handles MongoDB command started events for tracing
    /// </summary>
    /// <param name="commandEvent">The command started event</param>
    private void HandleCommandStarted(CommandStartedEvent commandEvent)
    {
        var activityName = $"MongoDB.{commandEvent.CommandName}";
        var activity = _activitySource.StartActivity(activityName, ActivityKind.Client);
        
        if (activity != null)
        {
            // Store the activity for later completion
            _activityTracker[commandEvent.RequestId] = activity;
            
            // Standard database tags following OpenTelemetry semantic conventions
            activity.SetTag("db.system", "mongodb");
            activity.SetTag("db.name", commandEvent.DatabaseNamespace?.DatabaseName ?? "unknown");
            activity.SetTag("db.operation", commandEvent.CommandName);
            activity.SetTag("db.statement", commandEvent.Command?.ToString() ?? "unknown");
            activity.SetTag("db.mongodb.command_name", commandEvent.CommandName);
            activity.SetTag("mongodb.request_id", commandEvent.RequestId.ToString());
            
            // Aspire-specific tags for visual styling
            activity.SetTag("service.name", "MongoDB");
            activity.SetTag("service.version", "1.0.0");
            activity.SetTag("component", "database");
            activity.SetTag("span.kind", "client");
            activity.SetTag("db.type", "nosql");
            activity.SetTag("otel.library.name", "BankingProject.MongoDB");
            activity.SetTag("otel.library.version", "1.0.0");
            
            // Custom tags for color/category identification in Aspire dashboard
            activity.SetTag("trace.category", "database");
            activity.SetTag("trace.color", GetOperationColor(commandEvent.CommandName));
            activity.SetTag("operation.type", "database");
            activity.SetTag("db.technology", "mongodb");
            activity.SetTag("resource.name", $"MongoDB.{commandEvent.CommandName}");
            
            // Extract collection name safely
            ExtractCollectionName(commandEvent, activity);
            
            _logger.LogDebug("[MongoDB Trace] Started activity: {ActivityName} for command: {CommandName}", 
                activity.DisplayName, commandEvent.CommandName);
        }
    }

    /// <summary>
    /// Handles MongoDB command succeeded events for tracing
    /// </summary>
    /// <param name="commandEvent">The command succeeded event</param>
    private void HandleCommandSucceeded(CommandSucceededEvent commandEvent)
    {
        if (_activityTracker.TryRemove(commandEvent.RequestId, out var activity))
        {
            activity.SetTag("db.response_time_ms", commandEvent.Duration.TotalMilliseconds);
            activity.SetTag("db.success", true);
            activity.SetTag("mongodb.duration_ms", commandEvent.Duration.TotalMilliseconds);
            activity.SetTag("performance.color", GetPerformanceColor(commandEvent.Duration));
            activity.SetStatus(ActivityStatusCode.Ok);
            
            _logger.LogDebug("[MongoDB Trace] Completed activity: {ActivityName} in {Duration}ms", 
                activity.DisplayName, commandEvent.Duration.TotalMilliseconds);
            
            activity.Dispose();
        }
    }

    /// <summary>
    /// Handles MongoDB command failed events for tracing
    /// </summary>
    /// <param name="commandEvent">The command failed event</param>
    private void HandleCommandFailed(CommandFailedEvent commandEvent)
    {
        if (_activityTracker.TryRemove(commandEvent.RequestId, out var activity))
        {
            var errorMessage = commandEvent.Failure?.Message ?? "Unknown error";
            
            activity.SetTag("db.error", errorMessage);
            activity.SetTag("db.success", false);
            activity.SetTag("mongodb.error", errorMessage);
            activity.SetTag("trace.color", "red");
            activity.SetStatus(ActivityStatusCode.Error, errorMessage);
            
            _logger.LogWarning("[MongoDB Trace] Failed activity: {ActivityName} with error: {Error}", 
                activity.DisplayName, errorMessage);
            
            activity.Dispose();
        }
    }

    /// <summary>
    /// Extracts collection name from MongoDB command safely
    /// </summary>
    /// <param name="commandEvent">The command event</param>
    /// <param name="activity">The activity to tag</param>
    private void ExtractCollectionName(CommandStartedEvent commandEvent, Activity activity)
    {
        try
        {
            if (commandEvent.Command?.ElementCount > 0)
            {
                var firstElement = commandEvent.Command.FirstOrDefault();
                if (firstElement.Name != null && firstElement.Value != null && firstElement.Value.IsString)
                {
                    var collectionName = firstElement.Value.AsString;
                    activity.SetTag("db.mongodb.collection", collectionName);
                    _logger.LogDebug("[MongoDB Trace] Collection: {CollectionName}", collectionName);
                    return;
                }
                
                if (commandEvent.Command.Contains("collection"))
                {
                    var collectionElement = commandEvent.Command.GetElement("collection");
                    var collectionName = collectionElement.Value?.AsString ?? "unknown";
                    activity.SetTag("db.mongodb.collection", collectionName);
                    _logger.LogDebug("[MongoDB Trace] Collection (alt): {CollectionName}", collectionName);
                    return;
                }
            }
            
            activity.SetTag("db.mongodb.collection", "unknown");
        }
        catch (Exception ex)
        {
            activity.SetTag("db.mongodb.collection", "unknown");
            activity.SetTag("db.mongodb.collection_error", ex.Message);
            _logger.LogWarning(ex, "[MongoDB Trace] Error extracting collection name");
        }
    }

    /// <summary>
    /// Gets color based on MongoDB operation type for visual differentiation
    /// </summary>
    /// <param name="commandName">The MongoDB command name</param>
    /// <returns>Color string for the operation</returns>
    private static string GetOperationColor(string commandName) => commandName switch
    {
        "find" or "count" or "distinct" => "blue",
        "insert" or "insertOne" or "insertMany" => "green",
        "update" or "updateOne" or "updateMany" or "replaceOne" => "orange",
        "delete" or "deleteOne" or "deleteMany" => "red",
        "aggregate" => "purple",
        _ => "gray"
    };

    /// <summary>
    /// Gets color based on operation performance for visual indication
    /// </summary>
    /// <param name="duration">The operation duration</param>
    /// <returns>Color string based on performance</returns>
    private static string GetPerformanceColor(TimeSpan duration) => duration.TotalMilliseconds switch
    {
        < 10 => "green",
        < 100 => "yellow",
        < 1000 => "orange",
        _ => "red"
    };
}
