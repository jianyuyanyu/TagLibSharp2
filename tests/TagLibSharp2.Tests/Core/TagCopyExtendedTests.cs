// Copyright (c) 2025 Stephen Shaw and contributors
// Extended CopyTo tests for additional coverage

using TagLibSharp2.Core;
using TagLibSharp2.Id3.Id3v2;
using TagLibSharp2.Id3.Id3v2.Frames;
using TagLibSharp2.Xiph;

namespace TagLibSharp2.Tests.Core;

[TestClass]
[TestCategory ("Unit")]
public class TagCopyExtendedTests
{
	[TestMethod]
	public void CopyTo_CopiesRemixer ()
	{
		var source = new Id3v2Tag { Remixer = "DJ Remix" };
		var target = new Id3v2Tag ();

		source.CopyTo (target);

		Assert.AreEqual ("DJ Remix", target.Remixer);
	}

	[TestMethod]
	public void CopyTo_CopiesMood ()
	{
		var source = new Id3v2Tag { Mood = "Energetic" };
		var target = new Id3v2Tag ();

		source.CopyTo (target);

		Assert.AreEqual ("Energetic", target.Mood);
	}

	[TestMethod]
	public void CopyTo_CopiesMediaType ()
	{
		var source = new Id3v2Tag { MediaType = "CD" };
		var target = new Id3v2Tag ();

		source.CopyTo (target);

		Assert.AreEqual ("CD", target.MediaType);
	}

	[TestMethod]
	public void CopyTo_CopiesLanguage ()
	{
		var source = new VorbisComment { Language = "eng" };
		var target = new VorbisComment ();

		source.CopyTo (target);

		Assert.AreEqual ("eng", target.Language);
	}

	[TestMethod]
	public void CopyTo_CopiesBarcode ()
	{
		var source = new VorbisComment { Barcode = "123456789012" };
		var target = new VorbisComment ();

		source.CopyTo (target);

		Assert.AreEqual ("123456789012", target.Barcode);
	}

	[TestMethod]
	public void CopyTo_CopiesCatalogNumber ()
	{
		var source = new VorbisComment { CatalogNumber = "WPCR-80001" };
		var target = new VorbisComment ();

		source.CopyTo (target);

		Assert.AreEqual ("WPCR-80001", target.CatalogNumber);
	}

	[TestMethod]
	public void CopyTo_CopiesEncodedBy ()
	{
		var source = new Id3v2Tag { EncodedBy = "Encoder Name" };
		var target = new Id3v2Tag ();

		source.CopyTo (target);

		Assert.AreEqual ("Encoder Name", target.EncodedBy);
	}

	[TestMethod]
	public void CopyTo_CopiesEncoderSettings ()
	{
		var source = new Id3v2Tag { EncoderSettings = "LAME 320kbps" };
		var target = new Id3v2Tag ();

		source.CopyTo (target);

		Assert.AreEqual ("LAME 320kbps", target.EncoderSettings);
	}

	[TestMethod]
	public void CopyTo_CopiesOriginalReleaseDate ()
	{
		var source = new VorbisComment { OriginalReleaseDate = "1985" };
		var target = new VorbisComment ();

		source.CopyTo (target);

		Assert.AreEqual ("1985", target.OriginalReleaseDate);
	}

	[TestMethod]
	public void CopyTo_CopiesDescription ()
	{
		var source = new VorbisComment { Description = "A great album" };
		var target = new VorbisComment ();

		source.CopyTo (target);

		Assert.AreEqual ("A great album", target.Description);
	}

	[TestMethod]
	public void CopyTo_CopiesDateTagged ()
	{
		var source = new VorbisComment { DateTagged = "2025-12-31" };
		var target = new VorbisComment ();

		source.CopyTo (target);

		Assert.AreEqual ("2025-12-31", target.DateTagged);
	}

