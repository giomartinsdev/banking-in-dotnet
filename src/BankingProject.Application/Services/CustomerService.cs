using System.Reflection;
using BankingProject.Domain.Context.CustomerAggregate.Repositories;
using BankingProject.Domain.Context.CustomerAggregate.ValueObjects;

namespace BankingProject.Application.Services;

public class CustomerService
{
    private readonly ICustomerRepository _customerRepository;
    
    public CustomerService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }
    
    public async Task SaveCustomerAsync(Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        await _customerRepository.SaveAsync(customer);
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
                    var parentPropName = parts[0];
                    var childPropName = parts[1];
                    
                    var parentProperty = customer.GetType().GetProperty(parentPropName, BindingFlags.Public | BindingFlags.Instance);
                    if (parentProperty != null)
                    {
                        var parentValue = parentProperty.GetValue(customer);
                        if (parentValue != null)
                        {
                            var childProperty = parentValue.GetType().GetProperty(childPropName, BindingFlags.Public | BindingFlags.Instance);
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
                            else
                            {
                                throw new InvalidOperationException($"Nested property '{childPropName}' not found or not writable in '{parentPropName}'");
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException($"Parent property '{parentPropName}' is null");
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException($"Parent property '{parentPropName}' not found");
                    }
                }
                else
                {
                    throw new InvalidOperationException($"Property path '{fieldName}' has unsupported format. Use 'Parent.Child' for nested properties");
                }
            }
            else
            {
                throw new InvalidOperationException($"Property '{fieldName}' not found on Customer");
            }
        }

        customer.ValidInformation.Update();
        await _customerRepository.UpdateAsync(customer);
        return customer;
    }
    
    public async Task<Customer?> GetCustomerByIdAsync(Guid id)
    {
        return await _customerRepository.GetByIdAsync(id);
    }
    
    public async Task<Customer?> FindCustomerByMerchantAsync(string merchantDocument)
    {
        ArgumentNullException.ThrowIfNull(merchantDocument);

        return await _customerRepository.GetByMerchantAsync(merchantDocument);
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
    
    public async Task UpdateCustomerAsync(Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);

        await _customerRepository.UpdateAsync(customer);
    }
}
