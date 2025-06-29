using System.Diagnostics;
using BankingProject.Domain.Abstractions;
using BankingProject.Domain.Context.CustomerAggregate.Repositories;
using BankingProject.Infrastructure.DataPersistence.MongoDB;
using BankingProject.Infrastructure.Instrumentation.Metrics;
using BankingProject.Infrastructure.MongoDB;
using MongoDB.Driver;

namespace BankingProject.Infrastructure;

/// <summary>
/// Base repository class with MongoDB tracing capabilities
/// </summary>
/// <typeparam name="TEntity">The entity type that implements IAggregateRoot</typeparam>
public abstract class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : IAggregateRoot
{
    protected readonly TracedMongoCollection<TEntity> DocumentCollection;

    /// <summary>
    /// Initializes a new instance of the GenericRepository class
    /// </summary>
    /// <param name="database">The MongoDB database</param>
    /// <param name="collectionName">The collection name enum</param>
    /// <param name="activitySource">The activity source for tracing</param>
    protected GenericRepository(IMongoDatabase database, CollectionsEnum collectionName, ActivitySource activitySource)
    {
        var rawCollection = database.GetCollection<TEntity>(collectionName.ToString());
        DocumentCollection = new TracedMongoCollection<TEntity>(rawCollection, activitySource);
    }

    /// <summary>
    /// Saves an entity to the database asynchronously
    /// </summary>
    /// <param name="entity">The entity to save</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task SaveAsync(TEntity entity)
    {
        ArgumentNullException.ThrowIfNull(entity);
        
        await DocumentCollection.InsertOneAsync(entity);
        InfrastructureMetrics.CreatedDocumentsIntoDatabaseMetric.CreatedOneDocumentOf(
            DocumentCollection.CollectionNamespace.CollectionName);
    }
}
