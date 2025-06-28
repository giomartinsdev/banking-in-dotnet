using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using BankingProject.Application.Services;
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

    [MapToApiVersion("1.0")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCustomer([FromBody] Customer customer)
    {
        using var activity = _activitySource.StartActivity
        (
            $"{HttpContext.Request.Path} | Creating a new customer",
            ActivityKind.Server
        )!;

        try
        {
            await _customerService.SaveCustomerAsync(customer);

            activity.SetStatus(ActivityStatusCode.Ok, "Customer created successfully");
            return Ok(customer);
        }
        catch (Exception e)
        {
            activity.AddException(e);
            activity.SetStatus(ActivityStatusCode.Error, "Error saving customer");
            return BadRequest(new { Error = e.Message, Details = "Failed to create customer" });
        }
    }

    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetCustomerById(Guid id)
    {
        using var activity = _activitySource.StartActivity
        (
            $"{HttpContext.Request.Path} | Getting customer by ID",
            ActivityKind.Server
        )!;

        try
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            activity.SetStatus(ActivityStatusCode.Ok, "Customer retrieved successfully");
            return Ok(customer);
        }
        catch (Exception e)
        {
            activity.AddException(e);
            activity.SetStatus(ActivityStatusCode.Error, "Error retrieving customer");
            return BadRequest(new { Error = e.Message, Details = "Failed to retrieve customer by ID" });
        }
    }

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

        try
        {
            var customers = await _customerService.GetAllCustomersAsync();
            activity.SetStatus(ActivityStatusCode.Ok, "All customers retrieved successfully");
            return Ok(customers);
        }
        catch (Exception e)
        {
            activity.AddException(e);
            activity.SetStatus(ActivityStatusCode.Error, "Error retrieving all customers");
            return BadRequest(new { Error = e.Message, Details = "Failed to retrieve all customers" });
        }
    }

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

        try
        {
            Customer? customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null) return NotFound("Customer not found");

            await _customerService.DeleteCustomerAsync(customer);
            activity.SetStatus(ActivityStatusCode.Ok, "Customer deleted successfully");
            return Ok();
        }
        catch (Exception e)
        {
            activity.AddException(e);
            activity.SetStatus(ActivityStatusCode.Error, "Error deleting customer");
            return BadRequest(new { Error = e.Message, Details = "Failed to delete customer" });
        }
    }

    [MapToApiVersion("1.0")]
    [HttpPatch("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCustomerField(Guid id, [FromQuery] Dictionary<string, string>? updates)
    {
        using var activity = _activitySource.StartActivity
        (
            $"{HttpContext.Request.Path} | Updating customer fields",
            ActivityKind.Server
        )!;

        try
        {
            if (updates == null || updates.Count == 0)
            {
                activity.SetStatus(ActivityStatusCode.Error, "No updates provided");
                return BadRequest(new { Error = "No updates provided" });
            }

            var updatedCustomer = await _customerService.UpdateFieldsAsync(id, updates);
            if (updatedCustomer == null)
            {
                activity.SetStatus(ActivityStatusCode.Error, "Customer not found");
                return NotFound(new { Error = "Customer not found" });
            }

            activity.SetStatus(ActivityStatusCode.Ok, "Customer updated successfully");
            return Ok(updatedCustomer);
        }
        catch (Exception e)
        {
            activity.AddException(e);
            activity.SetStatus(ActivityStatusCode.Error, "Error updating customer");
            return BadRequest(new { Error = e.Message });
        }
    }


    [MapToApiVersion("1.0")]
    [HttpPost("transfer")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransferBalance(Guid senderCustomerId, [FromQuery] Guid targetCustomerId, [FromQuery] int amount)
    {
        using var activity = _activitySource.StartActivity(
            $"{HttpContext.Request.Path} | Transferring balance to customer",
            ActivityKind.Server
        )!;
        try
        {
            await _customerService.TransferBalanceAsync(senderCustomerId, targetCustomerId, amount, $"{senderCustomerId} | {targetCustomerId} | {amount}");
            return Ok(new { Message = "Balance transferred successfully", Amount = amount, FromCustomerId = senderCustomerId, ToCustomerId = targetCustomerId });
        }
        catch (InvalidOperationException ex)
        {
            activity.AddException(ex);
            activity.SetStatus(ActivityStatusCode.Error, "Transfer failed due to insufficient balance or invalid customer");
            return BadRequest(new { Error = ex.Message });
        }
        catch (Exception e)
        {
            activity.AddException(e);
            activity.SetStatus(ActivityStatusCode.Error, "Error transferring balance");
            return BadRequest(new { Error = e.Message, Details = "Failed to transfer balance" });
        }
    }
}

