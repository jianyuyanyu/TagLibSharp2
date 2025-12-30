// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace TagLibSharp2.Ogg;

/// <summary>
/// Shared utilities for parsing and building Ogg pages.
/// </summary>
/// <remarks>
/// This class contains common functionality used by both <see cref="OggVorbisFile"/>
/// and <see cref="OggOpusFile"/> to avoid code duplication.
/// </remarks>
internal static class OggPageHelper
{
	/// <summary>
	/// Reads an Ogg page and extracts individual packets based on segment table.
	/// </summary>
	/// <param name="data">The binary data to parse.</param>
	/// <param name="validateCrc">Whether to validate CRC checksum.</param>
	/// <returns>The parse result containing page and extracted packets.</returns>
	public static OggPageWithSegmentsResult ReadOggPageWithSegments (ReadOnlySpan<byte> data, bool validateCrc = false)
	{
		const int minHeaderSize = 27;

		if (data.Length < minHeaderSize)
			return OggPageWithSegmentsResult.Failure ("Data too short");

		// Read basic page structure
		var pageResult = OggPage.Read (data, validateCrc);
		if (!pageResult.IsSuccess)
			return OggPageWithSegmentsResult.Failure (pageResult.Error ?? "Unknown error");

		// Now re-parse to get segment table info
		if (data[0] != 'O' || data[1] != 'g' || data[2] != 'g' || data[3] != 'S')
			return OggPageWithSegmentsResult.Failure ("Invalid Ogg magic");

		var segmentCount = data[26];
		if (data.Length < 27 + segmentCount)
			return OggPageWithSegmentsResult.Failure ("Data too short for segment table");

		var segmentTable = data.Slice (27, segmentCount);

		// Extract packets from segments
		// A segment of 255 bytes means the packet continues
		// A segment < 255 bytes ends the packet
		var segments = new List<byte[]> ();
		var isPacketComplete = new List<bool> ();
		var currentPacket = new List<byte> ();
		var pageDataOffset = 0;

		for (var i = 0; i < segmentCount; i++) {
			var segmentSize = segmentTable[i];
			if (pageDataOffset + segmentSize > pageResult.Page.Data.Length)
				break;

			// Add segment to current packet using efficient batch copy
			var segmentData = pageResult.Page.Data.Span.Slice (pageDataOffset, segmentSize);
#if NET8_0_OR_GREATER
			// Use CollectionsMarshal for zero-copy append on modern .NET
			// SetCount requires capacity >= new count, so ensure capacity first
			var oldCount = currentPacket.Count;
			currentPacket.EnsureCapacity (oldCount + segmentSize);
			System.Runtime.InteropServices.CollectionsMarshal.SetCount (currentPacket, oldCount + segmentSize);
			segmentData.CopyTo (System.Runtime.InteropServices.CollectionsMarshal.AsSpan (currentPacket).Slice (oldCount));
#else
			currentPacket.AddRange (segmentData.ToArray ());
#endif

			pageDataOffset += segmentSize;

			// If segment < 255, packet is complete
			if (segmentSize < 255) {
				segments.Add (currentPacket.ToArray ());
				isPacketComplete.Add (true);
				currentPacket.Clear ();
			}
		}

		// If there's remaining data in currentPacket, it continues to next page
		if (currentPacket.Count > 0) {
			segments.Add (currentPacket.ToArray ());
			isPacketComplete.Add (false);
		}

		return OggPageWithSegmentsResult.Success (pageResult.Page, pageResult.BytesConsumed, segments, isPacketComplete);
	}

