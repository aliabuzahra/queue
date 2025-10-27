using VirtualQueue.Domain.Common;
using VirtualQueue.Domain.Events;

namespace VirtualQueue.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Domain { get; private set; } = string.Empty;
    public string ApiKey { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    
    private readonly List<Queue> _queues = new();
    public IReadOnlyCollection<Queue> Queues => _queues.AsReadOnly();

    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Tenant() { } // EF Core constructor

    public Tenant(string name, string domain)
    {
        Name = name;
        Domain = domain;
        ApiKey = GenerateApiKey();
        
        _domainEvents.Add(new TenantCreatedEvent(Id, Name, Domain));
    }

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be null or empty", nameof(name));

        Name = name;
        MarkAsUpdated();
    }

    public void UpdateDomain(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            throw new ArgumentException("Domain cannot be null or empty", nameof(domain));

        Domain = domain;
        MarkAsUpdated();
    }

    public void Deactivate()
    {
        IsActive = false;
        MarkAsUpdated();
        
        _domainEvents.Add(new TenantDeactivatedEvent(Id));
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
        
        _domainEvents.Add(new TenantActivatedEvent(Id));
    }

    public void AddQueue(Queue queue)
    {
        if (queue == null)
            throw new ArgumentNullException(nameof(queue));

        _queues.Add(queue);
        MarkAsUpdated();
    }

    public void RemoveQueue(Guid queueId)
    {
        var queue = _queues.FirstOrDefault(q => q.Id == queueId);
        if (queue != null)
        {
            _queues.Remove(queue);
            MarkAsUpdated();
        }
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    private static string GenerateApiKey()
    {
        return Guid.NewGuid().ToString("N");
    }
}
