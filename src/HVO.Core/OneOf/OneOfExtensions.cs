using System;

namespace HVO.Core.OneOf;

/// <summary>
/// Extension methods for working with IOneOf discriminated unions
/// </summary>
public static class OneOfExtensions
{
    /// <summary>
    /// Checks if the OneOf contains a value of type T.
    /// </summary>
    /// <typeparam name="T">The type to check for</typeparam>
    /// <param name="oneOf">The OneOf instance to check</param>
    /// <returns>True if the contained value is of type T; otherwise false</returns>
    public static bool Is<T>(this IOneOf oneOf)
    {
        if (oneOf == null) throw new ArgumentNullException(nameof(oneOf));
        return oneOf.Value is T;
    }

    /// <summary>
    /// Attempts to get the value of type T from the OneOf.
    /// </summary>
    /// <typeparam name="T">The type to extract</typeparam>
    /// <param name="oneOf">The OneOf instance to extract from</param>
    /// <param name="value">When this method returns, contains the value if the type matched; otherwise the default value</param>
    /// <returns>True if the value was successfully extracted as type T; otherwise false</returns>
    public static bool TryGet<T>(this IOneOf oneOf, out T value)
    {
        if (oneOf == null) throw new ArgumentNullException(nameof(oneOf));
        if (oneOf.Value is T typed)
        {
            value = typed;
            return true;
        }
        value = default!;
        return false;
    }

    /// <summary>
    /// Gets the value as T or throws an InvalidOperationException if the value is not of type T.
    /// </summary>
    /// <typeparam name="T">The type to cast to</typeparam>
    /// <param name="oneOf">The OneOf instance to cast from</param>
    /// <returns>The contained value cast to type T</returns>
    /// <exception cref="InvalidOperationException">Thrown when the value is null or not of type T</exception>
    public static T As<T>(this IOneOf oneOf)
    {
        if (oneOf == null) throw new ArgumentNullException(nameof(oneOf));
        if (oneOf.Value is null)
            throw new InvalidOperationException("Cannot cast null value to type " + typeof(T).Name);

        if (oneOf.Value is not T)
            throw new InvalidOperationException($"Cannot cast value of type {oneOf.ValueType?.Name ?? "unknown"} to type {typeof(T).Name}");

        return (T)oneOf.Value;
    }
}
