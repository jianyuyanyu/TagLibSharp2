// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace TagLibSharp2.Core;

/// <summary>
/// Indicates the tag types present in a file.
/// </summary>
#pragma warning disable CA2217 // Do not mark enums with FlagsAttribute - AllTags is intentionally all bits set
[Flags]
public enum TagTypes
{
	/// <summary>
	/// No tags present.
	/// </summary>
	None = 0,

	/// <summary>
	/// ID3v1 tag.
	/// </summary>
	Id3v1 = 1 << 0,

	/// <summary>
	/// ID3v2 tag.
	/// </summary>
	Id3v2 = 1 << 1,

	/// <summary>
	/// APE tag.
	/// </summary>
	Ape = 1 << 2,

	/// <summary>
	/// Xiph Vorbis Comment (used in FLAC, Ogg Vorbis, Opus, etc.).
	/// </summary>
	Xiph = 1 << 3,

	/// <summary>
	/// Apple/iTunes metadata (MP4/M4A).
	/// </summary>
	Apple = 1 << 4,

	/// <summary>
	/// ASF/WMA metadata.
	/// </summary>
	Asf = 1 << 5,

	/// <summary>
	/// RIFF INFO chunk.
	/// </summary>
	RiffInfo = 1 << 6,

	/// <summary>
	/// Matroska tags.
	/// </summary>
	Matroska = 1 << 7,

	/// <summary>
	/// FLAC metadata blocks (pictures, etc.).
	/// </summary>
	FlacMetadata = 1 << 8,

	/// <summary>
	/// XMP metadata.
	/// </summary>
	Xmp = 1 << 9,

	/// <summary>
	/// All tag types.
	/// </summary>
	AllTags = unchecked((int)0xFFFFFFFF)
}
#pragma warning restore CA2217

/// <summary>
/// Abstract base class for all tag types (ID3v1, ID3v2, Vorbis Comments, etc.).
/// Provides common metadata properties with nullable support.
/// </summary>
public abstract class Tag
{
	/// <summary>
	/// Gets the type of tag represented by this instance.
	/// </summary>
	public abstract TagTypes TagType { get; }

	/// <summary>
	/// Gets or sets the title/song name.
	/// </summary>
	public abstract string? Title { get; set; }

	/// <summary>
	/// Gets or sets the primary artist/performer.
	/// </summary>
	/// <remarks>
	/// This returns the first performer. For multiple performers, use <see cref="Performers"/>.
	/// </remarks>
	public abstract string? Artist { get; set; }

	/// <summary>
	/// Gets or sets the performers/artists for this track.
	/// </summary>
	/// <remarks>
	/// This is the primary array of artists. The <see cref="Artist"/> property returns
	/// the first value from this array.
	/// </remarks>
#pragma warning disable CA1819 // Properties should not return arrays - TagLib# API compatibility
	public virtual string[] Performers {
		get => string.IsNullOrEmpty (Artist) ? [] : [Artist!];
		set => Artist = value is { Length: > 0 } ? value[0] : null;
	}
#pragma warning restore CA1819

	/// <summary>
	/// Gets or sets the album/collection name.
	/// </summary>
	public abstract string? Album { get; set; }

	/// <summary>
	/// Gets or sets the year of release.
	/// </summary>
	public abstract string? Year { get; set; }

	/// <summary>
	/// Gets or sets the original release date.
	/// </summary>
	/// <remarks>
	/// For reissues, remasters, or re-releases, this is the date of the original release.
	/// Format is typically YYYY or YYYY-MM-DD depending on tag format support.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? OriginalReleaseDate { get => null; set { } }

	/// <summary>
	/// Gets or sets the comment/description.
	/// </summary>
	public abstract string? Comment { get; set; }

	/// <summary>
	/// Gets or sets the genre name.
	/// </summary>
	/// <remarks>
	/// This returns the first genre. For multiple genres, use <see cref="Genres"/>.
	/// </remarks>
	public abstract string? Genre { get; set; }

	/// <summary>
	/// Gets or sets the genres for this track.
	/// </summary>
	/// <remarks>
	/// The <see cref="Genre"/> property returns the first value from this array.
	/// </remarks>
#pragma warning disable CA1819 // Properties should not return arrays - TagLib# API compatibility
	public virtual string[] Genres {
		get => string.IsNullOrEmpty (Genre) ? [] : [Genre!];
		set => Genre = value is { Length: > 0 } ? value[0] : null;
	}
#pragma warning restore CA1819

