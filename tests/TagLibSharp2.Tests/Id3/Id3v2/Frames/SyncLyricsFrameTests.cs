// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Core;
using TagLibSharp2.Id3.Id3v2;
using TagLibSharp2.Id3.Id3v2.Frames;

namespace TagLibSharp2.Tests.Id3.Id3v2.Frames;

[TestClass]
[TestCategory ("Unit")]
[TestCategory ("Id3")]
public class SyncLyricsFrameTests
{
	[TestMethod]
	public void Constructor_DefaultValues_HasCorrectDefaults ()
	{
		var frame = new SyncLyricsFrame ();

		Assert.AreEqual ("eng", frame.Language);
		Assert.AreEqual (SyncLyricsType.Lyrics, frame.ContentType);
		Assert.AreEqual (TimestampFormat.Milliseconds, frame.TimestampFormat);
		Assert.AreEqual ("", frame.Description);
		Assert.IsEmpty (frame.SyncItems);
	}

	[TestMethod]
	public void FrameId_ReturnsSYLT ()
	{
		Assert.AreEqual ("SYLT", SyncLyricsFrame.FrameId);
	}

	[TestMethod]
	public void AddSyncItem_AddsSingleItem ()
	{
		var frame = new SyncLyricsFrame ();

		frame.AddSyncItem ("Hello world", 1000);

		Assert.HasCount (1, frame.SyncItems);
		Assert.AreEqual ("Hello world", frame.SyncItems[0].Text);
		Assert.AreEqual (1000u, frame.SyncItems[0].Timestamp);
	}

