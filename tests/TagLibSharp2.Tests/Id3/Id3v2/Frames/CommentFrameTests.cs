// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Core;
using TagLibSharp2.Id3.Id3v2;
using TagLibSharp2.Id3.Id3v2.Frames;

namespace TagLibSharp2.Tests.Id3.Id3v2.Frames;

[TestClass]
[TestCategory ("Unit")]
[TestCategory ("Id3")]
public class CommentFrameTests
{
	[TestMethod]
	public void Constructor_DefaultValues_HasCorrectDefaults ()
	{
		var frame = new CommentFrame ("Test comment");

		Assert.AreEqual ("Test comment", frame.Text);
		Assert.AreEqual ("eng", frame.Language);
		Assert.AreEqual ("", frame.Description);
		Assert.AreEqual (TextEncodingType.Utf8, frame.Encoding);
	}

	[TestMethod]
	public void Constructor_CustomValues_StoresCorrectly ()
	{
		var frame = new CommentFrame ("Test", "deu", "iTunes", TextEncodingType.Utf16WithBom);

		Assert.AreEqual ("Test", frame.Text);
		Assert.AreEqual ("deu", frame.Language);
		Assert.AreEqual ("iTunes", frame.Description);
		Assert.AreEqual (TextEncodingType.Utf16WithBom, frame.Encoding);
	}

	[TestMethod]
	public void FrameId_ReturnsCOMM ()
	{
		Assert.AreEqual ("COMM", CommentFrame.FrameId);
	}

	[TestMethod]
	public void Read_Utf8Comment_ParsesCorrectly ()
	{
		// Build COMM frame: encoding(1) + lang(3) + description(n) + null + text(m)
		var data = BuildCommFrame (
			TextEncodingType.Utf8,
			"eng",
			"",
			"This is a test comment");

		var result = CommentFrame.Read (data, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("This is a test comment", result.Frame!.Text);
		Assert.AreEqual ("eng", result.Frame.Language);
		Assert.AreEqual ("", result.Frame.Description);
	}

	[TestMethod]
	public void Read_WithDescription_ParsesCorrectly ()
	{
		var data = BuildCommFrame (
			TextEncodingType.Utf8,
			"deu",
			"iTunes",
			"German comment");

		var result = CommentFrame.Read (data, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("German comment", result.Frame!.Text);
		Assert.AreEqual ("deu", result.Frame.Language);
		Assert.AreEqual ("iTunes", result.Frame.Description);
	}

	[TestMethod]
	public void Read_Latin1Encoding_ParsesCorrectly ()
	{
		var data = BuildCommFrame (
			TextEncodingType.Latin1,
			"eng",
			"",
			"Latin-1 comment");

		var result = CommentFrame.Read (data, Id3v2Version.V23);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("Latin-1 comment", result.Frame!.Text);
		Assert.AreEqual (TextEncodingType.Latin1, result.Frame.Encoding);
	}

	[TestMethod]
	public void Read_TooShort_ReturnsFailure ()
	{
		var data = new byte[] { 0x00, 0x65, 0x6E }; // Only 3 bytes

		var result = CommentFrame.Read (data, Id3v2Version.V24);

		Assert.IsFalse (result.IsSuccess);
	}

	[TestMethod]
	public void RenderContent_Utf8_RoundTrips ()
	{
		var original = new CommentFrame ("Test comment", "fra", "Desc");

		var rendered = original.RenderContent ();
		var parsed = CommentFrame.Read (rendered.Span, Id3v2Version.V24);

		Assert.IsTrue (parsed.IsSuccess);
		Assert.AreEqual ("Test comment", parsed.Frame!.Text);
		Assert.AreEqual ("fra", parsed.Frame.Language);
		Assert.AreEqual ("Desc", parsed.Frame.Description);
	}

	[TestMethod]
	public void RenderContent_Utf16WithBom_RoundTrips ()
	{
		var original = new CommentFrame (
			"Unicode comment with émoji 🎵",
			"jpn",
			"Spotify",
			TextEncodingType.Utf16WithBom);

		var rendered = original.RenderContent ();
		var parsed = CommentFrame.Read (rendered.Span, Id3v2Version.V24);

		Assert.IsTrue (parsed.IsSuccess);
		Assert.AreEqual ("Unicode comment with émoji 🎵", parsed.Frame!.Text);
		Assert.AreEqual ("jpn", parsed.Frame.Language);
		Assert.AreEqual ("Spotify", parsed.Frame.Description);
	}

	[TestMethod]
	public void Language_NormalizesToThreeChars ()
	{
		// Short language
		var frame1 = new CommentFrame ("Test", "en");
		Assert.AreEqual ("en ", frame1.Language);

		// Long language
		var frame2 = new CommentFrame ("Test", "english");
		Assert.AreEqual ("eng", frame2.Language);

		// Uppercase
		var frame3 = new CommentFrame ("Test", "ENG");
		Assert.AreEqual ("eng", frame3.Language);
	}

	[TestMethod]
	public void Language_EmptyUsesDefault ()
	{
		var frame = new CommentFrame ("Test", "");
		Assert.AreEqual ("eng", frame.Language);
	}

	static byte[] BuildCommFrame (TextEncodingType encoding, string language, string description, string text)
	{
		using var builder = new BinaryDataBuilder ();

		// Encoding byte
		builder.Add ((byte)encoding);

		// Language (3 bytes ASCII)
		var langBytes = System.Text.Encoding.ASCII.GetBytes (language.PadRight (3).Substring (0, 3));
		builder.Add (langBytes);

		// Description + terminator + text
		if (encoding == TextEncodingType.Utf16WithBom || encoding == TextEncodingType.Utf16BE) {
			// UTF-16 encoding
			var enc = encoding == TextEncodingType.Utf16WithBom
				? System.Text.Encoding.Unicode
				: System.Text.Encoding.BigEndianUnicode;

			if (encoding == TextEncodingType.Utf16WithBom) {
				// BOM for description
				builder.Add (new byte[] { 0xFF, 0xFE });
			}
			builder.Add (enc.GetBytes (description));
			builder.Add (new byte[] { 0x00, 0x00 }); // Double null terminator

			if (encoding == TextEncodingType.Utf16WithBom) {
				// BOM for text
				builder.Add (new byte[] { 0xFF, 0xFE });
			}
			builder.Add (enc.GetBytes (text));
		} else {
			// Latin-1 or UTF-8
			var enc = encoding == TextEncodingType.Latin1
				? System.Text.Encoding.Latin1
				: System.Text.Encoding.UTF8;

			builder.Add (enc.GetBytes (description));
			builder.Add ((byte)0x00); // Single null terminator
			builder.Add (enc.GetBytes (text));
		}

		return builder.ToBinaryData ().ToArray ();
	}
}
