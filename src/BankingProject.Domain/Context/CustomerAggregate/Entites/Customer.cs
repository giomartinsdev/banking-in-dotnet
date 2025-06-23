using BankingProject.Domain.Abstractions;
using BankingProject.Domain.Context.CustomerAggregate.ValueObjects;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BankingProject.Domain.Context.CustomerAggregate.Entites;

public class Customer : IAggregateRoot
{
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; private init; }
    public OpenPersonalInformation OpenPersonalInformation { get; set; }
    
    public PrivatePersonalInformation PrivatePersonalInformation { get; set; }

    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; set; } 
    
    public Customer(OpenPersonalInformation openPersonalInformation, PrivatePersonalInformation privatePersonalInformation)
    {
        OpenPersonalInformation = openPersonalInformation;
        PrivatePersonalInformation = privatePersonalInformation;
        Id = Guid.CreateVersion7();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

}
