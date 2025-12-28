// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Core;

namespace TagLibSharp2.Mpeg;

/// <summary>
/// Represents a VBRI header found in MP3 files encoded with Fraunhofer encoders.
/// </summary>
/// <remarks>
/// <para>
/// The VBRI header is always located at a fixed offset of 32 bytes after
/// the MPEG frame header in the first frame.
/// </para>
/// <para>
/// Structure:
/// </para>
/// <list type="bullet">
/// <item>"VBRI" (4 bytes)</item>
/// <item>Version (2 bytes, big-endian)</item>
/// <item>Delay (2 bytes, big-endian)</item>
/// <item>Quality (2 bytes, big-endian)</item>
/// <item>Total bytes (4 bytes, big-endian)</item>
/// <item>Total frames (4 bytes, big-endian)</item>
/// <item>TOC entries (2 bytes, big-endian)</item>
/// <item>TOC scale (2 bytes, big-endian)</item>
/// <item>TOC entry size (2 bytes, big-endian)</item>
/// <item>Frames per TOC entry (2 bytes, big-endian)</item>
/// <item>TOC data (variable)</item>
/// </list>
/// </remarks>
public sealed class VbriHeader
{
	/// <summary>
	/// The minimum header size in bytes (excluding TOC).
	/// </summary>
	public const int MinHeaderSize = 26;

	/// <summary>
	/// Gets the VBRI version.
	/// </summary>
	public int Version { get; }

	/// <summary>
	/// Gets the encoder delay in samples.
	/// </summary>
	public int Delay { get; }

	/// <summary>
	/// Gets the quality indicator.
	/// </summary>
	public int Quality { get; }

	/// <summary>
	/// Gets the total number of bytes in the audio data.
	/// </summary>
	public uint ByteCount { get; }

	/// <summary>
	/// Gets the total number of frames in the file.
	/// </summary>
	public uint FrameCount { get; }

	VbriHeader (int version, int delay, int quality, uint byteCount, uint frameCount)
	{
		Version = version;
		Delay = delay;
		Quality = quality;
		ByteCount = byteCount;
		FrameCount = frameCount;
	}

	/// <summary>
	/// Attempts to parse a VBRI header from binary data.
	/// </summary>
	/// <param name="data">The binary data to parse.</param>
	/// <param name="offset">The offset at which to start parsing.</param>
	/// <param name="header">The parsed header, or null if parsing failed.</param>
	/// <returns>True if parsing succeeded, false otherwise.</returns>
	public static bool TryParse (BinaryData data, int offset, out VbriHeader? header)
	{
		header = null;

		if (data.Length < offset + MinHeaderSize)
			return false;

		var span = data.Span.Slice (offset);

		// Check magic "VBRI"
		if (span[0] != 'V' || span[1] != 'B' || span[2] != 'R' || span[3] != 'I')
			return false;

		// Parse header fields (all big-endian)
		int version = (span[4] << 8) | span[5];
		int delay = (span[6] << 8) | span[7];
		int quality = (span[8] << 8) | span[9];

		uint byteCount = (uint)((span[10] << 24) | (span[11] << 16) | (span[12] << 8) | span[13]);
		uint frameCount = (uint)((span[14] << 24) | (span[15] << 16) | (span[16] << 8) | span[17]);

		header = new VbriHeader (version, delay, quality, byteCount, frameCount);
		return true;
	}
}
