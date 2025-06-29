using BankingProject.Domain.Abstractions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BankingProject.Domain.Context.CustomerAggregate.ValueObjects;

public class BalanceOperation : IValueObject
{
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; private init; }
    public int Amount { get; set; }
    public string Description { get; private init; }
    public ValidInformation ValidInformation { get; set; }

    public BalanceOperation
    (
        int amount, string description,
        ValidInformation validInformation)
    {
        Id = Guid.CreateVersion7();
        Amount = amount;
        Description = description;
        ValidInformation = validInformation;
    }

    public int NegativeBalanceOperationAmount()
    {
        return Amount * -1;
    }
}
