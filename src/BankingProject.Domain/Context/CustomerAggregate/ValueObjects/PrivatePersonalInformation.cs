using BankingProject.Domain.Abstractions;

namespace BankingProject.Domain.Context.CustomerAggregate.ValueObjects;

public class PrivatePersonalInformation : IValueObject
{
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string MerchantDocument { get; set; }
    public Password Password { get; set; }

    /// <summary>
    /// Parameterless constructor required for serialization
    /// </summary>
    public PrivatePersonalInformation()
    {
        Email = string.Empty;
        PhoneNumber = string.Empty;
        MerchantDocument = string.Empty;
        Password = new Password("salt", "hash", "pepper", 50);
    }

    /// <summary>
    /// Creates a new instance of PrivatePersonalInformation
    /// </summary>
    /// <param name="email">The customer's email address</param>
    /// <param name="phoneNumber">The customer's phone number</param>
    /// <param name="merchantDocument">The customer's merchant document</param>
    /// <param name="password">The customer's password</param>
    public PrivatePersonalInformation(string email, string phoneNumber, string merchantDocument, string password)
    {
        Email = email;
        PhoneNumber = phoneNumber;
        MerchantDocument = merchantDocument;
        Password = new Password(
            Password.GenerateSalt(),
            Password.ComputeHash(Password.GenerateSalt(), password, "pepper", 50),
            "pepper",
            50
        );
    }
}