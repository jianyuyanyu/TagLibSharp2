// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Ape;
using TagLibSharp2.Core;

namespace TagLibSharp2.Tests.Ape;

[TestClass]
public class ApePictureTests
{
	[TestMethod]
	public void Constructor_WithFilename_DetectsMimeType ()
	{
		// Arrange
		var imageData = new BinaryData ([0xFF, 0xD8, 0xFF, 0xE0]); // JPEG header

		// Act
		var picture = new ApePicture ("cover.jpg", PictureType.FrontCover, imageData);

		// Assert
		Assert.AreEqual ("image/jpeg", picture.MimeType);
		Assert.AreEqual (PictureType.FrontCover, picture.PictureType);
		Assert.AreEqual ("cover.jpg", picture.Description);
		Assert.AreEqual ("cover.jpg", picture.Filename);
	}

	[TestMethod]
	public void Constructor_WithMimeType_UsesProvidedMimeType ()
	{
		// Arrange
		var imageData = new BinaryData ([0x89, 0x50, 0x4E, 0x47]); // PNG header

		// Act
		var picture = new ApePicture ("image/png", PictureType.BackCover, "back.png", imageData);

		// Assert
		Assert.AreEqual ("image/png", picture.MimeType);
		Assert.AreEqual (PictureType.BackCover, picture.PictureType);
		Assert.AreEqual ("back.png", picture.Description);
	}

	[TestMethod]
	public void GetKey_ReturnsFrontCoverKey_ForFrontCover ()
	{
		var picture = new ApePicture ("cover.jpg", PictureType.FrontCover, new BinaryData ([0xFF, 0xD8]));
		Assert.AreEqual ("Cover Art (Front)", picture.GetKey ());
	}

	[TestMethod]
	public void GetKey_ReturnsBackCoverKey_ForBackCover ()
	{
		var picture = new ApePicture ("back.jpg", PictureType.BackCover, new BinaryData ([0xFF, 0xD8]));
		Assert.AreEqual ("Cover Art (Back)", picture.GetKey ());
	}

	[TestMethod]
	public void GetKey_ReturnsMediaKey_ForMedia ()
	{
		var picture = new ApePicture ("media.jpg", PictureType.Media, new BinaryData ([0xFF, 0xD8]));
		Assert.AreEqual ("Cover Art (Media)", picture.GetKey ());
	}

	[TestMethod]
	public void GetKey_ReturnsArtistKey_ForArtist ()
	{
		var picture = new ApePicture ("artist.jpg", PictureType.Artist, new BinaryData ([0xFF, 0xD8]));
		Assert.AreEqual ("Cover Art (Artist)", picture.GetKey ());
	}

	[TestMethod]
	public void GetKey_ReturnsArtistKey_ForLeadArtist ()
	{
		var picture = new ApePicture ("artist.jpg", PictureType.LeadArtist, new BinaryData ([0xFF, 0xD8]));
		Assert.AreEqual ("Cover Art (Artist)", picture.GetKey ());
	}

	[TestMethod]
	public void GetKey_ReturnsFrontCoverKey_ForOther ()
	{
		var picture = new ApePicture ("other.jpg", PictureType.Other, new BinaryData ([0xFF, 0xD8]));
		Assert.AreEqual ("Cover Art (Front)", picture.GetKey ());
	}

	[TestMethod]
	public void GetPictureTypeForKey_FrontCover ()
	{
		Assert.AreEqual (PictureType.FrontCover, ApePicture.GetPictureTypeForKey ("Cover Art (Front)"));
	}

	[TestMethod]
	public void GetPictureTypeForKey_BackCover ()
	{
		Assert.AreEqual (PictureType.BackCover, ApePicture.GetPictureTypeForKey ("Cover Art (Back)"));
	}

	[TestMethod]
	public void GetPictureTypeForKey_Media ()
	{
		Assert.AreEqual (PictureType.Media, ApePicture.GetPictureTypeForKey ("Cover Art (Media)"));
	}

	[TestMethod]
	public void GetPictureTypeForKey_Artist ()
	{
		Assert.AreEqual (PictureType.Artist, ApePicture.GetPictureTypeForKey ("Cover Art (Artist)"));
	}

	[TestMethod]
	public void GetPictureTypeForKey_CaseInsensitive ()
	{
		Assert.AreEqual (PictureType.FrontCover, ApePicture.GetPictureTypeForKey ("COVER ART (FRONT)"));
		Assert.AreEqual (PictureType.BackCover, ApePicture.GetPictureTypeForKey ("cover art (back)"));
	}

