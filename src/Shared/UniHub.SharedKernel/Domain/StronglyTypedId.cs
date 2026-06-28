namespace UniHub.SharedKernel.Domain;

/// <summary>
/// Base class for strongly-typed IDs.
/// Provides type safety for entity identifiers and prevents primitive obsession.
/// </summary>
/// <typeparam name="TId">The type of the strongly-typed ID.</typeparam>
public abstract record StronglyTypedId<TId>(TId Value)
    where TId : notnull
{
    /// <summary>
    /// Gets the underlying value of the ID.
    /// </summary>
    public TId Value { get; init; } = Value;

    /// <summary>
    /// Returns a string representation of the ID.
    /// </summary>
    public override string ToString() => Value.ToString() ?? string.Empty;
}

/// <summary>
/// Base class for GUID-based strongly-typed IDs.
/// Most commonly used type for entity identifiers.
/// </summary>
public abstract record GuidId : StronglyTypedId<Guid>
{
    /// <summary>
    /// Initializes a new instance with the specified GUID value.
    /// </summary>
    protected GuidId(Guid value) : base(value)
    {
    }

    /// <summary>
    /// Initializes a new instance with a new GUID.
    /// </summary>
    protected GuidId() : base(Guid.NewGuid())
    {
    }
}

/// <summary>
/// Base class for integer-based strongly-typed IDs.
/// Useful for legacy systems or when IDs are human-readable.
/// </summary>
public abstract record IntId : StronglyTypedId<int>
{
    /// <summary>
    /// Initializes a new instance with the specified integer value.
    /// </summary>
    protected IntId(int value) : base(value)
    {
    }
}

/// <summary>
/// Base class for string-based strongly-typed IDs.
/// Useful for external system integration or natural keys.
/// </summary>
public abstract record StringId : StronglyTypedId<string>
{
    /// <summary>
    /// Initializes a new instance with the specified string value.
    /// </summary>
    protected StringId(string value) : base(value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("String ID cannot be null or whitespace.", nameof(value));
        }
    }
}
