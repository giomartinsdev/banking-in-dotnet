using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

var builder = DistributedApplication.CreateBuilder(args);

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables()
    .Build();

var connectionString = config.GetConnectionString("MongoDb") ?? "mongodb://localhost:27017";
var mongoUrlBuilder = new MongoUrlBuilder(connectionString);

IResourceBuilder<ParameterResource> databaseUsername = builder
    .AddParameter("mongousername", mongoUrlBuilder.Username ?? "admin", true);
IResourceBuilder<ParameterResource> databasePassword = builder
    .AddParameter("mongopassword", mongoUrlBuilder.Password ?? "admin", true)
    .WithReferenceRelationship(databaseUsername);

IResourceBuilder<MongoDBServerResource> databaseServer = builder
    .AddMongoDB("banking-db-server", 27017, databaseUsername, databasePassword)
    .WithEnvironment("MONGO_INITDB_ROOT_USERNAME", databaseUsername.Resource.Value)
    .WithEnvironment("MONGO_INITDB_ROOT_PASSWORD", databasePassword.Resource.Value)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("banking-db-volume")
    .WithMongoExpress(configure =>
    {
        configure.WithEnvironment("ME_CONFIG_MONGODB_ADMINUSERNAME", databaseUsername.Resource.Value);
        configure.WithEnvironment("ME_CONFIG_MONGODB_ADMINPASSWORD", databasePassword.Resource.Value);
        configure.WithLifetime(ContainerLifetime.Persistent);
    });

databasePassword.WithReferenceRelationship(databaseServer);
databaseUsername.WithReferenceRelationship(databaseServer);

var api = builder.AddProject<Projects.BankingProject_API>("api")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(databaseServer);
builder.Build().Run();
