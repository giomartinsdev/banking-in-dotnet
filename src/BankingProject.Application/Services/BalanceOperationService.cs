using System.Diagnostics;
using BankingProject.Domain.Context.CustomerAggregate.Repositories;
using BankingProject.Domain.Context.CustomerAggregate.ValueObjects;

namespace BankingProject.Application.Services;

/// <summary>
/// Service for balance operation-related operations with distributed tracing
/// </summary>
public class BalanceOperationService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ActivitySource _activitySource;

    public BalanceOperationService(ICustomerRepository customerRepository, ActivitySource activitySource)
    {
        _customerRepository = customerRepository;
        _activitySource = activitySource;
    }

    /// <summary>
    /// Gets all balance operations for a specific customer
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Collection of balance operations</returns>
    public async Task<IEnumerable<BalanceOperation>> GetBalanceOperationsAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("BalanceOperationService.GetBalanceOperations");
        activity?.SetTag("operation", "get_balance_operations");
        activity?.SetTag("customer.id", customerId.ToString());
        
        try
        {
            var operations = await _customerRepository.GetBalanceOperationsAsync(customerId);
            activity?.SetTag("operations.count", operations.Count().ToString());
            activity?.SetStatus(ActivityStatusCode.Ok, $"Retrieved {operations.Count()} balance operations successfully");
            return operations;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }

    /// <summary>
    /// Gets a specific balance operation by ID
    /// </summary>
    /// <param name="id">The balance operation ID</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <returns>Balance operation or null if not found</returns>
    public async Task<BalanceOperation?> GetBalanceOperationByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("BalanceOperationService.GetBalanceOperationById");
        activity?.SetTag("operation", "get_balance_operation_by_id");
        activity?.SetTag("operation.id", id.ToString());
        
        try
        {
            var operation = await _customerRepository.GetBalanceOperationByIdAsync(id);
            activity?.SetTag("operation.found", operation != null ? "true" : "false");
            if (operation != null)
            {
                activity?.SetTag("operation.amount", operation.Amount.ToString());
                activity?.SetTag("operation.valid", operation.ValidInformation.IsValid.ToString());
            }
            activity?.SetStatus(ActivityStatusCode.Ok, operation != null ? "Balance operation retrieved successfully" : "Balance operation not found");
            return operation;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }

    /// <summary>
    /// Inserts a new balance operation for a customer
    /// </summary>
    /// <param name="customerId">The customer ID</param>
    /// <param name="entity">The balance operation to insert</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    /// <exception cref="ArgumentNullException">Thrown when entity is null</exception>
    public async Task InsertBalanceOperationAsync(Guid customerId, BalanceOperation entity, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("BalanceOperationService.InsertBalanceOperation");
        activity?.SetTag("operation", "insert_balance_operation");
        activity?.SetTag("customer.id", customerId.ToString());
        activity?.SetTag("operation.id", entity?.Id.ToString());
        activity?.SetTag("operation.amount", entity?.Amount.ToString());
        
        try
        {
            ArgumentNullException.ThrowIfNull(entity, nameof(entity));
            
            await _customerRepository.InsertBalanceOperationAsync(customerId, entity);
            activity?.SetStatus(ActivityStatusCode.Ok, "Balance operation inserted successfully");
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }

    /// <summary>
    /// Deletes a balance operation by ID (marks as invalid)
    /// </summary>
    /// <param name="id">The balance operation ID</param>
    /// <param name="cancellationToken">Cancellation token for the operation</param>
    public async Task DeleteBalanceOperationAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("BalanceOperationService.DeleteBalanceOperation");
        activity?.SetTag("operation", "delete_balance_operation");
        activity?.SetTag("operation.id", id.ToString());
        
        try
        {
            await _customerRepository.DeleteBalanceOperationAsync(id);
            activity?.SetStatus(ActivityStatusCode.Ok, "Balance operation deleted successfully");
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            throw;
        }
    }
}