	/// <summary>
	/// Gets or sets the track number.
	/// </summary>
	public abstract uint? Track { get; set; }

	/// <summary>
	/// Gets or sets the album artist (for compilations/various artists albums).
	/// </summary>
	/// <remarks>
	/// This returns the first album artist. For multiple album artists, use <see cref="AlbumArtists"/>.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? AlbumArtist { get => null; set { } }

	/// <summary>
	/// Gets or sets the album artists for this track.
	/// </summary>
	/// <remarks>
	/// Used for compilations and various artists albums. The <see cref="AlbumArtist"/>
	/// property returns the first value from this array.
	/// Not all tag formats support this field.
	/// </remarks>
#pragma warning disable CA1819 // Properties should not return arrays - TagLib# API compatibility
	public virtual string[] AlbumArtists {
		get => string.IsNullOrEmpty (AlbumArtist) ? [] : [AlbumArtist!];
		set => AlbumArtist = value is { Length: > 0 } ? value[0] : null;
	}
#pragma warning restore CA1819

	/// <summary>
	/// Gets or sets the disc number.
	/// </summary>
	/// <remarks>
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual uint? DiscNumber { get => null; set { } }

	/// <summary>
	/// Gets or sets the composer.
	/// </summary>
	/// <remarks>
	/// This returns the first composer. For multiple composers, use <see cref="Composers"/>.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? Composer { get => null; set { } }

	/// <summary>
	/// Gets or sets the composers for this track.
	/// </summary>
	/// <remarks>
	/// The <see cref="Composer"/> property returns the first value from this array.
	/// Not all tag formats support this field.
	/// </remarks>
#pragma warning disable CA1819 // Properties should not return arrays - TagLib# API compatibility
	public virtual string[] Composers {
		get => string.IsNullOrEmpty (Composer) ? [] : [Composer!];
		set => Composer = value is { Length: > 0 } ? value[0] : null;
	}
#pragma warning restore CA1819

	/// <summary>
	/// Gets or sets the beats per minute (BPM) of the track.
	/// </summary>
	/// <remarks>
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual uint? BeatsPerMinute { get => null; set { } }

	/// <summary>
	/// Gets or sets the conductor or director.
	/// </summary>
	/// <remarks>
	/// For classical music, this is typically the orchestra conductor.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? Conductor { get => null; set { } }

	/// <summary>
	/// Gets or sets the copyright information.
	/// </summary>
	/// <remarks>
	/// Typically in the format "YYYY Label Name" (e.g., "2024 Acme Records").
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? Copyright { get => null; set { } }

	/// <summary>
	/// Gets or sets whether this track is part of a compilation album.
	/// </summary>
	/// <remarks>
	/// Used by music players to group various artist albums.
	/// Not all tag formats support this field. Default implementation returns false.
	/// </remarks>
	public virtual bool IsCompilation { get => false; set { } }

	/// <summary>
	/// Gets or sets the total number of tracks on the album/disc.
	/// </summary>
	/// <remarks>
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual uint? TotalTracks { get => null; set { } }

	/// <summary>
	/// Gets or sets the total number of discs in the set.
	/// </summary>
	/// <remarks>
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual uint? TotalDiscs { get => null; set { } }

	/// <summary>
	/// Gets or sets the lyrics or text content.
	/// </summary>
	/// <remarks>
	/// For song lyrics, poems, or other text content associated with the media.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? Lyrics { get => null; set { } }

	/// <summary>
	/// Gets or sets the ISRC (International Standard Recording Code).
	/// </summary>
	/// <remarks>
	/// A 12-character alphanumeric code that uniquely identifies sound recordings.
	/// Format: CC-XXX-YY-NNNNN (country, registrant, year, designation).
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? Isrc { get => null; set { } }

	/// <summary>
	/// Gets or sets the publisher or record label.
	/// </summary>
	/// <remarks>
	/// The name of the record label or publisher of this recording.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? Publisher { get => null; set { } }

	/// <summary>
	/// Gets or sets the person or organization that encoded the audio.
	/// </summary>
	/// <remarks>
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? EncodedBy { get => null; set { } }

	/// <summary>
	/// Gets or sets the encoder settings used to encode the audio.
	/// </summary>
	/// <remarks>
	/// Typically contains software name and encoding parameters (e.g., "LAME 320kbps CBR").
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? EncoderSettings { get => null; set { } }

