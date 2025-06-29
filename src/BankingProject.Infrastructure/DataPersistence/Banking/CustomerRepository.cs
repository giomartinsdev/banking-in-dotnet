using System.Diagnostics;
using BankingProject.Domain.Context.CustomerAggregate.Repositories;
using BankingProject.Domain.Context.CustomerAggregate.ValueObjects;
using BankingProject.Infrastructure.MongoDB;
using MongoDB.Driver;

namespace BankingProject.Infrastructure.DataPersistence.Banking;

/// <summary>
/// Repository for Customer aggregate operations with MongoDB tracing
/// </summary>
public sealed class CustomerRepository : GenericRepository<Customer>, ICustomerRepository
{
    /// <summary>
    /// Initializes a new instance of the CustomerRepository class
    /// </summary>
    /// <param name="database">The MongoDB database</param>
    /// <param name="activitySource">The activity source for tracing</param>
    public CustomerRepository(IMongoDatabase database, ActivitySource activitySource) 
        : base(database, CollectionsEnum.Customers, activitySource)
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

    public async Task TransferBalanceAsync(Guid fromCustomerId, Guid toCustomerId, int amount, string description)
    {
        Customer? fromCustomer = await GetByIdAsync(fromCustomerId);
        Customer? toCustomer = await GetByIdAsync(toCustomerId);
        int customerBalance = fromCustomer?.GetBalance() ?? 0;

        if (fromCustomer == null || toCustomer == null)
            throw new InvalidOperationException("One or both customers not found");
        if (customerBalance < amount)
            throw new InvalidOperationException("User does not have enough balance");

        var validInformation = new ValidInformation(true);
        var op = new BalanceOperation(amount, description, validInformation);
        var negativeOp = new BalanceOperation(op.NegativeBalanceOperationAmount(), description, validInformation);

        await InsertBalanceOperationAsync(fromCustomerId, negativeOp);
        await InsertBalanceOperationAsync(toCustomerId, op);
    }
}
