// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Core;

namespace TagLibSharp2.Id3.Id3v2.Frames;

/// <summary>
/// Represents an ID3v2 APIC (Attached Picture) frame for embedding images such as album art.
/// </summary>
/// <remarks>
/// <para>
/// The APIC frame allows embedding of images in ID3v2 tags, commonly used for album art,
/// artist photos, and other visual content. Multiple APIC frames can exist in a tag,
/// distinguished by their <see cref="PictureType"/>.
/// </para>
/// <para>
/// APIC frame format (ID3v2.3/2.4):
/// </para>
/// <code>
/// Offset  Size  Field
/// 0       1     Text encoding (for description field)
/// 1       n     MIME type (null-terminated ASCII, e.g., "image/jpeg")
/// n+1     1     Picture type (see <see cref="PictureType"/>)
/// n+2     m     Description (null-terminated string in specified encoding)
/// n+m+2   rest  Picture data (raw image bytes)
/// </code>
/// <para>
/// Reference: ID3v2.3 section 4.15, ID3v2.4 section 4.14
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Add a JPEG front cover to a tag
/// var picture = PictureFrame.FromFile("cover.jpg", PictureType.FrontCover);
/// tag.AddPicture(picture);
///
/// // Or manually create a picture frame
/// var imageData = File.ReadAllBytes("cover.jpg");
/// var picture = new PictureFrame("image/jpeg", PictureType.FrontCover, "", imageData);
/// </code>
/// </example>
public sealed class PictureFrame : Picture
{
	/// <inheritdoc/>
	public override string MimeType { get; }

	/// <inheritdoc/>
	public override PictureType PictureType { get; }

	/// <inheritdoc/>
	public override string Description { get; }

	/// <inheritdoc/>
	public override BinaryData PictureData { get; }

	/// <summary>
	/// Gets the text encoding used for the description field.
	/// </summary>
	/// <value>
	/// The encoding type. This only affects the description field; MIME type is always ASCII.
	/// </value>
	public TextEncodingType TextEncoding { get; }

