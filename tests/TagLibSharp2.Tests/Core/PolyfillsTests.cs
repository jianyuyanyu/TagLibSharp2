// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;

using TagLibSharp2.Core;

namespace TagLibSharp2.Tests.Core;

[TestClass]
public class PolyfillsTests
{
	// Note: Polyfills are conditionally compiled based on target framework.
	// These tests verify the behavior regardless of which implementation is used.

	// Tests that exercise polyfills through BinaryData (works on all frameworks)
	[TestMethod]
	public void BinaryData_ToHexString_ReturnsLowercaseHex ()
	{
		var data = new BinaryData ([0xDE, 0xAD, 0xBE, 0xEF]);
		Assert.AreEqual ("deadbeef", data.ToHexString ());
	}

	[TestMethod]
	public void BinaryData_ToHexString_EmptyData_ReturnsEmptyString ()
	{
		var data = BinaryData.Empty;
		Assert.AreEqual (string.Empty, data.ToHexString ());
	}

	[TestMethod]
	public void BinaryData_ToHexStringUpper_ReturnsUppercaseHex ()
	{
		var data = new BinaryData ([0xDE, 0xAD, 0xBE, 0xEF]);
		Assert.AreEqual ("DEADBEEF", data.ToHexStringUpper ());
	}

	[TestMethod]
	public void BinaryData_ToHexStringUpper_EmptyData_ReturnsEmptyString ()
	{
		var data = BinaryData.Empty;
		Assert.AreEqual (string.Empty, data.ToHexStringUpper ());
	}

	[TestMethod]
	public void BinaryData_FromHexString_UppercaseHex_ReturnsCorrectData ()
	{
		var data = BinaryData.FromHexString ("DEADBEEF");
		CollectionAssert.AreEqual (new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, data.ToArray ());
	}

	[TestMethod]
	public void BinaryData_FromHexString_LowercaseHex_ReturnsCorrectData ()
	{
		var data = BinaryData.FromHexString ("deadbeef");
		CollectionAssert.AreEqual (new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, data.ToArray ());
	}

	[TestMethod]
	public void BinaryData_FromHexString_MixedCaseHex_ReturnsCorrectData ()
	{
		var data = BinaryData.FromHexString ("DeAdBeEf");
		CollectionAssert.AreEqual (new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, data.ToArray ());
	}

	[TestMethod]
	public void BinaryData_FromHexString_WithSpaces_StripsSpaces ()
	{
		var data = BinaryData.FromHexString ("DE AD BE EF");
		CollectionAssert.AreEqual (new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, data.ToArray ());
	}

	[TestMethod]
	public void BinaryData_FromHexString_With0xPrefix_ThrowsFormatException ()
	{
		// FromHexString does not handle 0x prefix - only strips spaces
		Assert.ThrowsExactly<FormatException> (() => BinaryData.FromHexString ("0xDEADBEEF"));
	}

	[TestMethod]
	public void BinaryData_FromHexString_EmptyString_ReturnsEmpty ()
	{
		var data = BinaryData.FromHexString (string.Empty);
		Assert.AreEqual (0, data.Length);
	}

	[TestMethod]
	public void BinaryData_ToStringLatin1_DecodesCorrectly ()
	{
		var data = new BinaryData ([0x48, 0x65, 0x6C, 0x6C, 0x6F]); // "Hello"
		Assert.AreEqual ("Hello", data.ToStringLatin1 ());
	}

	[TestMethod]
	public void BinaryData_ToStringLatin1_HandlesExtendedCharacters ()
	{
		// Test ISO-8859-1 characters beyond ASCII
		var data = new BinaryData ([0xE9, 0xE8, 0xE0, 0xFC]); // e with acute, grave, a with grave, u with umlaut
		var result = data.ToStringLatin1 ();
		Assert.AreEqual ("\xe9\xe8\xe0\xfc", result);
	}

