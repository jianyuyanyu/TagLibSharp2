// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Core;

namespace TagLibSharp2.Mpeg;

/// <summary>
/// Represents a Xing VBR header found in MP3 files.
/// </summary>
/// <remarks>
/// <para>
/// The Xing header is located in the first MPEG frame after ID3v2 tags
/// and contains information about variable bit rate files.
/// </para>
/// <para>
/// Structure:
/// </para>
/// <list type="bullet">
/// <item>"Xing" or "Info" (4 bytes) - Info indicates CBR encoded by LAME</item>
/// <item>Flags (4 bytes, big-endian)</item>
/// <item>Frame count (4 bytes if flag 0 set)</item>
/// <item>Byte count (4 bytes if flag 1 set)</item>
/// <item>TOC (100 bytes if flag 2 set)</item>
/// <item>Quality (4 bytes if flag 3 set)</item>
/// </list>
/// </remarks>
public sealed class XingHeader
{
	const uint FlagFrames = 0x0001;
	const uint FlagBytes = 0x0002;
	const uint FlagToc = 0x0004;
	const uint FlagQuality = 0x0008;

	/// <summary>
	/// Gets the total number of frames in the file.
	/// </summary>
	public uint? FrameCount { get; }

	/// <summary>
	/// Gets the total number of bytes in the audio data.
	/// </summary>
	public uint? ByteCount { get; }

	/// <summary>
	/// Gets a value indicating whether a TOC is present.
	/// </summary>
	public bool HasToc { get; }

	/// <summary>
	/// Gets the quality indicator (0-100).
	/// </summary>
	public uint? Quality { get; }

	/// <summary>
	/// Gets a value indicating whether this is a VBR file.
	/// </summary>
	/// <remarks>
	/// Returns true for "Xing" headers, false for "Info" headers.
	/// "Info" headers indicate CBR files encoded with LAME.
	/// </remarks>
	public bool IsVbr { get; }

	XingHeader (uint? frameCount, uint? byteCount, bool hasToc, uint? quality, bool isVbr)
	{
		FrameCount = frameCount;
		ByteCount = byteCount;
		HasToc = hasToc;
		Quality = quality;
		IsVbr = isVbr;
	}

	/// <summary>
	/// Attempts to parse a Xing header from binary data.
	/// </summary>
	/// <param name="data">The binary data to parse.</param>
	/// <param name="offset">The offset at which to start parsing.</param>
	/// <param name="header">The parsed header, or null if parsing failed.</param>
	/// <returns>True if parsing succeeded, false otherwise.</returns>
	public static bool TryParse (BinaryData data, int offset, out XingHeader? header)
	{
		header = null;

		// Need at least magic + flags (8 bytes)
		if (data.Length < offset + 8)
			return false;

		var span = data.Span.Slice (offset);

		// Check magic ("Xing" or "Info")
		var magic = data.Slice (offset, 4).ToStringLatin1 ();
		bool isVbr;
		if (magic == "Xing")
			isVbr = true;
		else if (magic == "Info")
			isVbr = false;
		else
			return false;

		// Read flags (big-endian)
		uint flags = (uint)((span[4] << 24) | (span[5] << 16) | (span[6] << 8) | span[7]);

		int position = 8;
		uint? frameCount = null;
		uint? byteCount = null;
		bool hasToc = false;
		uint? quality = null;

		// Read frame count if present
		if ((flags & FlagFrames) != 0) {
			if (data.Length < offset + position + 4)
				return false;
			frameCount = (uint)((span[position] << 24) | (span[position + 1] << 16) |
				(span[position + 2] << 8) | span[position + 3]);
			position += 4;
		}

		// Read byte count if present
		if ((flags & FlagBytes) != 0) {
			if (data.Length < offset + position + 4)
				return false;
			byteCount = (uint)((span[position] << 24) | (span[position + 1] << 16) |
				(span[position + 2] << 8) | span[position + 3]);
			position += 4;
		}

		// Skip TOC if present
		if ((flags & FlagToc) != 0) {
			if (data.Length < offset + position + 100)
				return false;
			hasToc = true;
			position += 100;
		}

		// Read quality if present
		if ((flags & FlagQuality) != 0) {
			if (data.Length < offset + position + 4)
				return false;
			quality = (uint)((span[position] << 24) | (span[position + 1] << 16) |
				(span[position + 2] << 8) | span[position + 3]);
		}

		header = new XingHeader (frameCount, byteCount, hasToc, quality, isVbr);
		return true;
	}
}