	[TestMethod]
	public void CopyTo_CopiesAmazonId ()
	{
		var source = new VorbisComment { AmazonId = "B000002UAL" };
		var target = new VorbisComment ();

		source.CopyTo (target);

		Assert.AreEqual ("B000002UAL", target.AmazonId);
	}

	[TestMethod]
	public void CopyTo_CopiesClassicalMetadata ()
	{
		var source = new VorbisComment {
			Work = "Symphony No. 9",
			Movement = "Allegro con brio",
			MovementNumber = 1,
			MovementTotal = 4
		};
		var target = new VorbisComment ();

		source.CopyTo (target);

		Assert.AreEqual ("Symphony No. 9", target.Work);
		Assert.AreEqual ("Allegro con brio", target.Movement);
		Assert.AreEqual (1u, target.MovementNumber);
		Assert.AreEqual (4u, target.MovementTotal);
	}

	[TestMethod]
	public void CopyTo_CopiesPerformersRole ()
	{
		var source = new VorbisComment ();
		source.Performers = ["John", "Jane"];
		source.PerformersRole = ["vocals", "guitar"];
		var target = new VorbisComment ();

		source.CopyTo (target);

		Assert.HasCount (2, target.PerformersRole!);
		Assert.AreEqual ("vocals", target.PerformersRole![0]);
		Assert.AreEqual ("guitar", target.PerformersRole[1]);
	}

	[TestMethod]
	public void CopyTo_CopiesSortArrays ()
	{
		var source = new VorbisComment {
			Performers = ["The Beatles", "Bob Dylan"],
			AlbumArtists = ["Various Artists"],
			Composers = ["Lennon", "McCartney"]
		};
		source.PerformersSort = ["Beatles, The", "Dylan, Bob"];
		source.AlbumArtistsSort = ["Compilation"];
		source.ComposersSort = ["Lennon, John", "McCartney, Paul"];
		var target = new VorbisComment ();

		source.CopyTo (target);

		Assert.HasCount (2, target.PerformersSort!);
		Assert.HasCount (1, target.AlbumArtistsSort!);
		Assert.HasCount (2, target.ComposersSort!);
	}

	[TestMethod]
	public void CopyTo_CopiesComposerSort ()
	{
		var source = new VorbisComment { ComposerSort = "Bach, Johann Sebastian" };
		var target = new VorbisComment ();

		source.CopyTo (target);

		Assert.AreEqual ("Bach, Johann Sebastian", target.ComposerSort);
	}

	[TestMethod]
	public void CopyTo_CopiesR128Gains ()
	{
		var source = new VorbisComment {
			R128TrackGain = "-256",
			R128AlbumGain = "-512"
		};
		var target = new VorbisComment ();

		source.CopyTo (target);

		Assert.AreEqual ("-256", target.R128TrackGain);
		Assert.AreEqual ("-512", target.R128AlbumGain);
	}

	[TestMethod]
	public void CopyTo_CopiesRemainingMusicBrainzIds ()
	{
		var source = new VorbisComment {
			MusicBrainzRecordingId = "recording-123",
			MusicBrainzWorkId = "work-456",
			MusicBrainzDiscId = "disc-789",
			MusicBrainzReleaseCountry = "US"
		};
		var target = new VorbisComment ();

		source.CopyTo (target);

		Assert.AreEqual ("recording-123", target.MusicBrainzRecordingId);
		Assert.AreEqual ("work-456", target.MusicBrainzWorkId);
		Assert.AreEqual ("disc-789", target.MusicBrainzDiscId);
		Assert.AreEqual ("US", target.MusicBrainzReleaseCountry);
	}

	[TestMethod]
	public void CopyTo_CopiesAcoustId ()
	{
		var source = new VorbisComment {
			AcoustIdId = "acoustid-uuid",
			AcoustIdFingerprint = "AQAA..."
		};
		var target = new VorbisComment ();

		source.CopyTo (target);

		Assert.AreEqual ("acoustid-uuid", target.AcoustIdId);
		Assert.AreEqual ("AQAA...", target.AcoustIdFingerprint);
	}

