// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Asf;
using TagLibSharp2.Core;

namespace TagLibSharp2.Tests.Asf;

[TestClass]
[TestCategory ("Unit")]
[TestCategory ("Asf")]
public class AsfTagTests
{
	// ═══════════════════════════════════════════════════════════════
	// Content Description Mappings
	// ═══════════════════════════════════════════════════════════════

	[TestMethod]
	public void Title_Get_ReturnsFromContentDescription ()
	{
		var content = new AsfContentDescription ("Test Title", "", "", "", "");
		var tag = new AsfTag (content, null);

		Assert.AreEqual ("Test Title", tag.Title);
	}

	[TestMethod]
	public void Title_Set_UpdatesContentDescription ()
	{
		var tag = new AsfTag ();
		tag.Title = "New Title";

		Assert.AreEqual ("New Title", tag.Title);
	}

	[TestMethod]
	public void Artist_Get_ReturnsFromAuthor ()
	{
		var content = new AsfContentDescription ("", "Test Artist", "", "", "");
		var tag = new AsfTag (content, null);

		Assert.AreEqual ("Test Artist", tag.Artist);
	}

	[TestMethod]
	public void Artist_Set_UpdatesAuthor ()
	{
		var tag = new AsfTag ();
		tag.Artist = "New Artist";

		Assert.AreEqual ("New Artist", tag.Artist);
	}

	[TestMethod]
	public void Copyright_Get_ReturnsFromContentDescription ()
	{
		var content = new AsfContentDescription ("", "", "2025 Test", "", "");
		var tag = new AsfTag (content, null);

		Assert.AreEqual ("2025 Test", tag.Copyright);
	}

	[TestMethod]
	public void Comment_Get_ReturnsFromContentDescription ()
	{
		var content = new AsfContentDescription ("", "", "", "A comment", "");
		var tag = new AsfTag (content, null);

		Assert.AreEqual ("A comment", tag.Comment);
	}

	// ═══════════════════════════════════════════════════════════════
	// Extended Attribute Mappings
	// ═══════════════════════════════════════════════════════════════

	[TestMethod]
	public void Album_Get_ReturnsFromWmAlbumTitle ()
	{
		var extended = new AsfExtendedContentDescription ([
			AsfDescriptor.CreateString ("WM/AlbumTitle", "Test Album")
		]);
		var tag = new AsfTag (null, extended);

		Assert.AreEqual ("Test Album", tag.Album);
	}

	[TestMethod]
	public void Album_Set_CreatesWmAlbumTitle ()
	{
		var tag = new AsfTag ();
		tag.Album = "New Album";

		Assert.AreEqual ("New Album", tag.Album);
	}

	[TestMethod]
	public void Year_Get_ReturnsFromWmYear ()
	{
		var extended = new AsfExtendedContentDescription ([
			AsfDescriptor.CreateString ("WM/Year", "2025")
		]);
		var tag = new AsfTag (null, extended);

		Assert.AreEqual ("2025", tag.Year);
	}

	[TestMethod]
	public void Year_Set_CreatesWmYear ()
	{
		var tag = new AsfTag ();
		tag.Year = "2025";

		Assert.AreEqual ("2025", tag.Year);
	}

	[TestMethod]
	public void Track_Get_FromDword_ReturnsValue ()
	{
		var extended = new AsfExtendedContentDescription ([
			AsfDescriptor.CreateDword ("WM/TrackNumber", 5)
		]);
		var tag = new AsfTag (null, extended);

		Assert.AreEqual (5u, tag.Track);
	}

	[TestMethod]
	public void Track_Get_FromString_ParsesValue ()
	{
		var extended = new AsfExtendedContentDescription ([
			AsfDescriptor.CreateString ("WM/TrackNumber", "7")
		]);
		var tag = new AsfTag (null, extended);

		Assert.AreEqual (7u, tag.Track);
	}

	[TestMethod]
	public void Track_Set_CreatesDword ()
	{
		var tag = new AsfTag ();
		tag.Track = 10;

		Assert.AreEqual (10u, tag.Track);
	}

