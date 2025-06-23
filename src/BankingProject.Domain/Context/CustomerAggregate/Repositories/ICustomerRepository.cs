using BankingProject.Domain.Context.CustomerAggregate.Entites;

namespace BankingProject.Domain.Context.CustomerAggregate.Repositories;


public interface ICustomerRepository : IGenericRepository<Customer>
{
    public Task<Customer?> GetByIdAsync(Guid id);
    public Task<Customer?> GetByMerchantAsync(string merchantDocument);
    public Task<IEnumerable<Customer>> GetAllAsync();
    public Task DeleteAsync(Customer entity);
    public Task UpdateAsync(Customer entity);

}