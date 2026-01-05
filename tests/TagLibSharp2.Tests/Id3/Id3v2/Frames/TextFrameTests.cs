// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Id3.Id3v2;
using TagLibSharp2.Id3.Id3v2.Frames;

namespace TagLibSharp2.Tests.Id3.Id3v2.Frames;

// Uses TestBuilders.Id3v2.CreateTextFrameData for encoding-specific frame data

[TestClass]
[TestCategory ("Unit")]
[TestCategory ("Id3")]
[TestCategory ("Id3v2")]
public class TextFrameTests
{
	// Text Frame Format:
	// Offset  Size  Field
	// 0       1     Text encoding (0=Latin-1, 1=UTF-16 w/BOM, 2=UTF-16BE, 3=UTF-8)
	// 1       n     Text content (may be null-terminated)

	[TestMethod]
	[DataRow (TextEncodingType.Latin1, "Test Title")]
	[DataRow (TextEncodingType.Utf16WithBom, "Unicode: Test")]
	[DataRow (TextEncodingType.Utf16BE, "Big Endian Test")]
	[DataRow (TextEncodingType.Utf8, "UTF-8: Test")]
	public void Read_AllEncodings_ParsesCorrectly (TextEncodingType encoding, string text)
	{
		var data = TestBuilders.Id3v2.CreateTextFrameData (encoding, text);

		var result = TextFrame.Read ("TIT2", data, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess, $"Failed for encoding {encoding}");
		Assert.AreEqual ("TIT2", result.Frame!.Id);
		Assert.AreEqual (text, result.Frame.Text);
		Assert.AreEqual (encoding, result.Frame.Encoding);
	}

	[TestMethod]
	public void Read_InvalidEncoding_ReturnsFailure ()
	{
		var data = new byte[] { 0x05, (byte)'T', (byte)'e', (byte)'s', (byte)'t' };

		var result = TextFrame.Read ("TIT2", data, Id3v2Version.V24);

		Assert.IsFalse (result.IsSuccess);
		Assert.IsNotNull (result.Error);
	}



	[TestMethod]
	[DataRow ("TIT2", "Song Title", Id3v2Version.V24)]
	[DataRow ("TPE1", "Artist Name", Id3v2Version.V24)]
	[DataRow ("TALB", "Album Name", Id3v2Version.V24)]
	[DataRow ("TYER", "2024", Id3v2Version.V23)]
	[DataRow ("TDRC", "2024-12-25", Id3v2Version.V24)]
	[DataRow ("TCON", "Rock", Id3v2Version.V24)]
	[DataRow ("TRCK", "5/12", Id3v2Version.V24)]
	public void Read_StandardFrames_ParsesCorrectly (string frameId, string text, Id3v2Version version)
	{
		var data = TestBuilders.Id3v2.CreateTextFrameData (TextEncodingType.Latin1, text);

		var result = TextFrame.Read (frameId, data, version);

		Assert.IsTrue (result.IsSuccess, $"Failed for frame {frameId}");
		Assert.AreEqual (frameId, result.Frame!.Id);
		Assert.AreEqual (text, result.Frame.Text);
	}

	[TestMethod]
	public void Read_TCON_NumericGenre_ParsesCorrectly ()
	{
		// ID3v2 can encode genres as "(17)" meaning genre index 17 (Rock)
		var data = TestBuilders.Id3v2.CreateTextFrameData (TextEncodingType.Latin1, "(17)");

		var result = TextFrame.Read ("TCON", data, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("(17)", result.Frame!.Text);
	}

	[TestMethod]
	public void Read_EmptyText_ReturnsEmptyString ()
	{
		var data = new byte[] { 0x00 }; // Just encoding byte, no text

		var result = TextFrame.Read ("TIT2", data, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("", result.Frame!.Text);
	}

	[TestMethod]
	public void Read_NullTerminatedText_TrimsNull ()
	{
		// Latin-1 with null terminator
		var data = new byte[] { 0x00, (byte)'A', (byte)'B', 0x00 };

		var result = TextFrame.Read ("TIT2", data, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("AB", result.Frame!.Text);
	}

	[TestMethod]
	public void Read_TooShort_ReturnsFailure ()
	{
		byte[] data = [];

		var result = TextFrame.Read ("TIT2", data, Id3v2Version.V24);

		Assert.IsFalse (result.IsSuccess);
		Assert.IsNotNull (result.Error);
	}

	[TestMethod]
	public void Read_MultipleNullSeparatedValues_PreservesAll ()
	{
		// ID3v2.4 uses null separators for multi-value text frames
		// Example: "Value1\0Value2\0" (trailing null stripped)
		var data = new byte[] { 0x00, (byte)'A', 0x00, (byte)'B', 0x00 };

		var result = TextFrame.Read ("TIT2", data, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		// Preserves all values with null separators (trailing null stripped)
		Assert.AreEqual ("A\0B", result.Frame!.Text);
	}



	[TestMethod]
	public void Render_Latin1_CreatesCorrectData ()
	{
		var frame = new TextFrame ("TIT2", "Test", TextEncodingType.Latin1);

		var data = frame.RenderContent ();

		Assert.AreEqual (0x00, data[0]); // Latin-1 encoding
		Assert.AreEqual ((byte)'T', data[1]);
		Assert.AreEqual ((byte)'e', data[2]);
		Assert.AreEqual ((byte)'s', data[3]);
		Assert.AreEqual ((byte)'t', data[4]);
	}

	[TestMethod]
	public void Render_Utf8_CreatesCorrectData ()
	{
		var frame = new TextFrame ("TIT2", "Test", TextEncodingType.Utf8);

		var data = frame.RenderContent ();

		Assert.AreEqual (0x03, data[0]); // UTF-8 encoding
	}

	[TestMethod]
	public void Render_RoundTrip_PreservesData ()
	{
		var original = new TextFrame ("TPE1", "Test Artist", TextEncodingType.Utf8);

		var rendered = original.RenderContent ();
		var result = TextFrame.Read ("TPE1", rendered.Span, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (original.Id, result.Frame!.Id);
		Assert.AreEqual (original.Text, result.Frame.Text);
		Assert.AreEqual (original.Encoding, result.Frame.Encoding);
	}

	[TestMethod]
	public void Render_Utf16_RoundTrip_PreservesData ()
	{
		var original = new TextFrame ("TIT2", "日本語", TextEncodingType.Utf16WithBom);

		var rendered = original.RenderContent ();
		var result = TextFrame.Read ("TIT2", rendered.Span, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (original.Text, result.Frame!.Text);
	}

}
