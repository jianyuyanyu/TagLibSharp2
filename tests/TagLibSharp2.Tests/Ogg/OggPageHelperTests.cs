// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Core;
using TagLibSharp2.Ogg;

namespace TagLibSharp2.Tests.Ogg;

[TestClass]
[TestCategory ("Unit")]
[TestCategory ("Ogg")]
public class OggPageHelperTests
{
	// ==========================================================================
	// ReadOggPageWithSegments Tests
	// ==========================================================================

	[TestMethod]
	public void ReadOggPageWithSegments_DataTooShort_ReturnsFailure ()
	{
		var data = new byte[10]; // Less than 27-byte header

		var result = OggPageHelper.ReadOggPageWithSegments (data);

		Assert.IsFalse (result.IsSuccess);
		Assert.IsNotNull (result.Error);
	}

	[TestMethod]
	public void ReadOggPageWithSegments_InvalidOggMagic_ReturnsFailure ()
	{
		var data = new byte[30];
		data[0] = (byte)'X'; // Invalid magic

		var result = OggPageHelper.ReadOggPageWithSegments (data);

		Assert.IsFalse (result.IsSuccess);
	}

	[TestMethod]
	public void ReadOggPageWithSegments_ValidPage_ReturnsSuccess ()
	{
		var pageData = TestBuilders.Ogg.CreatePage (
			new byte[] { 0x01, 0x02, 0x03 },
			sequenceNumber: 0,
			flags: OggPageFlags.BeginOfStream);

		var result = OggPageHelper.ReadOggPageWithSegments (pageData);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (1, result.Segments.Count);
		Assert.IsTrue (result.IsPacketComplete[0]);
	}

	[TestMethod]
	public void ReadOggPageWithSegments_PacketExactly255Bytes_HasTwoSegments ()
	{
		// A 255-byte packet should be encoded as [255, 0] segments
		var packet = new byte[255];
		var pageData = TestBuilders.Ogg.CreatePage (packet, 0, OggPageFlags.None);

		var result = OggPageHelper.ReadOggPageWithSegments (pageData);

		Assert.IsTrue (result.IsSuccess);
		// The packet is complete (ends with 0-length segment)
		Assert.AreEqual (1, result.Segments.Count);
		Assert.AreEqual (255, result.Segments[0].Length);
		Assert.IsTrue (result.IsPacketComplete[0]);
	}

	[TestMethod]
	public void ReadOggPageWithSegments_PacketMultipleOf255_MarkedComplete ()
	{
		// A 510-byte packet should be encoded as [255, 255, 0] segments
		var packet = new byte[510];
		var pageData = TestBuilders.Ogg.CreatePage (packet, 0, OggPageFlags.None);

		var result = OggPageHelper.ReadOggPageWithSegments (pageData);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (1, result.Segments.Count);
		Assert.AreEqual (510, result.Segments[0].Length);
		Assert.IsTrue (result.IsPacketComplete[0]);
	}

	[TestMethod]
	public void ReadOggPageWithSegments_PacketContinuesToNextPage_MarkedIncomplete ()
	{
		// Create a page where the last segment is 255 (indicates continuation)
		var builder = new BinaryDataBuilder ();
		builder.Add (TestConstants.Magic.Ogg);
		builder.Add ((byte)0); // Version
		builder.Add ((byte)OggPageFlags.None); // Flags
		builder.AddUInt64LE (0); // Granule
		builder.AddUInt32LE (1); // Serial
		builder.AddUInt32LE (0); // Sequence
		builder.AddUInt32LE (0); // CRC placeholder
		builder.Add ((byte)1); // 1 segment
		builder.Add ((byte)255); // Segment size 255 = continuation
		builder.Add (new byte[255]); // Data

		var page = builder.ToArray ();
		// Calculate CRC
		var crc = OggCrc.Calculate (page);
		page[22] = (byte)(crc & 0xFF);
		page[23] = (byte)((crc >> 8) & 0xFF);
		page[24] = (byte)((crc >> 16) & 0xFF);
		page[25] = (byte)((crc >> 24) & 0xFF);

		var result = OggPageHelper.ReadOggPageWithSegments (page);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (1, result.Segments.Count);
		Assert.IsFalse (result.IsPacketComplete[0], "Packet ending with 255-byte segment should be incomplete");
	}

