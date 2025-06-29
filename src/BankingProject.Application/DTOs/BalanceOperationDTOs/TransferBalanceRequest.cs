using System.ComponentModel.DataAnnotations;

namespace BankingProject.Application.DTOs.BalanceOperationDTOs;

/// <summary>
/// Data transfer object for balance transfer operations
/// </summary>
public sealed class TransferBalanceRequest
{
    /// <summary>
    /// Gets or sets the target customer ID to receive the balance
    /// </summary>
    [Required(ErrorMessage = "Target customer ID is required")]
    public Guid TargetCustomerId { get; set; }

    /// <summary>
    /// Gets or sets the amount to transfer
    /// </summary>
    [Required(ErrorMessage = "Amount is required")]
    [Range(1, int.MaxValue, ErrorMessage = "Amount must be greater than zero")]
    public int Amount { get; set; }

    /// <summary>
    /// Gets or sets the optional description for the transfer
    /// </summary>
    [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
    public string Description { get; set; } = string.Empty;
}
