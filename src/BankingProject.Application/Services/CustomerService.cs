using System.Diagnostics;
using BankingProject.Application.DTOs.BalanceOperationDTOs;
using BankingProject.Application.DTOs.CustomerDTOs;
using BankingProject.Application.Extensions;
using BankingProject.Domain.Context.CustomerAggregate.Repositories;

namespace BankingProject.Application.Services;

/// <summary>
/// Service for customer-related operations following DDD and Microsoft standards
/// </summary>
public class CustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ActivitySource _activitySource;

    public CustomerService(ICustomerRepository customerRepository, ActivitySource activitySource)
    {
        _customerRepository = customerRepository;
        _activitySource = activitySource;
    }

    /// <summary>
    /// Creates a new customer from a DTO request
    /// </summary>
    /// <param name="request">The create customer request DTO</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>CustomerResponse DTO</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    public async Task<CustomerResponse> CreateCustomerAsync(CreateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("CustomerService.CreateCustomer");
        activity?.SetTag("operation", "create_customer");
        activity?.SetTag("customer.email", request?.Email);
        
        try
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            var customer = request.ToDomainEntity();
            await _customerRepository.SaveAsync(customer);
            
            var response = customer.ToResponse();
            activity?.SetTag("customer.id", response.Id.ToString());
            activity?.SetStatus(ActivityStatusCode.Ok, "Customer created successfully");
            
            return response;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }

    /// <summary>
    /// Gets a customer by ID and returns as DTO
    /// </summary>
    /// <param name="id">The customer ID</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>CustomerResponse DTO or null if not found</returns>
    public async Task<CustomerResponse?> GetCustomerByIdDtoAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("CustomerService.GetCustomerById");
        activity?.SetTag("operation", "get_customer_by_id");
        activity?.SetTag("customer.id", id.ToString());
        
        try
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            var response = customer?.ToResponse();
            
            if (response != null)
            {
                activity?.SetTag("customer.found", "true");
                activity?.SetTag("customer.email", response.Email);
                activity?.SetStatus(ActivityStatusCode.Ok, "Customer retrieved successfully");
            }
            else
            {
                activity?.SetTag("customer.found", "false");
                activity?.SetStatus(ActivityStatusCode.Ok, "Customer not found");
            }
            
            return response;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }

    /// <summary>
    /// Gets all customers and returns as DTOs
    /// </summary>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Collection of CustomerResponse DTOs</returns>
    public async Task<IEnumerable<CustomerResponse>> GetAllCustomersDtoAsync(CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("CustomerService.GetAllCustomers");
        activity?.SetTag("operation", "get_all_customers");
        
        try
        {
            var customers = await _customerRepository.GetAllAsync();
            var responses = customers.ToResponseList();
            
            activity?.SetTag("customers.count", responses.Count().ToString());
            activity?.SetStatus(ActivityStatusCode.Ok, $"Retrieved {responses.Count()} customers successfully");
            
            return responses;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }

    /// <summary>
    /// Transfers balance between customers using DTOs
    /// </summary>
    /// <param name="senderCustomerId">The sender customer ID</param>
    /// <param name="request">The transfer request DTO</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>TransferBalanceResponse DTO</returns>
    public async Task<TransferBalanceResponse> TransferBalanceDtoAsync(Guid senderCustomerId, TransferBalanceRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("CustomerService.TransferBalance");
        activity?.SetTag("operation", "transfer_balance");
        activity?.SetTag("sender.id", senderCustomerId.ToString());
        activity?.SetTag("recipient.id", request?.TargetCustomerId.ToString());
        activity?.SetTag("amount", request?.Amount.ToString());
        
        try
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            await _customerRepository.TransferBalanceAsync(
                senderCustomerId, 
                request.TargetCustomerId, 
                request.Amount, 
                request.Description);

            var response = new TransferBalanceResponse
            {
                Message = "Balance transferred successfully",
                Amount = request.Amount,
                FromCustomerId = senderCustomerId,
                ToCustomerId = request.TargetCustomerId,
                TransactionDate = DateTime.UtcNow,
                Description = request.Description
            };
            
            activity?.SetStatus(ActivityStatusCode.Ok, "Balance transferred successfully");
            return response;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }

    /// <summary>
    /// Deletes a customer by ID
    /// </summary>
    /// <param name="id">The customer ID</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>True if deleted, false if not found</returns>
    public async Task<bool> DeleteCustomerByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("CustomerService.DeleteCustomer");
        activity?.SetTag("operation", "delete_customer");
        activity?.SetTag("customer.id", id.ToString());
        
        try
        {
            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                activity?.SetTag("customer.found", "false");
                activity?.SetStatus(ActivityStatusCode.Ok, "Customer not found");
                return false;
            }

            await _customerRepository.DeleteAsync(customer);
            activity?.SetTag("customer.found", "true");
            activity?.SetStatus(ActivityStatusCode.Ok, "Customer deleted successfully");
            return true;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }

    /// <summary>
    /// Updates a customer with the provided DTO data
    /// </summary>
    /// <param name="id">The customer ID</param>
    /// <param name="request">The update customer request DTO</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Updated CustomerResponse DTO or null if customer not found</returns>
    /// <exception cref="ArgumentNullException">Thrown when request is null</exception>
    /// <exception cref="ArgumentException">Thrown when no fields are provided for update</exception>
    public async Task<CustomerResponse?> UpdateCustomerAsync(Guid id, UpdateCustomerRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("CustomerService.UpdateCustomer");
        activity?.SetTag("operation", "update_customer");
        activity?.SetTag("customer.id", id.ToString());
        activity?.SetTag("update.firstName", !string.IsNullOrWhiteSpace(request?.FirstName) ? "true" : "false");
        activity?.SetTag("update.lastName", !string.IsNullOrWhiteSpace(request?.LastName) ? "true" : "false");
        activity?.SetTag("update.email", !string.IsNullOrWhiteSpace(request?.Email) ? "true" : "false");
        activity?.SetTag("update.phoneNumber", !string.IsNullOrWhiteSpace(request?.PhoneNumber) ? "true" : "false");
        
        try
        {
            ArgumentNullException.ThrowIfNull(request, nameof(request));

            if (!request.HasAnyValue())
            {
                throw new ArgumentException("At least one field must be provided for update", nameof(request));
            }

            var customer = await _customerRepository.GetByIdAsync(id);
            if (customer == null)
            {
                activity?.SetTag("customer.found", "false");
                activity?.SetStatus(ActivityStatusCode.Ok, "Customer not found");
                return null;
            }

            customer.UpdateFromRequest(request);
            
            await _customerRepository.UpdateAsync(customer);
            
            var response = customer.ToResponse();
            activity?.SetTag("customer.found", "true");
            activity?.SetTag("customer.email", response.Email);
            activity?.SetStatus(ActivityStatusCode.Ok, "Customer updated successfully");
            
            return response;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }

}
