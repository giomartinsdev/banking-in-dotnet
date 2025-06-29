namespace BankingProject.Domain.Abstractions;

public abstract class DomainEvent
{
    public string EventType { get; init; }
    public DateTime CreationTimestamp { get; init; }
        
    protected internal DomainEvent()
    {
        string subTypeImplementationClassName = GetType().Name;
            
        EventType = subTypeImplementationClassName;
        CreationTimestamp = DateTime.Now;
    }
}
