using System.Diagnostics;
using BankingProject.Application.Services;
using BankingProject.Domain.Context.CustomerAggregate.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace BankingProject.ApiService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BalanceOperationController : ControllerBase
{
    private readonly BalanceOperationService _balanceOperationService;
    private readonly ActivitySource _activitySource;
    
    public BalanceOperationController(BalanceOperationService balanceOperationService, ActivitySource activitySource)
    {
        _balanceOperationService = balanceOperationService;
        _activitySource = activitySource;
    }
    
    [HttpPost("customer/{customerId:guid}")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBalanceOperation(Guid customerId, [FromBody] BalanceOperation entity)
    {
        using var activity = _activitySource.StartActivity
        (
            $"{HttpContext.Request.Path} | Creating a new balance operation",
            ActivityKind.Server
        )!;

        try
        {
            await _balanceOperationService.InsertBalanceOperationAsync(customerId, entity);

            activity.SetStatus(ActivityStatusCode.Ok, "BalanceOperation created successfully");
            return CreatedAtAction(nameof(GetBalanceOperationById), new { id = entity.Id }, entity);
        }
        catch (Exception e)
        {
            activity.AddException(e);
            activity.SetStatus(ActivityStatusCode.Error, "Error saving BalanceOperation");
            return BadRequest(new { Error = activity.Status == ActivityStatusCode.Error });
        }
    }
    
    [MapToApiVersion("1.0")]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBalanceOperationById(Guid id)
    {
        using var activity = _activitySource.StartActivity
        (
            $"{HttpContext.Request.Path} | Getting balance operation by ID",
            ActivityKind.Server
        )!;

        try
        {
            var balanceOperation = await _balanceOperationService.GetBalanceOperationByIdAsync(id);
            if (balanceOperation == null)
            {
                return NotFound();
            }

            activity.SetStatus(ActivityStatusCode.Ok, "BalanceOperation retrieved successfully");
            return Ok(balanceOperation);
        }
        catch (Exception e)
        {
            activity.AddException(e);
            activity.SetStatus(ActivityStatusCode.Error, "Error retrieving BalanceOperation");
            return BadRequest(new { Error = activity.Status == ActivityStatusCode.Error });
        }
    }
    
    [MapToApiVersion("1.0")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBalanceOperationsByCustomerId(Guid customerId)
    {
        using var activity = _activitySource.StartActivity
        (
            $"{HttpContext.Request.Path} | Getting balance operations by customer ID",
            ActivityKind.Server
        )!;

        try
        {
            var balanceOperations = await _balanceOperationService.GetBalanceOperationsAsync(customerId);
            if (!balanceOperations.Any())
            {
                return NotFound();
            }

            activity.SetStatus(ActivityStatusCode.Ok, "BalanceOperations retrieved successfully");
            return Ok(balanceOperations);
        }
        catch (Exception e)
        {
            activity.AddException(e);
            activity.SetStatus(ActivityStatusCode.Error, "Error retrieving BalanceOperations");
            return BadRequest(new { Error = activity.Status == ActivityStatusCode.Error });
        }
    }
    
    [MapToApiVersion("1.0")]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBalanceOperation(Guid id)
    {
        using var activity = _activitySource.StartActivity
        (
            $"{HttpContext.Request.Path} | Deleting balance operation by ID",
            ActivityKind.Server
        )!;

        try
        {
            await _balanceOperationService.DeleteBalanceOperationAsync(id);
            activity.SetStatus(ActivityStatusCode.Ok, "BalanceOperation deleted successfully");
            return Ok();
        }
        catch (Exception e)
        {
            activity.AddException(e);
            activity.SetStatus(ActivityStatusCode.Error, "Error deleting BalanceOperation");
            return BadRequest(new { Error = activity.Status == ActivityStatusCode.Error });
        }
    }
    
}