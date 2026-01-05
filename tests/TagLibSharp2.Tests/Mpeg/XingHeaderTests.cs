// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Core;
using TagLibSharp2.Mpeg;

namespace TagLibSharp2.Tests.Mpeg;

[TestClass]
[TestCategory ("Unit")]
[TestCategory ("Mpeg")]
public class XingHeaderTests
{
	// Xing header structure:
	// - "Xing" or "Info" (4 bytes)
	// - Flags (4 bytes, big-endian)
	//   - Bit 0: Frame count present
	//   - Bit 1: Byte count present
	//   - Bit 2: TOC present
	//   - Bit 3: Quality indicator present
	// - Frame count (4 bytes, big-endian, if flag set)
	// - Byte count (4 bytes, big-endian, if flag set)
	// - TOC (100 bytes, if flag set)
	// - Quality (4 bytes, if flag set)

	[TestMethod]
	public void TryParse_ValidXingWithFrameCount_ReturnsTrue ()
	{
		// Xing header with frame count flag only
		byte[] data = [
			0x58, 0x69, 0x6E, 0x67, // "Xing"
			0x00, 0x00, 0x00, 0x01, // Flags: frame count present
			0x00, 0x00, 0x10, 0x00  // Frame count: 4096
		];

		var result = XingHeader.TryParse (new BinaryData (data), 0, out var header);

		Assert.IsTrue (result);
		Assert.IsNotNull (header);
		Assert.AreEqual (4096u, header.FrameCount);
		Assert.IsNull (header.ByteCount);
	}

	[TestMethod]
	public void TryParse_ValidInfoHeader_ReturnsTrue ()
	{
		// "Info" is used for CBR files encoded with LAME
		byte[] data = [
			0x49, 0x6E, 0x66, 0x6F, // "Info"
			0x00, 0x00, 0x00, 0x03, // Flags: frame count + byte count
			0x00, 0x00, 0x20, 0x00, // Frame count: 8192
			0x00, 0x80, 0x00, 0x00  // Byte count: 8388608
		];

		var result = XingHeader.TryParse (new BinaryData (data), 0, out var header);

		Assert.IsTrue (result);
		Assert.AreEqual (8192u, header!.FrameCount);
		Assert.AreEqual (8388608u, header.ByteCount);
	}

	[TestMethod]
	public void TryParse_InvalidMagic_ReturnsFalse ()
	{
		byte[] data = [
			0x58, 0x58, 0x58, 0x58, // "XXXX" - not Xing or Info
			0x00, 0x00, 0x00, 0x01,
			0x00, 0x00, 0x10, 0x00
		];

		var result = XingHeader.TryParse (new BinaryData (data), 0, out _);

		Assert.IsFalse (result);
	}

	[TestMethod]
	public void TryParse_TooShort_ReturnsFalse ()
	{
		byte[] data = [0x58, 0x69, 0x6E, 0x67]; // Just "Xing"

		var result = XingHeader.TryParse (new BinaryData (data), 0, out _);

		Assert.IsFalse (result);
	}

	[TestMethod]
	public void TryParse_NoFlags_NoData ()
	{
		// No flags set = no additional data
		byte[] data = [
			0x58, 0x69, 0x6E, 0x67, // "Xing"
			0x00, 0x00, 0x00, 0x00  // No flags
		];

		var result = XingHeader.TryParse (new BinaryData (data), 0, out var header);

		Assert.IsTrue (result);
		Assert.IsNull (header!.FrameCount);
		Assert.IsNull (header.ByteCount);
	}

	[TestMethod]
	public void TryParse_WithQuality_ParsedCorrectly ()
	{
		// Flags: frame count + quality
		byte[] data = [
			0x58, 0x69, 0x6E, 0x67, // "Xing"
			0x00, 0x00, 0x00, 0x09, // Flags: 0x01 + 0x08 (frames + quality)
			0x00, 0x00, 0x10, 0x00, // Frame count: 4096
			0x00, 0x00, 0x00, 0x64  // Quality: 100
		];

		var result = XingHeader.TryParse (new BinaryData (data), 0, out var header);

		Assert.IsTrue (result);
		Assert.AreEqual (4096u, header!.FrameCount);
		Assert.AreEqual (100u, header.Quality);
	}