	[TestMethod]
	public void CopyTo_ReplayGainOnly_OnlyCopiesReplayGain ()
	{
		var source = new VorbisComment {
			Title = "Test",
			ReplayGainTrackGain = "-6.5 dB",
			R128TrackGain = "-256"
		};
		var target = new VorbisComment ();

		source.CopyTo (target, TagCopyOptions.ReplayGain);

		Assert.IsNull (target.Title);
		Assert.AreEqual ("-6.5 dB", target.ReplayGainTrackGain);
		Assert.AreEqual ("-256", target.R128TrackGain);
	}

	[TestMethod]
	public void CopyTo_SortOrderOnly_OnlyCopiesSortOrder ()
	{
		var source = new Id3v2Tag {
			Title = "Test",
			TitleSort = "Test, The"
		};
		var target = new Id3v2Tag ();

		source.CopyTo (target, TagCopyOptions.SortOrder);

		Assert.IsNull (target.Title);
		Assert.AreEqual ("Test, The", target.TitleSort);
	}

	[TestMethod]
	public void CopyTo_ExtendedOnly_CopiesExtendedNotBasic ()
	{
		var source = new Id3v2Tag {
			Title = "Test", // Basic
			AlbumArtist = "Various" // Extended
		};
		var target = new Id3v2Tag ();

		source.CopyTo (target, TagCopyOptions.Extended);

		Assert.IsNull (target.Title);
		Assert.AreEqual ("Various", target.AlbumArtist);
	}

	[TestMethod]
	public void CopyTo_None_CopiesNothing ()
	{
		var source = new Id3v2Tag {
			Title = "Test",
			Artist = "Artist",
			Album = "Album",
			Year = "2024",
			Comment = "Comment",
			Genre = "Rock",
			Track = 5,
			AlbumArtist = "Various",
			TitleSort = "Sort Test",
			ReplayGainTrackGain = "-6.5 dB",
			MusicBrainzTrackId = "mb-123"
		};
		var target = new Id3v2Tag ();

		source.CopyTo (target, TagCopyOptions.None);

		Assert.IsNull (target.Title);
		Assert.IsNull (target.Artist);
		Assert.IsNull (target.Album);
		Assert.IsNull (target.TitleSort);
		Assert.IsNull (target.ReplayGainTrackGain);
		Assert.IsNull (target.MusicBrainzTrackId);
	}

	[TestMethod]
	public void CopyTo_BasicOnly_OnlyCopiesBasic ()
	{
		var source = new VorbisComment {
			Title = "Test",
			Artist = "Artist",
			Album = "Album",
			Year = "2024",
			Comment = "Comment",
			Genre = "Rock",
			Track = 5,
			AlbumArtist = "Various", // Extended
			TitleSort = "Sort Test" // SortOrder
		};
		var target = new VorbisComment ();

		source.CopyTo (target, TagCopyOptions.Basic);

		Assert.AreEqual ("Test", target.Title);
		Assert.AreEqual ("Artist", target.Artist);
		Assert.AreEqual ("Album", target.Album);
		Assert.AreEqual ("2024", target.Year);
		Assert.AreEqual ("Comment", target.Comment);
		Assert.AreEqual ("Rock", target.Genre);
		Assert.AreEqual (5u, target.Track);
		Assert.IsNull (target.AlbumArtist); // Extended not copied
		Assert.IsNull (target.TitleSort); // SortOrder not copied
	}

