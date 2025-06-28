namespace BankingProject.Application.DTOs.BalanceOperationDTOs;

/// <summary>
/// Data transfer object for balance transfer response
/// </summary>
public sealed class TransferBalanceResponse
{
    /// <summary>
    /// Gets or sets the success message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the transferred amount
    /// </summary>
    public int Amount { get; set; }

    /// <summary>
    /// Gets or sets the sender customer ID
    /// </summary>
    public Guid FromCustomerId { get; set; }

    /// <summary>
    /// Gets or sets the recipient customer ID
    /// </summary>
    public Guid ToCustomerId { get; set; }

    /// <summary>
    /// Gets or sets the transaction timestamp
    /// </summary>
    public DateTime TransactionDate { get; set; }

    /// <summary>
    /// Gets or sets the transaction description
    /// </summary>
    public string Description { get; set; } = string.Empty;
}
