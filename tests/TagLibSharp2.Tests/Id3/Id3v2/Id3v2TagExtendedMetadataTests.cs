// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Id3.Id3v2;

namespace TagLibSharp2.Tests.Id3.Id3v2;

/// <summary>
/// Tests for extended metadata properties: Conductor, Copyright, Compilation,
/// Lyrics, TotalTracks, TotalDiscs, and UFID frames.
/// </summary>
[TestClass]
[TestCategory ("Unit")]
[TestCategory ("Id3")]
[TestCategory ("Id3v2")]
public class Id3v2TagExtendedMetadataTests
{
	#region Conductor (TPE3) Tests

	[TestMethod]
	public void Conductor_GetSet_Works ()
	{
		var tag = new Id3v2Tag (Id3v2Version.V24);

		tag.Conductor = "Herbert von Karajan";

		Assert.AreEqual ("Herbert von Karajan", tag.Conductor);
		Assert.AreEqual ("Herbert von Karajan", tag.GetTextFrame ("TPE3"));
	}

	[TestMethod]
	public void Conductor_SetNull_ClearsField ()
	{
		var tag = new Id3v2Tag (Id3v2Version.V24);
		tag.Conductor = "Herbert von Karajan";

		tag.Conductor = null;

		Assert.IsNull (tag.Conductor);
		Assert.IsNull (tag.GetTextFrame ("TPE3"));
	}

	[TestMethod]
	public void Conductor_FromFile_ParsesCorrectly ()
	{
		var data = CreateTagWithTextFrame ("TPE3", "Leonard Bernstein", version: 4);

		var result = Id3v2Tag.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("Leonard Bernstein", result.Tag!.Conductor);
	}

	[TestMethod]
	public void Conductor_RoundTrip_PreservesValue ()
	{
		var original = new Id3v2Tag (Id3v2Version.V24) { Conductor = "Sir Simon Rattle" };

		var rendered = original.Render ();
		var result = Id3v2Tag.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("Sir Simon Rattle", result.Tag!.Conductor);
	}

	#endregion

	#region Copyright (TCOP) Tests

	[TestMethod]
	public void Copyright_GetSet_Works ()
	{
		var tag = new Id3v2Tag (Id3v2Version.V24);

		tag.Copyright = "2024 Acme Records";

		Assert.AreEqual ("2024 Acme Records", tag.Copyright);
		Assert.AreEqual ("2024 Acme Records", tag.GetTextFrame ("TCOP"));
	}

	[TestMethod]
	public void Copyright_SetNull_ClearsField ()
	{
		var tag = new Id3v2Tag (Id3v2Version.V24);
		tag.Copyright = "2024 Acme Records";

		tag.Copyright = null;

		Assert.IsNull (tag.Copyright);
		Assert.IsNull (tag.GetTextFrame ("TCOP"));
	}

	[TestMethod]
	public void Copyright_FromFile_ParsesCorrectly ()
	{
		var data = CreateTagWithTextFrame ("TCOP", "2023 Universal Music", version: 4);

		var result = Id3v2Tag.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("2023 Universal Music", result.Tag!.Copyright);
	}

	[TestMethod]
	public void Copyright_RoundTrip_PreservesValue ()
	{
		var original = new Id3v2Tag (Id3v2Version.V24) { Copyright = "2025 Independent" };

		var rendered = original.Render ();
		var result = Id3v2Tag.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("2025 Independent", result.Tag!.Copyright);
	}

	#endregion

	#region Compilation (TCMP) Tests

	[TestMethod]
	public void IsCompilation_GetSet_Works ()
	{
		var tag = new Id3v2Tag (Id3v2Version.V24);

		tag.IsCompilation = true;

		Assert.IsTrue (tag.IsCompilation);
		Assert.AreEqual ("1", tag.GetTextFrame ("TCMP"));
	}

	[TestMethod]
	public void IsCompilation_SetFalse_ClearsField ()
	{
		var tag = new Id3v2Tag (Id3v2Version.V24);
		tag.IsCompilation = true;

		tag.IsCompilation = false;

		Assert.IsFalse (tag.IsCompilation);
		Assert.IsNull (tag.GetTextFrame ("TCMP"));
	}

	[TestMethod]
	public void IsCompilation_FromFile_ParsesOne ()
	{
		var data = CreateTagWithTextFrame ("TCMP", "1", version: 4);

		var result = Id3v2Tag.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.IsTrue (result.Tag!.IsCompilation);
	}

	[TestMethod]
	public void IsCompilation_FromFile_ParsesZero ()
	{
		var data = CreateTagWithTextFrame ("TCMP", "0", version: 4);

		var result = Id3v2Tag.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.IsFalse (result.Tag!.IsCompilation);
	}

	[TestMethod]
	public void IsCompilation_RoundTrip_PreservesValue ()
	{
		var original = new Id3v2Tag (Id3v2Version.V24) { IsCompilation = true };

		var rendered = original.Render ();
		var result = Id3v2Tag.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.IsTrue (result.Tag!.IsCompilation);
	}

	#endregion

	#region TotalTracks Tests

	[TestMethod]
	public void TotalTracks_GetFromSlashFormat_Works ()
	{
		var data = CreateTagWithTextFrame ("TRCK", "5/12", version: 4);

		var result = Id3v2Tag.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (5u, result.Tag!.Track);
		Assert.AreEqual (12u, result.Tag.TotalTracks);
	}

	[TestMethod]
	public void TotalTracks_SetWithTrack_FormatsCorrectly ()
	{
		var tag = new Id3v2Tag (Id3v2Version.V24);

		tag.Track = 3;
		tag.TotalTracks = 10;

		Assert.AreEqual ("3/10", tag.GetTextFrame ("TRCK"));
	}

