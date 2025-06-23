using BankingProject.Domain.Abstractions;
using BankingProject.Domain.Context.CustomerAggregate.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

public class Customer : IAggregateRoot
{
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; private init; }
    public OpenPersonalInformation OpenPersonalInformation { get; set; }
    public PrivatePersonalInformation PrivatePersonalInformation { get; set; }
    public List<BalanceOperation> BalanceOperations { get; set; }
    public ValidInformation ValidInformation { get; set; }
    public Customer
    (
        OpenPersonalInformation openPersonalInformation,
        PrivatePersonalInformation privatePersonalInformation,
        ValidInformation validInformation,
        List<BalanceOperation>? balanceOperations = null
    )
    {
        OpenPersonalInformation = openPersonalInformation;
        PrivatePersonalInformation = privatePersonalInformation;
        BalanceOperations = balanceOperations ?? new List<BalanceOperation>();
        ValidInformation = validInformation;
        Id = Guid.CreateVersion7();
    }
}