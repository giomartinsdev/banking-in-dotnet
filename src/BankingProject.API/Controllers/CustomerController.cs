using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BankingProject.Application.Services;
using BankingProject.Application.DTOs.CustomerDTOs;
using BankingProject.Application.DTOs.BalanceOperationDTOs;
using BankingProject.Domain.Context.CustomerAggregate.ValueObjects;

namespace BankingProject.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly CustomerService _customerService;
    private readonly ActivitySource _activitySource;

    public CustomerController(CustomerService customerService, ActivitySource activitySource)
    {
        _customerService = customerService;
        _activitySource = activitySource;
    }

    /// <summary>
    /// Creates a new customer
    /// </summary>
    /// <param name="request">The create customer request DTO</param>
    /// <returns>Created customer response</returns>
    [MapToApiVersion("1.0")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
    {
        using var activity = _activitySource.StartActivity
        (
            $"{HttpContext.Request.Path} | Creating a new customer",
            ActivityKind.Server
        )!;

        var customerResponse = await _customerService.CreateCustomerAsync(request);

        activity.SetStatus(ActivityStatusCode.Ok, "Customer created successfully");
        return CreatedAtAction(nameof(GetCustomerById), new { id = customerResponse.Id }, customerResponse);
    }

    /// <summary>
    /// Gets a customer by their unique identifier
    /// </summary>
    /// <param name="id">The customer's unique identifier</param>
    /// <returns>Customer details</returns>
    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCustomerById(Guid id)
    {
        using var activity = _activitySource.StartActivity
        (
            $"{HttpContext.Request.Path} | Getting customer by ID",
            ActivityKind.Server
        )!;

        var customer = await _customerService.GetCustomerByIdDtoAsync(id);
        if (customer == null)
        {
            activity.SetStatus(ActivityStatusCode.Error, "Customer not found");
            return NotFound(new { Error = "Customer not found" });
        }

        activity.SetStatus(ActivityStatusCode.Ok, "Customer retrieved successfully");
        return Ok(customer);
    }

    /// <summary>
    /// Gets all customers
    /// </summary>
    /// <returns>List of all customers</returns>
    [MapToApiVersion("1.0")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAllCustomers()
    {
        using var activity = _activitySource.StartActivity
        (
            $"{HttpContext.Request.Path} | Getting all customers",
            ActivityKind.Server
        )!;

        var customers = await _customerService.GetAllCustomersDtoAsync();
        activity.SetStatus(ActivityStatusCode.Ok, "All customers retrieved successfully");
        return Ok(customers);
    }

    /// <summary>
    /// Deletes a customer by their unique identifier
    /// </summary>
    /// <param name="id">The customer's unique identifier</param>
    /// <returns>Deletion confirmation</returns>
    [MapToApiVersion("1.0")]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCustomer(Guid id)
    {
        using var activity = _activitySource.StartActivity
        (
            $"{HttpContext.Request.Path} | Deleting customer by ID",
            ActivityKind.Server
        )!;

        var deleted = await _customerService.DeleteCustomerByIdAsync(id);
        if (!deleted)
        {
            activity.SetStatus(ActivityStatusCode.Error, "Customer not found");
            return NotFound(new { Error = "Customer not found" });
        }

        activity.SetStatus(ActivityStatusCode.Ok, "Customer deleted successfully");
        return Ok(new { Message = "Customer deleted successfully" });
    }

    /// <summary>
    /// Updates specific customer fields
    /// </summary>
    /// <param name="id">The customer's unique identifier</param>
    /// <param name="request">The update request DTO</param>
    /// <returns>Updated customer information</returns>
    [MapToApiVersion("1.0")]
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomerRequest request)
    {
        using var activity = _activitySource.StartActivity
        (
            $"{HttpContext.Request.Path} | Updating customer fields",
            ActivityKind.Server
        )!;

        if (request == null)
        {
            activity.SetStatus(ActivityStatusCode.Error, "No update data provided");
            return BadRequest(new { Error = "No update data provided" });
        }

        if (!request.HasAnyValue())
        {
            activity.SetStatus(ActivityStatusCode.Error, "At least one field must be provided for update");
            return BadRequest(new { Error = "At least one field must be provided for update" });
        }

        var updatedCustomer = await _customerService.UpdateCustomerAsync(id, request);
        if (updatedCustomer == null)
        {
            activity.SetStatus(ActivityStatusCode.Error, "Customer not found");
            return NotFound(new { Error = "Customer not found" });
        }

        activity.SetStatus(ActivityStatusCode.Ok, "Customer updated successfully");
        return Ok(updatedCustomer);
    }


    /// <summary>
    /// Transfers balance from one customer to another
    /// </summary>
    /// <param name="senderCustomerId">The ID of the customer sending the balance</param>
    /// <param name="request">The transfer request DTO</param>
    /// <returns>Transfer confirmation</returns>
    [MapToApiVersion("1.0")]
    [HttpPost("{senderCustomerId:guid}/transfer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransferBalance(Guid senderCustomerId, [FromBody] TransferBalanceRequest request)
    {
        using var activity = _activitySource.StartActivity(
            $"{HttpContext.Request.Path} | Transferring balance to customer",
            ActivityKind.Server
        )!;
        
        // Validate input parameters
        if (request == null)
        {
            activity.SetStatus(ActivityStatusCode.Error, "Transfer request is required");
            return BadRequest(new { Error = "Transfer request is required" });
        }

        if (senderCustomerId == Guid.Empty)
        {
            activity.SetStatus(ActivityStatusCode.Error, "Invalid sender customer ID");
            return BadRequest(new { Error = "Sender customer ID cannot be empty" });
        }
        
        if (request.TargetCustomerId == Guid.Empty)
        {
            activity.SetStatus(ActivityStatusCode.Error, "Invalid target customer ID");
            return BadRequest(new { Error = "Target customer ID cannot be empty" });
        }
        
        if (request.Amount <= 0)
        {
            activity.SetStatus(ActivityStatusCode.Error, "Invalid transfer amount");
            return BadRequest(new { Error = "Transfer amount must be greater than zero" });
        }

        var transferResponse = await _customerService.TransferBalanceDtoAsync(senderCustomerId, request);
        
        activity.SetStatus(ActivityStatusCode.Ok, "Balance transferred successfully");
        return Ok(transferResponse);
    }
}

