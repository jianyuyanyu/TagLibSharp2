// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Ape;
using TagLibSharp2.Core;

namespace TagLibSharp2.Tests.Ape;

[TestClass]
public class ApeTagPictureTests
{
	[TestMethod]
	public void Pictures_Get_ReturnsEmpty_WhenNoPictures ()
	{
		// Arrange
		var tag = new ApeTag ();

		// Act
		var pictures = tag.Pictures;

		// Assert
		Assert.IsNotNull (pictures);
		Assert.AreEqual (0, pictures.Length);
	}

	[TestMethod]
	public void Pictures_Set_AddsSinglePicture ()
	{
		// Arrange
		var tag = new ApeTag ();
		var imageData = new BinaryData ([0xFF, 0xD8, 0xFF, 0xE0]);
		var picture = new ApePicture ("cover.jpg", PictureType.FrontCover, imageData);

		// Act
		tag.Pictures = [picture];

		// Assert
		Assert.AreEqual (1, tag.Pictures.Length);
		Assert.AreEqual ("image/jpeg", tag.Pictures[0].MimeType);
		Assert.AreEqual (PictureType.FrontCover, tag.Pictures[0].PictureType);
	}

	[TestMethod]
	public void Pictures_Set_AddsMultiplePictures ()
	{
		// Arrange
		var tag = new ApeTag ();
		var front = new ApePicture ("front.jpg", PictureType.FrontCover, new BinaryData ([0xFF, 0xD8]));
		var back = new ApePicture ("back.png", PictureType.BackCover, new BinaryData ([0x89, 0x50]));

		// Act
		tag.Pictures = [front, back];

		// Assert
		Assert.AreEqual (2, tag.Pictures.Length);
		Assert.AreEqual (PictureType.FrontCover, tag.Pictures[0].PictureType);
		Assert.AreEqual (PictureType.BackCover, tag.Pictures[1].PictureType);
	}

	[TestMethod]
	public void Pictures_Set_ReplacesExisting ()
	{
		// Arrange
		var tag = new ApeTag ();
		var old = new ApePicture ("old.jpg", PictureType.FrontCover, new BinaryData ([0x47, 0x49]));
		tag.Pictures = [old];

		var @new = new ApePicture ("new.jpg", PictureType.BackCover, new BinaryData ([0xFF, 0xD8]));

		// Act
		tag.Pictures = [@new];

		// Assert
		Assert.AreEqual (1, tag.Pictures.Length);
		Assert.AreEqual ("new.jpg", ((ApePicture)tag.Pictures[0]).Filename);
	}

	[TestMethod]
	public void Pictures_Set_Null_ClearsPictures ()
	{
		// Arrange
		var tag = new ApeTag ();
		var picture = new ApePicture ("cover.jpg", PictureType.FrontCover, new BinaryData ([0xFF, 0xD8]));
		tag.Pictures = [picture];

		// Act
		tag.Pictures = null!;

		// Assert
		Assert.AreEqual (0, tag.Pictures.Length);
	}

	[TestMethod]
	public void Pictures_RoundTrip_PreservesData ()
	{
		// Arrange
		var tag = new ApeTag ();
		var imageData = new byte[256];
		for (int i = 0; i < 256; i++)
			imageData[i] = (byte)i;

		var original = new ApePicture ("cover.jpg", PictureType.FrontCover, new BinaryData (imageData));
		tag.Pictures = [original];

		// Act - Get pictures back
		var retrieved = tag.Pictures;

		// Assert
		Assert.AreEqual (1, retrieved.Length);
		Assert.AreEqual ("image/jpeg", retrieved[0].MimeType);
		Assert.AreEqual (PictureType.FrontCover, retrieved[0].PictureType);
		CollectionAssert.AreEqual (imageData, retrieved[0].PictureData.ToArray ());
	}

	[TestMethod]
	public void Pictures_AcceptsNonApePicture ()
	{
		// Arrange
		var tag = new ApeTag ();
		var genericPicture = new GenericPicture ("image/png", PictureType.BackCover, "Generic", new BinaryData ([0x89, 0x50]));

		// Act
		tag.Pictures = [genericPicture];

		// Assert
		Assert.AreEqual (1, tag.Pictures.Length);
		Assert.AreEqual ("image/png", tag.Pictures[0].MimeType);
	}

	[TestMethod]
	public void Pictures_UsesCorrectKeys_ForDifferentTypes ()
	{
		// Arrange
		var tag = new ApeTag ();
		var front = new ApePicture ("front.jpg", PictureType.FrontCover, new BinaryData ([0xFF, 0xD8]));
		var back = new ApePicture ("back.jpg", PictureType.BackCover, new BinaryData ([0xFF, 0xD8]));
		var media = new ApePicture ("media.jpg", PictureType.Media, new BinaryData ([0xFF, 0xD8]));
		var artist = new ApePicture ("artist.jpg", PictureType.Artist, new BinaryData ([0xFF, 0xD8]));

		// Act
		tag.Pictures = [front, back, media, artist];

		// Assert - All 4 pictures stored
		Assert.AreEqual (4, tag.Pictures.Length);

		// Verify they come back in expected order (front, back, media, artist)
		var retrieved = tag.Pictures;
		Assert.AreEqual (PictureType.FrontCover, retrieved[0].PictureType);
		Assert.AreEqual (PictureType.BackCover, retrieved[1].PictureType);
		Assert.AreEqual (PictureType.Media, retrieved[2].PictureType);
		Assert.AreEqual (PictureType.Artist, retrieved[3].PictureType);
	}

	// Simple IPicture implementation for testing
	sealed class GenericPicture : IPicture
	{
		public GenericPicture (string mimeType, PictureType pictureType, string description, BinaryData data)
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
