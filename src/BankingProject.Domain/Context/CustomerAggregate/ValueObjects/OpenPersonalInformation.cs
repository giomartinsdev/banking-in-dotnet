using BankingProject.Domain.Abstractions;

namespace BankingProject.Domain.Context.CustomerAggregate.ValueObjects;

/// <summary>
/// Represents open personal information that can be publicly accessed
/// </summary>
public class OpenPersonalInformation : IValueObject
{
    /// <summary>
    /// Gets or sets the customer's first name
    /// </summary>
    public string FirstName { get; set; }
    
    /// <summary>
    /// Gets or sets the customer's last name
    /// </summary>
    public string LastName { get; set; }
    
    /// <summary>
    /// Creates a new instance of OpenPersonalInformation
    /// </summary>
    /// <param name="firstName">The customer's first name</param>
    /// <param name="lastName">The customer's last name</param>
    public OpenPersonalInformation(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }
}