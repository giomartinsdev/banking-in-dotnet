using System.Reflection;
using BankingProject.Application.DTOs.BalanceOperationDTOs;
using BankingProject.Application.DTOs.CustomerDTOs;
using BankingProject.Application.Extensions;
using BankingProject.Domain.Context.CustomerAggregate.Repositories;

namespace BankingProject.Application.Services;

public class CustomerService
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    #region DTO-based Methods (Recommended for API Controllers)

    /// <summary>
    /// Creates a new customer from a DTO request
    /// </summary>
    /// <param name="request">The create customer request DTO</param>
    /// <returns>CustomerResponse DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    public async Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        var customer = request.ToDomainEntity();
        await _customerRepository.SaveAsync(customer);
        
        return customer.ToResponse();
    }

    /// <summary>
    /// Gets a customer by ID and returns as DTO
    /// </summary>
    /// <param name="id">The customer ID</param>
    /// <returns>CustomerResponse DTO or null if not found</returns>
    public async Task<CustomerResponse?> GetCustomerByIdDtoAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        return customer?.ToResponse();
    }

    /// <summary>
    /// Gets all customers and returns as DTOs
    /// </summary>
    /// <returns>Collection of CustomerResponse DTOs</returns>
    public async Task<IEnumerable<CustomerResponse>> GetAllCustomersDtoAsync()
    {
        var customers = await _customerRepository.GetAllAsync();
        return customers.ToResponseList();
    }

    /// <summary>
    /// Transfers balance between customers using DTOs
    /// </summary>
    /// <param name="senderCustomerId">The sender customer ID</param>
    /// <param name="request">The transfer request DTO</param>
    /// <returns>TransferBalanceResponse DTO</returns>
    public async Task<TransferBalanceResponse> TransferBalanceDtoAsync(Guid senderCustomerId, TransferBalanceRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        await _customerRepository.TransferBalanceAsync(
            senderCustomerId, 
            request.TargetCustomerId, 
            request.Amount, 
            request.Description);

        return new TransferBalanceResponse
        {
            Message = "Balance transferred successfully",
            Amount = request.Amount,
            FromCustomerId = senderCustomerId,
            ToCustomerId = request.TargetCustomerId,
            TransactionDate = DateTime.UtcNow,
            Description = request.Description
        };
    }

    /// <summary>
    /// Deletes a customer by ID
    /// </summary>
    /// <param name="id">The customer ID</param>
    /// <returns>True if deleted, false if not found</returns>
    public async Task<bool> DeleteCustomerByIdAsync(Guid id)
    {
        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            return false;
        }

        await _customerRepository.DeleteAsync(customer);
        return true;
    }

    /// <summary>
    /// Updates a customer with the provided DTO data
    /// </summary>
    /// <param name="id">The customer ID</param>
    /// <param name="request">The update customer request DTO</param>
    /// <returns>Updated CustomerResponse DTO or null if customer not found</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    /// <exception cref="ArgumentException">Thrown when no fields are provided for update</exception>
    public async Task<CustomerResponse?> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request)
    {
        ArgumentNullException.ThrowIfNull(request, nameof(request));

        if (!request.HasAnyValue())
        {
            throw new ArgumentException("At least one field must be provided for update", nameof(request));
        }

        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            return null;
        }

        // Update the customer using the mapping extension
        customer.UpdateFromRequest(request);
        
        // Save the updated customer
        await _customerRepository.UpdateAsync(customer);
        
        return customer.ToResponse();
    }

    #endregion

    #region Legacy Methods (for backward compatibility)

    public async Task SaveCustomerAsync(Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);
        await _customerRepository.SaveAsync(customer);
    }

    public async Task<Customer?> GetCustomerByIdAsync(Guid id)
    {
        return await _customerRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
    {
        return await _customerRepository.GetAllAsync();
    }

    public async Task DeleteCustomerAsync(Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);
        await _customerRepository.DeleteAsync(customer);
    }

    public async Task TransferBalanceAsync(Guid fromCustomerId, Guid toCustomerId, int amount, string description = "")
    {
        await _customerRepository.TransferBalanceAsync(fromCustomerId, toCustomerId, amount, description);
    }

    public async Task<Customer?> UpdateFieldsAsync(Guid id, Dictionary<string, string>? updates)
    {
        ArgumentNullException.ThrowIfNull(updates);

        var customer = await _customerRepository.GetByIdAsync(id);
        if (customer == null)
        {
            return null;
        }

        foreach (var update in updates)
        {
            var fieldName = update.Key;
            var fieldValue = update.Value;

            var property = customer.GetType().GetProperty(fieldName, BindingFlags.Public | BindingFlags.Instance);
            
            if (property != null && property.CanWrite)
            {
                try
                {
                    property.SetValue(customer, Convert.ChangeType(fieldValue, property.PropertyType));
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Cannot set property '{fieldName}' to value '{fieldValue}': {ex.Message}");
                }
            }
            else if (fieldName.Contains('.'))
            {
                var parts = fieldName.Split('.');
                if (parts.Length == 2)
                {
                    var parentProperty = customer.GetType().GetProperty(parts[0], BindingFlags.Public | BindingFlags.Instance);
                    if (parentProperty != null)
                    {
                        var parentValue = parentProperty.GetValue(customer);
                        if (parentValue != null)
                        {
                            var childProperty = parentValue.GetType().GetProperty(parts[1], BindingFlags.Public | BindingFlags.Instance);
                            if (childProperty != null && childProperty.CanWrite)
                            {
                                try
                                {
                                    childProperty.SetValue(parentValue, Convert.ChangeType(fieldValue, childProperty.PropertyType));
                                }
                                catch (Exception ex)
                                {
                                    throw new InvalidOperationException($"Cannot set nested property '{fieldName}' to value '{fieldValue}': {ex.Message}");
                                }
                            }
                        }
                    }
                }
            }
        }

        await _customerRepository.UpdateAsync(customer);
        return customer;
    }

    #endregion
}