	/// <summary>
	/// Gets or sets the content group or grouping for the track.
	/// </summary>
	/// <remarks>
	/// Used to group related tracks (e.g., "Summer Hits 2024", "Workout Mix").
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? Grouping { get => null; set { } }

	/// <summary>
	/// Gets or sets the subtitle or description refinement.
	/// </summary>
	/// <remarks>
	/// Additional information about the track (e.g., "Radio Edit", "Live Version").
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? Subtitle { get => null; set { } }

	/// <summary>
	/// Gets or sets the remixer or modifier of the track.
	/// </summary>
	/// <remarks>
	/// The artist who remixed or modified the original track.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? Remixer { get => null; set { } }

	/// <summary>
	/// Gets or sets the initial musical key of the track.
	/// </summary>
	/// <remarks>
	/// The key in which the track is performed (e.g., "Am", "F#m", "Cmaj").
	/// Useful for DJs for harmonic mixing.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? InitialKey { get => null; set { } }

	/// <summary>
	/// Gets or sets the mood of the track.
	/// </summary>
	/// <remarks>
	/// Describes the emotional character (e.g., "Energetic", "Melancholic", "Uplifting").
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? Mood { get => null; set { } }

	/// <summary>
	/// Gets or sets the original media type.
	/// </summary>
	/// <remarks>
	/// Describes the source media (e.g., "CD", "Vinyl", "DIG/A" for digital analog).
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? MediaType { get => null; set { } }

	/// <summary>
	/// Gets or sets the language of the audio content.
	/// </summary>
	/// <remarks>
	/// Typically an ISO 639-2 three-letter language code (e.g., "eng", "jpn", "deu").
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? Language { get => null; set { } }

	/// <summary>
	/// Gets or sets the barcode (UPC/EAN) of the release.
	/// </summary>
	/// <remarks>
	/// The Universal Product Code or European Article Number for the release.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? Barcode { get => null; set { } }

	/// <summary>
	/// Gets or sets the catalog number assigned by the label.
	/// </summary>
	/// <remarks>
	/// The label's catalog identifier for the release (e.g., "WPCR-80001").
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? CatalogNumber { get => null; set { } }

	// Sort Order properties

	/// <summary>
	/// Gets or sets the sort order for the album title.
	/// </summary>
	/// <remarks>
	/// Used for sorting when the display name should differ from the sort name.
	/// For example, "The White Album" might sort as "White Album, The".
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? AlbumSort { get => null; set { } }

	/// <summary>
	/// Gets or sets the sort order for the artist name.
	/// </summary>
	/// <remarks>
	/// Used for sorting when the display name should differ from the sort name.
	/// For example, "The Beatles" might sort as "Beatles, The".
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? ArtistSort { get => null; set { } }

	/// <summary>
	/// Gets or sets the sort order for the title.
	/// </summary>
	/// <remarks>
	/// Used for sorting when the display name should differ from the sort name.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? TitleSort { get => null; set { } }

	/// <summary>
	/// Gets or sets the sort order for the album artist name.
	/// </summary>
	/// <remarks>
	/// Used for sorting when the display name should differ from the sort name.
	/// For example, "Various Artists" compilations might sort under a specific letter.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? AlbumArtistSort { get => null; set { } }

	/// <summary>
	/// Gets or sets the sort names for the performers.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is a parallel array to the performers list. Each element provides the sort name
	/// for the corresponding performer at the same index. For example, if performers are
	/// ["The Beatles", "David Bowie"], PerformersSort might be ["Beatles, The", "Bowie, David"].
	/// </para>
	/// <para>
	/// Not all tag formats support multiple performer sort names.
	/// Default implementation returns null.
	/// </para>
	/// </remarks>
#pragma warning disable CA1819 // Properties should not return arrays - TagLib# API compatibility
	public virtual string[]? PerformersSort { get => null; set { } }
#pragma warning restore CA1819

	/// <summary>
	/// Gets or sets the sort names for the album artists.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is a parallel array to the album artists list. Each element provides the sort name
	/// for the corresponding album artist at the same index.
	/// </para>
	/// <para>
	/// Not all tag formats support multiple album artist sort names.
	/// Default implementation returns null.
	/// </para>
	/// </remarks>
#pragma warning disable CA1819 // Properties should not return arrays - TagLib# API compatibility
	public virtual string[]? AlbumArtistsSort { get => null; set { } }
#pragma warning restore CA1819