	/// <summary>
	/// Initializes a new instance of the <see cref="PictureFrame"/> class.
	/// </summary>
	/// <param name="mimeType">
	/// The MIME type of the image (e.g., "image/jpeg", "image/png").
	/// Use DetectMimeType to auto-detect from image data.
	/// </param>
	/// <param name="pictureType">
	/// The picture type. Use <see cref="PictureType.FrontCover"/> for album art.
	/// </param>
	/// <param name="description">
	/// A description of the image. Use empty string if no description is needed.
	/// </param>
	/// <param name="pictureData">The raw image data.</param>
	/// <param name="encoding">The text encoding for the description. Defaults to UTF-8.</param>
	/// <exception cref="ArgumentNullException">
	/// Thrown when <paramref name="mimeType"/>, <paramref name="description"/>,
	/// or <paramref name="pictureData"/> is null.
	/// </exception>
	public PictureFrame (string mimeType, PictureType pictureType, string description, byte[] pictureData,
		TextEncodingType encoding = TextEncodingType.Utf8)
		: this (mimeType, pictureType, description, new BinaryData (pictureData), encoding)
	{
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="PictureFrame"/> class.
	/// </summary>
	/// <param name="mimeType">
	/// The MIME type of the image (e.g., "image/jpeg", "image/png").
	/// Use DetectMimeType to auto-detect from image data.
	/// </param>
	/// <param name="pictureType">
	/// The picture type. Use <see cref="PictureType.FrontCover"/> for album art.
	/// </param>
	/// <param name="description">
	/// A description of the image. Use empty string if no description is needed.
	/// </param>
	/// <param name="pictureData">The raw image data as <see cref="BinaryData"/>.</param>
	/// <param name="encoding">The text encoding for the description. Defaults to UTF-8.</param>
	/// <exception cref="ArgumentNullException">
	/// Thrown when <paramref name="mimeType"/> or <paramref name="description"/> is null.
	/// </exception>
	public PictureFrame (string mimeType, PictureType pictureType, string description, BinaryData pictureData,
		TextEncodingType encoding = TextEncodingType.Utf8)
	{
#if NETSTANDARD2_0 || NETSTANDARD2_1
		if (mimeType is null)
			throw new ArgumentNullException (nameof (mimeType));
		if (description is null)
			throw new ArgumentNullException (nameof (description));
#else
		ArgumentNullException.ThrowIfNull (mimeType);
		ArgumentNullException.ThrowIfNull (description);
#endif
		MimeType = mimeType;
		PictureType = pictureType;
		Description = description;
		PictureData = pictureData;
		TextEncoding = encoding;
	}

	/// <summary>
	/// Creates a <see cref="PictureFrame"/> from image data with automatic MIME type detection.
	/// </summary>
	/// <param name="imageData">The raw image data.</param>
	/// <param name="pictureType">The picture type. Defaults to <see cref="PictureType.FrontCover"/>.</param>
	/// <param name="description">Optional description. Defaults to empty string.</param>
	/// <returns>A new <see cref="PictureFrame"/> with the detected MIME type.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="imageData"/> is null.</exception>
	/// <example>
	/// <code>
	/// var imageBytes = File.ReadAllBytes("cover.jpg");
	/// var picture = PictureFrame.FromBytes(imageBytes);
	/// tag.AddPicture(picture);
	/// </code>
	/// </example>
	public static PictureFrame FromBytes (byte[] imageData, PictureType pictureType = PictureType.FrontCover,
		string description = "")
	{
#if NETSTANDARD2_0 || NETSTANDARD2_1
		if (imageData is null)
			throw new ArgumentNullException (nameof (imageData));
#else
		ArgumentNullException.ThrowIfNull (imageData);
#endif
		var mimeType = DetectMimeType (imageData);
		return new PictureFrame (mimeType, pictureType, description, imageData);
	}

	/// <summary>
	/// Creates a <see cref="PictureFrame"/> from a file with automatic MIME type detection.
	/// </summary>
	/// <param name="path">The path to the image file.</param>
	/// <param name="pictureType">The picture type. Defaults to <see cref="PictureType.FrontCover"/>.</param>
	/// <param name="description">Optional description. Defaults to empty string.</param>
	/// <returns>A new <see cref="PictureFrame"/> with the detected MIME type.</returns>
	/// <exception cref="ArgumentNullException">Thrown when <paramref name="path"/> is null.</exception>
	/// <exception cref="System.IO.FileNotFoundException">Thrown when the file does not exist.</exception>
	/// <example>
	/// <code>
	/// var picture = PictureFrame.FromFile("cover.jpg");
	/// tag.AddPicture(picture);
	/// </code>
	/// </example>
	public static PictureFrame FromFile (string path, PictureType pictureType = PictureType.FrontCover,
		string description = "")
	{
#if NETSTANDARD2_0 || NETSTANDARD2_1
		if (path is null)
			throw new ArgumentNullException (nameof (path));
#else
		ArgumentNullException.ThrowIfNull (path);
#endif
		var data = System.IO.File.ReadAllBytes (path);
		var mimeType = DetectMimeType (data, path);
		return new PictureFrame (mimeType, pictureType, description, data);
	}

	/// <summary>
	/// Attempts to read an APIC frame from binary data.
	/// </summary>
	/// <param name="data">The frame content data (excluding the 10-byte frame header).</param>
	/// <param name="version">The ID3v2 version (affects text encoding validation).</param>
	/// <returns>
	/// A <see cref="PictureFrameReadResult"/> indicating success or failure.
	/// Check <see cref="PictureFrameReadResult.IsSuccess"/> before accessing
	/// <see cref="PictureFrameReadResult.Frame"/>.
	/// </returns>
	/// <remarks>
	/// <para>Common failure reasons:</para>
	/// <list type="bullet">
	/// <item>Data too short (less than 4 bytes)</item>
	/// <item>Invalid text encoding byte for the version</item>
	/// <item>MIME type not null-terminated</item>
	/// </list>
	/// </remarks>
	public static PictureFrameReadResult Read (ReadOnlySpan<byte> data, Id3v2Version version)
	{
		if (data.Length < 4)
			return PictureFrameReadResult.Failure ("APIC frame data is too short");

		var offset = 0;

		// Text encoding - validate based on version
		var encodingByte = data[offset++];

		// ID3v2.3 only supports encodings 0 (Latin1) and 1 (UTF-16 with BOM)
		// ID3v2.4 adds encodings 2 (UTF-16BE) and 3 (UTF-8)
		if (version == Id3v2Version.V23 && encodingByte > 1)
			return PictureFrameReadResult.Failure ($"Invalid text encoding for ID3v2.3: {encodingByte} (only 0-1 allowed)");
		if (encodingByte > 3)
			return PictureFrameReadResult.Failure ($"Invalid text encoding: {encodingByte}");

		var encoding = (TextEncodingType)encodingByte;

		// MIME type (null-terminated ASCII)
		var mimeTypeEnd = FindNullTerminator (data.Slice (offset), 1);
		if (mimeTypeEnd < 0)
			return PictureFrameReadResult.Failure ("MIME type not null-terminated");

		var mimeType = System.Text.Encoding.ASCII.GetString (data.Slice (offset, mimeTypeEnd));

		// Default empty MIME type to "image/" per spec
		if (string.IsNullOrEmpty (mimeType))
			mimeType = "image/";

		offset += mimeTypeEnd + 1; // Skip null terminator

		if (offset >= data.Length)
			return PictureFrameReadResult.Failure ("Frame ends after MIME type");

		// Picture type
		var pictureType = (PictureType)data[offset++];

		if (offset >= data.Length)
			return PictureFrameReadResult.Failure ("Frame ends after picture type");

		// Description (null-terminated in specified encoding)
		var nullSize = GetNullTerminatorSize (encoding);
		var descriptionEnd = FindNullTerminator (data.Slice (offset), nullSize);

		string description;
		if (descriptionEnd < 0) {
			// No null terminator - treat rest as empty description with all remaining data as image
			description = string.Empty;
			// Don't advance offset - all remaining data is picture data
		} else {
			description = DecodeString (data.Slice (offset, descriptionEnd), encoding);
			offset += descriptionEnd + nullSize; // Skip description and null terminator
		}

		// Picture data (remaining bytes)
		var pictureData = offset < data.Length
			? new BinaryData (data.Slice (offset))
			: BinaryData.Empty;

		var frame = new PictureFrame (mimeType, pictureType, description, pictureData, encoding);

		return PictureFrameReadResult.Success (frame, data.Length);
	}

	/// <summary>
	/// Renders the frame content to binary data for writing to a file.
	/// </summary>
	/// <returns>
	/// The complete frame content (excluding the 10-byte frame header).
	/// </returns>
	/// <remarks>
	/// The returned data does NOT include the frame header (frame ID + size + flags).
	/// Use <see cref="Id3v2Tag.Render()"/> to create a complete tag with frame headers.
	/// </remarks>
	public BinaryData RenderContent ()
	{
		var mimeBytes = System.Text.Encoding.ASCII.GetBytes (MimeType);
		var descBytes = EncodeString (Description, TextEncoding);
		var nullSize = GetNullTerminatorSize (TextEncoding);

		// Calculate total size
		var totalSize = 1 + // encoding
			mimeBytes.Length + 1 + // MIME + null
			1 + // picture type
			descBytes.Length + nullSize + // description + null
			PictureData.Length; // image data

		// Add BOM size for UTF-16
		if (TextEncoding == TextEncodingType.Utf16WithBom)
			totalSize += 2;

		using var builder = new BinaryDataBuilder (totalSize);

		// Encoding byte
		builder.Add ((byte)TextEncoding);

		// MIME type (null-terminated)
		builder.Add (mimeBytes);
		builder.Add (0);

		// Picture type
		builder.Add ((byte)PictureType);

		// Description with BOM if UTF-16
		if (TextEncoding == TextEncodingType.Utf16WithBom) {
			builder.Add (0xFF);
			builder.Add (0xFE);
		}
		builder.Add (descBytes);

		// Null terminator for description
		builder.AddZeros (nullSize);

		// Picture data
		builder.Add (PictureData);

		return builder.ToBinaryData ();
	}

	static int FindNullTerminator (ReadOnlySpan<byte> data, int nullSize)
	{
		if (nullSize == 1)
			return data.IndexOf ((byte)0);

		// Double-null for UTF-16 - must have at least 2 bytes
		if (data.Length < 2)
			return -1;

		for (var i = 0; i <= data.Length - 2; i += 2) {
			if (data[i] == 0 && data[i + 1] == 0)
				return i;
		}
		return -1;
	}

	static int GetNullTerminatorSize (TextEncodingType encoding) =>
		encoding is TextEncodingType.Utf16WithBom or TextEncodingType.Utf16BE ? 2 : 1;

	static string DecodeString (ReadOnlySpan<byte> data, TextEncodingType encoding)
	{
		if (data.IsEmpty)
			return string.Empty;

		return encoding switch {
			TextEncodingType.Latin1 => Polyfills.Latin1.GetString (data),
			TextEncodingType.Utf8 => System.Text.Encoding.UTF8.GetString (data),
			TextEncodingType.Utf16WithBom => DecodeUtf16WithBom (data),
			TextEncodingType.Utf16BE => System.Text.Encoding.BigEndianUnicode.GetString (data),
			_ => string.Empty
		};
	}

	static string DecodeUtf16WithBom (ReadOnlySpan<byte> data)
	{
		if (data.Length < 2)
			return string.Empty;

		// Use tuple pattern matching for BOM detection
		return (data[0], data[1]) switch {
			(0xFF, 0xFE) => System.Text.Encoding.Unicode.GetString (data.Slice (2)),
			(0xFE, 0xFF) => System.Text.Encoding.BigEndianUnicode.GetString (data.Slice (2)),
			_ => System.Text.Encoding.Unicode.GetString (data) // No BOM, assume little endian
		};
	}

	static byte[] EncodeString (string text, TextEncodingType encoding)
	{
		return encoding switch {
			TextEncodingType.Latin1 => Polyfills.Latin1.GetBytes (text),
			TextEncodingType.Utf8 => System.Text.Encoding.UTF8.GetBytes (text),
			TextEncodingType.Utf16WithBom => System.Text.Encoding.Unicode.GetBytes (text),
			TextEncodingType.Utf16BE => System.Text.Encoding.BigEndianUnicode.GetBytes (text),
			_ => []
		};
	}
}

/// <summary>
/// Represents the result of attempting to read a <see cref="PictureFrame"/> from binary data.
/// </summary>
/// <remarks>
/// This is a result type that avoids throwing exceptions for malformed data.
/// Always check <see cref="IsSuccess"/> before accessing <see cref="Frame"/>.
/// </remarks>
public readonly struct PictureFrameReadResult : IEquatable<PictureFrameReadResult>
{
	/// <summary>
	/// Gets the successfully parsed picture frame, or null if parsing failed.
	/// </summary>
	public PictureFrame? Frame { get; }

	/// <summary>
	/// Gets a value indicating whether the frame was parsed successfully.
	/// </summary>
	public bool IsSuccess => Frame is not null && Error is null;

	/// <summary>
	/// Gets the error message if parsing failed, or null if successful.
	/// </summary>
	public string? Error { get; }

	/// <summary>
	/// Gets the number of bytes consumed from the input data.
	/// </summary>
	public int BytesConsumed { get; }

	PictureFrameReadResult (PictureFrame? frame, string? error, int bytesConsumed)
	{
		Frame = frame;
		Error = error;
		BytesConsumed = bytesConsumed;
	}

	/// <summary>
	/// Creates a successful result.
	/// </summary>
	/// <param name="frame">The parsed frame.</param>
	/// <param name="bytesConsumed">The number of bytes consumed.</param>
	/// <returns>A successful result.</returns>
	public static PictureFrameReadResult Success (PictureFrame frame, int bytesConsumed) =>
		new (frame, null, bytesConsumed);

	/// <summary>
	/// Creates a failure result.
	/// </summary>
	/// <param name="error">The error message.</param>
	/// <returns>A failure result.</returns>
	public static PictureFrameReadResult Failure (string error) =>
		new (null, error, 0);

	/// <inheritdoc/>
	public bool Equals (PictureFrameReadResult other) =>
		ReferenceEquals (Frame, other.Frame) &&
		Error == other.Error &&
		BytesConsumed == other.BytesConsumed;

	/// <inheritdoc/>
	public override bool Equals (object? obj) =>
		obj is PictureFrameReadResult other && Equals (other);

	/// <inheritdoc/>
	public override int GetHashCode () =>
		HashCode.Combine (Frame, Error, BytesConsumed);

	/// <summary>
	/// Determines whether two results are equal.
	/// </summary>
	public static bool operator == (PictureFrameReadResult left, PictureFrameReadResult right) =>
		left.Equals (right);

	/// <summary>
	/// Determines whether two results are not equal.
	/// </summary>
	public static bool operator != (PictureFrameReadResult left, PictureFrameReadResult right) =>
		!left.Equals (right);
}
