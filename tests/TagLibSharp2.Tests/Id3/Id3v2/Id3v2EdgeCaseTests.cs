// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Core;
using TagLibSharp2.Id3.Id3v2;
using TagLibSharp2.Id3.Id3v2.Frames;

namespace TagLibSharp2.Tests.Id3.Id3v2;

/// <summary>
/// Tests for ID3v2 edge cases and cross-tagger compatibility.
/// </summary>
/// <remarks>
/// These tests cover quirks from various taggers:
/// - iTunes: Uses syncsafe frame sizes in v2.3 tags
/// - Various taggers: Missing BOM in UTF-16 text frames
/// </remarks>
[TestClass]
public class Id3v2EdgeCaseTests
{
	#region Syncsafe Fallback Tests

	[TestMethod]
	public void Read_V23WithSyncsafeFrameSize_ParsesCorrectly ()
	{
		// iTunes sometimes writes v2.3 tags with syncsafe frame sizes (v2.4 style)
		// This test simulates a frame with a size that would be interpreted differently
		// depending on syncsafe vs big-endian interpretation

		// Frame content: encoding byte (1) + "Hello" (5) = 6 bytes
		// Syncsafe representation of 6: 0x00 0x00 0x00 0x06
		// Big-endian would also be: 0x00 0x00 0x00 0x06
		// So we need a size where MSB would be set for non-syncsafe

		// Let's use a size where syncsafe and big-endian differ:
		// Syncsafe 0x00 0x00 0x00 0x7F = 127 bytes
		// Big-endian 0x00 0x00 0x00 0x7F = 127 bytes (same)

		// Better test: use size 128
		// Syncsafe: 0x00 0x00 0x01 0x00 = 128 (each byte contributes 7 bits: (1<<7) + 0 = 128)
		// Big-endian: 0x00 0x00 0x01 0x00 = 256

		// Create a v2.3 tag with a frame that has syncsafe-encoded size
		var frameContent = CreateTextFrameContent ("A" + new string ('X', 126)); // 1 + 127 = 128 bytes
		var tagData = CreateV23TagWithSyncsafeFrameSize ("TIT2", frameContent);

		var result = Id3v2Tag.Read (tagData);

		Assert.IsTrue (result.IsSuccess, $"Failed to parse: {result.Error}");
		Assert.IsNotNull (result.Tag!.Title, "Title should not be null");
		Assert.IsTrue (result.Tag.Title.StartsWith ('A'), "Title should start with 'A'");
	}

	[TestMethod]
	public void Read_V23WithNormalFrameSize_ParsesCorrectly ()
	{
		// Standard v2.3 with big-endian frame size should still work
		var data = TestBuilders.Id3v2.CreateTagWithTextFrame (
			TestConstants.FrameIds.Title,
			"Standard Test",
			TestConstants.Id3v2.Version3);

		var result = Id3v2Tag.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("Standard Test", result.Tag!.Title);
	}

	[TestMethod]
	public void Read_V23FrameWithMsbSetInSize_DetectsSyncsafeFallback ()
	{
		// When a v2.3 frame has a size byte with MSB set (>= 0x80), it might be
		// from a buggy tagger using syncsafe. Test that we can detect and handle this.

		// Create frame where big-endian size would have MSB set but syncsafe doesn't
		// Size = 200 bytes: Big-endian = 0x00 0x00 0x00 0xC8 (MSB set in last byte)
		// Syncsafe 200 = 0x00 0x00 0x01 0x48

		var frameContent = CreateTextFrameContent (new string ('Y', 199)); // 1 + 199 = 200 bytes
		var tagData = CreateV23TagWithRawFrameSize ("TIT2", frameContent, 0x00, 0x00, 0x01, 0x48);

		var result = Id3v2Tag.Read (tagData);

		Assert.IsTrue (result.IsSuccess, $"Failed to parse: {result.Error}");
		Assert.IsNotNull (result.Tag?.Title);
	}

	#endregion

	#region UTF-16 BOM Fallback Tests

	[TestMethod]
	public void TextFrame_Utf16WithoutBom_FallsBackToLittleEndian ()
	{
		// Some taggers write UTF-16 encoded text without BOM
		// Should fall back to little-endian (Windows default)
		var data = CreateUtf16FrameWithoutBom ("Hello");

		var result = TextFrame.Read ("TIT2", data, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess, $"Failed to parse: {result.Error}");
		Assert.AreEqual ("Hello", result.Frame?.Text);
	}

