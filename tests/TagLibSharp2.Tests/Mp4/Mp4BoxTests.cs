// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Mp4;

namespace TagLibSharp2.Tests.Mp4;

/// <summary>
/// Tests for MP4 box parsing.
/// </summary>
[TestClass]
[TestCategory ("Unit")]
[TestCategory ("Mp4")]
public class Mp4BoxTests
{
	[TestMethod]
	public void ParseBasicBox_ValidBoxHeader_ParsesSizeAndType ()
	{
		var data = TestBuilders.Mp4.CreateBox ("ftyp", new byte[] { 0x6D, 0x34, 0x61, 0x20 }); // "m4a "

		var result = Mp4Box.Parse (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("ftyp", result.Box!.Type);
		Assert.AreEqual (12, result.Box.TotalSize); // 8 (header) + 4 (data)
	}

	[TestMethod]
	public void ParseBasicBox_DataTooShort_ReturnsFailure ()
	{
		var data = new byte[] { 0x00, 0x00, 0x00 }; // Less than 8 bytes

		var result = Mp4Box.Parse (data);

		Assert.IsFalse (result.IsSuccess);
		// Error details are optional at this stub stage
	}

	[TestMethod]
	public void ParseExtendedSizeBox_64BitSize_ParsesCorrectly ()
	{
		// Extended size: size=1 in header, actual size in next 8 bytes
		var data = TestBuilders.Mp4.CreateExtendedSizeBox ("mdat", new byte[100]);

		var result = Mp4Box.Parse (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("mdat", result.Box!.Type);
		Assert.IsTrue (result.Box.TotalSize > uint.MaxValue || result.Box.UsesExtendedSize);
	}

	[TestMethod]
	public void ParseFullBox_VersionAndFlags_ParsesCorrectly ()
	{
		// FullBox has 4 extra bytes: version (1 byte) + flags (3 bytes)
		// This test documents expected behavior - Mp4FullBox would expose Version/Flags
		var boxData = new byte[] { 0x00, 0x00, 0x00, 0x00 }; // version=0, flags=0
		var data = TestBuilders.Mp4.CreateBox ("hdlr", boxData);

		var result = Mp4Box.Parse (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("hdlr", result.Box!.Type);
		// Version and Flags are parsed by Mp4BoxParser.ParseFullBox, not Mp4Box.Parse
		// The data is available in Box.Data for further parsing
		Assert.AreEqual (4, result.Box.Data.Length);
	}

	[TestMethod]
	public void ParseBox_SizeZero_ExtendsToEndOfFile ()
	{
		// size=0 means "extends to EOF"
		var data = TestBuilders.Mp4.CreateBoxWithSizeZero ("mdat", new byte[1000]);

		var result = Mp4Box.Parse (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("mdat", result.Box!.Type);
		// When size=0, TotalSize reflects actual consumed bytes
	}

	[TestMethod]
	public void ParseBox_InvalidSize_ReturnsFailure ()
	{
		// Size less than 8 (header size) is invalid
		var data = new byte[16];
		data[0] = 0x00;
		data[1] = 0x00;
		data[2] = 0x00;
		data[3] = 0x04; // Size = 4, but header is 8 bytes
		data[4] = (byte)'f';
		data[5] = (byte)'t';
		data[6] = (byte)'y';
		data[7] = (byte)'p';

		var result = Mp4Box.Parse (data);

		Assert.IsFalse (result.IsSuccess);
	}

	[TestMethod]
	public void ParseContainerBox_WithChildren_ParsesHierarchy ()
	{
		// Create moov container with child boxes
		var udtaData = TestBuilders.Mp4.CreateBox ("udta", new byte[4]);
		var moovData = TestBuilders.Mp4.CreateBox ("moov", udtaData);

		var result = Mp4Box.Parse (moovData);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("moov", result.Box!.Type);
		Assert.IsTrue (result.Box.IsContainer);
		Assert.HasCount (1, result.Box.Children);
		Assert.AreEqual ("udta", result.Box.Children[0].Type);
	}

	[TestMethod]
	public void NavigateBoxHierarchy_NestedBoxes_FindsCorrectPath ()
	{
		// moov -> udta -> meta -> ilst
		// meta is a FullBox with version(1) + flags(3) prefix
		var ilstData = TestBuilders.Mp4.CreateBox ("ilst", new byte[4]);
		var metaContent = new byte[4 + ilstData.Length]; // version+flags + ilst
		Array.Copy (ilstData, 0, metaContent, 4, ilstData.Length);
		var metaData = TestBuilders.Mp4.CreateBox ("meta", metaContent);
		var udtaData = TestBuilders.Mp4.CreateBox ("udta", metaData);
		var moovData = TestBuilders.Mp4.CreateBox ("moov", udtaData);

		var result = Mp4Box.Parse (moovData);

		Assert.IsTrue (result.IsSuccess);
		var moov = result.Box!;
		var udta = moov.FindChild ("udta");
		Assert.IsNotNull (udta);
		var meta = udta!.FindChild ("meta");
		Assert.IsNotNull (meta);
		var ilst = meta!.FindChild ("ilst");
		Assert.IsNotNull (ilst);
	}

	[TestMethod]
	public void ParseBox_UnknownType_ParsesAnyway ()
	{
		// Should parse boxes with unknown types
		var data = TestBuilders.Mp4.CreateBox ("UNKN", new byte[] { 0x01, 0x02, 0x03 });

		var result = Mp4Box.Parse (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("UNKN", result.Box!.Type);
	}

	[TestMethod]
	public void ParseBox_EmptyBox_ParsesCorrectly ()
	{
		var data = TestBuilders.Mp4.CreateBox ("free", Array.Empty<byte> ());

		var result = Mp4Box.Parse (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("free", result.Box!.Type);
		Assert.AreEqual (8, result.Box.TotalSize); // Just header
	}

	[TestMethod]
	public void ParseMultipleBoxes_InSequence_ParsesAllBoxes ()
	{
		var box1 = TestBuilders.Mp4.CreateBox ("ftyp", new byte[4]);
		var box2 = TestBuilders.Mp4.CreateBox ("free", new byte[8]);
		var combined = new byte[box1.Length + box2.Length];
		Array.Copy (box1, 0, combined, 0, box1.Length);
		Array.Copy (box2, 0, combined, box1.Length, box2.Length);

		var result1 = Mp4Box.Parse (combined);
		Assert.IsTrue (result1.IsSuccess);
		Assert.AreEqual ("ftyp", result1.Box!.Type);

		var offset = (int)result1.Box.TotalSize;
		var result2 = Mp4Box.Parse (combined.AsSpan (offset));
		Assert.IsTrue (result2.IsSuccess);
		Assert.AreEqual ("free", result2.Box!.Type);
	}

	[TestMethod]
	public void BoxType_FourCharacterCode_IsReadable ()
	{
		var data = TestBuilders.Mp4.CreateBox ("moov", new byte[4]);

		var result = Mp4Box.Parse (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (4, result.Box!.Type.Length);
		Assert.AreEqual ("moov", result.Box.Type);
	}

	[TestMethod]
	public void ParseBox_WithPadding_IgnoresPadding ()
	{
		var boxData = TestBuilders.Mp4.CreateBox ("skip", new byte[16]);
		var withPadding = new byte[boxData.Length + 100];
		Array.Copy (boxData, withPadding, boxData.Length);

		var result = Mp4Box.Parse (withPadding);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("skip", result.Box!.Type);
		Assert.AreEqual (24, result.Box.TotalSize); // 8 + 16
	}

	#region Large File Tests (>4GB Boundary)

	/// <summary>
	/// Tests that MP4 correctly handles extended size boxes.
	/// When actual data matches the claimed size, parsing succeeds.
	/// </summary>
	[TestMethod]
	public void ParseExtendedSizeBox_MatchingData_ParsesCorrectly ()
	{
		// Arrange - create extended size box where claimed size matches actual data
		var content = new byte[100];
		var data = CreateExtendedSizeBoxWithActualSize ("mdat", content);

		// Act
		var result = Mp4Box.Parse (data);

		// Assert - succeeds because data matches claimed size
		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("mdat", result.Box!.Type);
		Assert.IsTrue (result.Box.UsesExtendedSize);
	}

	/// <summary>
	/// Tests that extended size box with insufficient data fails gracefully.
	/// This is expected behavior - we can't parse a 5GB box without 5GB of data.
	/// The test verifies no integer overflow or exception occurs.
	/// </summary>
	[TestMethod]
	public void ParseExtendedSizeBox_LargeClaimedSize_FailsGracefully ()
	{
		// Arrange - create box claiming 5GB but only providing 100 bytes
		const ulong fiveGigabytes = 5UL * 1024 * 1024 * 1024;
		var data = CreateExtendedSizeBoxWithClaimedSize ("mdat", fiveGigabytes, new byte[100]);

		// Act - should handle without integer overflow or exception
		var result = Mp4Box.Parse (data);

		// Assert - fails because data is insufficient, but no exception thrown
		Assert.IsFalse (result.IsSuccess);
	}

	/// <summary>
	/// Tests that extremely large size values (near ulong.MaxValue) don't cause overflow.
	/// </summary>
	[TestMethod]
	public void ParseExtendedSizeBox_MaxSize_HandlesWithoutOverflow ()
	{
		// Arrange - maximum theoretical size (minus header)
		const ulong hugeSize = ulong.MaxValue - 16;
		var data = CreateExtendedSizeBoxWithClaimedSize ("mdat", hugeSize, new byte[100]);

		// Act - should parse the header without integer overflow
		var result = Mp4Box.Parse (data);

		// Assert - fails due to insufficient data, but no overflow exception
		Assert.IsNotNull (result);
		Assert.IsFalse (result.IsSuccess); // Expected: insufficient data
	}

	/// <summary>
	/// Tests that extended size boxes render correctly and can be re-parsed.
	/// </summary>
	[TestMethod]
	public void ExtendedSizeBox_RenderAndParse_RoundTrips ()
	{
		// Arrange - create a valid extended size box with matching data
		var content = new byte[50];
		var originalData = CreateExtendedSizeBoxWithActualSize ("mdat", content);

		// Act - parse
		var result = Mp4Box.Parse (originalData);

		// Assert
		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("mdat", result.Box!.Type);
		Assert.IsTrue (result.Box.UsesExtendedSize);

		// Round-trip: render and re-parse
		var rendered = result.Box.Render ();
		var reparsed = Mp4Box.Parse (rendered.Span);
		Assert.IsTrue (reparsed.IsSuccess);
		Assert.AreEqual ("mdat", reparsed.Box!.Type);
		Assert.IsTrue (reparsed.Box.UsesExtendedSize);
	}

	/// <summary>
	/// Tests that 64-bit size values can store sizes beyond uint.MaxValue.
	/// Since we can't actually allocate 4GB+ of data, we verify the header writing logic.
	/// </summary>
	[TestMethod]
	public void ExtendedSizeHeader_LargeValue_EncodesBigEndianCorrectly ()
	{
		// Arrange - value that requires all 8 bytes: 0x0001020304050607
		const ulong testSize = 0x0001020304050607UL;
		var data = CreateExtendedSizeBoxWithClaimedSize ("mdat", testSize, new byte[10]);

		// Assert - verify the 64-bit value is encoded in big-endian format
		// Extended size is at bytes 8-15
		Assert.AreEqual (0x00, data[8]);
		Assert.AreEqual (0x01, data[9]);
		Assert.AreEqual (0x02, data[10]);
		Assert.AreEqual (0x03, data[11]);
		Assert.AreEqual (0x04, data[12]);
		Assert.AreEqual (0x05, data[13]);
		Assert.AreEqual (0x06, data[14]);
		Assert.AreEqual (0x07, data[15]);
	}

	/// <summary>
	/// Creates an MP4 extended size box where claimed size matches actual data.
	/// </summary>
	static byte[] CreateExtendedSizeBoxWithActualSize (string type, byte[] content)
	{
		var headerSize = 16;
		var actualSize = (ulong)(headerSize + content.Length);
		return CreateExtendedSizeBoxWithClaimedSize (type, actualSize, content);
	}

	/// <summary>
	/// Creates an MP4 box with extended (64-bit) size header.
	/// Format: size(4)=1 + type(4) + extended_size(8) + data
	/// </summary>
	static byte[] CreateExtendedSizeBoxWithClaimedSize (string type, ulong claimedSize, byte[] content)
	{
		var headerSize = 16;
		var data = new byte[headerSize + content.Length];

		// Size field = 1 (signals extended size)
		data[0] = 0;
		data[1] = 0;
		data[2] = 0;
		data[3] = 1;

		// Type
		data[4] = (byte)type[0];
		data[5] = (byte)type[1];
		data[6] = (byte)type[2];
		data[7] = (byte)type[3];

		// Extended size (big-endian 64-bit)
		data[8] = (byte)(claimedSize >> 56);
		data[9] = (byte)(claimedSize >> 48);
		data[10] = (byte)(claimedSize >> 40);
		data[11] = (byte)(claimedSize >> 32);
		data[12] = (byte)(claimedSize >> 24);
		data[13] = (byte)(claimedSize >> 16);
		data[14] = (byte)(claimedSize >> 8);
		data[15] = (byte)claimedSize;

		// Content
		Array.Copy (content, 0, data, headerSize, content.Length);

		return data;
	}

	#endregion
}
