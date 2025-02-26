using System;

public static class EnumExtensions
{
    public static T ToEnum<T>(this string value) where T : struct
    {
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("Value cannot be null or empty.", nameof(value));
        }

        return (T)Enum.Parse(typeof(T), value, true);
    }
}