	[TestMethod]
	public void BinaryData_FromStringLatin1_EncodesCorrectly ()
	{
		var data = BinaryData.FromStringLatin1 ("Hello");
		CollectionAssert.AreEqual (new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }, data.ToArray ());
	}

	[TestMethod]
	public void BinaryData_RoundTrip_HexString ()
	{
		var original = new BinaryData ([0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF]);
		var hex = original.ToHexString ();
		var restored = BinaryData.FromHexString (hex);
		CollectionAssert.AreEqual (original.ToArray (), restored.ToArray ());
	}

	[TestMethod]
	public void BinaryData_RoundTrip_Latin1 ()
	{
		var original = BinaryData.FromStringLatin1 ("Test\xe9\xf1"); // Test with extended chars
		var str = original.ToStringLatin1 ();
		var restored = BinaryData.FromStringLatin1 (str);
		CollectionAssert.AreEqual (original.ToArray (), restored.ToArray ());
	}

	// Test Latin1 encoding behavior via System.Text.Encoding
	[TestMethod]
	public void Latin1Encoding_EncodesCorrectly ()
	{
		var latin1 = Encoding.GetEncoding (28591);
		var bytes = latin1.GetBytes ("Hello");
		CollectionAssert.AreEqual (new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }, bytes);
	}

	[TestMethod]
	public void Latin1Encoding_DecodesCorrectly ()
	{
		var latin1 = Encoding.GetEncoding (28591);
		var str = latin1.GetString (new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F });
		Assert.AreEqual ("Hello", str);
	}

	[TestMethod]
	public void Latin1Encoding_HandlesExtendedCharacters ()
	{
		var latin1 = Encoding.GetEncoding (28591);
		var bytes = new byte[] { 0xE9, 0xE8, 0xE0, 0xFC };
		var str = latin1.GetString (bytes);
		Assert.AreEqual ("\xe9\xe8\xe0\xfc", str);
	}

#if NETSTANDARD2_0 || NETSTANDARD2_1
	// Direct polyfill tests - only available on older frameworks
	[TestMethod]
	public void Polyfills_ToHexString_EmptyArray_ReturnsEmptyString ()
	{
		Assert.AreEqual (string.Empty, Polyfills.ToHexString ([]));
	}

	[TestMethod]
	public void Polyfills_ToHexString_SingleByte_ReturnsUppercaseHex ()
	{
		Assert.AreEqual ("FF", Polyfills.ToHexString ([0xFF]));
		Assert.AreEqual ("00", Polyfills.ToHexString ([0x00]));
		Assert.AreEqual ("0A", Polyfills.ToHexString ([0x0A]));
		Assert.AreEqual ("AB", Polyfills.ToHexString ([0xAB]));
	}

	[TestMethod]
	public void Polyfills_ToHexString_MultipleBytes_ReturnsUppercaseHex ()
	{
		Assert.AreEqual ("DEADBEEF", Polyfills.ToHexString ([0xDE, 0xAD, 0xBE, 0xEF]));
		Assert.AreEqual ("0102030405", Polyfills.ToHexString ([0x01, 0x02, 0x03, 0x04, 0x05]));
	}

	[TestMethod]
	public void Polyfills_ToHexStringLower_EmptyArray_ReturnsEmptyString ()
	{
		Assert.AreEqual (string.Empty, Polyfills.ToHexStringLower ([]));
	}

	[TestMethod]
	public void Polyfills_ToHexStringLower_SingleByte_ReturnsLowercaseHex ()
	{
		Assert.AreEqual ("ff", Polyfills.ToHexStringLower ([0xFF]));
		Assert.AreEqual ("00", Polyfills.ToHexStringLower ([0x00]));
		Assert.AreEqual ("0a", Polyfills.ToHexStringLower ([0x0A]));
		Assert.AreEqual ("ab", Polyfills.ToHexStringLower ([0xAB]));
	}

	[TestMethod]
	public void Polyfills_ToHexStringLower_MultipleBytes_ReturnsLowercaseHex ()
	{
		Assert.AreEqual ("deadbeef", Polyfills.ToHexStringLower ([0xDE, 0xAD, 0xBE, 0xEF]));
	}

	[TestMethod]
	public void Polyfills_FromHexString_EmptyString_ReturnsEmptyArray ()
	{
		CollectionAssert.AreEqual (Array.Empty<byte> (), Polyfills.FromHexString (string.Empty));
	}

	[TestMethod]
	public void Polyfills_FromHexString_UppercaseHex_ReturnsBytes ()
	{
		CollectionAssert.AreEqual (new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, Polyfills.FromHexString ("DEADBEEF"));
	}

	[TestMethod]
	public void Polyfills_FromHexString_LowercaseHex_ReturnsBytes ()
	{
		CollectionAssert.AreEqual (new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, Polyfills.FromHexString ("deadbeef"));
	}

	[TestMethod]
	public void Polyfills_FromHexString_MixedCaseHex_ReturnsBytes ()
	{
		CollectionAssert.AreEqual (new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }, Polyfills.FromHexString ("DeAdBeEf"));
	}

	[TestMethod]
	public void Polyfills_FromHexString_SinglePair_ReturnsBytes ()
	{
		CollectionAssert.AreEqual (new byte[] { 0x00 }, Polyfills.FromHexString ("00"));
		CollectionAssert.AreEqual (new byte[] { 0xFF }, Polyfills.FromHexString ("FF"));
	}

	[TestMethod]
	public void Polyfills_FromHexString_OddLength_ThrowsFormatException ()
	{
		Assert.ThrowsExactly<FormatException> (() => Polyfills.FromHexString ("ABC")); // Odd length
	}

	[TestMethod]
	public void Polyfills_FromHexString_InvalidCharacter_ThrowsFormatException ()
	{
		Assert.ThrowsExactly<FormatException> (() => Polyfills.FromHexString ("GG")); // G is not a valid hex character
	}

	[TestMethod]
	public void Polyfills_ThrowIfNull_NotNull_DoesNotThrow ()
	{
		Polyfills.ThrowIfNull ("test", nameof (PolyfillsTests));
		// If we reach here, no exception was thrown
	}

	[TestMethod]
	public void Polyfills_ThrowIfNull_Null_ThrowsArgumentNullException ()
	{
		Assert.ThrowsExactly<ArgumentNullException> (() => Polyfills.ThrowIfNull (null, "paramName"));
	}

	[TestMethod]
	public void Polyfills_Latin1_ReturnsValidEncoding ()
	{
		var latin1 = Polyfills.Latin1;
		Assert.IsNotNull (latin1);
		Assert.AreEqual ("iso-8859-1", latin1.WebName.ToLowerInvariant ());
	}

	[TestMethod]
	public void Polyfills_Latin1_EncodesCorrectly ()
	{
		var latin1 = Polyfills.Latin1;
		var bytes = latin1.GetBytes ("Hello");
		CollectionAssert.AreEqual (new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F }, bytes);
	}

	[TestMethod]
	public void Polyfills_Latin1_DecodesCorrectly ()
	{
		var latin1 = Polyfills.Latin1;
		var str = latin1.GetString (new byte[] { 0x48, 0x65, 0x6C, 0x6C, 0x6F });
		Assert.AreEqual ("Hello", str);
	}
