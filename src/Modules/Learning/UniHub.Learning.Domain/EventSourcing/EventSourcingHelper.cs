using System.Text.Json;
using UniHub.SharedKernel.Domain;

namespace UniHub.Learning.Domain.EventSourcing;

/// <summary>
/// Helper class for event sourcing operations
/// </summary>
public static class EventSourcingHelper
{
    /// <summary>
    /// Serialize domain event to JSON
    /// </summary>
    public static string SerializeEvent<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent
    {
        return JsonSerializer.Serialize(domainEvent, new JsonSerializerOptions
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>
    /// Deserialize JSON to domain event
    /// </summary>
    public static IDomainEvent? DeserializeEvent(string eventData, string eventType)
    {
        var type = Type.GetType(eventType);
        if (type == null)
        {
            throw new InvalidOperationException($"Event type not found: {eventType}");
        }

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Deserialize(eventData, type, options) as IDomainEvent;
    }

    /// <summary>
    /// Get assembly qualified name for event type
    /// </summary>
    public static string GetEventTypeName<TEvent>() where TEvent : IDomainEvent
    {
        return typeof(TEvent).AssemblyQualifiedName ?? typeof(TEvent).FullName ?? typeof(TEvent).Name;
    }

    /// <summary>
    /// Get assembly qualified name for event type from instance
    /// </summary>
    public static string GetEventTypeName(IDomainEvent domainEvent)
    {
        var type = domainEvent.GetType();
        return type.AssemblyQualifiedName ?? type.FullName ?? type.Name;
    }
}