	[TestMethod]
	public void TryParse_WithToc_ParsedCorrectly ()
	{
		// Flags: frame count + TOC
		var builder = new BinaryDataBuilder ();
		builder.Add (0x58, 0x69, 0x6E, 0x67); // "Xing"
		builder.Add (0x00, 0x00, 0x00, 0x05); // Flags: frames + TOC
		builder.Add (0x00, 0x00, 0x10, 0x00); // Frame count: 4096
		builder.Add (new byte[100]); // TOC (100 bytes)

		var result = XingHeader.TryParse (builder.ToBinaryData (), 0, out var header);

		Assert.IsTrue (result);
		Assert.AreEqual (4096u, header!.FrameCount);
		Assert.IsTrue (header.HasToc);
	}

	[TestMethod]
	public void TryParse_AllFlagsSet_ParsedCorrectly ()
	{
		var builder = new BinaryDataBuilder ();
		builder.Add (0x58, 0x69, 0x6E, 0x67); // "Xing"
		builder.Add (0x00, 0x00, 0x00, 0x0F); // All flags: frames + bytes + TOC + quality
		builder.AddUInt32BE (10000); // Frame count
		builder.AddUInt32BE (5000000); // Byte count
		builder.Add (new byte[100]); // TOC
		builder.AddUInt32BE (85); // Quality

		var result = XingHeader.TryParse (builder.ToBinaryData (), 0, out var header);

		Assert.IsTrue (result);
		Assert.AreEqual (10000u, header!.FrameCount);
		Assert.AreEqual (5000000u, header.ByteCount);
		Assert.IsTrue (header.HasToc);
		Assert.AreEqual (85u, header.Quality);
	}

	[TestMethod]
	public void TryParse_AtOffset_WorksCorrectly ()
	{
		byte[] data = [
			0x00, 0x00, 0x00, 0x00, // Padding
			0x58, 0x69, 0x6E, 0x67, // "Xing" at offset 4
			0x00, 0x00, 0x00, 0x01,
			0x00, 0x00, 0x08, 0x00  // Frame count: 2048
		];

		var result = XingHeader.TryParse (new BinaryData (data), 4, out var header);

		Assert.IsTrue (result);
		Assert.AreEqual (2048u, header!.FrameCount);
	}

	[TestMethod]
	public void TryParse_ByteCountWithoutFrameCount_ParsedCorrectly ()
	{
		// Only byte count flag set
		byte[] data = [
			0x58, 0x69, 0x6E, 0x67, // "Xing"
			0x00, 0x00, 0x00, 0x02, // Flags: byte count only
			0x00, 0x10, 0x00, 0x00  // Byte count: 1048576
		];

		var result = XingHeader.TryParse (new BinaryData (data), 0, out var header);

		Assert.IsTrue (result);
		Assert.IsNull (header!.FrameCount);
		Assert.AreEqual (1048576u, header.ByteCount);
	}

	[TestMethod]
	public void IsVbr_XingHeader_ReturnsTrue ()
	{
		byte[] data = [
			0x58, 0x69, 0x6E, 0x67, // "Xing"
			0x00, 0x00, 0x00, 0x01,
			0x00, 0x00, 0x10, 0x00
		];

		XingHeader.TryParse (new BinaryData (data), 0, out var header);

		Assert.IsTrue (header!.IsVbr);
	}

	[TestMethod]
	public void IsVbr_InfoHeader_ReturnsFalse ()
	{
		// "Info" indicates CBR file encoded with LAME
		byte[] data = [
			0x49, 0x6E, 0x66, 0x6F, // "Info"
			0x00, 0x00, 0x00, 0x01,
			0x00, 0x00, 0x10, 0x00
		];

		XingHeader.TryParse (new BinaryData (data), 0, out var header);

		Assert.IsFalse (header!.IsVbr);
	}
}
