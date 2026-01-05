// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace TagLibSharp2.Core;

/// <summary>
/// Interface for picture/image data in media files.
/// </summary>
/// <remarks>
/// <para>
/// This interface provides a common contract for picture handling across
/// different media formats (ID3v2 APIC, FLAC PICTURE, etc.).
/// </para>
/// <para>
/// Implementations include <see cref="TagLibSharp2.Id3.Id3v2.Frames.PictureFrame"/> for ID3v2
/// and FlacPicture for FLAC files. The abstract <see cref="Picture"/> class provides
/// shared helper methods for implementations.
/// </para>
/// </remarks>
public interface IPicture
{
	/// <summary>
	/// Gets the MIME type of the image (e.g., "image/jpeg", "image/png").
	/// </summary>
	string MimeType { get; }

	/// <summary>
	/// Gets the picture type indicating the image's purpose.
	/// </summary>
	PictureType PictureType { get; }

	/// <summary>
	/// Gets the picture description text.
	/// </summary>
	string Description { get; }

	/// <summary>
	/// Gets the raw picture data.
	/// </summary>
	BinaryData PictureData { get; }
}
