using System.Diagnostics;
using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace BankingProject.Infrastructure.DataPersistence.MongoDB;

/// <summary>
/// Wrapper for IMongoCollection that adds OpenTelemetry tracing for MongoDB operations
/// </summary>
/// <typeparam name="TDocument">The document type</typeparam>
public sealed class TracedMongoCollection<TDocument>
{
    private readonly IMongoCollection<TDocument> _collection;
    private readonly ActivitySource _activitySource;

    /// <summary>
    /// Initializes a new instance of the TracedMongoCollection class
    /// </summary>
    /// <param name="collection">The underlying MongoDB collection</param>
    /// <param name="activitySource">The activity source for tracing</param>
    public TracedMongoCollection(IMongoCollection<TDocument> collection, ActivitySource activitySource)
    {
        _collection = collection ?? throw new ArgumentNullException(nameof(collection));
        _activitySource = activitySource ?? throw new ArgumentNullException(nameof(activitySource));
    }

    public CollectionNamespace CollectionNamespace => _collection.CollectionNamespace;
    public IMongoDatabase Database => _collection.Database;
    public IBsonSerializer<TDocument> DocumentSerializer => _collection.DocumentSerializer;
    public IMongoIndexManager<TDocument> Indexes => _collection.Indexes;
    public MongoCollectionSettings Settings => _collection.Settings;

