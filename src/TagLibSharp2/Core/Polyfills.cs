// Copyright (c) 2025 Stephen Shaw and contributors
// Polyfills for netstandard2.0/2.1 compatibility

using System.Text;

namespace TagLibSharp2.Core;

/// <summary>
/// Internal polyfills for older framework compatibility.
/// </summary>
internal static class Polyfills
{
#if NETSTANDARD2_0 || NETSTANDARD2_1
	/// <summary>
	/// Polyfill for ArgumentNullException.ThrowIfNull (available in .NET 6+).
	/// </summary>
	public static void ThrowIfNull (object? argument, string? paramName = null)
	{
		if (argument is null)
			throw new ArgumentNullException (paramName);
	}
#endif

#if NETSTANDARD2_0
	/// <summary>
	/// Polyfill for Array.Fill (available in .NET Core 2.0+ / netstandard2.1).
	/// </summary>
	public static void ArrayFill<T> (T[] array, T value, int startIndex, int count)
	{
		for (var i = startIndex; i < startIndex + count; i++)
			array[i] = value;
	}

	/// <summary>
	/// Polyfill for Array.Fill (available in .NET Core 2.0+ / netstandard2.1).
	/// </summary>
	public static void ArrayFill<T> (T[] array, T value)
	{
		for (var i = 0; i < array.Length; i++)
			array[i] = value;
	}
#endif

#if NETSTANDARD2_0 || NETSTANDARD2_1
	static readonly Encoding _latin1 = Encoding.GetEncoding (28591);
#endif

	/// <summary>
	/// Gets Latin1 (ISO-8859-1) encoding, available as Encoding.Latin1 in .NET 5+.
	/// </summary>
	public static Encoding Latin1 =>
#if NETSTANDARD2_0 || NETSTANDARD2_1
		_latin1;
#else
		Encoding.Latin1;
#endif

#if NETSTANDARD2_0 || NETSTANDARD2_1
	/// <summary>
	/// Polyfill for Convert.ToHexString (available in .NET 5+).
	/// </summary>
	public static string ToHexString (byte[] data)
	{
		if (data.Length == 0)
			return string.Empty;

		var chars = new char[data.Length * 2];
		const string hex = "0123456789ABCDEF";
		for (var i = 0; i < data.Length; i++) {
			chars[i * 2] = hex[data[i] >> 4];
			chars[i * 2 + 1] = hex[data[i] & 0xF];
		}
		return new string (chars);
	}

	/// <summary>
	/// Polyfill for Convert.ToHexStringLower (available in .NET 9+).
	/// </summary>
	public static string ToHexStringLower (byte[] data)
	{
		if (data.Length == 0)
			return string.Empty;

		var chars = new char[data.Length * 2];
		const string hex = "0123456789abcdef";
		for (var i = 0; i < data.Length; i++) {
			chars[i * 2] = hex[data[i] >> 4];
			chars[i * 2 + 1] = hex[data[i] & 0xF];
		}
		return new string (chars);
	}

	/// <summary>
	/// Polyfill for Convert.FromHexString (available in .NET 5+).
	/// </summary>
	public static byte[] FromHexString (string hex)
	{
		if (hex.Length == 0)
			return [];

		if (hex.Length % 2 != 0)
			throw new FormatException ("Hex string must have even length");

		var data = new byte[hex.Length / 2];
		for (var i = 0; i < data.Length; i++) {
			data[i] = (byte)((GetHexValue (hex[i * 2]) << 4) | GetHexValue (hex[i * 2 + 1]));
		}
		return data;
	}

	static int GetHexValue (char c) => c switch {
		>= '0' and <= '9' => c - '0',
		>= 'a' and <= 'f' => c - 'a' + 10,
		>= 'A' and <= 'F' => c - 'A' + 10,
		_ => throw new FormatException ($"Invalid hex character: {c}")
	};
#endif

#if NETSTANDARD2_0
	/// <summary>
	/// Polyfill for string.Replace with StringComparison (available in .NET Standard 2.1+).
	/// </summary>
	/// <remarks>
	/// <para>
	/// This polyfill only supports <see cref="StringComparison.Ordinal"/>. All other comparison
	/// types will throw <see cref="NotSupportedException"/>. This limitation exists because
	/// implementing culture-aware string replacement correctly on .NET Standard 2.0 requires
	/// complex logic that is not needed for the library's internal use cases.
	/// </para>
	/// <para>
	/// If you need culture-aware replacement on .NET Standard 2.0, use the native
	/// <see cref="string.Replace(string, string)"/> method or upgrade to .NET Standard 2.1+.
	/// </para>
	/// </remarks>
	/// <param name="str">The string to search within.</param>
	/// <param name="oldValue">The string to replace.</param>
	/// <param name="newValue">The replacement string.</param>
	/// <param name="comparison">The comparison type. Only <see cref="StringComparison.Ordinal"/> is supported.</param>
	/// <returns>A new string with all occurrences replaced.</returns>
	/// <exception cref="NotSupportedException">Thrown when <paramref name="comparison"/> is not <see cref="StringComparison.Ordinal"/>.</exception>
	public static string Replace (string str, string oldValue, string newValue, StringComparison comparison)
	{
		if (comparison != StringComparison.Ordinal)
			throw new NotSupportedException ($"StringComparison.{comparison} is not supported in this polyfill. Only StringComparison.Ordinal is supported.");

		return str.Replace (oldValue, newValue);
	}

	/// <summary>
	/// Gets string from span using the specified encoding.
	/// </summary>
	public static string GetString (this Encoding encoding, ReadOnlySpan<byte> bytes) =>
		encoding.GetString (bytes.ToArray ());
#endif
}
