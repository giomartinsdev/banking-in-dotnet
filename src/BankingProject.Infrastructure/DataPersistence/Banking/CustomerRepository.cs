using BankingProject.Domain.Context.CustomerAggregate.Repositories;
using BankingProject.Domain.Context.CustomerAggregate.ValueObjects;
using BankingProject.Infrastructure.MongoDB;
using MongoDB.Driver;

namespace BankingProject.Infrastructure.DataPersistence.Banking;

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
        entity.ValidInformation.UpdateValidity(false);
        return DocumentCollection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
    }

    public Task UpdateAsync(Customer entity)
    {
        entity.ValidInformation.Update();
        return DocumentCollection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
    }

    public async Task<IEnumerable<BalanceOperation>> GetBalanceOperationsAsync(Guid customerId)
    {
        return await DocumentCollection
            .Find(x => x.Id == customerId)
            .Project(c => c.BalanceOperations)
            .FirstOrDefaultAsync()!;
    }

    public async Task<BalanceOperation?> GetBalanceOperationByIdAsync(Guid id)
    {
        var filter = Builders<Customer>.Filter.ElemMatch(c => c.BalanceOperations, bo => bo.Id == id);
        var customer = await DocumentCollection.Find(filter).FirstOrDefaultAsync();
        return customer.BalanceOperations.FirstOrDefault(bo => bo.Id == id);
    }

    public async Task InsertBalanceOperationAsync(Guid customerId, BalanceOperation entity)
    {
        var filter = Builders<Customer>.Filter.Eq(x => x.Id, customerId);
        var update = Builders<Customer>.Update.Push(c => c.BalanceOperations, entity);
        await DocumentCollection.UpdateOneAsync(filter, update);
    }

    public async Task DeleteBalanceOperationAsync(Guid id)
    {
        var customerFilter = Builders<Customer>.Filter.ElemMatch(c => c.BalanceOperations, bo => bo.Id == id);
        var customer = await DocumentCollection.Find(customerFilter).FirstOrDefaultAsync();

        if (customer != null)
        {
            var balanceOperation = customer.BalanceOperations.FirstOrDefault(bo => bo.Id == id);
            if (balanceOperation != null)
            {
                balanceOperation.ValidInformation.UpdateValidity(false);
                await DocumentCollection.ReplaceOneAsync(x => x.Id == customer.Id, customer);
            }
        }
    }

    public async Task TransferBalanceAsync(Guid fromCustomerId, Guid toCustomerId, int amount, string description = "") 
    {
        Customer? fromCustomer = await GetByIdAsync(fromCustomerId);
        Customer? toCustomer = await GetByIdAsync(toCustomerId);

        if (fromCustomer == null || toCustomer == null)
            throw new InvalidOperationException("One or both customers not found");
        if (fromCustomer.Balance < amount)
            throw new InvalidOperationException("User does not have enough balance");

        BalanceOperation positive = new(amount, description, new ValidInformation(true));
        BalanceOperation negative = new(amount * -1, description, new ValidInformation(true));

        await InsertBalanceOperationAsync(fromCustomerId, negative);
        await InsertBalanceOperationAsync(toCustomerId, positive);
    }
    
}