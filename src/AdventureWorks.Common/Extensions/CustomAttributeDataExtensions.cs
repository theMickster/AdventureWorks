using System.Reflection;
#pragma warning disable CS8625

namespace AdventureWorks.Common.Extensions;
public static class CustomAttributeDataExtensions
{
    /// <summary>
    /// Try Parse a <c>System.Reflection.CustomAttributeData</c> into a string
    /// </summary>
    /// <param name="attribute">(this)</param>
    /// <param name="output">the string to parse attribute data into</param>
    /// <returns>True if success</returns>
    public static bool TryParse(this CustomAttributeData attribute, out string output)
    {
        output = null;

        if (attribute is not { ConstructorArguments: { } } ||
            attribute.ConstructorArguments.All(a => a.ArgumentType != typeof(string)))
        {
            return false;
        }

        output = attribute.ConstructorArguments.First(a => a.ArgumentType == typeof(string)).Value as string ?? string.Empty;

        return true;
    }
}