	[TestMethod]
	public void Read_SimpleFrame_ParsesCorrectly ()
	{
		var data = BuildSyltFrame (
			TextEncodingType.Latin1,
			"eng",
			TimestampFormat.Milliseconds,
			SyncLyricsType.Lyrics,
			"",
			[("Hello", 0u), ("World", 500u)]);

		var result = SyncLyricsFrame.Read (data, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("eng", result.Frame!.Language);
		Assert.AreEqual (SyncLyricsType.Lyrics, result.Frame.ContentType);
		Assert.HasCount (2, result.Frame.SyncItems);
		Assert.AreEqual ("Hello", result.Frame.SyncItems[0].Text);
		Assert.AreEqual (0u, result.Frame.SyncItems[0].Timestamp);
		Assert.AreEqual ("World", result.Frame.SyncItems[1].Text);
		Assert.AreEqual (500u, result.Frame.SyncItems[1].Timestamp);
	}

	[TestMethod]
	public void Read_WithDescription_ParsesCorrectly ()
	{
		var data = BuildSyltFrame (
			TextEncodingType.Utf8,
			"deu",
			TimestampFormat.MpegFrames,
			SyncLyricsType.Chords,
			"Guitar Chords",
			[("Am", 0u), ("C", 100u), ("G", 200u)]);

		var result = SyncLyricsFrame.Read (data, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("deu", result.Frame!.Language);
		Assert.AreEqual ("Guitar Chords", result.Frame.Description);
		Assert.AreEqual (SyncLyricsType.Chords, result.Frame.ContentType);
		Assert.HasCount (3, result.Frame.SyncItems);
	}

	[TestMethod]
	public void Read_TooShort_ReturnsFailure ()
	{
		var data = new byte[] { 0x00, 0x65, 0x6E }; // Only 3 bytes

		var result = SyncLyricsFrame.Read (data, Id3v2Version.V24);

		Assert.IsFalse (result.IsSuccess);
	}

	[TestMethod]
	public void RenderContent_RoundTrips ()
	{
		var original = new SyncLyricsFrame {
			Language = "fra",
			Description = "Karaoke",
			ContentType = SyncLyricsType.Lyrics,
			TimestampFormat = TimestampFormat.Milliseconds
		};
		original.AddSyncItem ("Première ligne", 0);
		original.AddSyncItem ("Deuxième ligne", 2500);
		original.AddSyncItem ("Troisième ligne", 5000);

		var rendered = original.RenderContent ();
		var parsed = SyncLyricsFrame.Read (rendered.Span, Id3v2Version.V24);

		Assert.IsTrue (parsed.IsSuccess);
		Assert.AreEqual ("fra", parsed.Frame!.Language);
		Assert.AreEqual ("Karaoke", parsed.Frame.Description);
		Assert.HasCount (3, parsed.Frame.SyncItems);
		Assert.AreEqual ("Première ligne", parsed.Frame.SyncItems[0].Text);
		Assert.AreEqual (0u, parsed.Frame.SyncItems[0].Timestamp);
		Assert.AreEqual ("Deuxième ligne", parsed.Frame.SyncItems[1].Text);
		Assert.AreEqual (2500u, parsed.Frame.SyncItems[1].Timestamp);
	}

	[TestMethod]
	public void ClearSyncItems_RemovesAllItems ()
	{
		var frame = new SyncLyricsFrame ();
		frame.AddSyncItem ("Line 1", 0);
		frame.AddSyncItem ("Line 2", 1000);

		frame.ClearSyncItems ();

		Assert.IsEmpty (frame.SyncItems);
	}

	[TestMethod]
	public void RenderContent_EmptyLanguage_UsesDefault ()
	{
		var frame = new SyncLyricsFrame {
			Language = "",
			Description = "Test"
		};
		frame.AddSyncItem ("Test", 0);

		var rendered = frame.RenderContent ();
		var parsed = SyncLyricsFrame.Read (rendered.Span, Id3v2Version.V24);

		Assert.IsTrue (parsed.IsSuccess);
		Assert.AreEqual ("eng", parsed.Frame!.Language);
	}

	[TestMethod]
	public void RenderContent_ShortLanguage_IsPadded ()
	{
		var frame = new SyncLyricsFrame {
			Language = "de",
			Description = "Test"
		};
		frame.AddSyncItem ("Test", 0);

		var rendered = frame.RenderContent ();
		var parsed = SyncLyricsFrame.Read (rendered.Span, Id3v2Version.V24);

		Assert.IsTrue (parsed.IsSuccess);
		Assert.AreEqual ("de ", parsed.Frame!.Language);
	}

	[TestMethod]
	public void RenderContent_LongLanguage_IsTruncated ()
	{
		var frame = new SyncLyricsFrame {
			Language = "deutsch",
			Description = "Test"
		};
		frame.AddSyncItem ("Test", 0);

		var rendered = frame.RenderContent ();
		var parsed = SyncLyricsFrame.Read (rendered.Span, Id3v2Version.V24);

		Assert.IsTrue (parsed.IsSuccess);
		Assert.AreEqual ("deu", parsed.Frame!.Language);
	}

	[TestMethod]
	public void Read_InvalidEncoding_ReturnsFailure ()
	{
		// Build frame with invalid encoding byte (5)
		var data = new byte[] { 5, (byte)'e', (byte)'n', (byte)'g', 2, 1 };

		var result = SyncLyricsFrame.Read (data, Id3v2Version.V24);

		Assert.IsFalse (result.IsSuccess);
		StringAssert.Contains (result.Error, "encoding");
	}

	[TestMethod]
	public void SyncLyricsItem_Equality_WorksCorrectly ()
	{
		var item1 = new SyncLyricsItem ("Hello", 1000);
		var item2 = new SyncLyricsItem ("Hello", 1000);
		var item3 = new SyncLyricsItem ("World", 1000);

		Assert.IsTrue (item1 == item2);
		Assert.IsFalse (item1 == item3);
		Assert.IsTrue (item1 != item3);
		Assert.AreEqual (item1.GetHashCode (), item2.GetHashCode ());
	}

	[TestMethod]
	public void SyncLyricsItem_EqualsObject_WorksCorrectly ()
	{
		var item1 = new SyncLyricsItem ("Hello", 1000);
		object item2 = new SyncLyricsItem ("Hello", 1000);
		object notAnItem = "not an item";

		Assert.IsTrue (item1.Equals (item2));
		Assert.IsFalse (item1.Equals (notAnItem));
		Assert.IsFalse (item1.Equals (null));
	}

	[TestMethod]
	public void Read_DifferentContentTypes_ParseCorrectly ()
	{
		foreach (var contentType in new[] { SyncLyricsType.Other, SyncLyricsType.TextTranscription,
			SyncLyricsType.PartNames, SyncLyricsType.Events, SyncLyricsType.Trivia,
			SyncLyricsType.WebPageUrls, SyncLyricsType.ImageUrls }) {

			var data = BuildSyltFrame (
				TextEncodingType.Utf8,
				"eng",
				TimestampFormat.Milliseconds,
				contentType,
				"",
				[("Test", 0u)]);

			var result = SyncLyricsFrame.Read (data, Id3v2Version.V24);

			Assert.IsTrue (result.IsSuccess);
			Assert.AreEqual (contentType, result.Frame!.ContentType);
		}
	}

	[TestMethod]
	public void RenderContent_Utf16WithBom_RoundTrips ()
	{
		var frame = new SyncLyricsFrame {
			Encoding = TextEncodingType.Utf16WithBom,
			Language = "jpn",
			Description = "日本語",
			ContentType = SyncLyricsType.Lyrics
		};
		frame.AddSyncItem ("こんにちは", 0);
		frame.AddSyncItem ("世界", 1000);

		var rendered = frame.RenderContent ();
		var parsed = SyncLyricsFrame.Read (rendered.Span, Id3v2Version.V24);

		Assert.IsTrue (parsed.IsSuccess);
		Assert.AreEqual ("日本語", parsed.Frame!.Description);
		Assert.HasCount (2, parsed.Frame.SyncItems);
		Assert.AreEqual ("こんにちは", parsed.Frame.SyncItems[0].Text);
	}

	static byte[] BuildSyltFrame (
		TextEncodingType encoding,
		string language,
		TimestampFormat timestampFormat,
		SyncLyricsType contentType,
		string description,
		(string text, uint timestamp)[] items)
	{
		using var builder = new BinaryDataBuilder ();

		// Encoding
		builder.Add ((byte)encoding);

		// Language (3 bytes)
		var langBytes = System.Text.Encoding.ASCII.GetBytes (language.PadRight (3).Substring (0, 3));
		builder.Add (langBytes);

		// Timestamp format
		builder.Add ((byte)timestampFormat);

		// Content type
		builder.Add ((byte)contentType);

		// Description (null-terminated)
		if (encoding is TextEncodingType.Latin1 or TextEncodingType.Utf8) {
			var enc = encoding == TextEncodingType.Latin1
				? System.Text.Encoding.Latin1
				: System.Text.Encoding.UTF8;
			builder.Add (enc.GetBytes (description));
			builder.Add ((byte)0x00);
		} else {
			var enc = encoding == TextEncodingType.Utf16WithBom
				? System.Text.Encoding.Unicode
				: System.Text.Encoding.BigEndianUnicode;
			if (encoding == TextEncodingType.Utf16WithBom)
				builder.Add (new byte[] { 0xFF, 0xFE });
			builder.Add (enc.GetBytes (description));
			builder.Add (new byte[] { 0x00, 0x00 });
		}

		// Sync items: text + null + timestamp(4 bytes big-endian)
		foreach (var (text, timestamp) in items) {
			if (encoding is TextEncodingType.Latin1 or TextEncodingType.Utf8) {
				var enc = encoding == TextEncodingType.Latin1
					? System.Text.Encoding.Latin1
					: System.Text.Encoding.UTF8;
				builder.Add (enc.GetBytes (text));
				builder.Add ((byte)0x00);
			} else {
				var enc = encoding == TextEncodingType.Utf16WithBom
					? System.Text.Encoding.Unicode
					: System.Text.Encoding.BigEndianUnicode;
				builder.Add (enc.GetBytes (text));
				builder.Add (new byte[] { 0x00, 0x00 });
			}

			// Timestamp (big-endian 4 bytes)
			builder.Add ((byte)((timestamp >> 24) & 0xFF));
			builder.Add ((byte)((timestamp >> 16) & 0xFF));
			builder.Add ((byte)((timestamp >> 8) & 0xFF));
			builder.Add ((byte)(timestamp & 0xFF));
		}

		return builder.ToBinaryData ().ToArray ();
	}
}
