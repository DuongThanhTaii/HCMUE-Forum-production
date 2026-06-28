using UniHub.SharedKernel.Domain;
using UniHub.SharedKernel.Results;

namespace UniHub.Career.Domain.JobPostings;

/// <summary>
/// Value object representing the salary range for a job posting.
/// </summary>
public sealed class SalaryRange : ValueObject
{
    /// <summary>Minimum salary amount.</summary>
    public decimal MinAmount { get; private set; }

    /// <summary>Maximum salary amount.</summary>
    public decimal MaxAmount { get; private set; }

    /// <summary>Currency code (e.g., "VND", "USD").</summary>
    public string Currency { get; private set; }

    /// <summary>Payment period (e.g., "month", "year", "hour").</summary>
    public string Period { get; private set; }

    private static readonly HashSet<string> SupportedCurrencies = new(StringComparer.OrdinalIgnoreCase)
    {
        "VND", "USD", "EUR", "GBP", "JPY", "SGD", "AUD"
    };

    private static readonly HashSet<string> SupportedPeriods = new(StringComparer.OrdinalIgnoreCase)
    {
        "hour", "day", "week", "month", "year"
    };

    /// <summary>Private constructor for EF Core.</summary>
    private SalaryRange() 
    { 
        Currency = string.Empty;
        Period = string.Empty;
    }

    private SalaryRange(decimal minAmount, decimal maxAmount, string currency, string period)
    {
        MinAmount = minAmount;
        MaxAmount = maxAmount;
        Currency = currency;
        Period = period;
    }

    /// <summary>
    /// Creates a new SalaryRange value object.
    /// </summary>
    public static Result<SalaryRange> Create(
        decimal minAmount,
        decimal maxAmount,
        string currency,
        string period)
    {
        if (minAmount < 0)
            return Result.Failure<SalaryRange>(
                new Error("SalaryRange.NegativeMin", "Minimum salary cannot be negative."));

        if (maxAmount < 0)
            return Result.Failure<SalaryRange>(
                new Error("SalaryRange.NegativeMax", "Maximum salary cannot be negative."));

        if (minAmount > maxAmount)
            return Result.Failure<SalaryRange>(
                new Error("SalaryRange.InvalidRange", "Minimum salary cannot exceed maximum salary."));

        if (string.IsNullOrWhiteSpace(currency))
            return Result.Failure<SalaryRange>(
                new Error("SalaryRange.EmptyCurrency", "Currency is required."));

        if (!SupportedCurrencies.Contains(currency))
            return Result.Failure<SalaryRange>(
                new Error("SalaryRange.UnsupportedCurrency", $"Currency '{currency}' is not supported. Supported: {string.Join(", ", SupportedCurrencies)}"));

        if (string.IsNullOrWhiteSpace(period))
            return Result.Failure<SalaryRange>(
                new Error("SalaryRange.EmptyPeriod", "Payment period is required."));

        if (!SupportedPeriods.Contains(period))
            return Result.Failure<SalaryRange>(
                new Error("SalaryRange.UnsupportedPeriod", $"Period '{period}' is not supported. Supported: {string.Join(", ", SupportedPeriods)}"));

        return Result.Success(new SalaryRange(minAmount, maxAmount, currency.ToUpperInvariant(), period.ToLowerInvariant()));
    }

    /// <summary>
    /// Returns formatted string like "10,000,000 - 15,000,000 VND/month".
    /// </summary>
    public override string ToString()
        => $"{MinAmount:N0} - {MaxAmount:N0} {Currency}/{Period}";

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return MinAmount;
        yield return MaxAmount;
        yield return Currency;
        yield return Period;
    }
}