	[TestMethod]
	public void CopyTo_MusicBrainzOnly_CopiesMusicBrainzIds ()
	{
		var source = new VorbisComment {
			Title = "Test",
			MusicBrainzTrackId = "track-123",
			MusicBrainzReleaseId = "release-456",
			MusicBrainzArtistId = "artist-789",
			MusicBrainzReleaseGroupId = "rg-abc",
			MusicBrainzAlbumArtistId = "aa-def",
			MusicBrainzReleaseStatus = "official",
			MusicBrainzReleaseType = "album"
		};
		var target = new VorbisComment ();

		source.CopyTo (target, TagCopyOptions.MusicBrainz);

		Assert.IsNull (target.Title);
		Assert.AreEqual ("track-123", target.MusicBrainzTrackId);
		Assert.AreEqual ("release-456", target.MusicBrainzReleaseId);
		Assert.AreEqual ("artist-789", target.MusicBrainzArtistId);
		Assert.AreEqual ("rg-abc", target.MusicBrainzReleaseGroupId);
		Assert.AreEqual ("aa-def", target.MusicBrainzAlbumArtistId);
		Assert.AreEqual ("official", target.MusicBrainzReleaseStatus);
		Assert.AreEqual ("album", target.MusicBrainzReleaseType);
	}

	[TestMethod]
	public void CopyTo_NullTarget_ThrowsArgumentNullException ()
	{
		var source = new VorbisComment { Title = "Test" };

		Assert.ThrowsExactly<ArgumentNullException> (() => source.CopyTo (null!));
	}

	[TestMethod]
	public void CopyTo_CopiesIsCompilation ()
	{
		var source = new VorbisComment { IsCompilation = true };
		var target = new VorbisComment ();

		source.CopyTo (target);

		Assert.IsTrue (target.IsCompilation);
	}

	[TestMethod]
	public void CopyTo_IsCompilationFalse_DoesNotOverwrite ()
	{
		var source = new VorbisComment { IsCompilation = false };
		var target = new VorbisComment { IsCompilation = true };

		source.CopyTo (target);

		// False doesn't overwrite - only true is copied
		Assert.IsTrue (target.IsCompilation);
	}

	[TestMethod]
	public void CopyTo_PicturesOnly_OnlyCopiesPictures ()
	{
		var picture = new PictureFrame (
			"image/png",
			PictureType.FrontCover,
			"Cover",
			new BinaryData ([0x89, 0x50, 0x4E, 0x47]));
		var source = new VorbisComment {
			Title = "Test"
		};
		source.Pictures = [picture];
		var target = new VorbisComment ();

		source.CopyTo (target, TagCopyOptions.Pictures);

		Assert.IsNull (target.Title);
		Assert.HasCount (1, target.Pictures);
	}

	[TestMethod]
	public void Performers_SetNull_ClearsArtist ()
	{
		var tag = new VorbisComment { Artist = "Test" };

		tag.Performers = null!;

		Assert.IsNull (tag.Artist);
	}

	[TestMethod]
	public void Performers_SetEmpty_ClearsArtist ()
	{
		var tag = new VorbisComment { Artist = "Test" };

		tag.Performers = [];

		Assert.IsNull (tag.Artist);
	}

	[TestMethod]
	public void Genres_SetNull_ClearsGenre ()
	{
		var tag = new VorbisComment { Genre = "Rock" };

		tag.Genres = null!;

		Assert.IsNull (tag.Genre);
	}

	[TestMethod]
	public void Genres_SetEmpty_ClearsGenre ()
	{
		var tag = new VorbisComment { Genre = "Rock" };

		tag.Genres = [];

		Assert.IsNull (tag.Genre);
	}

	[TestMethod]
	public void AlbumArtists_SetNull_ClearsAlbumArtist ()
	{
		var tag = new VorbisComment { AlbumArtist = "Various" };

		tag.AlbumArtists = null!;

		Assert.IsNull (tag.AlbumArtist);
	}

	[TestMethod]
	public void Composers_SetNull_ClearsComposer ()
	{
		var tag = new VorbisComment { Composer = "Bach" };

		tag.Composers = null!;

		Assert.IsNull (tag.Composer);
	}
}
