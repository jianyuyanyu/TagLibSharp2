// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Xiph;

namespace TagLibSharp2.Tests.Xiph;

/// <summary>
/// Tests for extended metadata properties in VorbisComment: Conductor, Copyright,
/// Compilation, ISRC, Publisher, and Lyrics.
/// </summary>
[TestClass]
[TestCategory ("Unit")]
[TestCategory ("Xiph")]
public class VorbisCommentExtendedMetadataTests
{
	// Conductor Tests

	[TestMethod]
	public void Conductor_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.Conductor = "Herbert von Karajan";

		Assert.AreEqual ("Herbert von Karajan", comment.Conductor);
		Assert.AreEqual ("Herbert von Karajan", comment.GetValue ("CONDUCTOR"));
	}

	[TestMethod]
	public void Conductor_SetNull_ClearsField ()
	{
		var comment = new VorbisComment ("test");
		comment.Conductor = "Herbert von Karajan";

		comment.Conductor = null;

		Assert.IsNull (comment.Conductor);
		Assert.IsNull (comment.GetValue ("CONDUCTOR"));
	}

	[TestMethod]
	public void Conductor_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") { Conductor = "Sir Simon Rattle" };

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("Sir Simon Rattle", result.Tag!.Conductor);
	}

	// Copyright Tests

	[TestMethod]
	public void Copyright_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.Copyright = "2024 Acme Records";

		Assert.AreEqual ("2024 Acme Records", comment.Copyright);
		Assert.AreEqual ("2024 Acme Records", comment.GetValue ("COPYRIGHT"));
	}

	[TestMethod]
	public void Copyright_SetNull_ClearsField ()
	{
		var comment = new VorbisComment ("test");
		comment.Copyright = "2024 Acme Records";

		comment.Copyright = null;

		Assert.IsNull (comment.Copyright);
		Assert.IsNull (comment.GetValue ("COPYRIGHT"));
	}

	[TestMethod]
	public void Copyright_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") { Copyright = "2025 Independent" };

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("2025 Independent", result.Tag!.Copyright);
	}

	// Compilation Tests

	[TestMethod]
	public void IsCompilation_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.IsCompilation = true;

		Assert.IsTrue (comment.IsCompilation);
		Assert.AreEqual ("1", comment.GetValue ("COMPILATION"));
	}

	[TestMethod]
	public void IsCompilation_SetFalse_ClearsField ()
	{
		var comment = new VorbisComment ("test");
		comment.IsCompilation = true;

		comment.IsCompilation = false;

		Assert.IsFalse (comment.IsCompilation);
		Assert.IsNull (comment.GetValue ("COMPILATION"));
	}

	[TestMethod]
	public void IsCompilation_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") { IsCompilation = true };

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.IsTrue (result.Tag!.IsCompilation);
	}

	// ISRC Tests

	[TestMethod]
	public void Isrc_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.Isrc = "USRC17607839";

		Assert.AreEqual ("USRC17607839", comment.Isrc);
		Assert.AreEqual ("USRC17607839", comment.GetValue ("ISRC"));
	}

	[TestMethod]
	public void Isrc_SetNull_ClearsField ()
	{
		var comment = new VorbisComment ("test");
		comment.Isrc = "USRC17607839";

		comment.Isrc = null;

		Assert.IsNull (comment.Isrc);
		Assert.IsNull (comment.GetValue ("ISRC"));
	}

	[TestMethod]
	public void Isrc_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") { Isrc = "GBAYE0000351" };

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("GBAYE0000351", result.Tag!.Isrc);
	}

	// Publisher Tests

	[TestMethod]
	public void Publisher_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.Publisher = "Sub Pop Records";

		Assert.AreEqual ("Sub Pop Records", comment.Publisher);
		Assert.AreEqual ("Sub Pop Records", comment.GetValue ("LABEL"));
	}

	[TestMethod]
	public void Publisher_SetNull_ClearsField ()
	{
		var comment = new VorbisComment ("test");
		comment.Publisher = "Sub Pop Records";

		comment.Publisher = null;

		Assert.IsNull (comment.Publisher);
		Assert.IsNull (comment.GetValue ("LABEL"));
	}

	[TestMethod]
	public void Publisher_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") { Publisher = "4AD Records" };

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("4AD Records", result.Tag!.Publisher);
	}

	// Lyrics Tests

	[TestMethod]
	public void Lyrics_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.Lyrics = "Hello world, these are the lyrics";

		Assert.AreEqual ("Hello world, these are the lyrics", comment.Lyrics);
		Assert.AreEqual ("Hello world, these are the lyrics", comment.GetValue ("LYRICS"));
	}

	[TestMethod]
	public void Lyrics_SetNull_ClearsField ()
	{
		var comment = new VorbisComment ("test");
		comment.Lyrics = "Some lyrics";

		comment.Lyrics = null;

		Assert.IsNull (comment.Lyrics);
		Assert.IsNull (comment.GetValue ("LYRICS"));
	}

	[TestMethod]
	public void Lyrics_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") {
			Lyrics = "Verse 1\nChorus\nVerse 2"
		};

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("Verse 1\nChorus\nVerse 2", result.Tag!.Lyrics);
	}

	[TestMethod]
	public void Lyrics_WithUnicode_PreservesValue ()
	{
		var original = new VorbisComment ("test") {
			Lyrics = "日本語の歌詞\n中文歌词\nКириллица"
		};

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("日本語の歌詞\n中文歌词\nКириллица", result.Tag!.Lyrics);
	}

	// TotalTracks Tests

	[TestMethod]
	public void TotalTracks_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.TotalTracks = 12;

		Assert.AreEqual (12u, comment.TotalTracks);
		Assert.AreEqual ("12", comment.GetValue ("TOTALTRACKS"));
	}

	[TestMethod]
	public void TotalTracks_FromTrackNumber_ParsesSlashFormat ()
	{
		var comment = new VorbisComment ("test");
		comment.AddField ("TRACKNUMBER", "5/12");

		Assert.AreEqual (5u, comment.Track);
		Assert.AreEqual (12u, comment.TotalTracks);
	}

	// TotalDiscs Tests

	[TestMethod]
	public void TotalDiscs_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.TotalDiscs = 3;

		Assert.AreEqual (3u, comment.TotalDiscs);
		Assert.AreEqual ("3", comment.GetValue ("TOTALDISCS"));
	}
}
