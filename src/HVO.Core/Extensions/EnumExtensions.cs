using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

namespace HVO.Core.Extensions;

/// <summary>
/// Extension methods for Enum types
/// </summary>
public static class EnumExtensions
{
    private static readonly ConcurrentDictionary<Enum, string> DescriptionCache
        = new ConcurrentDictionary<Enum, string>();

    /// <summary>
    /// Gets the description from the Description attribute, or the enum value name if not present.
    /// Results are cached to avoid repeated reflection lookups.
    /// </summary>
    /// <param name="value">The enum value</param>
    /// <returns>The description or enum value name</returns>
    public static string GetDescription(this Enum value)
    {
        if (value == null)
            return string.Empty;

        return DescriptionCache.GetOrAdd(value, static v =>
        {
            var fieldInfo = v.GetType().GetField(v.ToString());
            if (fieldInfo == null)
                return v.ToString();

            var attributes = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 ? attributes[0].Description : v.ToString();
        });
    }

    /// <summary>
    /// Retrieves a specific attribute applied to an enum value.
    /// </summary>
    /// <typeparam name="TEnum">The enum type.</typeparam>
    /// <typeparam name="TAttribute">The attribute type to fetch.</typeparam>
    /// <param name="value">The enum value being inspected.</param>
    /// <returns>The attribute instance if found; otherwise <c>null</c>.</returns>
    public static TAttribute? GetEnumAttribute<TEnum, TAttribute>(this TEnum value)
        where TEnum : struct, Enum
        where TAttribute : Attribute
    {
        var members = typeof(TEnum).GetMember(value.ToString());
        if (members.Length == 0)
            return null;

        return members[0].GetCustomAttribute<TAttribute>();
    }
}
