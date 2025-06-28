using System.ComponentModel.DataAnnotations;

namespace BankingProject.Application.DTOs.CustomerDTOs;

/// <summary>
/// Data transfer object for updating customer fields
/// </summary>
public sealed class UpdateCustomerRequest
{
    /// <summary>
    /// Gets or sets the customer's first name
    /// </summary>
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the customer's last name
    /// </summary>
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the customer's email address
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the customer's phone number
    /// </summary>
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string? PhoneNumber { get; set; }
}
