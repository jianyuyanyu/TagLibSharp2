// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Core;

namespace TagLibSharp2.Aiff;

/// <summary>
/// Audio properties parsed from an AIFF COMM (Common) chunk.
/// </summary>
/// <remarks>
/// <para>
/// COMM chunk structure (18 bytes minimum for AIFF):
/// </para>
/// <list type="bullet">
/// <item>Bytes 0-1:  Number of channels (16-bit big-endian)</item>
/// <item>Bytes 2-5:  Number of sample frames (32-bit big-endian)</item>
/// <item>Bytes 6-7:  Bits per sample (16-bit big-endian)</item>
/// <item>Bytes 8-17: Sample rate (80-bit extended precision float)</item>
/// </list>
/// <para>
/// AIFC extends this with compression fields:
/// </para>
/// <list type="bullet">
/// <item>Bytes 18-21: Compression type (4-char code like "NONE", "sowt", "fl32")</item>
/// <item>Bytes 22+:   Compression name (Pascal string)</item>
/// </list>
/// </remarks>
public class AiffAudioProperties : IMediaProperties
{
	/// <summary>
	/// Minimum size of the COMM chunk data for AIFF.
	/// </summary>
	public const int MinCommSize = 18;

	/// <summary>
	/// Minimum size of the COMM chunk data for AIFC (with compression fields).
	/// </summary>
	public const int MinAifcCommSize = 22;

	/// <summary>
	/// Gets the number of audio channels.
	/// </summary>
	public int Channels { get; }

	/// <summary>
	/// Gets the total number of sample frames.
	/// </summary>
	public uint SampleFrames { get; }

	/// <summary>
	/// Gets the bits per sample.
	/// </summary>
	public int BitsPerSample { get; }

	/// <summary>
	/// Gets the sample rate in Hz.
	/// </summary>
	public int SampleRate { get; }

	/// <summary>
	/// Gets the audio duration.
	/// </summary>
	public TimeSpan Duration { get; }

	/// <summary>
	/// Gets the bitrate in kbps.
	/// </summary>
	public int Bitrate { get; }

	/// <summary>
	/// Gets the AIFC compression type (4-character code).
	/// </summary>
	/// <remarks>
	/// Common values include:
	/// <list type="bullet">
	/// <item>"NONE" - Uncompressed</item>
	/// <item>"sowt" - Little-endian PCM (byte-swapped)</item>
	/// <item>"fl32" - 32-bit IEEE float</item>
	/// <item>"fl64" - 64-bit IEEE float</item>
	/// <item>"alaw" - A-law compression</item>
	/// <item>"ulaw" - mu-law compression</item>
	/// </list>
	/// Null for standard AIFF files (only AIFC has compression fields).
	/// </remarks>
	public string? CompressionType { get; }

	/// <summary>
	/// Gets the AIFC compression name (human-readable description).
	/// </summary>
	/// <remarks>
	/// This is a Pascal string in the file, typically containing a description
	/// like "not compressed" for NONE compression type.
	/// Null for standard AIFF files.
	/// </remarks>
	public string? CompressionName { get; }

	/// <summary>
	/// Gets the codec name for this audio format.
	/// </summary>
	/// <remarks>
	/// Returns "AIFF" for standard AIFF files, or the compression type for AIFC files.
	/// </remarks>
	public string? Codec { get; }

	AiffAudioProperties (int channels, uint sampleFrames, int bitsPerSample, int sampleRate,
		string? compressionType = null, string? compressionName = null)
	{
		Channels = channels;
		SampleFrames = sampleFrames;
		BitsPerSample = bitsPerSample;
		SampleRate = sampleRate;
		CompressionType = compressionType;
		CompressionName = compressionName;
		Codec = compressionType ?? "AIFF";

		// Calculate duration
		if (sampleRate > 0 && sampleFrames > 0)
			Duration = TimeSpan.FromSeconds ((double)sampleFrames / sampleRate);
		else
			Duration = TimeSpan.Zero;

		// Calculate bitrate: (sampleRate * bitsPerSample * channels) / 1000
		if (sampleRate > 0)
			Bitrate = (sampleRate * bitsPerSample * channels) / 1000;
	}

	/// <summary>
	/// Attempts to parse audio properties from COMM chunk data.
	/// </summary>
	/// <param name="commData">The COMM chunk data (without header).</param>
	/// <param name="properties">The parsed properties, or null if parsing failed.</param>
	/// <returns>True if parsing succeeded; otherwise, false.</returns>
	public static bool TryParse (BinaryData commData, out AiffAudioProperties? properties)
	{
		properties = null;

		if (commData.Length < MinCommSize)
			return false;

		var span = commData.Span;

		// Parse channels (big-endian)
		int channels = (span[0] << 8) | span[1];

		// Parse sample frames (big-endian)
		uint sampleFrames = (uint)(
			(span[2] << 24) |
			(span[3] << 16) |
			(span[4] << 8) |
			span[5]);

		// Parse bits per sample (big-endian)
		int bitsPerSample = (span[6] << 8) | span[7];

		// Parse sample rate from 80-bit extended float
		var sampleRateDouble = ExtendedFloat.ToDouble (commData.Slice (8, 10));
		int sampleRate = (int)Math.Round (sampleRateDouble);

		// Parse AIFC compression fields if present
		string? compressionType = null;
		string? compressionName = null;

		if (commData.Length >= MinAifcCommSize) {
			// Compression type: 4-character code at offset 18
			compressionType = commData.Slice (18, 4).ToStringLatin1 ();

			// Compression name: Pascal string starting at offset 22
			if (commData.Length > 22) {
				var nameLength = span[22];
				if (commData.Length >= 23 + nameLength) {
					compressionName = nameLength > 0
						? commData.Slice (23, nameLength).ToStringLatin1 ()
						: string.Empty;
				}
			}
		}

		properties = new AiffAudioProperties (channels, sampleFrames, bitsPerSample, sampleRate,
			compressionType, compressionName);
		return true;
	}
}
