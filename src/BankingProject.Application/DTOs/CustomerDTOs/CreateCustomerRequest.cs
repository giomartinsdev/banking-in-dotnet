using System.ComponentModel.DataAnnotations;

namespace BankingProject.Application.DTOs.CustomerDTOs;

/// <summary>
/// Data transfer object for creating a new customer
/// </summary>
public sealed class CreateCustomerRequest
{
    /// <summary>
    /// Gets or sets the customer's first name
    /// </summary>
    [Required(ErrorMessage = "First name is required")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's last name
    /// </summary>
    [Required(ErrorMessage = "Last name is required")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's email address
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's phone number
    /// </summary>
    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's merchant document
    /// </summary>
    [Required(ErrorMessage = "Merchant document is required")]
    [StringLength(20, ErrorMessage = "Merchant document cannot exceed 20 characters")]
    public string MerchantDocument { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the customer's password
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, ErrorMessage = "Password must be at least 6 characters long", MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

}
