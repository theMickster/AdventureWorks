namespace AdventureWorks.Common.Settings;

/// <summary>
/// A fun example of using AKV to load a configuration class via
///   the IOptions design pattern.
/// </summary>
public sealed class AkvExampleSettings
{
    public string MyFavoriteComedicMovie { get; set; } = default!;
}