	[TestMethod]
	public void ReadOggPageWithSegments_MultiplePacketsOnPage_ExtractsAll ()
	{
		// Create a page with multiple packets using proper segment table
		var builder = new BinaryDataBuilder ();
		builder.Add (TestConstants.Magic.Ogg);
		builder.Add ((byte)0); // Version
		builder.Add ((byte)OggPageFlags.None); // Flags
		builder.AddUInt64LE (0); // Granule
		builder.AddUInt32LE (1); // Serial
		builder.AddUInt32LE (0); // Sequence
		builder.AddUInt32LE (0); // CRC placeholder
		builder.Add ((byte)3); // 3 segments
		builder.Add ((byte)10); // Packet 1: 10 bytes
		builder.Add ((byte)20); // Packet 2: 20 bytes
		builder.Add ((byte)5); // Packet 3: 5 bytes
		builder.Add (new byte[35]); // Total data

		var page = builder.ToArray ();
		var crc = OggCrc.Calculate (page);
		page[22] = (byte)(crc & 0xFF);
		page[23] = (byte)((crc >> 8) & 0xFF);
		page[24] = (byte)((crc >> 16) & 0xFF);
		page[25] = (byte)((crc >> 24) & 0xFF);

		var result = OggPageHelper.ReadOggPageWithSegments (page);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (3, result.Segments.Count);
		Assert.AreEqual (10, result.Segments[0].Length);
		Assert.AreEqual (20, result.Segments[1].Length);
		Assert.AreEqual (5, result.Segments[2].Length);
		Assert.IsTrue (result.IsPacketComplete[0]);
		Assert.IsTrue (result.IsPacketComplete[1]);
		Assert.IsTrue (result.IsPacketComplete[2]);
	}

	// ==========================================================================
	// FindLastGranulePosition Tests
	// ==========================================================================

	[TestMethod]
	public void FindLastGranulePosition_EmptyData_ReturnsZero ()
	{
		var data = Array.Empty<byte> ();

		var result = OggPageHelper.FindLastGranulePosition (data);

		Assert.AreEqual (0ul, result);
	}

	[TestMethod]
	public void FindLastGranulePosition_NoValidPages_ReturnsZero ()
	{
		var data = new byte[100]; // All zeros, no valid pages

		var result = OggPageHelper.FindLastGranulePosition (data);

		Assert.AreEqual (0ul, result);
	}

	[TestMethod]
	public void FindLastGranulePosition_SinglePageWithGranule_ReturnsGranule ()
	{
		var granulePosition = 48000ul; // 1 second at 48kHz
		var page = CreatePageWithGranule (granulePosition, OggPageFlags.EndOfStream);

		var result = OggPageHelper.FindLastGranulePosition (page);

		Assert.AreEqual (granulePosition, result);
	}

	[TestMethod]
	public void FindLastGranulePosition_MultiplePagesWithGranules_ReturnsLast ()
	{
		using var builder = new BinaryDataBuilder ();
		builder.Add (CreatePageWithGranule (48000ul, OggPageFlags.BeginOfStream, sequence: 0));
		builder.Add (CreatePageWithGranule (96000ul, OggPageFlags.None, sequence: 1));
		builder.Add (CreatePageWithGranule (144000ul, OggPageFlags.EndOfStream, sequence: 2));

		var result = OggPageHelper.FindLastGranulePosition (builder.ToArray ());

		Assert.AreEqual (144000ul, result);
	}

	[TestMethod]
	public void FindLastGranulePosition_GranuleIsAllOnes_ReturnsZero ()
	{
		// Granule position of 0xFFFFFFFFFFFFFFFF indicates "no packets complete on this page"
		var page = CreatePageWithGranule (0xFFFFFFFFFFFFFFFF, OggPageFlags.EndOfStream);

		var result = OggPageHelper.FindLastGranulePosition (page);

		Assert.AreEqual (0ul, result);
	}

