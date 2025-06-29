namespace BankingProject.Application.DTOs.CustomerDTOs;

/// <summary>
/// Data transfer object for customer response
/// </summary>
public sealed class CustomerResponse
{
    /// <summary>
    /// Gets or sets the customer's unique identifier
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the customer's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's phone number
    /// </summary>
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's current balance
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Gets or sets the customer's creation date
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets whether the customer is active
    /// </summary>
    public bool IsActive { get; set; }
}
