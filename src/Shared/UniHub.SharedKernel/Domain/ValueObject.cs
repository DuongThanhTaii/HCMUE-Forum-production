namespace UniHub.SharedKernel.Domain;

/// <summary>
/// Base class for value objects in DDD.
/// A value object is defined by its attributes, not its identity.
/// Two value objects with the same attributes are considered equal.
/// Value objects are immutable.
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Gets the components that define equality for this value object.
    /// </summary>
    /// <returns>A collection of components used for equality comparison.</returns>
    protected abstract IEnumerable<object?> GetEqualityComponents();

    /// <summary>
    /// Determines whether the specified object is equal to the current value object.
    /// </summary>
    public override bool Equals(object? obj)
    {
        return obj is ValueObject other && ValuesAreEqual(other);
    }

    /// <summary>
    /// Determines whether the specified value object is equal to the current value object.
    /// </summary>
    public bool Equals(ValueObject? other)
    {
        return other is not null && ValuesAreEqual(other);
    }

    /// <summary>
    /// Determines whether the values of two value objects are equal.
    /// </summary>
    private bool ValuesAreEqual(ValueObject other)
    {
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    /// <summary>
    /// Returns the hash code for this value object.
    /// </summary>
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Where(component => component is not null)
            .Aggregate(default(int), (current, component) => HashCode.Combine(current, component));
    }

    /// <summary>
    /// Determines whether two value objects are equal.
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return Equals(left, right);
    }

    /// <summary>
    /// Determines whether two value objects are not equal.
    /// </summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !Equals(left, right);
    }
}
