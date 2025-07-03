using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace AdventureWorks.Common.Extensions;

public static class EnumExtensions
{
    /// <summary>
    /// When you have applied the attribute Description
    /// to a specific enum, this will allow you to output
    /// that. It will default to just outputting the value
    /// if the attribute is not present.
    /// Caution-http://stackoverflow.com/a/1415187/1248536 < Caution-http://stackoverflow.com/a/1415187/1248536 > 
    /// See AssertType for an example of how how it is
    /// being used.
    /// 
    /// I would also warn if the plan is to use this in massive loops 
    /// Since it is using reflection, you should shy away from using this.
    /// Instead implement a specific extension for you enumType 
    /// Caution-http://stackoverflow.com/a/1415460/1248536 < Caution-http://stackoverflow.com/a/1415460/1248536 > 
    /// </summary>
    /// <param name="value"></param>
    /// <returns>Description from attribute</returns>
    public static string GetDescription(this Enum value)
    {
        var type = value.GetType();
        var name = Enum.GetName(type, value);

        if (name == null)
        {
            return value.ToString();
        }

        var field = type.GetField(name);

        if (field == null)
        {
            return value.ToString();
        }

        if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attr)
        {
            return attr.Description;
        }

        return value.ToString();
    }

    /// <summary>
    /// Gets the display name attribute for the specified enum (if one exists), otherwise the enum 
    /// is simply converted to a string.
    /// </summary>
    /// <param name="enum">The @enum.</param>
    /// <returns>System.String.</returns>
    public static string GetDisplayName(this Enum @enum)
    {
        if (@enum == null)
        {
            return string.Empty;
        }

        var enumType = @enum.GetType();

        var memberInfo = enumType.GetMember(@enum.ToString());

        if (memberInfo == null || memberInfo.Length <= 0)
        {
            return @enum.ToString();
        }

        var displayNameAttribute = memberInfo.FirstOrDefault()!.GetCustomAttribute<DisplayAttribute>();

        if (displayNameAttribute != null)
        {
            return displayNameAttribute.Name ?? string.Empty;
        }

        return @enum.ToString();
    }
}