	/// <summary>
	/// Gets or sets the sort names for the composers.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is a parallel array to the composers list. Each element provides the sort name
	/// for the corresponding composer at the same index. For example, if composers are
	/// ["Johann Sebastian Bach"], ComposersSort might be ["Bach, Johann Sebastian"].
	/// </para>
	/// <para>
	/// Not all tag formats support multiple composer sort names.
	/// Default implementation returns null.
	/// </para>
	/// </remarks>
#pragma warning disable CA1819 // Properties should not return arrays - TagLib# API compatibility
	public virtual string[]? ComposersSort { get => null; set { } }
#pragma warning restore CA1819

	// ReplayGain properties

	/// <summary>
	/// Gets or sets the ReplayGain track gain value (e.g., "-6.50 dB").
	/// </summary>
	/// <remarks>
	/// ReplayGain values are stored as strings including the "dB" suffix.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? ReplayGainTrackGain { get => null; set { } }

	/// <summary>
	/// Gets or sets the ReplayGain track peak value (e.g., "0.988547").
	/// </summary>
	/// <remarks>
	/// Peak values are stored as decimal strings between 0.0 and 1.0.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? ReplayGainTrackPeak { get => null; set { } }

	/// <summary>
	/// Gets or sets the ReplayGain album gain value (e.g., "-6.50 dB").
	/// </summary>
	/// <remarks>
	/// ReplayGain values are stored as strings including the "dB" suffix.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? ReplayGainAlbumGain { get => null; set { } }

	/// <summary>
	/// Gets or sets the ReplayGain album peak value (e.g., "0.988547").
	/// </summary>
	/// <remarks>
	/// Peak values are stored as decimal strings between 0.0 and 1.0.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? ReplayGainAlbumPeak { get => null; set { } }

	// MusicBrainz IDs

	/// <summary>
	/// Gets or sets the MusicBrainz Track ID (Recording MBID).
	/// </summary>
	/// <remarks>
	/// A UUID identifying this specific recording in the MusicBrainz database.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? MusicBrainzTrackId { get => null; set { } }

	/// <summary>
	/// Gets or sets the MusicBrainz Release ID (Album MBID).
	/// </summary>
	/// <remarks>
	/// A UUID identifying this specific release in the MusicBrainz database.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? MusicBrainzReleaseId { get => null; set { } }

	/// <summary>
	/// Gets or sets the MusicBrainz Artist ID.
	/// </summary>
	/// <remarks>
	/// A UUID identifying the primary artist in the MusicBrainz database.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? MusicBrainzArtistId { get => null; set { } }

	/// <summary>
	/// Gets or sets the MusicBrainz Release Group ID.
	/// </summary>
	/// <remarks>
	/// A UUID identifying the release group (album across all releases) in the MusicBrainz database.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? MusicBrainzReleaseGroupId { get => null; set { } }

	/// <summary>
	/// Gets or sets the MusicBrainz Album Artist ID.
	/// </summary>
	/// <remarks>
	/// A UUID identifying the album artist in the MusicBrainz database.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? MusicBrainzAlbumArtistId { get => null; set { } }

	/// <summary>
	/// Gets or sets the MusicBrainz Release Artist ID.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is an alias for <see cref="MusicBrainzAlbumArtistId"/> for TagLib# compatibility.
	/// Both properties read/write the same underlying data.
	/// </para>
	/// <para>
	/// A UUID identifying the release artist in the MusicBrainz database.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </para>
	/// </remarks>
	public virtual string? MusicBrainzReleaseArtistId {
		get => MusicBrainzAlbumArtistId;
		set => MusicBrainzAlbumArtistId = value;
	}

	/// <summary>
	/// Gets or sets the MusicBrainz Recording ID.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A UUID identifying the specific recording in the MusicBrainz database.
	/// This differs from TrackId which identifies the track on a specific release.
	/// </para>
	/// <para>
	/// In ID3v2, this is stored in a UFID frame with owner "http://musicbrainz.org".
	/// In Vorbis Comments, this is stored as MUSICBRAINZ_TRACKID (same as TrackId).
	/// Not all tag formats support this field. Default implementation returns null.
	/// </para>
	/// </remarks>
	public virtual string? MusicBrainzRecordingId { get => null; set { } }

	/// <summary>
	/// Gets or sets the MusicBrainz Work ID.
	/// </summary>
	/// <remarks>
	/// A UUID identifying the musical work (composition) in the MusicBrainz database.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? MusicBrainzWorkId { get => null; set { } }

