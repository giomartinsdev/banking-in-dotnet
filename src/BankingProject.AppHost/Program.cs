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
string username = mongoUrlBuilder.Username ?? "admin";
string password = mongoUrlBuilder.Password ?? "admin";

IResourceBuilder<ParameterResource> databaseUsername = builder.AddParameter("mongousername", publishValueAsDefault: true, value: username);
IResourceBuilder<ParameterResource> databasePassword = builder.AddParameter("mongopassword", publishValueAsDefault: true, value: password);

var kafkaServer = builder
    .AddKafka("banking-kafka-server", 9092)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithKafkaUI();

IResourceBuilder<MongoDBDatabaseResource> databaseServer = builder
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
    })
    .AddDatabase("banking-db");

builder.AddProject<Projects.BankingProject_API>("api")
    .WithExternalHttpEndpoints()
    .WithHttpHealthCheck("/health")
    .WithReference(databaseServer)
    .WaitFor(databaseServer)
    .WithReference(kafkaServer)
    .WaitFor(kafkaServer);

builder.Build().Run();