	[TestMethod]
	public void TotalTracks_SetWithoutTrack_SetsTrackToOne ()
	{
		var tag = new Id3v2Tag (Id3v2Version.V24);

		tag.TotalTracks = 15;

		// When setting total without track, track defaults to 1
		Assert.AreEqual ("1/15", tag.GetTextFrame ("TRCK"));
	}

	[TestMethod]
	public void TotalTracks_SetNull_PreservesTrackOnly ()
	{
		var tag = new Id3v2Tag (Id3v2Version.V24);
		tag.Track = 5;
		tag.TotalTracks = 12;

		tag.TotalTracks = null;

		Assert.AreEqual ("5", tag.GetTextFrame ("TRCK"));
		Assert.AreEqual (5u, tag.Track);
	}

	[TestMethod]
	public void TotalTracks_RoundTrip_PreservesValue ()
	{
		var original = new Id3v2Tag (Id3v2Version.V24) {
			Track = 7,
			TotalTracks = 14
		};

		var rendered = original.Render ();
		var result = Id3v2Tag.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (7u, result.Tag!.Track);
		Assert.AreEqual (14u, result.Tag.TotalTracks);
	}

	#endregion

	#region TotalDiscs Tests

	[TestMethod]
	public void TotalDiscs_GetFromSlashFormat_Works ()
	{
		var data = CreateTagWithTextFrame ("TPOS", "2/3", version: 4);

		var result = Id3v2Tag.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (2u, result.Tag!.DiscNumber);
		Assert.AreEqual (3u, result.Tag.TotalDiscs);
	}

	[TestMethod]
	public void TotalDiscs_SetWithDisc_FormatsCorrectly ()
	{
		var tag = new Id3v2Tag (Id3v2Version.V24);

		tag.DiscNumber = 1;
		tag.TotalDiscs = 2;

		Assert.AreEqual ("1/2", tag.GetTextFrame ("TPOS"));
	}

	[TestMethod]
	public void TotalDiscs_SetWithoutDisc_SetsDiscToOne ()
	{
		var tag = new Id3v2Tag (Id3v2Version.V24);

		tag.TotalDiscs = 3;

		Assert.AreEqual ("1/3", tag.GetTextFrame ("TPOS"));
	}

	[TestMethod]
	public void TotalDiscs_SetNull_PreservesDiscOnly ()
	{
		var tag = new Id3v2Tag (Id3v2Version.V24);
		tag.DiscNumber = 2;
		tag.TotalDiscs = 3;

		tag.TotalDiscs = null;

		Assert.AreEqual ("2", tag.GetTextFrame ("TPOS"));
		Assert.AreEqual (2u, tag.DiscNumber);
	}

	[TestMethod]
	public void TotalDiscs_RoundTrip_PreservesValue ()
	{
		var original = new Id3v2Tag (Id3v2Version.V24) {
			DiscNumber = 2,
			TotalDiscs = 4
		};

		var rendered = original.Render ();
		var result = Id3v2Tag.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (2u, result.Tag!.DiscNumber);
		Assert.AreEqual (4u, result.Tag.TotalDiscs);
	}

	#endregion

	#region Helper Methods

	static byte[] CreateTagWithTextFrame (string frameId, string text, byte version)
	{
		// Frame content: encoding (1) + text
		var textBytes = System.Text.Encoding.Latin1.GetBytes (text);
		var frameContent = new byte[1 + textBytes.Length];
		frameContent[0] = 0; // Latin-1 encoding
		Array.Copy (textBytes, 0, frameContent, 1, textBytes.Length);

		// Frame header (10 bytes) + content
		var frameSize = frameContent.Length;
		var frame = new byte[10 + frameSize];

		// Frame ID
		var idBytes = System.Text.Encoding.ASCII.GetBytes (frameId);
		Array.Copy (idBytes, 0, frame, 0, 4);

		// Size (big-endian for v2.3, syncsafe for v2.4)
		if (version == 4) {
			// Syncsafe
			frame[4] = (byte)((frameSize >> 21) & 0x7F);
			frame[5] = (byte)((frameSize >> 14) & 0x7F);
			frame[6] = (byte)((frameSize >> 7) & 0x7F);
			frame[7] = (byte)(frameSize & 0x7F);
		} else {
			// Big-endian
			frame[4] = (byte)((frameSize >> 24) & 0xFF);
			frame[5] = (byte)((frameSize >> 16) & 0xFF);
			frame[6] = (byte)((frameSize >> 8) & 0xFF);
			frame[7] = (byte)(frameSize & 0xFF);
		}

		// Flags (2 bytes, zeroes)
		frame[8] = 0;
		frame[9] = 0;

		// Content
		Array.Copy (frameContent, 0, frame, 10, frameSize);

		// Combine header + frame
		var totalSize = (uint)frame.Length;
		var header = CreateMinimalTag (version, totalSize);

		var result = new byte[header.Length + frame.Length];
		Array.Copy (header, result, header.Length);
		Array.Copy (frame, 0, result, header.Length, frame.Length);

		return result;
	}

	static byte[] CreateMinimalTag (byte version, uint size)
	{
		var data = new byte[10];
		data[0] = (byte)'I';
		data[1] = (byte)'D';
		data[2] = (byte)'3';
		data[3] = version;
		data[4] = 0; // revision
		data[5] = 0; // flags

		// Syncsafe size
		data[6] = (byte)((size >> 21) & 0x7F);
		data[7] = (byte)((size >> 14) & 0x7F);
		data[8] = (byte)((size >> 7) & 0x7F);
		data[9] = (byte)(size & 0x7F);

		return data;
	}

	#endregion
}