	[TestMethod]
	public void GetPictureTypeForKey_Unknown_ReturnsOther ()
	{
		Assert.AreEqual (PictureType.Other, ApePicture.GetPictureTypeForKey ("Cover Art (Unknown)"));
		Assert.AreEqual (PictureType.Other, ApePicture.GetPictureTypeForKey ("SomethingElse"));
	}

	[TestMethod]
	public void FromBinaryData_ParsesFilenameAndData ()
	{
		// Arrange - APE binary format: filename + null + data
		var filename = "cover.jpg";
		var imageData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 };
		var rawData = new byte[filename.Length + 1 + imageData.Length];
		System.Text.Encoding.UTF8.GetBytes (filename, 0, filename.Length, rawData, 0);
		rawData[filename.Length] = 0; // null terminator
		Array.Copy (imageData, 0, rawData, filename.Length + 1, imageData.Length);

		var binaryData = new ApeBinaryData (filename, imageData);

		// Act
		var picture = ApePicture.FromBinaryData ("Cover Art (Front)", binaryData);

		// Assert
		Assert.AreEqual (PictureType.FrontCover, picture.PictureType);
		Assert.AreEqual ("cover.jpg", picture.Filename);
		Assert.AreEqual ("image/jpeg", picture.MimeType);
		CollectionAssert.AreEqual (imageData, picture.PictureData.ToArray ());
	}

	[TestMethod]
	public void FromBinaryData_ThrowsArgumentNullException_WhenDataNull ()
	{
		Assert.ThrowsExactly<ArgumentNullException> (() => ApePicture.FromBinaryData ("Cover Art (Front)", null!));
	}

	[TestMethod]
	public void FromPicture_ReturnsSameInstance_WhenAlreadyApePicture ()
	{
		// Arrange
		var original = new ApePicture ("cover.jpg", PictureType.FrontCover, new BinaryData ([0xFF, 0xD8]));

		// Act
		var result = ApePicture.FromPicture (original);

		// Assert
		Assert.AreSame (original, result);
	}

	[TestMethod]
	public void FromPicture_CopiesData_FromOtherIPicture ()
	{
		// Arrange
		var data = new BinaryData ([0xFF, 0xD8]);
		var mockPicture = new TestPicture ("image/jpeg", PictureType.BackCover, "album.jpg", data);

		// Act
		var result = ApePicture.FromPicture (mockPicture);

		// Assert
		Assert.AreEqual ("image/jpeg", result.MimeType);
		Assert.AreEqual (PictureType.BackCover, result.PictureType);
		Assert.AreEqual ("album.jpg", result.Filename);
		CollectionAssert.AreEqual (data.ToArray (), result.PictureData.ToArray ());
	}

	[TestMethod]
	public void FromPicture_GeneratesFilename_WhenDescriptionNotFilename ()
	{
		// Arrange
		var data = new BinaryData ([0xFF, 0xD8]);
		var mockPicture = new TestPicture ("image/jpeg", PictureType.FrontCover, "Album Art", data);

		// Act
		var result = ApePicture.FromPicture (mockPicture);

		// Assert
		Assert.AreEqual ("cover.jpg", result.Filename);
	}

	[TestMethod]
	public void FromPicture_UsesDescriptionAsFilename_WhenDescriptionLooksLikeFilename ()
	{
		// Arrange
		var data = new BinaryData ([0x89, 0x50, 0x4E, 0x47]);
		var mockPicture = new TestPicture ("image/png", PictureType.FrontCover, "my-cover.png", data);

		// Act
		var result = ApePicture.FromPicture (mockPicture);

		// Assert
		Assert.AreEqual ("my-cover.png", result.Filename);
	}

	[TestMethod]
	public void FromPicture_ThrowsArgumentNullException_WhenNull ()
	{
		Assert.ThrowsExactly<ArgumentNullException> (() => ApePicture.FromPicture (null!));
	}

	// Simple test implementation of IPicture
	sealed class TestPicture : IPicture
	{
		public TestPicture (string mimeType, PictureType pictureType, string description, BinaryData data)
		{
			MimeType = mimeType;
			PictureType = pictureType;
			Description = description;
			PictureData = data;
		}

		public string MimeType { get; }
		public PictureType PictureType { get; }
		public string Description { get; }
		public BinaryData PictureData { get; }
	}
}
