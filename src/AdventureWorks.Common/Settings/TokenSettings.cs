namespace AdventureWorks.Common.Settings;

public sealed class TokenSettings
{
    public const string SettingsRootName = "TokenSettings";

    public int TokenExpirationInSeconds { get; set; }

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public string Subject { get; set; } = string.Empty;

    public string Key { get; set; } = string.Empty;
}