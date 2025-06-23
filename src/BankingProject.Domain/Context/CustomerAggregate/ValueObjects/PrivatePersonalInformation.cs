using BankingProject.Domain.Abstractions;

namespace BankingProject.Domain.Context.CustomerAggregate.ValueObjects;

public class PrivatePersonalInformation : IValueObject
{
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string MerchantDocument { get; set; }
    public Password Password { get; set; }


    public PrivatePersonalInformation()
    {
    }

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