	[TestMethod]
	public void Genre_Get_ReturnsFromWmGenre ()
	{
		var extended = new AsfExtendedContentDescription ([
			AsfDescriptor.CreateString ("WM/Genre", "Rock")
		]);
		var tag = new AsfTag (null, extended);

		Assert.AreEqual ("Rock", tag.Genre);
	}

	[TestMethod]
	public void Genre_Set_CreatesWmGenre ()
	{
		var tag = new AsfTag ();
		tag.Genre = "Jazz";

		Assert.AreEqual ("Jazz", tag.Genre);
	}

	[TestMethod]
	public void AlbumArtist_Get_ReturnsFromWmAlbumArtist ()
	{
		var extended = new AsfExtendedContentDescription ([
			AsfDescriptor.CreateString ("WM/AlbumArtist", "Various Artists")
		]);
		var tag = new AsfTag (null, extended);

		Assert.AreEqual ("Various Artists", tag.AlbumArtist);
	}

	[TestMethod]
	public void Composer_Get_ReturnsFromWmComposer ()
	{
		var extended = new AsfExtendedContentDescription ([
			AsfDescriptor.CreateString ("WM/Composer", "Bach")
		]);
		var tag = new AsfTag (null, extended);

		Assert.AreEqual ("Bach", tag.Composer);
	}

	[TestMethod]
	public void Conductor_Get_ReturnsFromWmConductor ()
	{
		var extended = new AsfExtendedContentDescription ([
			AsfDescriptor.CreateString ("WM/Conductor", "Karajan")
		]);
		var tag = new AsfTag (null, extended);

		Assert.AreEqual ("Karajan", tag.Conductor);
	}

	[TestMethod]
	public void DiscNumber_Get_FromPartOfSet_ReturnsFirst ()
	{
		var extended = new AsfExtendedContentDescription ([
			AsfDescriptor.CreateString ("WM/PartOfSet", "2/3")
		]);
		var tag = new AsfTag (null, extended);

		Assert.AreEqual (2u, tag.DiscNumber);
	}

	[TestMethod]
	public void DiscCount_Get_FromPartOfSet_ReturnsSecond ()
	{
		var extended = new AsfExtendedContentDescription ([
			AsfDescriptor.CreateString ("WM/PartOfSet", "2/3")
		]);
		var tag = new AsfTag (null, extended);

		Assert.AreEqual (3u, tag.DiscCount);
	}

	// ═══════════════════════════════════════════════════════════════
	// Clear Tests
	// ═══════════════════════════════════════════════════════════════

	[TestMethod]
	public void Clear_RemovesAllMetadata ()
	{
		var content = new AsfContentDescription ("Title", "Artist", "Copyright", "Comment", "Rating");
		var extended = new AsfExtendedContentDescription ([
			AsfDescriptor.CreateString ("WM/AlbumTitle", "Album"),
			AsfDescriptor.CreateString ("WM/Genre", "Rock")
		]);
		var tag = new AsfTag (content, extended);

		tag.Clear ();

		Assert.AreEqual ("", tag.Title);
		Assert.AreEqual ("", tag.Artist);
		Assert.AreEqual ("", tag.Album);
		Assert.AreEqual ("", tag.Genre);
	}

	// ═══════════════════════════════════════════════════════════════
	// Empty Tag Tests
	// ═══════════════════════════════════════════════════════════════

	[TestMethod]
	public void NewTag_AllPropertiesEmpty ()
	{
		var tag = new AsfTag ();

		Assert.AreEqual ("", tag.Title);
		Assert.AreEqual ("", tag.Artist);
		Assert.AreEqual ("", tag.Album);
		Assert.IsTrue (string.IsNullOrEmpty (tag.Year));
		Assert.IsNull (tag.Track);
		Assert.AreEqual ("", tag.Genre);
	}

	[TestMethod]
	public void IsEmpty_NewTag_ReturnsTrue ()
	{
		var tag = new AsfTag ();

		Assert.IsTrue (tag.IsEmpty);
	}

	[TestMethod]
	public void IsEmpty_WithTitle_ReturnsFalse ()
	{
		var tag = new AsfTag ();
		tag.Title = "Test";

		Assert.IsFalse (tag.IsEmpty);
	}

