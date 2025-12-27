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

	// Sort Tags Tests

	[TestMethod]
	public void AlbumSort_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.AlbumSort = "White Album, The";

		Assert.AreEqual ("White Album, The", comment.AlbumSort);
		Assert.AreEqual ("White Album, The", comment.GetValue ("ALBUMSORT"));
	}

	[TestMethod]
	public void AlbumSort_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") { AlbumSort = "Abbey Road" };

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("Abbey Road", result.Tag!.AlbumSort);
	}

	[TestMethod]
	public void ArtistSort_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.ArtistSort = "Beatles, The";

		Assert.AreEqual ("Beatles, The", comment.ArtistSort);
		Assert.AreEqual ("Beatles, The", comment.GetValue ("ARTISTSORT"));
	}

	[TestMethod]
	public void ArtistSort_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") { ArtistSort = "Radiohead" };

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("Radiohead", result.Tag!.ArtistSort);
	}

	[TestMethod]
	public void TitleSort_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.TitleSort = "Love Is All You Need";

		Assert.AreEqual ("Love Is All You Need", comment.TitleSort);
		Assert.AreEqual ("Love Is All You Need", comment.GetValue ("TITLESORT"));
	}

	[TestMethod]
	public void TitleSort_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") { TitleSort = "Yesterday" };

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("Yesterday", result.Tag!.TitleSort);
	}

	[TestMethod]
	public void AlbumArtistSort_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.AlbumArtistSort = "Various";

		Assert.AreEqual ("Various", comment.AlbumArtistSort);
		Assert.AreEqual ("Various", comment.GetValue ("ALBUMARTISTSORT"));
	}

	[TestMethod]
	public void AlbumArtistSort_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") { AlbumArtistSort = "Compilation Artists" };

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("Compilation Artists", result.Tag!.AlbumArtistSort);
	}

	// Original Release Date (ORIGINALDATE/ORIGINALYEAR) Tests

	[TestMethod]
	public void OriginalReleaseDate_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.OriginalReleaseDate = "1969-09-26";

		Assert.AreEqual ("1969-09-26", comment.OriginalReleaseDate);
		Assert.AreEqual ("1969-09-26", comment.GetValue ("ORIGINALDATE"));
	}

	[TestMethod]
	public void OriginalReleaseDate_FallsBackToOriginalYear ()
	{
		var comment = new VorbisComment ("test");
		// Only set ORIGINALYEAR
		comment.AddField ("ORIGINALYEAR", "1969");

		Assert.AreEqual ("1969", comment.OriginalReleaseDate);
	}

	[TestMethod]
	public void OriginalReleaseDate_PrefersOriginalDateOverYear ()
	{
		var comment = new VorbisComment ("test");
		// Set both
		comment.AddField ("ORIGINALDATE", "1969-09-26");
		comment.AddField ("ORIGINALYEAR", "1969");

		Assert.AreEqual ("1969-09-26", comment.OriginalReleaseDate);
	}

	[TestMethod]
	public void OriginalReleaseDate_SetNull_ClearsField ()
	{
		var comment = new VorbisComment ("test");
		comment.OriginalReleaseDate = "1969-09-26";

		comment.OriginalReleaseDate = null;

		Assert.IsNull (comment.OriginalReleaseDate);
		Assert.IsNull (comment.GetValue ("ORIGINALDATE"));
	}

	[TestMethod]
	public void OriginalReleaseDate_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") { OriginalReleaseDate = "1969-09-26" };

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("1969-09-26", result.Tag!.OriginalReleaseDate);
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

	// EncodedBy (ENCODED-BY) Tests

	[TestMethod]
	public void EncodedBy_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.EncodedBy = "LAME 3.100";

		Assert.AreEqual ("LAME 3.100", comment.EncodedBy);
		Assert.AreEqual ("LAME 3.100", comment.GetValue ("ENCODED-BY"));
	}

	[TestMethod]
	public void EncodedBy_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") { EncodedBy = "flac 1.4.0" };

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("flac 1.4.0", result.Tag!.EncodedBy);
	}

	// EncoderSettings (ENCODER) Tests

	[TestMethod]
	public void EncoderSettings_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.EncoderSettings = "Lavf58.29.100";

		Assert.AreEqual ("Lavf58.29.100", comment.EncoderSettings);
		Assert.AreEqual ("Lavf58.29.100", comment.GetValue ("ENCODER"));
	}

	[TestMethod]
	public void EncoderSettings_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") { EncoderSettings = "libFLAC 1.3.2" };

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("libFLAC 1.3.2", result.Tag!.EncoderSettings);
	}

	// Grouping Tests

	[TestMethod]
	public void Grouping_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.Grouping = "Summer Hits 2024";

		Assert.AreEqual ("Summer Hits 2024", comment.Grouping);
		Assert.AreEqual ("Summer Hits 2024", comment.GetValue ("GROUPING"));
	}

	[TestMethod]
	public void Grouping_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") { Grouping = "Workout Mix" };

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("Workout Mix", result.Tag!.Grouping);
	}

	// Subtitle Tests

	[TestMethod]
	public void Subtitle_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.Subtitle = "Radio Edit";

		Assert.AreEqual ("Radio Edit", comment.Subtitle);
		Assert.AreEqual ("Radio Edit", comment.GetValue ("SUBTITLE"));
	}

	[TestMethod]
	public void Subtitle_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") { Subtitle = "Extended Mix" };

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("Extended Mix", result.Tag!.Subtitle);
	}

	// Remixer Tests

	[TestMethod]
	public void Remixer_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.Remixer = "Tiësto";

		Assert.AreEqual ("Tiësto", comment.Remixer);
		Assert.AreEqual ("Tiësto", comment.GetValue ("REMIXER"));
	}

	[TestMethod]
	public void Remixer_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") { Remixer = "David Guetta" };

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("David Guetta", result.Tag!.Remixer);
	}

	// InitialKey (KEY) Tests

	[TestMethod]
	public void InitialKey_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.InitialKey = "Am";

		Assert.AreEqual ("Am", comment.InitialKey);
		Assert.AreEqual ("Am", comment.GetValue ("KEY"));
	}

	[TestMethod]
	public void InitialKey_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") { InitialKey = "F#m" };

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("F#m", result.Tag!.InitialKey);
	}

	// Mood Tests

	[TestMethod]
	public void Mood_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.Mood = "Energetic";

		Assert.AreEqual ("Energetic", comment.Mood);
		Assert.AreEqual ("Energetic", comment.GetValue ("MOOD"));
	}

	[TestMethod]
	public void Mood_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") { Mood = "Melancholic" };

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("Melancholic", result.Tag!.Mood);
	}

	// MediaType (MEDIA) Tests

	[TestMethod]
	public void MediaType_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.MediaType = "CD";

		Assert.AreEqual ("CD", comment.MediaType);
		Assert.AreEqual ("CD", comment.GetValue ("MEDIA"));
	}

	[TestMethod]
	public void MediaType_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") { MediaType = "Vinyl" };

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("Vinyl", result.Tag!.MediaType);
	}

	// Language Tests

	[TestMethod]
	public void Language_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.Language = "eng";

		Assert.AreEqual ("eng", comment.Language);
		Assert.AreEqual ("eng", comment.GetValue ("LANGUAGE"));
	}

	[TestMethod]
	public void Language_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") { Language = "jpn" };

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("jpn", result.Tag!.Language);
	}

	// Barcode Tests

	[TestMethod]
	public void Barcode_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.Barcode = "012345678901";

		Assert.AreEqual ("012345678901", comment.Barcode);
		Assert.AreEqual ("012345678901", comment.GetValue ("BARCODE"));
	}

	[TestMethod]
	public void Barcode_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") { Barcode = "5099749534728" };

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("5099749534728", result.Tag!.Barcode);
	}

	// CatalogNumber Tests

	[TestMethod]
	public void CatalogNumber_GetSet_Works ()
	{
		var comment = new VorbisComment ("test");

		comment.CatalogNumber = "WPCR-80001";

		Assert.AreEqual ("WPCR-80001", comment.CatalogNumber);
		Assert.AreEqual ("WPCR-80001", comment.GetValue ("CATALOGNUMBER"));
	}

	[TestMethod]
	public void CatalogNumber_RoundTrip_PreservesValue ()
	{
		var original = new VorbisComment ("test") { CatalogNumber = "ECM 1064/65" };

		var rendered = original.Render ();
		var result = VorbisComment.Read (rendered.Span);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("ECM 1064/65", result.Tag!.CatalogNumber);
	}
}
