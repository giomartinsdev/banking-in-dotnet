using System.Diagnostics;
using BankingProject.Infrastructure.DataPersistence.MongoDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace BankingProject.Infrastructure;

public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.ConfigureOpenTelemetry();

        builder.AddDefaultHealthChecks();

        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();

            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource("BankingProject.API")
                    .AddSource("BankingProject.MongoDB") // Add MongoDB-specific source
                    .AddSource("MongoDB.Driver.Core.Extensions.DiagnosticSources")
                    .AddSource("MongoDB") // Add generic MongoDB source
                    .AddAspNetCoreInstrumentation(tracing =>
                        tracing.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                    )
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    /// <summary>
    /// Adds MongoDB infrastructure services with TracedMongoCollection wrapper
    /// </summary>
    /// <typeparam name="TBuilder">The builder type</typeparam>
    /// <param name="builder">The application builder</param>
    /// <returns>The builder for chaining</returns>
    public static TBuilder AddMongoDbInfrastructure<TBuilder>(this TBuilder builder) 
        where TBuilder : IHostApplicationBuilder
    {
        // Configure BSON serialization
        BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));
        
        // Register ActivitySource for MongoDB tracing
        builder.Services.AddSingleton<ActivitySource>(sp => 
            new ActivitySource("BankingProject.MongoDB", "1.0.0"));

        // Register MongoDB client without event-based tracing
        builder.Services.AddSingleton<IMongoClient>(sp =>
        {
            var configuration = sp.GetRequiredService<IConfiguration>();
            
            var connectionString = configuration.GetConnectionString("banking-db-server") ?? 
                                  configuration.GetConnectionString("MongoDb") ?? 
                                  "mongodb://localhost:27017";
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("MongoDB connection string is not configured.");
            }
            
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            settings.ConnectTimeout = TimeSpan.FromSeconds(30);
            settings.SocketTimeout = TimeSpan.FromSeconds(30);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(15);
            settings.MaxConnectionIdleTime = TimeSpan.FromMinutes(10);
            settings.MaxConnectionPoolSize = 50;
            settings.RetryWrites = true;
            settings.RetryReads = true;
            
            return new MongoClient(settings);
        });

        // Register MongoDB database
        builder.Services.AddScoped<IMongoDatabase>(sp =>
        {
            var client = sp.GetRequiredService<IMongoClient>();
            var configuration = sp.GetRequiredService<IConfiguration>();
            var databaseName = configuration["MongoDb:DatabaseName"];
            
            if (string.IsNullOrEmpty(databaseName))
            {
                throw new InvalidOperationException("MongoDB database name is not configured.");
            }
            
            return client.GetDatabase(databaseName);
        });

        // Register TracedMongoCollection factory method
        builder.Services.AddScoped(typeof(TracedMongoCollection<>), typeof(TracedMongoCollection<>));

        return builder;
    }

    /// <summary>
    /// Adds default health checks
    /// </summary>
    /// <typeparam name="TBuilder">The builder type</typeparam>
    /// <param name="builder">The application builder</param>
    /// <returns>The builder for chaining</returns>
    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapHealthChecks(HealthEndpointPath);

            app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }
}
