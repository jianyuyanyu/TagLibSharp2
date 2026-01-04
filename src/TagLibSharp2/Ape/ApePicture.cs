// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.IO;

using TagLibSharp2.Core;

namespace TagLibSharp2.Ape;

/// <summary>
/// Represents a picture stored in an APE tag via Cover Art keys.
/// </summary>
/// <remarks>
/// <para>
/// APE tag binary format: filename + null byte + picture data
/// </para>
/// <para>
/// Picture type is determined by the key name:
/// </para>
/// <list type="bullet">
/// <item>"Cover Art (Front)" → FrontCover</item>
/// <item>"Cover Art (Back)" → BackCover</item>
/// <item>"Cover Art (Media)" → Media</item>
/// <item>"Cover Art (Artist)" → Artist</item>
/// <item>Other "Cover Art (*)" → Other</item>
/// </list>
/// </remarks>
public sealed class ApePicture : Picture
{
	/// <summary>
	/// Key for front cover art.
	/// </summary>
	public const string FrontCoverKey = "Cover Art (Front)";

	/// <summary>
	/// Key for back cover art.
	/// </summary>
	public const string BackCoverKey = "Cover Art (Back)";

	/// <summary>
	/// Key for media image (e.g., CD).
	/// </summary>
	public const string MediaKey = "Cover Art (Media)";

	/// <summary>
	/// Key for artist image.
	/// </summary>
	public const string ArtistKey = "Cover Art (Artist)";

	readonly string _mimeType;
	readonly PictureType _pictureType;
	readonly string _description;
	readonly BinaryData _pictureData;
	readonly string _filename;

	/// <summary>
	/// Gets the original filename from the APE tag.
	/// </summary>
	public string Filename => _filename;

	/// <summary>
	/// Creates a new APE picture.
	/// </summary>
	/// <param name="filename">The filename (used for description and MIME type detection).</param>
	/// <param name="pictureType">The type of picture.</param>
	/// <param name="pictureData">The raw picture data.</param>
	public ApePicture (string filename, PictureType pictureType, BinaryData pictureData)
	{
		_filename = filename ?? "cover.jpg";
		_pictureType = pictureType;
		_description = _filename;
		_pictureData = pictureData;
		_mimeType = DetectMimeType (pictureData.Span, _filename);
	}

	/// <summary>
	/// Creates a new APE picture with explicit MIME type.
	/// </summary>
	/// <param name="mimeType">The MIME type of the image.</param>
	/// <param name="pictureType">The type of picture.</param>
	/// <param name="filename">The filename for storage.</param>
	/// <param name="pictureData">The raw picture data.</param>
	public ApePicture (string mimeType, PictureType pictureType, string filename, BinaryData pictureData)
	{
		_mimeType = mimeType ?? "image/jpeg";
		_pictureType = pictureType;
		_filename = filename ?? "cover.jpg";
		_description = _filename;
		_pictureData = pictureData;
	}

	/// <inheritdoc/>
	public override string MimeType => _mimeType;

	/// <inheritdoc/>
	public override PictureType PictureType => _pictureType;

	/// <inheritdoc/>
	public override string Description => _description;

	/// <inheritdoc/>
	public override BinaryData PictureData => _pictureData;

	/// <summary>
	/// Gets the APE tag key for this picture type.
	/// </summary>
	public string GetKey () => GetKeyForPictureType (_pictureType);

	/// <summary>
	/// Creates an APE picture from an ApeBinaryData.
	/// </summary>
	/// <param name="key">The APE tag key (determines picture type).</param>
	/// <param name="data">The binary data containing filename and picture.</param>
	/// <returns>The parsed picture.</returns>
	/// <exception cref="ArgumentNullException">Thrown if data is null.</exception>
	public static ApePicture FromBinaryData (string key, ApeBinaryData data)
	{
#if NETSTANDARD2_0 || NETSTANDARD2_1
		if (data is null)
			throw new ArgumentNullException (nameof (data));
#else
		ArgumentNullException.ThrowIfNull (data);
#endif

		var pictureType = GetPictureTypeForKey (key);
		return new ApePicture (data.Filename, pictureType, new BinaryData (data.Data));
	}

	/// <summary>
	/// Creates an APE picture from an IPicture.
	/// </summary>
	/// <param name="picture">The source picture.</param>
	/// <returns>An APE picture.</returns>
	/// <exception cref="ArgumentNullException">Thrown if picture is null.</exception>
	public static ApePicture FromPicture (IPicture picture)
	{
#if NETSTANDARD2_0 || NETSTANDARD2_1
		if (picture is null)
			throw new ArgumentNullException (nameof (picture));
#else
		ArgumentNullException.ThrowIfNull (picture);
#endif

		if (picture is ApePicture apePic)
			return apePic;

		// Generate filename from MIME type and description
		var filename = GenerateFilename (picture.MimeType, picture.Description);

		return new ApePicture (
			picture.MimeType,
			picture.PictureType,
			filename,
			picture.PictureData);
	}

	/// <summary>
	/// Gets the picture type for an APE tag key.
	/// </summary>
	public static PictureType GetPictureTypeForKey (string key)
	{
		if (string.Equals (key, FrontCoverKey, StringComparison.OrdinalIgnoreCase))
			return PictureType.FrontCover;
		if (string.Equals (key, BackCoverKey, StringComparison.OrdinalIgnoreCase))
			return PictureType.BackCover;
		if (string.Equals (key, MediaKey, StringComparison.OrdinalIgnoreCase))
			return PictureType.Media;
		if (string.Equals (key, ArtistKey, StringComparison.OrdinalIgnoreCase))
			return PictureType.Artist;
		return PictureType.Other;
	}

	/// <summary>
	/// Gets the APE tag key for a picture type.
	/// </summary>
	public static string GetKeyForPictureType (PictureType type)
	{
		return type switch {
			PictureType.FrontCover => FrontCoverKey,
			PictureType.BackCover => BackCoverKey,
			PictureType.Media => MediaKey,
			PictureType.Artist or PictureType.LeadArtist => ArtistKey,
			_ => FrontCoverKey // Default to front cover
		};
	}

	static string GenerateFilename (string mimeType, string description)
	{
		var extension = GetExtensionForMimeType (mimeType);

		// Use description if it looks like a filename
#if NETSTANDARD2_0
		if (!string.IsNullOrEmpty (description) && description.IndexOf ('.') >= 0)
			return description;
#else
		if (!string.IsNullOrEmpty (description) && description.Contains ('.', StringComparison.Ordinal))
			return description;
#endif

		return $"cover{extension}";
	}

	static string GetExtensionForMimeType (string mimeType)
	{
		// Use ToUpperInvariant for comparison to satisfy CA1308
		return mimeType.ToUpperInvariant () switch {
			"IMAGE/JPEG" => ".jpg",
			"IMAGE/PNG" => ".png",
			"IMAGE/GIF" => ".gif",
			"IMAGE/BMP" => ".bmp",
			"IMAGE/WEBP" => ".webp",
			"IMAGE/TIFF" => ".tiff",
			_ => ".jpg"
		};
	}
}
