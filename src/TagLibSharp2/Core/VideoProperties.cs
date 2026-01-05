// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace TagLibSharp2.Core;

/// <summary>
/// Represents video stream properties extracted from a media file.
/// </summary>
/// <remarks>
/// This is an immutable value type for optimal performance in batch operations.
/// Use <see cref="IsValid"/> to check if properties were successfully extracted.
/// </remarks>
/// <param name="Duration">The duration of the video.</param>
/// <param name="Width">The video width in pixels.</param>
/// <param name="Height">The video height in pixels.</param>
/// <param name="Bitrate">The video bitrate in kilobits per second (kbps).</param>
/// <param name="FrameRate">The video frame rate in frames per second.</param>
/// <param name="Codec">The video codec name (e.g., "H.264", "HEVC", "VP9").</param>
public readonly record struct VideoProperties (
	TimeSpan Duration,
	int Width,
	int Height,
	int Bitrate,
	double FrameRate,
	string? Codec = null)
{
	/// <summary>
	/// Gets a value indicating whether this instance contains valid video properties.
	/// </summary>
	/// <remarks>
	/// Returns true if width and height are greater than zero.
	/// </remarks>
	public bool IsValid => Width > 0 && Height > 0;

	/// <summary>
	/// Gets an empty instance with no video properties.
	/// </summary>
	public static VideoProperties Empty => default;

	/// <inheritdoc/>
	public override string ToString ()
	{
		if (!IsValid)
			return "No video properties";

		var parts = new List<string>
		{
			$"{Width}x{Height}"
		};

		if (Duration > TimeSpan.Zero)
			parts.Add ($"{Duration:mm\\:ss}");
		if (Bitrate > 0)
			parts.Add ($"{Bitrate}kbps");
		if (FrameRate > 0)
			parts.Add ($"{FrameRate:F2}fps");
		if (!string.IsNullOrEmpty (Codec))
			parts.Add (Codec!);

		return string.Join (", ", parts);
	}
}
