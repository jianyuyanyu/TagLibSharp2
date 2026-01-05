// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Core;
using TagLibSharp2.Mp4;

namespace TagLibSharp2.Tests.Mp4;

[TestClass]
public class Mp4DataAtomTests
{
	[TestMethod]
	public void Parse_WithValidData_ExtractsTypeAndValue ()
	{
		// Arrange: version(1) + flags/type(3) + locale(4) + value
		var data = new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, (byte)'H', (byte)'i' };

		// Act
		var atom = Mp4DataAtom.Parse (data);

		// Assert
		Assert.AreEqual (Mp4AtomMapping.TypeUtf8, atom.TypeIndicator);
		Assert.AreEqual ("Hi", atom.ToUtf8String ());
	}

	[TestMethod]
	public void Parse_WithShortData_ReturnsEmptyAtom ()
	{
		var data = new byte[] { 0x00, 0x00, 0x00 };
		var atom = Mp4DataAtom.Parse (data);

		Assert.AreEqual (Mp4AtomMapping.TypeBinary, atom.TypeIndicator);
		Assert.IsTrue (atom.Data.IsEmpty);
	}

	[TestMethod]
	public void Parse_WithExactly8Bytes_ReturnsEmptyData ()
	{
		var data = new byte[] { 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 };
		var atom = Mp4DataAtom.Parse (data);

		Assert.AreEqual (Mp4AtomMapping.TypeUtf8, atom.TypeIndicator);
		Assert.IsTrue (atom.Data.IsEmpty);
	}

	[TestMethod]
	public void ToUtf8String_WithEmptyData_ReturnsNull ()
	{
		var atom = new Mp4DataAtom (Mp4AtomMapping.TypeUtf8, BinaryData.Empty);
		Assert.IsNull (atom.ToUtf8String ());
	}

	[TestMethod]
	public void ToUInt32_WithEmptyData_ReturnsNull ()
	{
		var atom = new Mp4DataAtom (Mp4AtomMapping.TypeInteger, BinaryData.Empty);
		Assert.IsNull (atom.ToUInt32 ());
	}

	[TestMethod]
	public void ToUInt32_With1Byte_ReturnsValue ()
	{
		var atom = new Mp4DataAtom (Mp4AtomMapping.TypeInteger, new BinaryData (new byte[] { 0x42 }));
		Assert.AreEqual (0x42u, atom.ToUInt32 ());
	}

	[TestMethod]
	public void ToUInt32_With2Bytes_ReturnsBigEndianValue ()
	{
		var atom = new Mp4DataAtom (Mp4AtomMapping.TypeInteger, new BinaryData (new byte[] { 0x01, 0x02 }));
		Assert.AreEqual (0x0102u, atom.ToUInt32 ());
	}

	[TestMethod]
	public void ToUInt32_With3Bytes_ReturnsBigEndianValue ()
	{
		var atom = new Mp4DataAtom (Mp4AtomMapping.TypeInteger, new BinaryData (new byte[] { 0x01, 0x02, 0x03 }));
		Assert.AreEqual (0x010203u, atom.ToUInt32 ());
	}

	[TestMethod]
	public void ToUInt32_With4Bytes_ReturnsBigEndianValue ()
	{
		var atom = new Mp4DataAtom (Mp4AtomMapping.TypeInteger, new BinaryData (new byte[] { 0x01, 0x02, 0x03, 0x04 }));
		Assert.AreEqual (0x01020304u, atom.ToUInt32 ());
	}

	[TestMethod]
	public void ToUInt32_With8Bytes_ReturnsTruncatedValue ()
	{
		var atom = new Mp4DataAtom (Mp4AtomMapping.TypeInteger, new BinaryData (new byte[] { 0x00, 0x00, 0x00, 0x00, 0x12, 0x34, 0x56, 0x78 }));
		Assert.AreEqual (0x12345678u, atom.ToUInt32 ());
	}

	[TestMethod]
	public void ToUInt32_With5Bytes_ReturnsNull ()
	{
		var atom = new Mp4DataAtom (Mp4AtomMapping.TypeInteger, new BinaryData (new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 }));
		Assert.IsNull (atom.ToUInt32 ());
	}

	[TestMethod]
	public void ToBoolean_WithEmptyData_ReturnsFalse ()
	{
		var atom = new Mp4DataAtom (Mp4AtomMapping.TypeInteger, BinaryData.Empty);
		Assert.IsFalse (atom.ToBoolean ());
	}

	[TestMethod]
	public void ToBoolean_WithZero_ReturnsFalse ()
	{
		var atom = new Mp4DataAtom (Mp4AtomMapping.TypeInteger, new BinaryData (new byte[] { 0x00 }));
		Assert.IsFalse (atom.ToBoolean ());
	}

	[TestMethod]
	public void ToBoolean_WithNonZero_ReturnsTrue ()
	{
		var atom = new Mp4DataAtom (Mp4AtomMapping.TypeInteger, new BinaryData (new byte[] { 0x01 }));
		Assert.IsTrue (atom.ToBoolean ());
	}

	[TestMethod]
	public void TryParseTrackDisc_WithValidData_ReturnsNumberAndTotal ()
	{
		// Structure: [0][0][number:2][total:2][0][0]
		var data = new byte[] { 0x00, 0x00, 0x00, 0x05, 0x00, 0x0A };
		var atom = new Mp4DataAtom (Mp4AtomMapping.TypeBinary, new BinaryData (data));

		Assert.IsTrue (atom.TryParseTrackDisc (out var number, out var total));
		Assert.AreEqual (5u, number);
		Assert.AreEqual (10u, total);
	}

	[TestMethod]
	public void TryParseTrackDisc_WithShortData_ReturnsFalse ()
	{
		var atom = new Mp4DataAtom (Mp4AtomMapping.TypeBinary, new BinaryData (new byte[] { 0x00, 0x00, 0x00 }));

		Assert.IsFalse (atom.TryParseTrackDisc (out var number, out var total));
		Assert.AreEqual (0u, number);
		Assert.AreEqual (0u, total);
	}

	[TestMethod]
	public void Create_String_ProducesValidDataAtom ()
	{
		var data = Mp4DataAtom.Create ("Test");

		Assert.AreEqual (12, data.Length); // 8 header + 4 chars
		Assert.AreEqual (Mp4AtomMapping.TypeUtf8, data[3]);
		Assert.AreEqual ((byte)'T', data[8]);
	}

	[TestMethod]
	public void Create_String_Null_ProducesEmptyString ()
	{
		var data = Mp4DataAtom.Create (null!);

		Assert.AreEqual (8, data.Length); // header only
		Assert.AreEqual (Mp4AtomMapping.TypeUtf8, data[3]);
	}

	[TestMethod]
	public void Create_UInt32_1Byte_ProducesValidData ()
	{
		var data = Mp4DataAtom.Create (42u, 1);

		Assert.AreEqual (9, data.Length);
		Assert.AreEqual (Mp4AtomMapping.TypeInteger, data[3]);
		Assert.AreEqual (42, data[8]);
	}

	[TestMethod]
	public void Create_UInt32_2Bytes_ProducesValidData ()
	{
		var data = Mp4DataAtom.Create (0x0102u, 2);

		Assert.AreEqual (10, data.Length);
		Assert.AreEqual (Mp4AtomMapping.TypeInteger, data[3]);
		Assert.AreEqual (0x01, data[8]);
		Assert.AreEqual (0x02, data[9]);
	}

	[TestMethod]
	public void Create_UInt32_4Bytes_ProducesValidData ()
	{
		var data = Mp4DataAtom.Create (0x01020304u, 4);

		Assert.AreEqual (12, data.Length);
		Assert.AreEqual (Mp4AtomMapping.TypeInteger, data[3]);
		Assert.AreEqual (0x01, data[8]);
		Assert.AreEqual (0x02, data[9]);
		Assert.AreEqual (0x03, data[10]);
		Assert.AreEqual (0x04, data[11]);
	}

	[TestMethod]
	public void Create_Boolean_True_ProducesNonZero ()
	{
		var data = Mp4DataAtom.Create (true);

		Assert.AreEqual (9, data.Length);
		Assert.AreEqual (1, data[8]);
	}

	[TestMethod]
	public void Create_Boolean_False_ProducesZero ()
	{
		var data = Mp4DataAtom.Create (false);

		Assert.AreEqual (9, data.Length);
		Assert.AreEqual (0, data[8]);
	}

	[TestMethod]
	public void CreateTrackDisc_ProducesValidStructure ()
	{
		var data = Mp4DataAtom.CreateTrackDisc (5, 12);

		Assert.AreEqual (16, data.Length);
		Assert.AreEqual (0, data[3]); // Binary type

		// Check track number at bytes 10-11
		Assert.AreEqual (0, data[10]);
		Assert.AreEqual (5, data[11]);

		// Check total at bytes 12-13
		Assert.AreEqual (0, data[12]);
		Assert.AreEqual (12, data[13]);
	}

	[TestMethod]
	public void CreateImage_Jpeg_SetsCorrectType ()
	{
		var imageData = new BinaryData (new byte[] { 0xFF, 0xD8, 0xFF });
		var data = Mp4DataAtom.CreateImage (imageData, isJpeg: true);

		Assert.AreEqual (11, data.Length);
		Assert.AreEqual (Mp4AtomMapping.TypeJpeg, data[3]);
		Assert.AreEqual (0xFF, data[8]);
	}

	[TestMethod]
	public void CreateImage_Png_SetsCorrectType ()
	{
		var imageData = new BinaryData (new byte[] { 0x89, 0x50, 0x4E, 0x47 });
		var data = Mp4DataAtom.CreateImage (imageData, isJpeg: false);

		Assert.AreEqual (12, data.Length);
		Assert.AreEqual (Mp4AtomMapping.TypePng, data[3]);
		Assert.AreEqual (0x89, data[8]);
	}
}