    public async Task<IAsyncCursor<TResult>> AggregateAsync<TResult>(
        PipelineDefinition<TDocument, TResult> pipeline,
        AggregateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("MongoDB.Aggregate");
        activity?.SetTag("db.system", "mongodb");
        activity?.SetTag("db.operation", "aggregate");
        activity?.SetTag("db.collection.name", CollectionNamespace.CollectionName);
        activity?.SetTag("db.name", CollectionNamespace.DatabaseNamespace.DatabaseName);

        try
        {
            var result = await _collection.AggregateAsync(pipeline, options, cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<BulkWriteResult<TDocument>> BulkWriteAsync(
        IEnumerable<WriteModel<TDocument>> requests,
        BulkWriteOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("MongoDB.BulkWrite");
        activity?.SetTag("db.system", "mongodb");
        activity?.SetTag("db.operation", "bulkWrite");
        activity?.SetTag("db.collection.name", CollectionNamespace.CollectionName);
        activity?.SetTag("db.name", CollectionNamespace.DatabaseNamespace.DatabaseName);

        try
        {
            var result = await _collection.BulkWriteAsync(requests, options, cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("db.mongodb.writes", result.RequestCount);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<long> CountDocumentsAsync(
        FilterDefinition<TDocument> filter,
        CountOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("MongoDB.CountDocuments");
        activity?.SetTag("db.system", "mongodb");
        activity?.SetTag("db.operation", "countDocuments");
        activity?.SetTag("db.collection.name", CollectionNamespace.CollectionName);
        activity?.SetTag("db.name", CollectionNamespace.DatabaseNamespace.DatabaseName);

        try
        {
            var result = await _collection.CountDocumentsAsync(filter, options, cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("db.mongodb.count", result);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<DeleteResult> DeleteManyAsync(
        FilterDefinition<TDocument> filter,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("MongoDB.DeleteMany");
        activity?.SetTag("db.system", "mongodb");
        activity?.SetTag("db.operation", "deleteMany");
        activity?.SetTag("db.collection.name", CollectionNamespace.CollectionName);
        activity?.SetTag("db.name", CollectionNamespace.DatabaseNamespace.DatabaseName);

        try
        {
            var result = await _collection.DeleteManyAsync(filter, cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("db.mongodb.deleted", result.DeletedCount);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<DeleteResult> DeleteOneAsync(
        FilterDefinition<TDocument> filter,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("MongoDB.DeleteOne");
        activity?.SetTag("db.system", "mongodb");
        activity?.SetTag("db.operation", "deleteOne");
        activity?.SetTag("db.collection.name", CollectionNamespace.CollectionName);
        activity?.SetTag("db.name", CollectionNamespace.DatabaseNamespace.DatabaseName);

        try
        {
            var result = await _collection.DeleteOneAsync(filter, cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("db.mongodb.deleted", result.DeletedCount);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<IAsyncCursor<TDocument>> FindAsync(
        FilterDefinition<TDocument> filter,
        FindOptions<TDocument>? options = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("MongoDB.Find");
        activity?.SetTag("db.system", "mongodb");
        activity?.SetTag("db.operation", "find");
        activity?.SetTag("db.collection.name", CollectionNamespace.CollectionName);
        activity?.SetTag("db.name", CollectionNamespace.DatabaseNamespace.DatabaseName);
        activity?.SetTag("trace.category", "database");
        activity?.SetTag("trace.color", "blue");

        try
        {
            var result = await _collection.FindAsync(filter, options, cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }

        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public IFindFluent<TDocument, TDocument> Find(
        FilterDefinition<TDocument> filter,
        FindOptions? options = null)
    {
        using var activity = _activitySource.StartActivity("MongoDB.Find");
        activity?.SetTag("db.system", "mongodb");
        activity?.SetTag("db.operation", "find");
        activity?.SetTag("db.collection.name", CollectionNamespace.CollectionName);
        activity?.SetTag("db.name", CollectionNamespace.DatabaseNamespace.DatabaseName);
        activity?.SetTag("trace.category", "database");
        activity?.SetTag("trace.color", "blue");

        try
        {
            var result = _collection.Find(filter, options);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<IAsyncCursor<TDocument>> FindAsync(
        Expression<Func<TDocument, bool>> filter,
        FindOptions<TDocument>? options = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("MongoDB.Find");
        activity?.SetTag("db.system", "mongodb");
        activity?.SetTag("db.operation", "find");
        activity?.SetTag("db.collection.name", CollectionNamespace.CollectionName);
        activity?.SetTag("db.name", CollectionNamespace.DatabaseNamespace.DatabaseName);
        activity?.SetTag("trace.category", "database");
        activity?.SetTag("trace.color", "blue");

        try
        {
            var result = await _collection.FindAsync(filter, options, cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public IFindFluent<TDocument, TDocument> Find(
        Expression<Func<TDocument, bool>> filter,
        FindOptions? options = null)
    {
        using var activity = _activitySource.StartActivity("MongoDB.Find");
        activity?.SetTag("db.system", "mongodb");
        activity?.SetTag("db.operation", "find");
        activity?.SetTag("db.collection.name", CollectionNamespace.CollectionName);
        activity?.SetTag("db.name", CollectionNamespace.DatabaseNamespace.DatabaseName);
        activity?.SetTag("trace.category", "database");
        activity?.SetTag("trace.color", "blue");

        try
        {
            var result = _collection.Find(filter, options);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<TDocument?> FindOneAndUpdateAsync(
        FilterDefinition<TDocument> filter,
        UpdateDefinition<TDocument> update,
        FindOneAndUpdateOptions<TDocument>? options = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("MongoDB.FindOneAndUpdate");
        activity?.SetTag("db.system", "mongodb");
        activity?.SetTag("db.operation", "findOneAndUpdate");
        activity?.SetTag("db.collection.name", CollectionNamespace.CollectionName);
        activity?.SetTag("db.name", CollectionNamespace.DatabaseNamespace.DatabaseName);

        try
        {
            var result = await _collection.FindOneAndUpdateAsync(filter, update, options, cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task InsertOneAsync(
        TDocument document,
        InsertOneOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("MongoDB.InsertOne");
        activity?.SetTag("db.system", "mongodb");
        activity?.SetTag("db.operation", "insertOne");
        activity?.SetTag("db.collection.name", CollectionNamespace.CollectionName);
        activity?.SetTag("db.name", CollectionNamespace.DatabaseNamespace.DatabaseName);

        try
        {
            await _collection.InsertOneAsync(document, options, cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<ReplaceOneResult> ReplaceOneAsync(
        FilterDefinition<TDocument> filter,
        TDocument replacement,
        ReplaceOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("MongoDB.ReplaceOne");
        activity?.SetTag("db.system", "mongodb");
        activity?.SetTag("db.operation", "replaceOne");
        activity?.SetTag("db.collection.name", CollectionNamespace.CollectionName);
        activity?.SetTag("db.name", CollectionNamespace.DatabaseNamespace.DatabaseName);

        try
        {
            var result = await _collection.ReplaceOneAsync(filter, replacement, options, cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("db.mongodb.modified", result.ModifiedCount);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<ReplaceOneResult> ReplaceOneAsync(
        Expression<Func<TDocument, bool>> filter,
        TDocument replacement,
        ReplaceOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("MongoDB.ReplaceOne");
        activity?.SetTag("db.system", "mongodb");
        activity?.SetTag("db.operation", "replaceOne");
        activity?.SetTag("db.collection.name", CollectionNamespace.CollectionName);
        activity?.SetTag("db.name", CollectionNamespace.DatabaseNamespace.DatabaseName);

        try
        {
            var result = await _collection.ReplaceOneAsync(filter, replacement, options, cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("db.mongodb.modified", result.ModifiedCount);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<UpdateResult> UpdateManyAsync(
        FilterDefinition<TDocument> filter,
        UpdateDefinition<TDocument> update,
        UpdateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("MongoDB.UpdateMany");
        activity?.SetTag("db.system", "mongodb");
        activity?.SetTag("db.operation", "updateMany");
        activity?.SetTag("db.collection.name", CollectionNamespace.CollectionName);
        activity?.SetTag("db.name", CollectionNamespace.DatabaseNamespace.DatabaseName);

        try
        {
            var result = await _collection.UpdateManyAsync(filter, update, options, cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("db.mongodb.modified", result.ModifiedCount);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

    public async Task<UpdateResult> UpdateOneAsync(
        FilterDefinition<TDocument> filter,
        UpdateDefinition<TDocument> update,
        UpdateOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("MongoDB.UpdateOne");
        activity?.SetTag("db.system", "mongodb");
        activity?.SetTag("db.operation", "updateOne");
        activity?.SetTag("db.collection.name", CollectionNamespace.CollectionName);
        activity?.SetTag("db.name", CollectionNamespace.DatabaseNamespace.DatabaseName);

        try
        {
            var result = await _collection.UpdateOneAsync(filter, update, options, cancellationToken);
            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.SetTag("db.mongodb.modified", result.ModifiedCount);
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            throw;
        }
    }

}
