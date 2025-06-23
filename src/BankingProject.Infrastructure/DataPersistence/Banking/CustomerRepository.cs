using System.Reflection.Metadata;
using BankingProject.Domain.Context.CustomerAggregate;
using BankingProject.Domain.Context.CustomerAggregate.Entites;
using BankingProject.Domain.Context.CustomerAggregate.Repositories;
using BankingProject.Infrastructure;
using BankingProject.Infrastructure.Instrumentation.Metrics;
using BankingProject.Infrastructure.MongoDB;
using MongoDB.Driver;

namespace Microsoft.Extensions.Hosting.DataPersistence.Banking;

public sealed class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    public CustomerRepository(IMongoDatabase database) : base(database, CollectionsEnum.Customers)
    {
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        var filter = Builders<Customer>.Filter.Eq(x => x.Id, id);
        return await DocumentCollection.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<Customer?> GetByMerchantAsync(string merchantDocument)
    {
        var filter = Builders<Customer>.Filter.Eq(
            x => x.PrivatePersonalInformation.MerchantDocument, merchantDocument);
        return await DocumentCollection.Find(filter).FirstOrDefaultAsync();
    }

    public Task<IEnumerable<Customer>> GetAllAsync()
    {
        return Task.FromResult(DocumentCollection
            .Find(FilterDefinition<Customer>.Empty, new FindOptions { BatchSize = 1000, NoCursorTimeout = true })
            .Sort(Builders<Customer>.Sort.Ascending(c => c.Id))
            .ToEnumerable());
    }

    public Task DeleteAsync(Customer entity)
    {
        return DocumentCollection.DeleteOneAsync(x => x.Id == entity.Id);
    }
   
    public Task UpdateAsync(Customer entity)
    {
        return DocumentCollection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
    }
}