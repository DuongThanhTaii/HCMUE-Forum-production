namespace UniHub.API.Integrations;

public sealed class UEBotIntegrationOptions
{
    public const string SectionName = "Integrations:UEBot";

    public string BaseUrl { get; set; } = "http://localhost:4010";

    public string ExchangePath { get; set; } = "/integrations/forum/exchange";

    public string SharedSecret { get; set; } = string.Empty;

    public string SyncApiBaseUrl { get; set; } = "http://localhost:4010";
}