	/// <summary>
	/// Finds the granule position from the last Ogg page to calculate total samples.
	/// </summary>
	/// <param name="data">The file data to scan.</param>
	/// <returns>The granule position from the last valid page.</returns>
	/// <remarks>
	/// Scans the entire file to find all Ogg pages and returns the granule position
	/// from the page with the EOS flag, or the last valid page if no EOS is found.
	/// Per RFC 3533, the EOS flag marks the last page of a logical stream.
	/// </remarks>
	public static ulong FindLastGranulePosition (ReadOnlySpan<byte> data)
	{
		// Scan from the beginning to find all pages
		// Track both the last valid granule and the EOS page's granule
		ulong lastGranulePosition = 0;
		ulong eosGranulePosition = 0;
		var foundEos = false;
		var offset = 0;

		while (offset < data.Length - 27) {
			// Look for "OggS" magic
			if (data[offset] == 'O' && data[offset + 1] == 'g' &&
				data[offset + 2] == 'g' && data[offset + 3] == 'S') {

				// Read flags (byte 5)
				var flags = data[offset + 5];
				var isEos = (flags & 0x04) != 0;

				// Read granule position (8 bytes little-endian at offset 6)
				var granule = (ulong)data[offset + 6] |
					((ulong)data[offset + 7] << 8) |
					((ulong)data[offset + 8] << 16) |
					((ulong)data[offset + 9] << 24) |
					((ulong)data[offset + 10] << 32) |
					((ulong)data[offset + 11] << 40) |
					((ulong)data[offset + 12] << 48) |
					((ulong)data[offset + 13] << 56);

				// Only update if this looks like a valid granule position
				// (not -1 which is used for non-audio pages)
				if (granule != 0xFFFFFFFFFFFFFFFF) {
					lastGranulePosition = granule;

					// Per RFC 3533, prefer granule from EOS page
					if (isEos) {
						eosGranulePosition = granule;
						foundEos = true;
					}
				}

				// Skip to after this page to find next page
				var segmentCount = data[offset + 26];
				if (offset + 27 + segmentCount < data.Length) {
					var pageSize = 27 + segmentCount;
					for (var i = 0; i < segmentCount && offset + 27 + i < data.Length; i++)
						pageSize += data[offset + 27 + i];

					offset += pageSize;
					continue;
				}
			}

			offset++;
		}

		// Prefer EOS page's granule if found, otherwise use last valid
		return foundEos ? eosGranulePosition : lastGranulePosition;
	}

	/// <summary>
	/// Builds an Ogg page from multiple packets with proper segment boundaries.
	/// </summary>
	/// <param name="packets">The packets to include in the page.</param>
	/// <param name="flags">Page flags (BOS, EOS, continuation).</param>
	/// <param name="granulePosition">The granule position for this page.</param>
	/// <param name="serialNumber">The stream serial number.</param>
	/// <param name="sequenceNumber">The page sequence number.</param>
	/// <returns>The built Ogg page as a byte array.</returns>
	public static byte[] BuildOggPage (byte[][] packets, OggPageFlags flags, ulong granulePosition,
		uint serialNumber, uint sequenceNumber)
	{
		// Build segment table - each packet needs proper segmentation
		var segments = new List<byte> ();
		var totalDataSize = 0;

		foreach (var packet in packets) {
			var remaining = packet.Length;
			while (remaining >= 255) {
				segments.Add (255);
				remaining -= 255;
			}
			// Final segment < 255 marks end of this packet
			segments.Add ((byte)remaining);
			totalDataSize += packet.Length;
		}

		// Ensure at least one segment
		if (segments.Count == 0)
			segments.Add (0);

		var headerSize = 27 + segments.Count;
		var page = new byte[headerSize + totalDataSize];

		// Magic "OggS"
		page[0] = (byte)'O';
		page[1] = (byte)'g';
		page[2] = (byte)'g';
		page[3] = (byte)'S';

		// Version
		page[4] = 0;

		// Flags
		page[5] = (byte)flags;

		// Granule position (8 bytes LE)
		for (var i = 0; i < 8; i++)
			page[6 + i] = (byte)(granulePosition >> (i * 8));

		// Serial number (4 bytes LE)
		for (var i = 0; i < 4; i++)
			page[14 + i] = (byte)(serialNumber >> (i * 8));

		// Sequence number (4 bytes LE)
		for (var i = 0; i < 4; i++)
			page[18 + i] = (byte)(sequenceNumber >> (i * 8));

		// CRC placeholder (will be calculated)
		page[22] = 0;
		page[23] = 0;
		page[24] = 0;
		page[25] = 0;

		// Segment count
		page[26] = (byte)segments.Count;

		// Segment table
		for (var i = 0; i < segments.Count; i++)
			page[27 + i] = segments[i];

		// Data - concatenate all packets
		var dataOffset = headerSize;
		foreach (var packet in packets) {
			packet.CopyTo (page, dataOffset);
			dataOffset += packet.Length;
		}

		// Calculate and set CRC
		var crc = OggCrc.Calculate (page);
		page[22] = (byte)(crc & 0xFF);
		page[23] = (byte)((crc >> 8) & 0xFF);
		page[24] = (byte)((crc >> 16) & 0xFF);
		page[25] = (byte)((crc >> 24) & 0xFF);

		return page;
	}