#endif

#if NETSTANDARD2_0
	[TestMethod]
	public void Polyfills_ArrayFill_FillsEntireArray ()
	{
		var array = new int[5];
		Polyfills.ArrayFill (array, 42);
		CollectionAssert.AreEqual (new[] { 42, 42, 42, 42, 42 }, array);
	}

	[TestMethod]
	public void Polyfills_ArrayFill_WithRange_FillsSpecifiedRange ()
	{
		var array = new int[] { 0, 0, 0, 0, 0 };
		Polyfills.ArrayFill (array, 42, 1, 3);
		CollectionAssert.AreEqual (new[] { 0, 42, 42, 42, 0 }, array);
	}

	[TestMethod]
	public void Polyfills_ArrayFill_WithRange_ZeroCount_DoesNothing ()
	{
		var array = new int[] { 1, 2, 3, 4, 5 };
		Polyfills.ArrayFill (array, 42, 2, 0);
		CollectionAssert.AreEqual (new[] { 1, 2, 3, 4, 5 }, array);
	}

	[TestMethod]
	public void Polyfills_Replace_OrdinalComparison_ReplacesSubstring ()
	{
		var result = Polyfills.Replace ("hello world", "world", "universe", StringComparison.Ordinal);
		Assert.AreEqual ("hello universe", result);
	}

	[TestMethod]
	public void Polyfills_Replace_OrdinalComparison_NoMatch_ReturnsOriginal ()
	{
		var result = Polyfills.Replace ("hello world", "xyz", "universe", StringComparison.Ordinal);
		Assert.AreEqual ("hello world", result);
	}

	[TestMethod]
	public void Polyfills_Replace_NonOrdinalComparison_ThrowsNotSupportedException ()
	{
		Assert.ThrowsExactly<NotSupportedException> (() =>
			Polyfills.Replace ("hello", "HELLO", "bye", StringComparison.OrdinalIgnoreCase));
	}

	[TestMethod]
	public void Polyfills_GetString_Extension_DecodesCorrectly ()
	{
		var latin1 = Polyfills.Latin1;
		ReadOnlySpan<byte> bytes = [0x48, 0x65, 0x6C, 0x6C, 0x6F];
		var str = latin1.GetString (bytes);
		Assert.AreEqual ("Hello", str);
	}
#endif
}
