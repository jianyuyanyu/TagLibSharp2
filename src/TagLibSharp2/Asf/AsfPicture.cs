// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Buffers.Binary;
using System.Text;

using TagLibSharp2.Core;

namespace TagLibSharp2.Asf;

/// <summary>
/// Represents a picture stored in an ASF/WMA file via the WM/Picture attribute.
/// </summary>
/// <remarks>
/// <para>
/// WM/Picture binary format (little-endian):
/// </para>
/// <list type="number">
/// <item>Picture Type (1 byte) - ID3 compatible values 0-20</item>
/// <item>Data Length (4 bytes LE) - size of picture data</item>
/// <item>MIME Type (UTF-16LE null-terminated string)</item>
/// <item>Description (UTF-16LE null-terminated string)</item>
/// <item>Picture Data (raw bytes)</item>
/// </list>
/// </remarks>
public sealed class AsfPicture : Picture
{
	/// <summary>
	/// The attribute name used for pictures in ASF files.
	/// </summary>
	public const string AttributeName = "WM/Picture";

	readonly string _mimeType;
	readonly PictureType _pictureType;
	readonly string _description;
	readonly BinaryData _pictureData;

	/// <summary>
	/// Creates a new ASF picture.
	/// </summary>
	/// <param name="mimeType">The MIME type of the image.</param>
	/// <param name="pictureType">The type of picture.</param>
	/// <param name="description">The picture description.</param>
	/// <param name="pictureData">The raw picture data.</param>
	public AsfPicture (string mimeType, PictureType pictureType, string description, BinaryData pictureData)
	{
		_mimeType = mimeType ?? "image/jpeg";
		_pictureType = pictureType;
		_description = description ?? "";
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
	/// Parses an ASF picture from binary data.
	/// </summary>
	/// <param name="data">The binary data from a WM/Picture attribute.</param>
	/// <returns>The parsed picture, or null if the data is invalid.</returns>
	public static AsfPicture? Parse (ReadOnlySpan<byte> data)
	{
		// Minimum size: 1 (type) + 4 (length) + 2 (MIME null) + 2 (desc null) = 9 bytes
		if (data.Length < 9)
			return null;

		int offset = 0;

		// Picture type (1 byte)
		var pictureType = (PictureType)data[offset];
		offset += 1;

		// Picture data length (4 bytes LE)
		var pictureDataLength = BinaryPrimitives.ReadUInt32LittleEndian (data.Slice (offset));
		offset += 4;

		// MIME type (UTF-16LE null-terminated)
		var (mimeType, mimeLength) = ReadUtf16NullTerminated (data.Slice (offset));
		if (mimeLength < 0)
			return null;
		offset += mimeLength;

		// Description (UTF-16LE null-terminated)
		var (description, descLength) = ReadUtf16NullTerminated (data.Slice (offset));
		if (descLength < 0)
			return null;
		offset += descLength;

		// Validate picture data length
		if (offset + pictureDataLength > data.Length)
			return null;

		// Picture data
		var pictureData = data.Slice (offset, (int)pictureDataLength).ToArray ();

		return new AsfPicture (mimeType, pictureType, description, new BinaryData (pictureData));
	}

	/// <summary>
	/// Renders the picture to binary data for storage in a WM/Picture attribute.
	/// </summary>
	/// <returns>The binary representation.</returns>
	public BinaryData Render ()
	{
		// Calculate size
		var mimeBytes = Encoding.Unicode.GetBytes (_mimeType);
		var descBytes = Encoding.Unicode.GetBytes (_description);

		// Size: 1 (type) + 4 (length) + mime + 2 (null) + desc + 2 (null) + data
		var totalSize = 1 + 4 + mimeBytes.Length + 2 + descBytes.Length + 2 + _pictureData.Length;
		var buffer = new byte[totalSize];
		var offset = 0;

		// Picture type (1 byte)
		buffer[offset] = (byte)_pictureType;
		offset += 1;

		// Picture data length (4 bytes LE)
		BinaryPrimitives.WriteUInt32LittleEndian (buffer.AsSpan (offset), (uint)_pictureData.Length);
		offset += 4;

		// MIME type (UTF-16LE null-terminated)
		Array.Copy (mimeBytes, 0, buffer, offset, mimeBytes.Length);
		offset += mimeBytes.Length;
		buffer[offset] = 0;
		buffer[offset + 1] = 0;
		offset += 2;

		// Description (UTF-16LE null-terminated)
		Array.Copy (descBytes, 0, buffer, offset, descBytes.Length);
		offset += descBytes.Length;
		buffer[offset] = 0;
		buffer[offset + 1] = 0;
		offset += 2;

		// Picture data
		var pictureBytes = _pictureData.ToArray ();
		Array.Copy (pictureBytes, 0, buffer, offset, pictureBytes.Length);

		return new BinaryData (buffer);
	}

	/// <summary>
	/// Creates an ASF picture from an IPicture.
	/// </summary>
	/// <param name="picture">The source picture.</param>
	/// <returns>An ASF picture.</returns>
	/// <exception cref="ArgumentNullException">Thrown if picture is null.</exception>
	public static AsfPicture FromPicture (IPicture picture)
	{
#if NETSTANDARD2_0 || NETSTANDARD2_1
		if (picture is null)
			throw new ArgumentNullException (nameof (picture));
#else
		ArgumentNullException.ThrowIfNull (picture);
#endif

		if (picture is AsfPicture asfPic)
			return asfPic;

		return new AsfPicture (
			picture.MimeType,
			picture.PictureType,
			picture.Description,
			picture.PictureData);
	}

	static (string value, int bytesConsumed) ReadUtf16NullTerminated (ReadOnlySpan<byte> data)
	{
		// Find null terminator (0x00 0x00)
		int nullPos = -1;
		for (int i = 0; i <= data.Length - 2; i += 2) {
			if (data[i] == 0 && data[i + 1] == 0) {
				nullPos = i;
				break;
			}
		}

		if (nullPos < 0)
			return ("", -1);

		var stringBytes = data.Slice (0, nullPos);
		var value = Encoding.Unicode.GetString (stringBytes.ToArray ());

		// Return bytes consumed including null terminator
		return (value, nullPos + 2);
	}
}