	[TestMethod]
	public void FindLastGranulePosition_PageWithEosFlag_PrefersEosPage ()
	{
		// When there are multiple valid granules, prefer the one from EOS page
		using var builder = new BinaryDataBuilder ();
		builder.Add (CreatePageWithGranule (999999ul, OggPageFlags.None, sequence: 0)); // Not EOS
		builder.Add (CreatePageWithGranule (48000ul, OggPageFlags.EndOfStream, sequence: 1)); // EOS

		var result = OggPageHelper.FindLastGranulePosition (builder.ToArray ());

		// Should return the last valid granule (48000 from EOS page)
		Assert.AreEqual (48000ul, result);
	}

	[TestMethod]
	public void FindLastGranulePosition_LargeFile_FindsLastPage ()
	{
		// Simulate a file larger than 64KB search window
		using var builder = new BinaryDataBuilder ();
		// Add padding to push last page beyond simple backward scan
		builder.Add (CreatePageWithGranule (1000ul, OggPageFlags.BeginOfStream, sequence: 0));
		builder.AddZeros (70000); // 70KB of padding
		builder.Add (CreatePageWithGranule (500000ul, OggPageFlags.EndOfStream, sequence: 1));

		var result = OggPageHelper.FindLastGranulePosition (builder.ToArray ());

		Assert.AreEqual (500000ul, result);
	}

	[TestMethod]
	public void FindLastGranulePosition_PagesNotInLast64KB_StillFindsLastPage ()
	{
		// Pages in the first half of the file, followed by large padding
		// This tests that we scan the entire file, not just last 64KB
		using var builder = new BinaryDataBuilder ();
		builder.Add (CreatePageWithGranule (48000ul, OggPageFlags.BeginOfStream, sequence: 0));
		builder.Add (CreatePageWithGranule (96000ul, OggPageFlags.EndOfStream, sequence: 1));
		builder.AddZeros (80000); // 80KB of garbage/padding at the end

		var result = OggPageHelper.FindLastGranulePosition (builder.ToArray ());

		Assert.AreEqual (96000ul, result);
	}

	[TestMethod]
	public void FindLastGranulePosition_OnlyEosPageGranule_IgnoresNonEosPages ()
	{
		// If a later page has EOS flag but lower granule, we should still prefer
		// the granule from the EOS page as it represents the true end of stream
		using var builder = new BinaryDataBuilder ();
		builder.Add (CreatePageWithGranule (999999ul, OggPageFlags.BeginOfStream, sequence: 0));
		builder.Add (CreatePageWithGranule (50000ul, OggPageFlags.EndOfStream, sequence: 1));

		var result = OggPageHelper.FindLastGranulePosition (builder.ToArray ());

		// Should return 50000 from the EOS page, not 999999
		Assert.AreEqual (50000ul, result);
	}

	// ==========================================================================
	// BuildOggPage Tests
	// ==========================================================================

	[TestMethod]
	public void BuildOggPage_EmptyPacketsArray_CreatesValidPage ()
	{
		var page = OggPageHelper.BuildOggPage ([], OggPageFlags.None, 0, 1, 0);

		Assert.IsNotNull (page);
		Assert.IsTrue (page.Length >= 28); // Header + at least 1 segment entry
		Assert.AreEqual ((byte)'O', page[0]);
		Assert.AreEqual ((byte)'g', page[1]);
		Assert.AreEqual ((byte)'g', page[2]);
		Assert.AreEqual ((byte)'S', page[3]);
	}

	[TestMethod]
	public void BuildOggPage_SingleSmallPacket_CorrectSegmentTable ()
	{
		var packet = new byte[] { 0x01, 0x02, 0x03 };
		var page = OggPageHelper.BuildOggPage ([packet], OggPageFlags.None, 0, 1, 0);

		// Verify segment count and table
		Assert.AreEqual (1, page[26]); // 1 segment
		Assert.AreEqual (3, page[27]); // Segment size = 3
	}

	[TestMethod]
	public void BuildOggPage_Packet255Bytes_HasZeroLengthTerminator ()
	{
		var packet = new byte[255];
		var page = OggPageHelper.BuildOggPage ([packet], OggPageFlags.None, 0, 1, 0);

		// Should have 2 segments: [255, 0]
		Assert.AreEqual (2, page[26]); // 2 segments
		Assert.AreEqual (255, page[27]); // First segment = 255
		Assert.AreEqual (0, page[28]); // Second segment = 0 (terminator)
	}

