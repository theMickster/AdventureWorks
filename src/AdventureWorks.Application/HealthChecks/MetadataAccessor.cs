using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using AdventureWorks.Common.Extensions;

namespace AdventureWorks.Application.HealthChecks;

/// <summary>
/// Static class that provides access to the current running processes meta data.
/// </summary>
public static class MetadataAccessor
{
    private static AssemblyVersionMetadata _assemblyVersionMetadata;

    /// <summary>
    /// Gets semantic Version, etc from Assembly Metadata
    /// <para>Using <c>GetEntryAssembly</c></para>
    /// </summary>
    [ExcludeFromCodeCoverage]
    public static AssemblyVersionMetadata ProgramMetadata
    {
        get
        {
            if (_assemblyVersionMetadata != null)
            {
                return _assemblyVersionMetadata;
            }

            var assembly = Assembly.GetEntryAssembly();

            return MetadataFromAssembly(assembly);

        }
    }

    /// <summary>
    /// Parse Metadata from Assembly
    /// </summary>
    /// <param name="assembly">Assembly</param>
    /// <returns>AssemblyVersionMetadata</returns>
    public static AssemblyVersionMetadata MetadataFromAssembly(Assembly assembly)
    {
        _assemblyVersionMetadata = new AssemblyVersionMetadata();

        if (assembly != null)
        {
            foreach (var attribute in assembly.GetCustomAttributesData())
            {
                if (!attribute.TryParse(out var value))
                {
                    value = string.Empty;
                }

                var name = attribute.AttributeType.Name;

                System.Diagnostics.Trace.WriteLine($"{name}, {value}");
                _assemblyVersionMetadata.PropertySet(name, value);
            }
        }

        return _assemblyVersionMetadata;
    }

    /// <summary>
    /// Dictionary of Public Properties
    /// </summary>
    /// <param name="atype"></param>
    /// <returns></returns>
    public static Dictionary<string, object> DictionaryFromType(object atype)
    {
        var dict = new Dictionary<string, object>();

        if (atype != null)
        {
            var t = atype.GetType();

            PropertyInfo[] props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (PropertyInfo prp in props)
            {
                object value = prp.GetValue(atype, Array.Empty<object>());

                dict.Add(prp.Name, value);
            }
        }

        return dict;
    }

}