	// ═══════════════════════════════════════════════════════════════
	// Unicode Tests
	// ═══════════════════════════════════════════════════════════════

	[TestMethod]
	public void Unicode_Title_Preserved ()
	{
		var tag = new AsfTag ();
		tag.Title = "日本語タイトル";

		Assert.AreEqual ("日本語タイトル", tag.Title);
	}

	[TestMethod]
	public void Unicode_Album_Preserved ()
	{
		var tag = new AsfTag ();
		tag.Album = "中文专辑";

		Assert.AreEqual ("中文专辑", tag.Album);
	}

	// ═══════════════════════════════════════════════════════════════
	// Content Description Setter Tests
	// ═══════════════════════════════════════════════════════════════

	[TestMethod]
	public void Copyright_Set_UpdatesContentDescription ()
	{
		var tag = new AsfTag ();
		tag.Copyright = "2025 Test";

		Assert.AreEqual ("2025 Test", tag.Copyright);
	}

	[TestMethod]
	public void Copyright_SetNull_SetsEmpty ()
	{
		var tag = new AsfTag ();
		tag.Copyright = "Test";
		tag.Copyright = null;

		Assert.AreEqual ("", tag.Copyright);
	}

	[TestMethod]
	public void Comment_Set_UpdatesContentDescription ()
	{
		var tag = new AsfTag ();
		tag.Comment = "A comment";

		Assert.AreEqual ("A comment", tag.Comment);
	}

	[TestMethod]
	public void Comment_SetNull_SetsEmpty ()
	{
		var tag = new AsfTag ();
		tag.Comment = "Test";
		tag.Comment = null;

		Assert.AreEqual ("", tag.Comment);
	}

	[TestMethod]
	public void Rating_GetSet_WorksCorrectly ()
	{
		var tag = new AsfTag ();
		tag.Rating = "5 stars";

		Assert.AreEqual ("5 stars", tag.Rating);
	}

	[TestMethod]
	public void Rating_SetNull_SetsEmpty ()
	{
		var tag = new AsfTag ();
		tag.Rating = "Test";
		tag.Rating = null!;

		Assert.AreEqual ("", tag.Rating);
	}

	// ═══════════════════════════════════════════════════════════════
	// Extended Metadata Setter Tests
	// ═══════════════════════════════════════════════════════════════

	[TestMethod]
	public void AlbumArtist_Set_UpdatesDescriptor ()
	{
		var tag = new AsfTag ();
		tag.AlbumArtist = "Various Artists";

		Assert.AreEqual ("Various Artists", tag.AlbumArtist);
	}

	[TestMethod]
	public void AlbumArtist_SetNull_RemovesDescriptor ()
	{
		var tag = new AsfTag ();
		tag.AlbumArtist = "Test";
		tag.AlbumArtist = null;

		Assert.IsNull (tag.AlbumArtist);
	}

	[TestMethod]
	public void Composer_Set_UpdatesDescriptor ()
	{
		var tag = new AsfTag ();
		tag.Composer = "Bach";

		Assert.AreEqual ("Bach", tag.Composer);
	}

	[TestMethod]
	public void Conductor_Set_UpdatesDescriptor ()
	{
		var tag = new AsfTag ();
		tag.Conductor = "Karajan";

		Assert.AreEqual ("Karajan", tag.Conductor);
	}

	[TestMethod]
	public void Publisher_GetSet_WorksCorrectly ()
	{
		var tag = new AsfTag ();
		tag.Publisher = "Sony Music";

		Assert.AreEqual ("Sony Music", tag.Publisher);
	}

	[TestMethod]
	public void Lyrics_GetSet_WorksCorrectly ()
	{
		var tag = new AsfTag ();
		tag.Lyrics = "These are the lyrics\nSecond line";

		Assert.AreEqual ("These are the lyrics\nSecond line", tag.Lyrics);
	}

	[TestMethod]
	public void Isrc_GetSet_WorksCorrectly ()
	{
		var tag = new AsfTag ();
		tag.Isrc = "USRC17607839";

		Assert.AreEqual ("USRC17607839", tag.Isrc);
	}

