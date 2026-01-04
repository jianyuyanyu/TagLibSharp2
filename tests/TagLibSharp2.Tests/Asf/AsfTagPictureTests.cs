// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Asf;
using TagLibSharp2.Core;

namespace TagLibSharp2.Tests.Asf;

[TestClass]
public class AsfTagPictureTests
{
	[TestMethod]
	public void Pictures_Get_ReturnsEmpty_WhenNoPictures ()
	{
		// Arrange
		var tag = new AsfTag ();

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
		var tag = new AsfTag ();
		var imageData = new BinaryData ([0xFF, 0xD8, 0xFF, 0xE0]);
		var picture = new AsfPicture ("image/jpeg", PictureType.FrontCover, "Front", imageData);

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
		var tag = new AsfTag ();
		var front = new AsfPicture ("image/jpeg", PictureType.FrontCover, "Front", new BinaryData ([0xFF, 0xD8]));
		var back = new AsfPicture ("image/png", PictureType.BackCover, "Back", new BinaryData ([0x89, 0x50]));

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
		var tag = new AsfTag ();
		var old = new AsfPicture ("image/gif", PictureType.Other, "Old", new BinaryData ([0x47, 0x49]));
		tag.Pictures = [old];

		var @new = new AsfPicture ("image/jpeg", PictureType.FrontCover, "New", new BinaryData ([0xFF, 0xD8]));

		// Act
		tag.Pictures = [@new];

		// Assert
		Assert.AreEqual (1, tag.Pictures.Length);
		Assert.AreEqual ("New", tag.Pictures[0].Description);
	}

	[TestMethod]
	public void Pictures_Set_Null_ClearsPictures ()
	{
		// Arrange
		var tag = new AsfTag ();
		var picture = new AsfPicture ("image/jpeg", PictureType.FrontCover, "Test", new BinaryData ([0xFF, 0xD8]));
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
		var tag = new AsfTag ();
		var imageData = new byte[256];
		for (int i = 0; i < 256; i++)
			imageData[i] = (byte)i;

		var original = new AsfPicture ("image/jpeg", PictureType.FrontCover, "Album artwork", new BinaryData (imageData));
		tag.Pictures = [original];

		// Act - Get pictures back
		var retrieved = tag.Pictures;

		// Assert
		Assert.AreEqual (1, retrieved.Length);
		Assert.AreEqual ("image/jpeg", retrieved[0].MimeType);
		Assert.AreEqual (PictureType.FrontCover, retrieved[0].PictureType);
		Assert.AreEqual ("Album artwork", retrieved[0].Description);
		CollectionAssert.AreEqual (imageData, retrieved[0].PictureData.ToArray ());
	}

	[TestMethod]
	public void Pictures_AcceptsNonAsfPicture ()
	{
		// Arrange
		var tag = new AsfTag ();
		var genericPicture = new GenericPicture ("image/png", PictureType.BackCover, "Generic", new BinaryData ([0x89, 0x50]));

		// Act
		tag.Pictures = [genericPicture];

		// Assert
		Assert.AreEqual (1, tag.Pictures.Length);
		Assert.AreEqual ("image/png", tag.Pictures[0].MimeType);
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
