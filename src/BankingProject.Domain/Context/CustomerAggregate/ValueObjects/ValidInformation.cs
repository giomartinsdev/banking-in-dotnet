using BankingProject.Domain.Abstractions;

namespace BankingProject.Domain.Context.CustomerAggregate.ValueObjects;

public class ValidInformation : IValueObject
{
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsValid { get; set; }

    public ValidInformation(bool isValid = true)
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
        IsValid = isValid;
    }

    public void UpdateValidity(bool isValid)
    {
        IsValid = isValid;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update()
    {
        UpdatedAt = DateTime.UtcNow;
    }
}