	/// <summary>
	/// Gets or sets the MusicBrainz Disc ID.
	/// </summary>
	/// <remarks>
	/// A hash identifying the CD table of contents in the MusicBrainz database.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? MusicBrainzDiscId { get => null; set { } }

	/// <summary>
	/// Gets or sets the MusicBrainz release status.
	/// </summary>
	/// <remarks>
	/// Values include: official, promotional, bootleg, pseudo-release.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? MusicBrainzReleaseStatus { get => null; set { } }

	/// <summary>
	/// Gets or sets the MusicBrainz release type.
	/// </summary>
	/// <remarks>
	/// Values include: album, single, ep, compilation, soundtrack, live, remix, etc.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? MusicBrainzReleaseType { get => null; set { } }

	/// <summary>
	/// Gets or sets the MusicBrainz release country.
	/// </summary>
	/// <remarks>
	/// ISO 3166-1 alpha-2 country code (e.g., "US", "GB", "JP").
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? MusicBrainzReleaseCountry { get => null; set { } }

	/// <summary>
	/// Gets or sets the sort order for the composer name.
	/// </summary>
	/// <remarks>
	/// Used for sorting when the display name should differ from the sort name.
	/// For example, "Johann Sebastian Bach" might sort as "Bach, Johann Sebastian".
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? ComposerSort { get => null; set { } }

	/// <summary>
	/// Gets or sets the date/time when the file was tagged.
	/// </summary>
	/// <remarks>
	/// Stored as an ISO 8601 formatted string (e.g., "2025-12-27" or "2025-12-27T10:30:00").
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? DateTagged { get => null; set { } }

	/// <summary>
	/// Gets or sets the description or synopsis of the content.
	/// </summary>
	/// <remarks>
	/// Used for story summaries, plot descriptions, or detailed content descriptions.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? Description { get => null; set { } }

	/// <summary>
	/// Gets or sets the Amazon Standard Identification Number (ASIN).
	/// </summary>
	/// <remarks>
	/// A 10-character alphanumeric identifier assigned by Amazon (e.g., "B000002UAL").
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	public virtual string? AmazonId { get => null; set { } }

	/// <summary>
	/// Gets or sets the MusicIP PUID (Portable Unique Identifier).
	/// </summary>
	/// <remarks>
	/// <b>Obsolete:</b> MusicIP service was discontinued. Use AcoustID fingerprints instead.
	/// This property is maintained for compatibility with legacy tagged files.
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
	[System.Obsolete ("MusicIP PUID is obsolete. MusicIP service was discontinued. Use AcoustID fingerprints instead.")]
	public virtual string? MusicIpId { get => null; set { } }

	/// <summary>
	/// Gets or sets the roles of the performers.
	/// </summary>
	/// <remarks>
	/// This is a parallel array to the performers (artists) list. Each element
	/// describes the role of the corresponding performer at the same index.
	/// For example, if Artists = ["John", "Jane"], PerformersRole might be ["vocals", "guitar"].
	/// Not all tag formats support this field. Default implementation returns null.
	/// </remarks>
#pragma warning disable CA1819 // Properties should not return arrays - TagLib# API compatibility
	public virtual string[]? PerformersRole { get => null; set { } }
#pragma warning restore CA1819

	/// <summary>
	/// Gets or sets the pictures/images attached to this media.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Returns album artwork, cover images, artist photos, etc.
	/// The first image of type <see cref="PictureType.FrontCover"/> is typically
	/// the primary album art.
	/// </para>
	/// <para>
	/// Not all tag formats support embedded images. Default implementation returns empty array.
	/// </para>
	/// </remarks>
#pragma warning disable CA1819 // Properties should not return arrays - TagLib# API compatibility
	public virtual IPicture[] Pictures { get => []; set { } }
#pragma warning restore CA1819

	/// <summary>
	/// Gets a value indicating whether all standard fields are empty or null.
	/// </summary>
	public virtual bool IsEmpty =>
		string.IsNullOrEmpty (Title) &&
		string.IsNullOrEmpty (Artist) &&
		string.IsNullOrEmpty (Album) &&
		string.IsNullOrEmpty (Year) &&
		string.IsNullOrEmpty (Comment) &&
		string.IsNullOrEmpty (Genre) &&
		Track is null;

	/// <summary>
	/// Serializes the tag to its binary representation.
	/// </summary>
	/// <returns>The binary representation of the tag.</returns>
	public abstract BinaryData Render ();

	/// <summary>
	/// Clears all tag data, resetting to an empty state.
	/// </summary>
	public abstract void Clear ();
}
