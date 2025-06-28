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
    [StringLength(50, MinimumLength = 1, ErrorMessage = "First name must be between 1 and 50 characters")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Gets or sets the customer's last name
    /// </summary>
    [StringLength(50, MinimumLength = 1, ErrorMessage = "Last name must be between 1 and 50 characters")]
    public string? LastName { get; set; }

    /// <summary>
    /// Gets or sets the customer's email address
    /// </summary>
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "Email must be between 5 and 100 characters")]
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the customer's phone number
    /// </summary>
    [Phone(ErrorMessage = "Invalid phone number format")]
    [StringLength(20, MinimumLength = 10, ErrorMessage = "Phone number must be between 10 and 20 characters")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Validates that at least one field is provided for update
    /// </summary>
    /// <returns>True if at least one field has a value</returns>
    public bool HasAnyValue()
    {
        return !string.IsNullOrWhiteSpace(FirstName) ||
               !string.IsNullOrWhiteSpace(LastName) ||
               !string.IsNullOrWhiteSpace(Email) ||
               !string.IsNullOrWhiteSpace(PhoneNumber);
    }
}
