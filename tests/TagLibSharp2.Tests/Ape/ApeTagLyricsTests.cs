// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Ape;

namespace TagLibSharp2.Tests.Ape;

[TestClass]
public class ApeTagLyricsTests
{
	[TestMethod]
	public void Lyrics_GetSet ()
	{
		var tag = new ApeTag ();

		tag.Lyrics = "These are the song lyrics\nLine 2 of lyrics";

		Assert.AreEqual ("These are the song lyrics\nLine 2 of lyrics", tag.Lyrics);
	}

	[TestMethod]
	public void Lyrics_ReturnsNull_WhenNotSet ()
	{
		var tag = new ApeTag ();

		Assert.IsNull (tag.Lyrics);
	}

	[TestMethod]
	public void Lyrics_SetToNull_Clears ()
	{
		var tag = new ApeTag ();
		tag.Lyrics = "Some lyrics";

		tag.Lyrics = null;

		Assert.IsNull (tag.Lyrics);
	}

	[TestMethod]
	public void Lyrics_PreservesUnicode ()
	{
		var tag = new ApeTag ();
		var japaneseLyrics = "日本語の歌詞\n二行目";

		tag.Lyrics = japaneseLyrics;

		Assert.AreEqual (japaneseLyrics, tag.Lyrics);
	}

	[TestMethod]
	public void Lyrics_RoundTrip ()
	{
		// Arrange
		var tag = new ApeTag ();
		var lyrics = "First verse\nSecond line\n\nChorus starts here";
		tag.Lyrics = lyrics;

		// Act - Render and re-parse
		var rendered = tag.RenderWithOptions (includeHeader: true);
		var parsed = ApeTag.Parse (rendered.Span);

		// Assert
		Assert.IsTrue (parsed.IsSuccess);
		Assert.AreEqual (lyrics, parsed.Tag!.Lyrics);
	}

	[TestMethod]
	public void Subtitle_GetSet ()
	{
		var tag = new ApeTag ();

		tag.Subtitle = "Live at Madison Square Garden";

		Assert.AreEqual ("Live at Madison Square Garden", tag.Subtitle);
	}

	[TestMethod]
	public void Publisher_GetSet ()
	{
		var tag = new ApeTag ();

		tag.Publisher = "Sony Music";

		Assert.AreEqual ("Sony Music", tag.Publisher);
	}

	[TestMethod]
	public void Isrc_GetSet ()
	{
		var tag = new ApeTag ();

		tag.Isrc = "USRC11234567";

		Assert.AreEqual ("USRC11234567", tag.Isrc);
	}

	[TestMethod]
	public void Barcode_GetSet ()
	{
		var tag = new ApeTag ();

		tag.Barcode = "0123456789012";

		Assert.AreEqual ("0123456789012", tag.Barcode);
	}

	[TestMethod]
	public void CatalogNumber_GetSet ()
	{
		var tag = new ApeTag ();

		tag.CatalogNumber = "CAT-001";

		Assert.AreEqual ("CAT-001", tag.CatalogNumber);
	}

	[TestMethod]
	public void Language_GetSet ()
	{
		var tag = new ApeTag ();

		tag.Language = "English";

		Assert.AreEqual ("English", tag.Language);
	}

	[TestMethod]
	public void ExtendedMetadata_RoundTrip ()
	{
		// Arrange
		var tag = new ApeTag ();
		tag.Subtitle = "Live Version";
		tag.Publisher = "Universal Music";
		tag.Isrc = "GBDVX1234567";
		tag.Barcode = "9876543210123";
		tag.CatalogNumber = "UMG-2024-001";
		tag.Language = "French";

		// Act - Render and re-parse
		var rendered = tag.RenderWithOptions (includeHeader: true);
		var parsed = ApeTag.Parse (rendered.Span);

		// Assert
		Assert.IsTrue (parsed.IsSuccess);
		Assert.AreEqual ("Live Version", parsed.Tag!.Subtitle);
		Assert.AreEqual ("Universal Music", parsed.Tag!.Publisher);
		Assert.AreEqual ("GBDVX1234567", parsed.Tag!.Isrc);
		Assert.AreEqual ("9876543210123", parsed.Tag!.Barcode);
		Assert.AreEqual ("UMG-2024-001", parsed.Tag!.CatalogNumber);
		Assert.AreEqual ("French", parsed.Tag!.Language);
	}
}