	// ═══════════════════════════════════════════════════════════════
	// Disc/BPM Tests
	// ═══════════════════════════════════════════════════════════════

	[TestMethod]
	public void DiscNumber_Set_CreatesPartOfSet ()
	{
		var tag = new AsfTag ();
		tag.DiscNumber = 2;

		Assert.AreEqual (2u, tag.DiscNumber);
	}

	[TestMethod]
	public void TotalDiscs_Set_UpdatesPartOfSet ()
	{
		var tag = new AsfTag ();
		tag.TotalDiscs = 5;

		Assert.AreEqual (5u, tag.TotalDiscs);
	}

	[TestMethod]
	public void DiscNumber_SetWithExistingCount_PreservesCount ()
	{
		var tag = new AsfTag ();
		tag.TotalDiscs = 5;
		tag.DiscNumber = 2;

		Assert.AreEqual (2u, tag.DiscNumber);
		Assert.AreEqual (5u, tag.TotalDiscs);
	}

	[TestMethod]
	public void TotalDiscs_SetWithExistingDisc_PreservesDisc ()
	{
		var tag = new AsfTag ();
		tag.DiscNumber = 2;
		tag.TotalDiscs = 5;

		Assert.AreEqual (2u, tag.DiscNumber);
		Assert.AreEqual (5u, tag.TotalDiscs);
	}

	[TestMethod]
	public void DiscNumber_SetNull_RemovesIfNoCount ()
	{
		var tag = new AsfTag ();
		tag.DiscNumber = 2;
		tag.DiscNumber = null;

		Assert.IsNull (tag.DiscNumber);
	}

	[TestMethod]
	public void BeatsPerMinute_GetSet_WorksCorrectly ()
	{
		var tag = new AsfTag ();
		tag.BeatsPerMinute = 120;

		Assert.AreEqual (120u, tag.BeatsPerMinute);
	}

	[TestMethod]
	public void BeatsPerMinute_GetFromString_ParsesValue ()
	{
		var extended = new AsfExtendedContentDescription ([
			AsfDescriptor.CreateString ("WM/BeatsPerMinute", "128")
		]);
		var tag = new AsfTag (null, extended);

		Assert.AreEqual (128u, tag.BeatsPerMinute);
	}

	[TestMethod]
	public void BeatsPerMinute_SetNull_RemovesDescriptor ()
	{
		var tag = new AsfTag ();
		tag.BeatsPerMinute = 120;
		tag.BeatsPerMinute = null;

		Assert.IsNull (tag.BeatsPerMinute);
	}

	[TestMethod]
	public void Track_SetNull_RemovesDescriptor ()
	{
		var tag = new AsfTag ();
		tag.Track = 5;
		tag.Track = null;

		Assert.IsNull (tag.Track);
	}

	// ═══════════════════════════════════════════════════════════════
	// Boolean & Compilation Tests
	// ═══════════════════════════════════════════════════════════════

	[TestMethod]
	public void IsCompilation_GetSet_WorksCorrectly ()
	{
		var tag = new AsfTag ();
		Assert.IsFalse (tag.IsCompilation);

		tag.IsCompilation = true;
		Assert.IsTrue (tag.IsCompilation);

		tag.IsCompilation = false;
		Assert.IsFalse (tag.IsCompilation);
	}

	// ═══════════════════════════════════════════════════════════════
	// MusicBrainz ID Tests
	// ═══════════════════════════════════════════════════════════════

	[TestMethod]
	public void MusicBrainzTrackId_GetSet_WorksCorrectly ()
	{
		var tag = new AsfTag ();
		tag.MusicBrainzTrackId = "12345678-1234-1234-1234-123456789012";

		Assert.AreEqual ("12345678-1234-1234-1234-123456789012", tag.MusicBrainzTrackId);
	}

	[TestMethod]
	public void MusicBrainzReleaseId_GetSet_WorksCorrectly ()
	{
		var tag = new AsfTag ();
		tag.MusicBrainzReleaseId = "abcdef12-1234-1234-1234-123456789012";

		Assert.AreEqual ("abcdef12-1234-1234-1234-123456789012", tag.MusicBrainzReleaseId);
	}

