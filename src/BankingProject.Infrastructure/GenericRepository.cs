using BankingProject.Domain.Abstractions;
using BankingProject.Domain.Context.CustomerAggregate.Repositories;
using BankingProject.Infrastructure.Instrumentation.Metrics;
using BankingProject.Infrastructure.MongoDB;
using MongoDB.Driver;

namespace BankingProject.Infrastructure;

public abstract class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : IAggregateRoot
{
    protected readonly IMongoCollection<TEntity> DocumentCollection;

    protected GenericRepository(IMongoDatabase database, CollectionsEnum collectionName)
    {
        DocumentCollection = database.GetCollection<TEntity>(collectionName.ToString());
    }
    
    public async Task SaveAsync(TEntity entity)
    {
        await DocumentCollection.InsertOneAsync(entity);
        InfrastructureMetrics.CreatedDocumentsIntoDatabaseMetric.CreatedOneDocumentOf(DocumentCollection.CollectionNamespace.CollectionName);
    }
}