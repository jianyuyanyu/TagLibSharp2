// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Core;

namespace TagLibSharp2.Id3;

/// <summary>
/// Represents an ID3v1 or ID3v1.1 tag.
/// ID3v1 is a fixed 128-byte structure stored at the end of an audio file.
/// </summary>
public sealed class Id3v1Tag : Tag
{
	/// <summary>
	/// The fixed size of an ID3v1 tag in bytes.
	/// </summary>
	public const int TagSize = 128;

	/// <inheritdoc/>
	public override TagTypes TagType => TagTypes.Id3v1;

	// Field offsets
	const int TitleOffset = 3;
	const int ArtistOffset = 33;
	const int AlbumOffset = 63;
	const int YearOffset = 93;
	const int CommentOffset = 97;
	const int GenreOffset = 127;

	// Field sizes
	const int TitleSize = 30;
	const int ArtistSize = 30;
	const int AlbumSize = 30;
	const int YearSize = 4;
	const int CommentSize = 30;
	const int CommentSizeV11 = 28;

	// ID3v1.1 track number position
	const int TrackNullOffset = 125;
	const int TrackNumberOffset = 126;

	// Backing fields
	string? _title;
	string? _artist;
	string? _album;
	string? _year;
	string? _comment;
	byte _genreIndex = 255;
	uint? _track;

	/// <inheritdoc/>
	public override string? Title {
		get => _title;
		set => _title = value;
	}

	/// <inheritdoc/>
	public override string? Artist {
		get => _artist;
		set => _artist = value;
	}

	/// <inheritdoc/>
	public override string? Album {
		get => _album;
		set => _album = value;
	}

	/// <inheritdoc/>
	public override string? Year {
		get => _year;
		set => _year = value;
	}

	/// <inheritdoc/>
	public override string? Comment {
		get => _comment;
		set => _comment = value;
	}

	/// <inheritdoc/>
	public override string? Genre {
		get => Id3v1Genre.GetName (_genreIndex);
		set => _genreIndex = Id3v1Genre.GetIndex (value);
	}

	/// <inheritdoc/>
	public override uint? Track {
		get => _track;
		set => _track = value;
	}

	/// <summary>
	/// Gets or sets the raw genre byte index (0-255).
	/// </summary>
	public byte GenreIndex {
		get => _genreIndex;
		set => _genreIndex = value;
	}

	/// <summary>
	/// Gets a value indicating whether this tag uses ID3v1.1 format (has track number).
	/// </summary>
	public bool IsVersion11 => _track.HasValue;

	/// <summary>
	/// Attempts to parse an ID3v1 tag from the provided data.
	/// </summary>
	/// <param name="data">The data to parse (must be at least 128 bytes).</param>
	/// <returns>A result indicating success, failure, or not found.</returns>
	public static TagReadResult<Id3v1Tag> Read (ReadOnlySpan<byte> data)
	{
		if (data.Length < TagSize)
			return TagReadResult<Id3v1Tag>.Failure ("Data is too short for ID3v1 tag");

		// Check for "TAG" magic
		if (data[0] != 'T' || data[1] != 'A' || data[2] != 'G')
			return TagReadResult<Id3v1Tag>.NotFound ();

		var tag = new Id3v1Tag ();

		// Parse fixed-width fields
		tag._title = ReadField (data, TitleOffset, TitleSize);
		tag._artist = ReadField (data, ArtistOffset, ArtistSize);
		tag._album = ReadField (data, AlbumOffset, AlbumSize);
		tag._year = ReadField (data, YearOffset, YearSize);
		tag._genreIndex = data[GenreOffset];

		// Check for ID3v1.1 (track number)
		// If byte 125 is 0 and byte 126 is non-zero, it's v1.1
		if (data[TrackNullOffset] == 0 && data[TrackNumberOffset] != 0) {
			tag._comment = ReadField (data, CommentOffset, CommentSizeV11);
			tag._track = data[TrackNumberOffset];
		} else {
			tag._comment = ReadField (data, CommentOffset, CommentSize);
			tag._track = null;
		}

		return TagReadResult<Id3v1Tag>.Success (tag, TagSize);
	}

	/// <summary>
	/// Attempts to parse an ID3v1 tag from the provided byte array.
	/// </summary>
	/// <param name="data">The byte array to parse.</param>
	/// <returns>A result indicating success, failure, or not found.</returns>
	public static TagReadResult<Id3v1Tag> Read (byte[] data) =>
		Read (data.AsSpan ());

	/// <inheritdoc/>
	public override BinaryData Render ()
	{
		using var builder = new BinaryDataBuilder (TagSize);

		// Magic "TAG"
		builder.Add ((byte)'T');
		builder.Add ((byte)'A');
		builder.Add ((byte)'G');

		// Fixed-width fields
		WriteField (builder, _title, TitleSize);
		WriteField (builder, _artist, ArtistSize);
		WriteField (builder, _album, AlbumSize);
		WriteField (builder, _year, YearSize);

		// Comment field handling differs for v1.0 vs v1.1
		if (_track.HasValue) {
			// ID3v1.1: 28 bytes comment + null + track
			WriteField (builder, _comment, CommentSizeV11);
			builder.Add (0); // Null separator
			builder.Add ((byte)Math.Min (_track.Value, 255));
		} else {
			// ID3v1.0: 30 bytes comment
			WriteField (builder, _comment, CommentSize);
		}

		// Genre
		builder.Add (_genreIndex);

		return builder.ToBinaryData ();
	}

	/// <inheritdoc/>
	public override void Clear ()
	{
		_title = null;
		_artist = null;
		_album = null;
		_year = null;
		_comment = null;
		_genreIndex = 255;
		_track = null;
	}

	/// <summary>
	/// Reads a null-padded Latin-1 field and trims trailing nulls and spaces.
	/// </summary>
	static string ReadField (ReadOnlySpan<byte> data, int offset, int size)
	{
		var field = data.Slice (offset, size);

		// Find the end of actual content (trim nulls and spaces from end)
		var end = size;
		while (end > 0 && (field[end - 1] == 0 || field[end - 1] == ' '))
			end--;

		if (end == 0)
			return string.Empty;

		return new BinaryData (field[..end]).ToStringLatin1 ();
	}

	/// <summary>
	/// Writes a string to the builder, truncating or null-padding to the specified size.
	/// </summary>
	static void WriteField (BinaryDataBuilder builder, string? value, int size)
	{
		if (string.IsNullOrEmpty (value)) {
			builder.AddZeros (size);
			return;
		}

		var encoded = BinaryData.FromStringLatin1 (value!);
		if (encoded.Length >= size) {
			// Truncate to fit
			builder.Add (encoded.Span[..size]);
		} else {
			// Write value and pad with zeros
			builder.Add (encoded);
			builder.AddZeros (size - encoded.Length);
		}
	}
}
