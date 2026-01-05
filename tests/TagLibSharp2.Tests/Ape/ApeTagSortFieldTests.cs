// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Ape;

namespace TagLibSharp2.Tests.Ape;

[TestClass]
public class ApeTagSortFieldTests
{
	[TestMethod]
	public void TitleSort_GetSet ()
	{
		var tag = new ApeTag ();

		tag.TitleSort = "Title, The";

		Assert.AreEqual ("Title, The", tag.TitleSort);
	}

	[TestMethod]
	public void ArtistSort_GetSet ()
	{
		var tag = new ApeTag ();

		tag.ArtistSort = "Beatles, The";

		Assert.AreEqual ("Beatles, The", tag.ArtistSort);
	}

	[TestMethod]
	public void AlbumSort_GetSet ()
	{
		var tag = new ApeTag ();

		tag.AlbumSort = "Abbey Road (Remastered)";

		Assert.AreEqual ("Abbey Road (Remastered)", tag.AlbumSort);
	}

	[TestMethod]
	public void AlbumArtistSort_GetSet ()
	{
		var tag = new ApeTag ();

		tag.AlbumArtistSort = "Various Artists";

		Assert.AreEqual ("Various Artists", tag.AlbumArtistSort);
	}

	[TestMethod]
	public void ComposerSort_GetSet ()
	{
		var tag = new ApeTag ();

		tag.ComposerSort = "Beethoven, Ludwig van";

		Assert.AreEqual ("Beethoven, Ludwig van", tag.ComposerSort);
	}

	[TestMethod]
	public void SortFields_ReturnNull_WhenNotSet ()
	{
		var tag = new ApeTag ();

		Assert.IsNull (tag.TitleSort);
		Assert.IsNull (tag.ArtistSort);
		Assert.IsNull (tag.AlbumSort);
		Assert.IsNull (tag.AlbumArtistSort);
		Assert.IsNull (tag.ComposerSort);
	}

	[TestMethod]
	public void SortFields_SetToNull_Clears ()
	{
		var tag = new ApeTag ();
		tag.TitleSort = "Test Sort";
		tag.ArtistSort = "Test Artist Sort";

		tag.TitleSort = null;
		tag.ArtistSort = null;

		Assert.IsNull (tag.TitleSort);
		Assert.IsNull (tag.ArtistSort);
	}

	[TestMethod]
	public void SortFields_RoundTrip ()
	{
		// Arrange
		var tag = new ApeTag ();
		tag.TitleSort = "Sort Title";
		tag.ArtistSort = "Sort Artist";
		tag.AlbumSort = "Sort Album";
		tag.AlbumArtistSort = "Sort Album Artist";
		tag.ComposerSort = "Sort Composer";

		// Act - Render and re-parse
		var rendered = tag.RenderWithOptions (includeHeader: true);
		var parsed = ApeTag.Parse (rendered.Span);

		// Assert
		Assert.IsTrue (parsed.IsSuccess);
		Assert.AreEqual ("Sort Title", parsed.Tag!.TitleSort);
		Assert.AreEqual ("Sort Artist", parsed.Tag!.ArtistSort);
		Assert.AreEqual ("Sort Album", parsed.Tag!.AlbumSort);
		Assert.AreEqual ("Sort Album Artist", parsed.Tag!.AlbumArtistSort);
		Assert.AreEqual ("Sort Composer", parsed.Tag!.ComposerSort);
	}

	[TestMethod]
	public void TitleSort_FallbackToUppercase ()
	{
		// Test that we can read both TitleSort and TITLESORT formats
		var tag = new ApeTag ();
		tag.SetValue ("TITLESORT", "Uppercase Sort");

		Assert.AreEqual ("Uppercase Sort", tag.TitleSort);
	}

	[TestMethod]
	public void ArtistSort_FallbackToUppercase ()
	{
		var tag = new ApeTag ();
		tag.SetValue ("ARTISTSORT", "Uppercase Artist Sort");

		Assert.AreEqual ("Uppercase Artist Sort", tag.ArtistSort);
	}

	[TestMethod]
	public void AlbumSort_FallbackToUppercase ()
	{
		var tag = new ApeTag ();
		tag.SetValue ("ALBUMSORT", "Uppercase Album Sort");

		Assert.AreEqual ("Uppercase Album Sort", tag.AlbumSort);
	}
}
