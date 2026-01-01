// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Core;
using TagLibSharp2.Id3.Id3v2;
using TagLibSharp2.Id3.Id3v2.Frames;

namespace TagLibSharp2.Tests.Id3.Id3v2.Frames;

[TestClass]
[TestCategory ("Unit")]
[TestCategory ("Id3")]
public class GeneralObjectFrameTests
{
	[TestMethod]
	public void Constructor_SetsProperties ()
	{
		var data = new BinaryData ([1, 2, 3, 4, 5]);
		var frame = new GeneralObjectFrame ("application/json", "config.json", "Configuration", data);

		Assert.AreEqual ("application/json", frame.MimeType);
		Assert.AreEqual ("config.json", frame.FileName);
		Assert.AreEqual ("Configuration", frame.Description);
		Assert.AreEqual (5, frame.Data.Length);
	}

	[TestMethod]
	public void FrameId_ReturnsGEOB ()
	{
		Assert.AreEqual ("GEOB", GeneralObjectFrame.FrameId);
	}

	[TestMethod]
	public void Read_SimpleFrame_ParsesCorrectly ()
	{
		var data = BuildGeobFrame (
			TextEncodingType.Latin1,
			"text/plain",
			"test.txt",
			"Test file",
			[72, 101, 108, 108, 111]); // "Hello"

		var result = GeneralObjectFrame.Read (data, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("text/plain", result.Frame!.MimeType);
		Assert.AreEqual ("test.txt", result.Frame.FileName);
		Assert.AreEqual ("Test file", result.Frame.Description);
		Assert.AreEqual (5, result.Frame.Data.Length);
	}

	[TestMethod]
	public void Read_EmptyFileName_ParsesCorrectly ()
	{
		var data = BuildGeobFrame (
			TextEncodingType.Utf8,
			"application/octet-stream",
			"",
			"Binary blob",
			[0xDE, 0xAD, 0xBE, 0xEF]);

		var result = GeneralObjectFrame.Read (data, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("", result.Frame!.FileName);
		Assert.AreEqual ("Binary blob", result.Frame.Description);
	}

	[TestMethod]
	public void Read_TooShort_ReturnsFailure ()
	{
		var result = GeneralObjectFrame.Read ([0x00], Id3v2Version.V24);

		Assert.IsFalse (result.IsSuccess);
	}

	[TestMethod]
	public void RenderContent_RoundTrips ()
	{
		var originalData = new BinaryData ([1, 2, 3, 4, 5, 6, 7, 8]);
		var original = new GeneralObjectFrame (
			"image/svg+xml",
			"icon.svg",
			"Application Icon",
			originalData);

		var rendered = original.RenderContent ();
		var parsed = GeneralObjectFrame.Read (rendered.Span, Id3v2Version.V24);

		Assert.IsTrue (parsed.IsSuccess);
		Assert.AreEqual ("image/svg+xml", parsed.Frame!.MimeType);
		Assert.AreEqual ("icon.svg", parsed.Frame.FileName);
		Assert.AreEqual ("Application Icon", parsed.Frame.Description);
		Assert.AreEqual (8, parsed.Frame.Data.Length);
	}

	[TestMethod]
	public void RenderContent_UnicodeFileNameAndDescription_RoundTrips ()
	{
		var originalData = new BinaryData ([0xFF, 0xFE]);
		var original = new GeneralObjectFrame (
			"text/plain",
			"日本語ファイル.txt",
			"Japanese file description",
			originalData,
			TextEncodingType.Utf8);

		var rendered = original.RenderContent ();
		var parsed = GeneralObjectFrame.Read (rendered.Span, Id3v2Version.V24);

		Assert.IsTrue (parsed.IsSuccess);
		Assert.AreEqual ("日本語ファイル.txt", parsed.Frame!.FileName);
	}

	[TestMethod]
	public void Read_InvalidEncoding_ReturnsFailure ()
	{
		// Build frame with invalid encoding byte (5)
		var data = new byte[] { 5, (byte)'t', (byte)'e', (byte)'x', (byte)'t', 0x00 };

		var result = GeneralObjectFrame.Read (data, Id3v2Version.V24);

		Assert.IsFalse (result.IsSuccess);
		StringAssert.Contains (result.Error, "encoding");
	}

	[TestMethod]
	public void Read_MissingMimeTerminator_ReturnsFailure ()
	{
		// Data has encoding but no null terminator for MIME type
		var data = new byte[] { 0x00, (byte)'t', (byte)'e', (byte)'x', (byte)'t' };

		var result = GeneralObjectFrame.Read (data, Id3v2Version.V24);

		Assert.IsFalse (result.IsSuccess);
		StringAssert.Contains (result.Error, "MIME");
	}

	[TestMethod]
	public void Read_Utf16BE_ParsesCorrectly ()
	{
		var data = BuildGeobFrame (
			TextEncodingType.Utf16BE,
			"text/plain",
			"test.txt",
			"Description",
			[0x01, 0x02, 0x03]);

		var result = GeneralObjectFrame.Read (data, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("test.txt", result.Frame!.FileName);
		Assert.AreEqual ("Description", result.Frame.Description);
	}

	[TestMethod]
	public void RenderContent_Utf16BE_RoundTrips ()
	{
		var originalData = new BinaryData ([1, 2, 3]);
		var original = new GeneralObjectFrame (
			"text/plain",
			"日本語.txt",
			"Description",
			originalData,
			TextEncodingType.Utf16BE);

		var rendered = original.RenderContent ();
		var parsed = GeneralObjectFrame.Read (rendered.Span, Id3v2Version.V24);

		Assert.IsTrue (parsed.IsSuccess);
		Assert.AreEqual ("日本語.txt", parsed.Frame!.FileName);
	}

	[TestMethod]
	public void RenderContent_Utf16WithBom_RoundTrips ()
	{
		var originalData = new BinaryData ([0xAB, 0xCD]);
		var original = new GeneralObjectFrame (
			"application/octet-stream",
			"test_日本語.bin",
			"Test Description",
			originalData,
			TextEncodingType.Utf16WithBom);

		var rendered = original.RenderContent ();
		var parsed = GeneralObjectFrame.Read (rendered.Span, Id3v2Version.V24);

		Assert.IsTrue (parsed.IsSuccess);
		Assert.AreEqual ("test_日本語.bin", parsed.Frame!.FileName);
		Assert.AreEqual (TextEncodingType.Utf16WithBom, parsed.Frame.Encoding);
	}

	[TestMethod]
	public void RenderContent_Latin1_RoundTrips ()
	{
		var originalData = new BinaryData ([0x01]);
		var original = new GeneralObjectFrame (
			"text/plain",
			"file.txt",
			"Simple Description",
			originalData,
			TextEncodingType.Latin1);

		var rendered = original.RenderContent ();
		var parsed = GeneralObjectFrame.Read (rendered.Span, Id3v2Version.V24);

		Assert.IsTrue (parsed.IsSuccess);
		Assert.AreEqual (TextEncodingType.Latin1, parsed.Frame!.Encoding);
	}

	[TestMethod]
	public void Read_Utf16WithBomBigEndian_ParsesCorrectly ()
	{
		// Build frame with UTF-16 big endian BOM (0xFE 0xFF)
		using var builder = new BinaryDataBuilder ();
		builder.Add ((byte)TextEncodingType.Utf16WithBom);
		builder.Add (System.Text.Encoding.Latin1.GetBytes ("text/plain"));
		builder.Add ((byte)0x00);
		// Filename with big-endian BOM
		builder.Add (new byte[] { 0xFE, 0xFF }); // Big-endian BOM
		builder.Add (System.Text.Encoding.BigEndianUnicode.GetBytes ("test"));
		builder.Add (new byte[] { 0x00, 0x00 });
		// Description with big-endian BOM
		builder.Add (new byte[] { 0xFE, 0xFF });
		builder.Add (System.Text.Encoding.BigEndianUnicode.GetBytes ("desc"));
		builder.Add (new byte[] { 0x00, 0x00 });
		// Object data
		builder.Add ((byte)0xAB);

		var result = GeneralObjectFrame.Read (builder.ToBinaryData ().Span, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("test", result.Frame!.FileName);
		Assert.AreEqual ("desc", result.Frame.Description);
	}

	[TestMethod]
	public void Read_Utf16NoBom_FallsBackToLittleEndian ()
	{
		// Build frame with UTF-16 but no BOM
		using var builder = new BinaryDataBuilder ();
		builder.Add ((byte)TextEncodingType.Utf16WithBom);
		builder.Add (System.Text.Encoding.Latin1.GetBytes ("text/plain"));
		builder.Add ((byte)0x00);
		// Filename with no BOM (just raw UTF-16LE)
		builder.Add (System.Text.Encoding.Unicode.GetBytes ("A"));
		builder.Add (new byte[] { 0x00, 0x00 });
		// Description
		builder.Add (System.Text.Encoding.Unicode.GetBytes ("B"));
		builder.Add (new byte[] { 0x00, 0x00 });
		// Object data
		builder.Add ((byte)0xFF);

		var result = GeneralObjectFrame.Read (builder.ToBinaryData ().Span, Id3v2Version.V24);

		// Should still parse (falls back to little-endian interpretation)
		Assert.IsTrue (result.IsSuccess);
	}

	static byte[] BuildGeobFrame (
		TextEncodingType encoding,
		string mimeType,
		string fileName,
		string description,
		byte[] objectData)
	{
		using var builder = new BinaryDataBuilder ();

		// Encoding
		builder.Add ((byte)encoding);

		// MIME type (always Latin-1, null-terminated)
		builder.Add (System.Text.Encoding.Latin1.GetBytes (mimeType));
		builder.Add ((byte)0x00);

		// File name and description use specified encoding
		if (encoding is TextEncodingType.Latin1 or TextEncodingType.Utf8) {
			var enc = encoding == TextEncodingType.Latin1
				? System.Text.Encoding.Latin1
				: System.Text.Encoding.UTF8;

			builder.Add (enc.GetBytes (fileName));
			builder.Add ((byte)0x00);
			builder.Add (enc.GetBytes (description));
			builder.Add ((byte)0x00);
		} else {
			var enc = encoding == TextEncodingType.Utf16WithBom
				? System.Text.Encoding.Unicode
				: System.Text.Encoding.BigEndianUnicode;

			if (encoding == TextEncodingType.Utf16WithBom)
				builder.Add (new byte[] { 0xFF, 0xFE });
			builder.Add (enc.GetBytes (fileName));
			builder.Add (new byte[] { 0x00, 0x00 });

			if (encoding == TextEncodingType.Utf16WithBom)
				builder.Add (new byte[] { 0xFF, 0xFE });
			builder.Add (enc.GetBytes (description));
			builder.Add (new byte[] { 0x00, 0x00 });
		}

		// Object data
		builder.Add (objectData);

		return builder.ToBinaryData ().ToArray ();
	}
}