	[TestMethod]
	public void TextFrame_Utf16WithLittleEndianBom_ParsesCorrectly ()
	{
		// Standard case: UTF-16 LE with BOM
		var text = "Hello";
		using var builder = new BinaryDataBuilder ();
		builder.Add ((byte)TextEncodingType.Utf16WithBom);
		builder.Add (new byte[] { 0xFF, 0xFE }); // LE BOM
		builder.Add (System.Text.Encoding.Unicode.GetBytes (text));

		var result = TextFrame.Read ("TIT2", builder.ToBinaryData ().Span, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("Hello", result.Frame?.Text);
	}

	[TestMethod]
	public void TextFrame_Utf16WithBigEndianBom_ParsesCorrectly ()
	{
		// Standard case: UTF-16 BE with BOM
		var text = "Hello";
		using var builder = new BinaryDataBuilder ();
		builder.Add ((byte)TextEncodingType.Utf16WithBom);
		builder.Add (new byte[] { 0xFE, 0xFF }); // BE BOM
		builder.Add (System.Text.Encoding.BigEndianUnicode.GetBytes (text));

		var result = TextFrame.Read ("TIT2", builder.ToBinaryData ().Span, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("Hello", result.Frame?.Text);
	}

	[TestMethod]
	public void LyricsFrame_Utf16WithoutBom_FallsBackToLittleEndian ()
	{
		// Some taggers write UTF-16 encoded lyrics without BOM
		var data = CreateUtf16LyricsFrameWithoutBom ("Hello Lyrics", "eng", "");

		var result = LyricsFrame.Read (data, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess, $"Failed to parse: {result.Error}");
		Assert.AreEqual ("Hello Lyrics", result.Frame?.Text);
	}

	[TestMethod]
	public void LyricsFrame_Utf16WithLittleEndianBom_ParsesCorrectly ()
	{
		// Standard case: UTF-16 LE with BOM
		using var builder = new BinaryDataBuilder ();
		builder.Add ((byte)TextEncodingType.Utf16WithBom);
		builder.Add (System.Text.Encoding.ASCII.GetBytes ("eng"));
		// Description with BOM
		builder.Add (new byte[] { 0xFF, 0xFE }); // LE BOM
		builder.Add (System.Text.Encoding.Unicode.GetBytes (""));
		builder.Add (new byte[] { 0x00, 0x00 }); // Null terminator
												 // Lyrics with BOM
		builder.Add (new byte[] { 0xFF, 0xFE }); // LE BOM
		builder.Add (System.Text.Encoding.Unicode.GetBytes ("Hello Lyrics"));

		var result = LyricsFrame.Read (builder.ToBinaryData ().Span, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("Hello Lyrics", result.Frame?.Text);
	}

	#endregion

	#region Helper Methods

	static byte[] CreateTextFrameContent (string text)
	{
		using var builder = new BinaryDataBuilder ();
		builder.Add ((byte)TextEncodingType.Latin1);
		builder.Add (System.Text.Encoding.Latin1.GetBytes (text));
		return builder.ToBinaryData ().ToArray ();
	}

	static byte[] CreateV23TagWithSyncsafeFrameSize (string frameId, byte[] frameContent)
	{
		// Encode frame size as syncsafe (like v2.4) but in a v2.3 tag
		var size = (uint)frameContent.Length;
		var sizeBytes = new byte[4];
		sizeBytes[0] = (byte)((size >> 21) & 0x7F);
		sizeBytes[1] = (byte)((size >> 14) & 0x7F);
		sizeBytes[2] = (byte)((size >> 7) & 0x7F);
		sizeBytes[3] = (byte)(size & 0x7F);

		return CreateV23TagWithRawFrameSize (frameId, frameContent, sizeBytes[0], sizeBytes[1], sizeBytes[2], sizeBytes[3]);
	}

	static byte[] CreateV23TagWithRawFrameSize (string frameId, byte[] frameContent, byte s0, byte s1, byte s2, byte s3)
	{
		using var builder = new BinaryDataBuilder ();

		// ID3v2.3 header
		builder.Add (System.Text.Encoding.ASCII.GetBytes ("ID3"));
		builder.Add ((byte)3); // Version 2.3
		builder.Add ((byte)0); // Revision
		builder.Add ((byte)0); // Flags

		// Tag size (syncsafe) - frame header (10) + frame content
		var tagSize = (uint)(10 + frameContent.Length);
		builder.Add ((byte)((tagSize >> 21) & 0x7F));
		builder.Add ((byte)((tagSize >> 14) & 0x7F));
		builder.Add ((byte)((tagSize >> 7) & 0x7F));
		builder.Add ((byte)(tagSize & 0x7F));

		// Frame header
		builder.Add (System.Text.Encoding.ASCII.GetBytes (frameId));
		builder.Add (s0);
		builder.Add (s1);
		builder.Add (s2);
		builder.Add (s3);
		builder.Add ((byte)0); // Flags byte 1
		builder.Add ((byte)0); // Flags byte 2

		// Frame content
		builder.Add (frameContent);

		return builder.ToBinaryData ().ToArray ();
	}

	static byte[] CreateUtf16FrameWithoutBom (string text)
	{
		using var builder = new BinaryDataBuilder ();
		builder.Add ((byte)TextEncodingType.Utf16WithBom);
		// No BOM, just raw UTF-16 LE bytes
		builder.Add (System.Text.Encoding.Unicode.GetBytes (text));
		return builder.ToBinaryData ().ToArray ();
	}

	static byte[] CreateUtf16LyricsFrameWithoutBom (string lyrics, string language, string description)
	{
		using var builder = new BinaryDataBuilder ();
		builder.Add ((byte)TextEncodingType.Utf16WithBom);
		builder.Add (System.Text.Encoding.ASCII.GetBytes (language.PadRight (3).Substring (0, 3)));
		// Description without BOM
		builder.Add (System.Text.Encoding.Unicode.GetBytes (description));
		builder.Add (new byte[] { 0x00, 0x00 }); // Null terminator
												 // Lyrics without BOM
		builder.Add (System.Text.Encoding.Unicode.GetBytes (lyrics));
		return builder.ToBinaryData ().ToArray ();
	}

	#endregion
}
