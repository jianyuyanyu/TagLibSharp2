// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Core;
using TagLibSharp2.Mpeg;

namespace TagLibSharp2.Tests.Mpeg;

[TestClass]
[TestCategory ("Unit")]
[TestCategory ("Mpeg")]
public class VbriHeaderTests
{
	// VBRI header structure (used by Fraunhofer encoder):
	// Offset 0:  "VBRI" (4 bytes)
	// Offset 4:  Version (2 bytes, big-endian)
	// Offset 6:  Delay (2 bytes, big-endian)
	// Offset 8:  Quality (2 bytes, big-endian)
	// Offset 10: Total bytes (4 bytes, big-endian)
	// Offset 14: Total frames (4 bytes, big-endian)
	// Offset 18: TOC entries (2 bytes, big-endian)
	// Offset 20: TOC scale (2 bytes, big-endian)
	// Offset 22: TOC entry size (2 bytes, big-endian)
	// Offset 24: Frames per TOC entry (2 bytes, big-endian)
	// Offset 26: TOC data (variable)

	[TestMethod]
	public void TryParse_ValidVbriHeader_ReturnsTrue ()
	{
		var builder = new BinaryDataBuilder ();
		builder.Add (0x56, 0x42, 0x52, 0x49); // "VBRI"
		builder.AddUInt16BE (1);               // Version
		builder.AddUInt16BE (0);               // Delay
		builder.AddUInt16BE (75);              // Quality
		builder.AddUInt32BE (5000000);         // Total bytes
		builder.AddUInt32BE (10000);           // Total frames
		builder.AddUInt16BE (100);             // TOC entries
		builder.AddUInt16BE (1);               // TOC scale
		builder.AddUInt16BE (2);               // TOC entry size (2 bytes each)
		builder.AddUInt16BE (100);             // Frames per entry
		builder.Add (new byte[200]);           // TOC data (100 entries * 2 bytes)

		var result = VbriHeader.TryParse (builder.ToBinaryData (), 0, out var header);

		Assert.IsTrue (result);
		Assert.IsNotNull (header);
		Assert.AreEqual (10000u, header.FrameCount);
		Assert.AreEqual (5000000u, header.ByteCount);
	}

	[TestMethod]
	public void TryParse_InvalidMagic_ReturnsFalse ()
	{
		byte[] data = [
			0x56, 0x42, 0x52, 0x58, // "VBRX" - wrong
			0x00, 0x01, 0x00, 0x00
		];

		var result = VbriHeader.TryParse (new BinaryData (data), 0, out _);

		Assert.IsFalse (result);
	}

	[TestMethod]
	public void TryParse_TooShort_ReturnsFalse ()
	{
		// VBRI header requires at least 26 bytes before TOC
		byte[] data = [0x56, 0x42, 0x52, 0x49, 0x00, 0x01];

		var result = VbriHeader.TryParse (new BinaryData (data), 0, out _);

		Assert.IsFalse (result);
	}

	[TestMethod]
	public void TryParse_AtOffset_WorksCorrectly ()
	{
		var builder = new BinaryDataBuilder ();
		builder.Add (new byte[10]); // Padding
		builder.Add (0x56, 0x42, 0x52, 0x49); // "VBRI"
		builder.AddUInt16BE (1);
		builder.AddUInt16BE (0);
		builder.AddUInt16BE (50);
		builder.AddUInt32BE (2500000);
		builder.AddUInt32BE (5000);
		builder.AddUInt16BE (0);  // No TOC entries
		builder.AddUInt16BE (1);
		builder.AddUInt16BE (2);
		builder.AddUInt16BE (100);

		var result = VbriHeader.TryParse (builder.ToBinaryData (), 10, out var header);

		Assert.IsTrue (result);
		Assert.AreEqual (5000u, header!.FrameCount);
		Assert.AreEqual (2500000u, header.ByteCount);
	}

	[TestMethod]
	public void Quality_ParsedCorrectly ()
	{
		var builder = new BinaryDataBuilder ();
		builder.Add (0x56, 0x42, 0x52, 0x49);
		builder.AddUInt16BE (1);
		builder.AddUInt16BE (0);
		builder.AddUInt16BE (100); // Quality = 100
		builder.AddUInt32BE (1000000);
		builder.AddUInt32BE (2500);
		builder.AddUInt16BE (0);
		builder.AddUInt16BE (1);
		builder.AddUInt16BE (2);
		builder.AddUInt16BE (100);

		VbriHeader.TryParse (builder.ToBinaryData (), 0, out var header);

		Assert.AreEqual (100, header!.Quality);
	}

	[TestMethod]
	public void Version_ParsedCorrectly ()
	{
		var builder = new BinaryDataBuilder ();
		builder.Add (0x56, 0x42, 0x52, 0x49);
		builder.AddUInt16BE (2);  // Version 2
		builder.AddUInt16BE (576);
		builder.AddUInt16BE (75);
		builder.AddUInt32BE (3000000);
		builder.AddUInt32BE (7500);
		builder.AddUInt16BE (0);
		builder.AddUInt16BE (1);
		builder.AddUInt16BE (2);
		builder.AddUInt16BE (100);

		VbriHeader.TryParse (builder.ToBinaryData (), 0, out var header);

		Assert.AreEqual (2, header!.Version);
	}

	[TestMethod]
	public void Delay_ParsedCorrectly ()
	{
		var builder = new BinaryDataBuilder ();
		builder.Add (0x56, 0x42, 0x52, 0x49);
		builder.AddUInt16BE (1);
		builder.AddUInt16BE (576); // Delay = 576 samples
		builder.AddUInt16BE (75);
		builder.AddUInt32BE (3000000);
		builder.AddUInt32BE (7500);
		builder.AddUInt16BE (0);
		builder.AddUInt16BE (1);
		builder.AddUInt16BE (2);
		builder.AddUInt16BE (100);

		VbriHeader.TryParse (builder.ToBinaryData (), 0, out var header);

		Assert.AreEqual (576, header!.Delay);
	}

	[TestMethod]
	public void MinHeaderSize_IsCorrect ()
	{
		// 4 (magic) + 2 (version) + 2 (delay) + 2 (quality) + 4 (bytes) + 4 (frames)
		// + 2 (toc entries) + 2 (scale) + 2 (entry size) + 2 (frames per entry) = 26
		Assert.AreEqual (26, VbriHeader.MinHeaderSize);
	}
}