	[TestMethod]
	public void BuildOggPage_Packet510Bytes_HasCorrectSegments ()
	{
		var packet = new byte[510];
		var page = OggPageHelper.BuildOggPage ([packet], OggPageFlags.None, 0, 1, 0);

		// Should have 3 segments: [255, 255, 0]
		Assert.AreEqual (3, page[26]); // 3 segments
		Assert.AreEqual (255, page[27]);
		Assert.AreEqual (255, page[28]);
		Assert.AreEqual (0, page[29]); // Terminator
	}

	[TestMethod]
	public void BuildOggPage_MultiplePackets_AllIncluded ()
	{
		var packets = new byte[][] {
			new byte[] { 0x01, 0x02 },
			new byte[] { 0x03, 0x04, 0x05 },
			new byte[] { 0x06 }
		};
		var page = OggPageHelper.BuildOggPage (packets, OggPageFlags.None, 0, 1, 0);

		// Verify segment table: [2, 3, 1]
		Assert.AreEqual (3, page[26]); // 3 segments
		Assert.AreEqual (2, page[27]);
		Assert.AreEqual (3, page[28]);
		Assert.AreEqual (1, page[29]);
	}

	[TestMethod]
	public void BuildOggPage_BosFlag_SetCorrectly ()
	{
		var page = OggPageHelper.BuildOggPage ([new byte[1]], OggPageFlags.BeginOfStream, 0, 1, 0);

		Assert.AreEqual (0x02, page[5] & 0x02); // BOS flag is bit 1
	}

	[TestMethod]
	public void BuildOggPage_EosFlag_SetCorrectly ()
	{
		var page = OggPageHelper.BuildOggPage ([new byte[1]], OggPageFlags.EndOfStream, 0, 1, 0);

		Assert.AreEqual (0x04, page[5] & 0x04); // EOS flag is bit 2
	}

	[TestMethod]
	public void BuildOggPage_GranulePosition_EncodedCorrectly ()
	{
		var granule = 0x123456789ABCDEF0ul;
		var page = OggPageHelper.BuildOggPage ([new byte[1]], OggPageFlags.None, granule, 1, 0);

		// Granule is at bytes 6-13, little-endian
		var readGranule = (ulong)page[6] |
			((ulong)page[7] << 8) |
			((ulong)page[8] << 16) |
			((ulong)page[9] << 24) |
			((ulong)page[10] << 32) |
			((ulong)page[11] << 40) |
			((ulong)page[12] << 48) |
			((ulong)page[13] << 56);

		Assert.AreEqual (granule, readGranule);
	}

	[TestMethod]
	public void BuildOggPage_CrcIsCalculatedCorrectly ()
	{
		var packet = new byte[] { 0x01, 0x02, 0x03 };
		var page = OggPageHelper.BuildOggPage ([packet], OggPageFlags.None, 0, 1, 0);

		// Verify CRC by re-calculating
		var storedCrc = (uint)(page[22] | (page[23] << 8) | (page[24] << 16) | (page[25] << 24));

		// Zero out CRC field and recalculate
		var pageCopy = (byte[])page.Clone ();
		pageCopy[22] = 0;
		pageCopy[23] = 0;
		pageCopy[24] = 0;
		pageCopy[25] = 0;
		var calculatedCrc = OggCrc.Calculate (pageCopy);

		Assert.AreEqual (calculatedCrc, storedCrc);
	}

	[TestMethod]
	public void BuildOggPage_RoundTrip_CanBeReadBack ()
	{
		var packet = new byte[] { 0xDE, 0xAD, 0xBE, 0xEF };
		var page = OggPageHelper.BuildOggPage ([packet], OggPageFlags.BeginOfStream, 12345, 42, 7);

		var readResult = OggPageHelper.ReadOggPageWithSegments (page);

		Assert.IsTrue (readResult.IsSuccess);
		Assert.AreEqual (1, readResult.Segments.Count);
		CollectionAssert.AreEqual (packet, readResult.Segments[0]);
		Assert.AreEqual (42u, readResult.Page.SerialNumber);
		Assert.AreEqual (7u, readResult.Page.SequenceNumber);
		Assert.IsTrue (readResult.Page.IsBeginOfStream);
	}

