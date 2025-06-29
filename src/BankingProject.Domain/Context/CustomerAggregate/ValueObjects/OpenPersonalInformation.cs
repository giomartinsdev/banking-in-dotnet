using BankingProject.Domain.Abstractions;

namespace BankingProject.Domain.Context.CustomerAggregate.ValueObjects;

public class OpenPersonalInformation : IValueObject
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public OpenPersonalInformation(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }
}
