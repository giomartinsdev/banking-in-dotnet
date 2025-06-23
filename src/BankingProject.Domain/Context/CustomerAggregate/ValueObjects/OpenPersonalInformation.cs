namespace BankingProject.Domain.Context.CustomerAggregate.ValueObjects;

public class OpenPersonalInformation
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public OpenPersonalInformation(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }
}