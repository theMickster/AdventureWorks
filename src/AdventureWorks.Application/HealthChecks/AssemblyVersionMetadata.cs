using System.Runtime.InteropServices;

namespace AdventureWorks.Application.HealthChecks;

/// <summary>
/// Model: Assembly Version Metadata
/// </summary>
public sealed class AssemblyVersionMetadata
{
    /// <summary>
    /// Gets or sets copyright
    /// </summary>
    public string Copyright { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets company
    /// </summary>
    public string Company { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets description
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// ServiceId (official GTP)
    /// </summary>
    public string ServiceId { get; set; } = string.Empty;

    /// <summary>
    /// Environment Name
    /// </summary>
    public string EnvironmentName { get; set; } = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? string.Empty;

    /// <summary>
    /// Gets or sets semantic Version <para>See: semver.org</para>
    /// </summary>
    public string SemanticVersion { get; set; } = string.Empty;

    private string _informationVersion = string.Empty;

    /// <summary>
    /// Gets or sets EY Program Version <c>PI.Sprint.Release.Patch</c>
    /// <para>This is set by the CD build</para>
    /// <para>It falls back to file version</para>
    /// </summary>
    public string InformationVersion
    {
        get
        {
            if (string.IsNullOrWhiteSpace(_informationVersion))
            {
                _informationVersion = FileVersion;
            }
            return _informationVersion;
        }
        set
        {
            _informationVersion = value;
        }
    }

    /// <summary>
    /// FileVersion (fallback) for assembly <c>InformationVersion</c> version
    /// </summary>
    public string FileVersion { get; set; } = string.Empty;

    /// <summary>
    /// Gets major version as it occurs on the path e.g. the 1st ocet of the Semantic Version
    /// </summary>
    public string MajorVersion
    {
        get
        {
            var version = "1";
            if (!string.IsNullOrWhiteSpace(SemanticVersion))
            {
                var data = SemanticVersion.Split(new char[] { '.' });
                if (data.Length > 1)
                {
                    version = data[0];
                }
            }

            return $"v{version}";
        }
    }

    /// <summary>
    /// Gets or sets product
    /// </summary>
    public string Product { get; set; } = string.Empty;

    /// <summary>
    /// Property Set
    /// </summary>
    /// <param name="name">name</param>
    /// <param name="value">value</param>
    public void PropertySet(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return;
        }

        switch (name.ToUpperInvariant().Trim())
        {
            case "ASSEMBLYCOPYRIGHTATTRIBUTE":
                Copyright = value;
                break;
            case "ASSEMBLYCOMPANYATTRIBUTE":
                Company = value;
                break;
            case "ASSEMBLYDESCRIPTIONATTRIBUTE":
                Description = value;
                break;
            case "ASSEMBLYVERSIONATTRIBUTE":
                InformationVersion = value;
                break;
            case "ASSEMBLYINFORMATIONALVERSIONATTRIBUTE":
                SemanticVersion = value;
                break;
            case "ASSEMBLYPRODUCTATTRIBUTE":
                Product = value;
                break;
            case "ASSEMBLYFILEVERSIONATTRIBUTE":
                FileVersion = value;
                break;
            case "ENVIRONMENT":
                EnvironmentName = value;
                break;
            case "SERVICEID":
                ServiceId = value;
                break;
        }
    }

    /// <summary>
    /// Framework
    /// </summary>
    public string Framework { get; set; } = RuntimeInformation.FrameworkDescription;

    /// <summary>
    /// OS Description
    /// </summary>
    public string OSDescription { get; set; } = RuntimeInformation.OSDescription;

    /// <summary>
    /// Runtime Identifier
    /// </summary>
    public string RuntimeIdentifier { get; set; } = RuntimeInformation.RuntimeIdentifier;

    /// <summary>
    /// Process Architecture
    /// </summary>
    public string ProcessArchitecture { get; set; } = RuntimeInformation.ProcessArchitecture.ToString();

    /// <summary>
    /// Formatted String (Changing this output is a breaking change!)
    /// </summary>
    /// <returns>formatted string with the product name, copyright, version, and description.</returns>
    public override string ToString() => $"{Product} {Copyright}. Semantic: {SemanticVersion}, Assembly: {InformationVersion}\n{Description}";

    /// <summary>
    /// As JSON
    /// </summary>
    /// <returns></returns>
    public string AsJson() => Newtonsoft.Json.JsonConvert.SerializeObject(this);
}
