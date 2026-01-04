// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Asf;

namespace TagLibSharp2.Tests.Asf;

[TestClass]
public class AsfTagSortFieldTests
{
	[TestMethod]
	public void TitleSort_GetSet ()
	{
		var tag = new AsfTag ();

		tag.TitleSort = "Sort Title";

		Assert.AreEqual ("Sort Title", tag.TitleSort);
	}

	[TestMethod]
	public void ArtistSort_GetSet ()
	{
		var tag = new AsfTag ();

		tag.ArtistSort = "Beatles, The";

		Assert.AreEqual ("Beatles, The", tag.ArtistSort);
	}

	[TestMethod]
	public void AlbumSort_GetSet ()
	{
		var tag = new AsfTag ();

		tag.AlbumSort = "White Album, The";

		Assert.AreEqual ("White Album, The", tag.AlbumSort);
	}

	[TestMethod]
	public void AlbumArtistSort_GetSet ()
	{
		var tag = new AsfTag ();

		tag.AlbumArtistSort = "Various Artists Sort";

		Assert.AreEqual ("Various Artists Sort", tag.AlbumArtistSort);
	}

	[TestMethod]
	public void ComposerSort_GetSet ()
	{
		var tag = new AsfTag ();

		tag.ComposerSort = "Bach, Johann Sebastian";

		Assert.AreEqual ("Bach, Johann Sebastian", tag.ComposerSort);
	}

	[TestMethod]
	public void SortFields_ReturnNull_WhenNotSet ()
	{
		var tag = new AsfTag ();

		Assert.IsNull (tag.TitleSort);
		Assert.IsNull (tag.ArtistSort);
		Assert.IsNull (tag.AlbumSort);
		Assert.IsNull (tag.AlbumArtistSort);
		Assert.IsNull (tag.ComposerSort);
	}

	[TestMethod]
	public void SortFields_SetToNull_Clears ()
	{
		var tag = new AsfTag ();
		tag.TitleSort = "Test";
		tag.ArtistSort = "Test";

		tag.TitleSort = null;
		tag.ArtistSort = null;

		Assert.IsNull (tag.TitleSort);
		Assert.IsNull (tag.ArtistSort);
	}
}
