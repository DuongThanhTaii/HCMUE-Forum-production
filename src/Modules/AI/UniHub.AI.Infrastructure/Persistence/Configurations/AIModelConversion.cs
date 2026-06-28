using System.Text.Json;

namespace UniHub.AI.Infrastructure.Persistence.Configurations;

internal static class AIModelConversion
{
    public static string ToStringListDb(List<string> values)
        => JsonSerializer.Serialize(values);

    public static List<string> ToStringListDomain(string raw)
        => JsonSerializer.Deserialize<List<string>>(raw) ?? new List<string>();
}
