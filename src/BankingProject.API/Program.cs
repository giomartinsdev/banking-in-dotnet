using System.Diagnostics;
using BankingProject.API.Extensions;
using BankingProject.Application.Services;
using BankingProject.Domain.Context.CustomerAggregate.Repositories;
using BankingProject.Infrastructure;
using BankingProject.Infrastructure.DataPersistence.Banking;
using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

var builder = WebApplication.CreateBuilder(args);

BsonSerializer.RegisterSerializer(new GuidSerializer(BsonType.String));

builder.AddServiceDefaults();

builder.Services.AddProblemDetails();

builder.Services.AddOpenApi();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddHealthChecks();

builder.Services.AddSwaggerGen
(
    option =>
    {
        option.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Banking Project API",
            Version = "v1",
            Description = "API for the Banking Project"
        });
    }
);

builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<BalanceOperationService>();
builder.Services.AddSingleton<ActivitySource>(sp => 
    new ActivitySource("BankingProject.API", "1.0.0"));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    
    // Try to get connection string from Aspire service discovery first
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
    settings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
    
    return new MongoClient(settings);
});

builder.Services.AddScoped<IMongoDatabase>(sp =>
{
    var client = sp.GetRequiredService<IMongoClient>();
    var databaseName = builder.Configuration["MongoDb:DatabaseName"];
    if (string.IsNullOrEmpty(databaseName))
    {
        throw new InvalidOperationException("MongoDB database name is not configured.");
    }
    return client.GetDatabase(databaseName);
});

var app = builder.Build();

// Global exception handling middleware (must be first in pipeline)
app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = string.Empty;
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
    });
}

app.UseRouting();

app.MapHealthChecks("/health");

app.MapControllers();

app.MapDefaultEndpoints();

app.Run();