	/// <summary>
	/// Renumbers audio pages with sequential sequence numbers and ensures EOS flag on last page.
	/// </summary>
	/// <param name="audioData">The raw audio page data.</param>
	/// <param name="serialNumber">The stream serial number to set on all pages.</param>
	/// <param name="startSequence">The starting sequence number (typically 2 after header pages).</param>
	/// <returns>The fixed audio pages with correct serial/sequence numbers and EOS flag.</returns>
	public static byte[] RenumberAudioPages (ReadOnlySpan<byte> audioData, uint serialNumber, uint startSequence)
	{
		// First pass: collect all pages to know which is the last one
		var pages = new List<(int Offset, int Length)> ();
		var offset = 0;

		while (offset < audioData.Length - 27) {
			// Check for OggS magic
			if (audioData[offset] != 'O' || audioData[offset + 1] != 'g' ||
				audioData[offset + 2] != 'g' || audioData[offset + 3] != 'S')
				break;

			var segmentCount = audioData[offset + 26];
			if (offset + 27 + segmentCount > audioData.Length)
				break;

			var pageDataSize = 0;
			for (var i = 0; i < segmentCount; i++)
				pageDataSize += audioData[offset + 27 + i];

			var totalPageSize = 27 + segmentCount + pageDataSize;
			if (offset + totalPageSize > audioData.Length)
				break;

			pages.Add ((offset, totalPageSize));
			offset += totalPageSize;
		}

		if (pages.Count == 0)
			return [];

		// Second pass: copy pages with renumbering and EOS fix
		var result = new byte[offset];
		var sequence = startSequence;

		for (var i = 0; i < pages.Count; i++) {
			var (pageOffset, pageLength) = pages[i];
			var isLastPage = (i == pages.Count - 1);

			// Copy the page
			audioData.Slice (pageOffset, pageLength).CopyTo (result.AsSpan (pageOffset));

			// Fix serial number (bytes 14-17, little-endian)
			result[pageOffset + 14] = (byte)(serialNumber & 0xFF);
			result[pageOffset + 15] = (byte)((serialNumber >> 8) & 0xFF);
			result[pageOffset + 16] = (byte)((serialNumber >> 16) & 0xFF);
			result[pageOffset + 17] = (byte)((serialNumber >> 24) & 0xFF);

			// Fix sequence number (bytes 18-21, little-endian)
			result[pageOffset + 18] = (byte)(sequence & 0xFF);
			result[pageOffset + 19] = (byte)((sequence >> 8) & 0xFF);
			result[pageOffset + 20] = (byte)((sequence >> 16) & 0xFF);
			result[pageOffset + 21] = (byte)((sequence >> 24) & 0xFF);

			// Fix EOS flag on last page (byte 5)
			if (isLastPage)
				result[pageOffset + 5] |= 0x04; // Set EOS bit

			// Recalculate CRC (must zero CRC bytes first)
			result[pageOffset + 22] = 0;
			result[pageOffset + 23] = 0;
			result[pageOffset + 24] = 0;
			result[pageOffset + 25] = 0;

			var crc = OggCrc.Calculate (result.AsSpan (pageOffset, pageLength));
			result[pageOffset + 22] = (byte)(crc & 0xFF);
			result[pageOffset + 23] = (byte)((crc >> 8) & 0xFF);
			result[pageOffset + 24] = (byte)((crc >> 16) & 0xFF);
			result[pageOffset + 25] = (byte)((crc >> 24) & 0xFF);

			sequence++;
		}

		return result;
	}
}

/// <summary>
/// Result of reading an Ogg page with extracted packet segments.
/// </summary>
internal readonly struct OggPageWithSegmentsResult
{
	public OggPage Page { get; }
	public bool IsSuccess { get; }
	public string? Error { get; }
	public int BytesConsumed { get; }
	public List<byte[]> Segments { get; }
	public List<bool> IsPacketComplete { get; }

	OggPageWithSegmentsResult (OggPage page, bool isSuccess, string? error, int bytesConsumed,
		List<byte[]> segments, List<bool> isPacketComplete)
	{
		Page = page;
		IsSuccess = isSuccess;
		Error = error;
		BytesConsumed = bytesConsumed;
		Segments = segments;
		IsPacketComplete = isPacketComplete;
	}

	public static OggPageWithSegmentsResult Success (OggPage page, int bytesConsumed,
		List<byte[]> segments, List<bool> isPacketComplete) =>
		new (page, true, null, bytesConsumed, segments, isPacketComplete);

	public static OggPageWithSegmentsResult Failure (string error) =>
		new (default, false, error, 0, [], []);
}