	// ==========================================================================
	// RenumberAudioPages Tests
	// ==========================================================================

	[TestMethod]
	public void RenumberAudioPages_EmptyInput_ReturnsEmptyArray ()
	{
		var result = OggPageHelper.RenumberAudioPages ([], 1, 2);

		Assert.AreEqual (0, result.Length);
	}

	[TestMethod]
	public void RenumberAudioPages_InvalidMagic_StopsProcessing ()
	{
		var invalidData = new byte[50];
		invalidData[0] = (byte)'X'; // Not "OggS"

		var result = OggPageHelper.RenumberAudioPages (invalidData, 1, 2);

		Assert.AreEqual (0, result.Length);
	}

	[TestMethod]
	public void RenumberAudioPages_SinglePage_SetsSequenceNumber ()
	{
		var page = TestBuilders.Ogg.CreatePage (new byte[10], 99, OggPageFlags.None);

		var result = OggPageHelper.RenumberAudioPages (page, 1, startSequence: 5);

		// Read back sequence number (bytes 18-21, little-endian)
		var seqNum = (uint)(result[18] | (result[19] << 8) | (result[20] << 16) | (result[21] << 24));
		Assert.AreEqual (5u, seqNum);
	}

	[TestMethod]
	public void RenumberAudioPages_SinglePage_SetsEosFlag ()
	{
		var page = TestBuilders.Ogg.CreatePage (new byte[10], 0, OggPageFlags.None); // No EOS initially

		var result = OggPageHelper.RenumberAudioPages (page, 1, 2);

		Assert.AreEqual (0x04, result[5] & 0x04, "Last page should have EOS flag set");
	}

	[TestMethod]
	public void RenumberAudioPages_MultiplePages_SequentialNumbers ()
	{
		using var builder = new BinaryDataBuilder ();
		builder.Add (TestBuilders.Ogg.CreatePage (new byte[10], 0, OggPageFlags.None));
		builder.Add (TestBuilders.Ogg.CreatePage (new byte[10], 1, OggPageFlags.None));
		builder.Add (TestBuilders.Ogg.CreatePage (new byte[10], 2, OggPageFlags.None));

		var result = OggPageHelper.RenumberAudioPages (builder.ToArray (), 1, startSequence: 10);

		// Find each page and verify sequence
		var offset = 0;
		for (uint expectedSeq = 10; expectedSeq < 13; expectedSeq++) {
			Assert.AreEqual ((byte)'O', result[offset]);
			var seqNum = (uint)(result[offset + 18] | (result[offset + 19] << 8) |
				(result[offset + 20] << 16) | (result[offset + 21] << 24));
			Assert.AreEqual (expectedSeq, seqNum, $"Page at offset {offset} should have sequence {expectedSeq}");

			// Move to next page
			var segCount = result[offset + 26];
			var pageDataSize = 0;
			for (var i = 0; i < segCount; i++)
				pageDataSize += result[offset + 27 + i];
			offset += 27 + segCount + pageDataSize;
		}
	}

	[TestMethod]
	public void RenumberAudioPages_MultiplePages_OnlyLastHasEos ()
	{
		using var builder = new BinaryDataBuilder ();
		builder.Add (TestBuilders.Ogg.CreatePage (new byte[10], 0, OggPageFlags.None));
		builder.Add (TestBuilders.Ogg.CreatePage (new byte[10], 1, OggPageFlags.None));
		builder.Add (TestBuilders.Ogg.CreatePage (new byte[10], 2, OggPageFlags.None));

		var result = OggPageHelper.RenumberAudioPages (builder.ToArray (), 1, 2);

		// Check each page's flags
		var pageOffsets = FindPageOffsets (result);
		Assert.AreEqual (3, pageOffsets.Count);

		for (var i = 0; i < pageOffsets.Count; i++) {
			var flags = result[pageOffsets[i] + 5];
			var hasEos = (flags & 0x04) != 0;
			if (i == pageOffsets.Count - 1)
				Assert.IsTrue (hasEos, "Last page should have EOS");
			else
				Assert.IsFalse (hasEos, $"Page {i} should not have EOS");
		}
	}