	[TestMethod]
	public void MusicBrainzArtistId_GetSet_WorksCorrectly ()
	{
		var tag = new AsfTag ();
		tag.MusicBrainzArtistId = "11111111-1111-1111-1111-111111111111";

		Assert.AreEqual ("11111111-1111-1111-1111-111111111111", tag.MusicBrainzArtistId);
	}

	[TestMethod]
	public void MusicBrainzAlbumArtistId_GetSet_WorksCorrectly ()
	{
		var tag = new AsfTag ();
		tag.MusicBrainzAlbumArtistId = "22222222-2222-2222-2222-222222222222";

		Assert.AreEqual ("22222222-2222-2222-2222-222222222222", tag.MusicBrainzAlbumArtistId);
	}

	// ═══════════════════════════════════════════════════════════════
	// ReplayGain Tests
	// ═══════════════════════════════════════════════════════════════

	[TestMethod]
	public void ReplayGainTrackGain_GetSet_WorksCorrectly ()
	{
		var tag = new AsfTag ();
		tag.ReplayGainTrackGain = "-5.50 dB";

		Assert.AreEqual ("-5.50 dB", tag.ReplayGainTrackGain);
	}

	[TestMethod]
	public void ReplayGainTrackPeak_GetSet_WorksCorrectly ()
	{
		var tag = new AsfTag ();
		tag.ReplayGainTrackPeak = "0.988312";

		Assert.AreEqual ("0.988312", tag.ReplayGainTrackPeak);
	}

	[TestMethod]
	public void ReplayGainAlbumGain_GetSet_WorksCorrectly ()
	{
		var tag = new AsfTag ();
		tag.ReplayGainAlbumGain = "-6.20 dB";

		Assert.AreEqual ("-6.20 dB", tag.ReplayGainAlbumGain);
	}

	[TestMethod]
	public void ReplayGainAlbumPeak_GetSet_WorksCorrectly ()
	{
		var tag = new AsfTag ();
		tag.ReplayGainAlbumPeak = "0.995123";

		Assert.AreEqual ("0.995123", tag.ReplayGainAlbumPeak);
	}

	// ═══════════════════════════════════════════════════════════════
	// Render Tests
	// ═══════════════════════════════════════════════════════════════

	[TestMethod]
	public void Render_EmptyTag_ReturnsValidData ()
	{
		var tag = new AsfTag ();
		var rendered = tag.Render ();

		Assert.IsNotNull (rendered);
		// Render returns only the descriptor content (2-byte count for empty tag)
		// The GUID + size header is added when embedding in a file
		Assert.IsTrue (rendered.Length >= 2);
	}

	[TestMethod]
	public void Render_WithMetadata_ReturnsValidData ()
	{
		var tag = new AsfTag ();
		tag.Album = "Test Album";
		tag.Genre = "Rock";
		tag.Track = 5;

		var rendered = tag.Render ();

		Assert.IsNotNull (rendered);
		Assert.IsTrue (rendered.Length > 50);
	}

	[TestMethod]
	public void RenderContentDescription_ReturnsValidData ()
	{
		var tag = new AsfTag ();
		tag.Title = "Test Title";
		tag.Artist = "Test Artist";

		var rendered = tag.RenderContentDescription ();

		Assert.IsNotNull (rendered);
		Assert.IsTrue (rendered.Length >= 24); // GUID (16) + size (8) minimum
	}

	[TestMethod]
	public void Render_RoundTrip_PreservesMetadata ()
	{
		var tag = new AsfTag ();
		tag.Album = "Test Album";
		tag.Genre = "Rock";
		tag.Track = 7;
		tag.AlbumArtist = "Various Artists";
		tag.BeatsPerMinute = 120;

		var rendered = tag.Render ();
		var result = AsfExtendedContentDescription.Parse (rendered.Span);
		Assert.IsTrue (result.IsSuccess);

		var newTag = new AsfTag (null, result.Value);
		Assert.AreEqual ("Test Album", newTag.Album);
		Assert.AreEqual ("Rock", newTag.Genre);
		Assert.AreEqual (7u, newTag.Track);
		Assert.AreEqual ("Various Artists", newTag.AlbumArtist);
		Assert.AreEqual (120u, newTag.BeatsPerMinute);
	}

