// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace TagLibSharp2.Core;

/// <summary>
/// Represents image properties extracted from a media file.
/// </summary>
/// <remarks>
/// This is an immutable value type for optimal performance in batch operations.
/// Use <see cref="IsValid"/> to check if properties were successfully extracted.
/// </remarks>
/// <param name="Width">The image width in pixels.</param>
/// <param name="Height">The image height in pixels.</param>
/// <param name="ColorDepth">The color depth in bits per pixel (e.g., 24 for RGB, 32 for RGBA).</param>
/// <param name="Format">The image format name (e.g., "JPEG", "PNG", "TIFF").</param>
public readonly record struct ImageProperties (
	int Width,
	int Height,
	int ColorDepth = 0,
	string? Format = null)
{
	/// <summary>
	/// Gets a value indicating whether this instance contains valid image properties.
	/// </summary>
	/// <remarks>
	/// Returns true if width and height are greater than zero.
	/// </remarks>
	public bool IsValid => Width > 0 && Height > 0;

	/// <summary>
	/// Gets an empty instance with no image properties.
	/// </summary>
	public static ImageProperties Empty => default;

	/// <inheritdoc/>
	public override string ToString ()
	{
		if (!IsValid)
			return "No image properties";

		var parts = new List<string>
		{
			$"{Width}x{Height}"
		};

		if (ColorDepth > 0)
			parts.Add ($"{ColorDepth}bpp");
		if (!string.IsNullOrEmpty (Format))
			parts.Add (Format!);

		return string.Join (", ", parts);
	}
}