	[TestMethod]
	public void RenumberAudioPages_RecalculatesCrc ()
	{
		var page = TestBuilders.Ogg.CreatePage (new byte[10], 0, OggPageFlags.None);
		var originalCrc = (uint)(page[22] | (page[23] << 8) | (page[24] << 16) | (page[25] << 24));

		var result = OggPageHelper.RenumberAudioPages (page, 1, startSequence: 999);

		var newCrc = (uint)(result[22] | (result[23] << 8) | (result[24] << 16) | (result[25] << 24));

		// CRC should be different (sequence changed)
		Assert.AreNotEqual (originalCrc, newCrc);

		// Verify new CRC is valid
		var pageCopy = (byte[])result.Clone ();
		pageCopy[22] = 0;
		pageCopy[23] = 0;
		pageCopy[24] = 0;
		pageCopy[25] = 0;
		var calculatedCrc = OggCrc.Calculate (pageCopy);
		Assert.AreEqual (calculatedCrc, newCrc);
	}

	[TestMethod]
	public void RenumberAudioPages_UpdatesSerialNumber ()
	{
		// Create page with serial number 99
		var builder = new BinaryDataBuilder ();
		builder.Add (TestConstants.Magic.Ogg);
		builder.Add ((byte)0); // Version
		builder.Add ((byte)OggPageFlags.None); // Flags
		builder.AddUInt64LE (0); // Granule
		builder.AddUInt32LE (99); // Original serial = 99
		builder.AddUInt32LE (0); // Sequence
		builder.AddUInt32LE (0); // CRC placeholder
		builder.Add ((byte)1); // 1 segment
		builder.Add ((byte)5); // 5 bytes
		builder.Add (new byte[5]); // Data

		var page = builder.ToArray ();
		var crc = OggCrc.Calculate (page);
		page[22] = (byte)(crc & 0xFF);
		page[23] = (byte)((crc >> 8) & 0xFF);
		page[24] = (byte)((crc >> 16) & 0xFF);
		page[25] = (byte)((crc >> 24) & 0xFF);

		// Request serial number 42
		var result = OggPageHelper.RenumberAudioPages (page, serialNumber: 42, startSequence: 0);

		// Read serial number back (bytes 14-17, little-endian)
		var resultSerial = (uint)(result[14] | (result[15] << 8) | (result[16] << 16) | (result[17] << 24));
		Assert.AreEqual (42u, resultSerial, "Serial number should be updated to 42");
	}

	// ==========================================================================
	// Helper Methods
	// ==========================================================================

	static byte[] CreatePageWithGranule (ulong granulePosition, OggPageFlags flags, uint sequence = 0)
	{
		var builder = new BinaryDataBuilder ();
		builder.Add (TestConstants.Magic.Ogg);
		builder.Add ((byte)0); // Version
		builder.Add ((byte)flags);
		builder.AddUInt64LE (granulePosition);
		builder.AddUInt32LE (1); // Serial
		builder.AddUInt32LE (sequence);
		builder.AddUInt32LE (0); // CRC placeholder
		builder.Add ((byte)1); // 1 segment
		builder.Add ((byte)1); // 1 byte of data
		builder.Add ((byte)0x00); // Data

		var page = builder.ToArray ();

		// Calculate CRC
		var crc = OggCrc.Calculate (page);
		page[22] = (byte)(crc & 0xFF);
		page[23] = (byte)((crc >> 8) & 0xFF);
		page[24] = (byte)((crc >> 16) & 0xFF);
		page[25] = (byte)((crc >> 24) & 0xFF);

		return page;
	}

	static List<int> FindPageOffsets (byte[] data)
	{
		var offsets = new List<int> ();
		for (var i = 0; i <= data.Length - 4; i++) {
			if (data[i] == 'O' && data[i + 1] == 'g' && data[i + 2] == 'g' && data[i + 3] == 'S')
				offsets.Add (i);
		}
		return offsets;
	}
}