	[TestMethod]
	public void RenderContentDescription_RoundTrip_PreservesMetadata ()
	{
		var tag = new AsfTag ();
		tag.Title = "Test Title";
		tag.Artist = "Test Artist";
		tag.Copyright = "2025";
		tag.Comment = "A comment";
		tag.Rating = "5 stars";

		var rendered = tag.RenderContentDescription ();
		var result = AsfContentDescription.Parse (rendered.Span);
		Assert.IsTrue (result.IsSuccess);

		var newTag = new AsfTag (result.Value, null);
		Assert.AreEqual ("Test Title", newTag.Title);
		Assert.AreEqual ("Test Artist", newTag.Artist);
		Assert.AreEqual ("2025", newTag.Copyright);
		Assert.AreEqual ("A comment", newTag.Comment);
		Assert.AreEqual ("5 stars", newTag.Rating);
	}

	// ═══════════════════════════════════════════════════════════════
	// Property Accessor Tests
	// ═══════════════════════════════════════════════════════════════

	[TestMethod]
	public void ContentDescription_Get_ReturnsObject ()
	{
		var content = new AsfContentDescription ("Title", "Artist", "", "", "");
		var tag = new AsfTag (content, null);

		var retrieved = tag.ContentDescription;

		Assert.AreEqual ("Title", retrieved.Title);
		Assert.AreEqual ("Artist", retrieved.Author);
	}

	[TestMethod]
	public void ExtendedContentDescription_Get_ReturnsObject ()
	{
		var extended = new AsfExtendedContentDescription ([
			AsfDescriptor.CreateString ("WM/AlbumTitle", "Album"),
			AsfDescriptor.CreateString ("WM/Genre", "Rock")
		]);
		var tag = new AsfTag (null, extended);

		var retrieved = tag.ExtendedContentDescription;
		var descriptorList = retrieved.Descriptors.ToList ();

		Assert.AreEqual (2, descriptorList.Count);
	}

	// ═══════════════════════════════════════════════════════════════
	// Edge Case Tests
	// ═══════════════════════════════════════════════════════════════

	[TestMethod]
	public void Track_GetFromInvalidString_ReturnsNull ()
	{
		var extended = new AsfExtendedContentDescription ([
			AsfDescriptor.CreateString ("WM/TrackNumber", "not a number")
		]);
		var tag = new AsfTag (null, extended);

		Assert.IsNull (tag.Track);
	}

	[TestMethod]
	public void BeatsPerMinute_GetFromInvalidString_ReturnsNull ()
	{
		var extended = new AsfExtendedContentDescription ([
			AsfDescriptor.CreateString ("WM/BeatsPerMinute", "fast")
		]);
		var tag = new AsfTag (null, extended);

		Assert.IsNull (tag.BeatsPerMinute);
	}

	[TestMethod]
	public void PartOfSet_SingleNumber_ParsesAsDisc ()
	{
		var extended = new AsfExtendedContentDescription ([
			AsfDescriptor.CreateString ("WM/PartOfSet", "3")
		]);
		var tag = new AsfTag (null, extended);

		Assert.AreEqual (3u, tag.DiscNumber);
		Assert.IsNull (tag.DiscCount);
	}

	[TestMethod]
	public void PartOfSet_InvalidFormat_ReturnsNull ()
	{
		var extended = new AsfExtendedContentDescription ([
			AsfDescriptor.CreateString ("WM/PartOfSet", "abc/def")
		]);
		var tag = new AsfTag (null, extended);

		Assert.IsNull (tag.DiscNumber);
		Assert.IsNull (tag.DiscCount);
	}

	[TestMethod]
	public void TagType_ReturnsAsf ()
	{
		var tag = new AsfTag ();

		Assert.AreEqual (TagTypes.Asf, tag.TagType);
	}
}
