using BankingProject.Domain.Context.CustomerAggregate.Repositories;
using BankingProject.Domain.Context.CustomerAggregate.ValueObjects;

namespace BankingProject.Application.Services;

public class BalanceOperationService
{
    private readonly ICustomerRepository _customerRepository;

    public BalanceOperationService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<IEnumerable<BalanceOperation>> GetBalanceOperationsAsync(Guid customerId)
    {
        return await _customerRepository.GetBalanceOperationsAsync(customerId);
    }
    public async Task<BalanceOperation?> GetBalanceOperationByIdAsync(Guid id)
    {
        return await _customerRepository.GetBalanceOperationByIdAsync(id);
    }
    public async Task InsertBalanceOperationAsync(Guid customerId, BalanceOperation entity)
    {
        await _customerRepository.InsertBalanceOperationAsync(customerId, entity);
    }
    public async Task DeleteBalanceOperationAsync(Guid id)
    {
        await _customerRepository.DeleteBalanceOperationAsync(id);
    }
}
