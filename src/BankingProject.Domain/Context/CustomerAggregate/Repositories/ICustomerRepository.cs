using BankingProject.Domain.Context.CustomerAggregate.ValueObjects;

namespace BankingProject.Domain.Context.CustomerAggregate.Repositories;


public interface ICustomerRepository : IGenericRepository<Customer>
{
    public Task<Customer?> GetByIdAsync(Guid id);
    public Task<Customer?> GetByMerchantAsync(string merchantDocument);
    public Task<IEnumerable<Customer>> GetAllAsync();
    public Task DeleteAsync(Customer entity);
    public Task UpdateAsync(Customer entity);

    public Task<IEnumerable<BalanceOperation>> GetBalanceOperationsAsync(Guid customerId);
    public Task<BalanceOperation?> GetBalanceOperationByIdAsync(Guid id);
    public Task InsertBalanceOperationAsync(Guid customerId, BalanceOperation entity);
    public Task DeleteBalanceOperationAsync(Guid id);
    
    public Task TransferBalanceAsync(Guid fromCustomerId, Guid toCustomerId, int amount, string description = "");
}