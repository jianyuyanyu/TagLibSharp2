// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Id3;

namespace TagLibSharp2.Tests.Id3;

[TestClass]
public class Id3v1GenreTests
{
	[TestMethod]
	public void GetName_Index0_ReturnsBlues ()
	{
		Assert.AreEqual ("Blues", Id3v1Genre.GetName (0));
	}

	[TestMethod]
	public void GetName_StandardGenres_ReturnCorrectNames ()
	{
		// Test a selection of standard ID3v1 genres (0-79)
		Assert.AreEqual ("Classic Rock", Id3v1Genre.GetName (1));
		Assert.AreEqual ("Country", Id3v1Genre.GetName (2));
		Assert.AreEqual ("Jazz", Id3v1Genre.GetName (8));
		Assert.AreEqual ("Metal", Id3v1Genre.GetName (9));
		Assert.AreEqual ("Rock", Id3v1Genre.GetName (17));
		Assert.AreEqual ("Pop", Id3v1Genre.GetName (13));
		Assert.AreEqual ("Hip-Hop", Id3v1Genre.GetName (7));
		Assert.AreEqual ("Classical", Id3v1Genre.GetName (32));
		Assert.AreEqual ("Hard Rock", Id3v1Genre.GetName (79));
	}

	[TestMethod]
	public void GetName_WinampExtensions_ReturnCorrectNames ()
	{
		// Test Winamp extensions (80-191)
		Assert.AreEqual ("Folk", Id3v1Genre.GetName (80));
		Assert.AreEqual ("Bluegrass", Id3v1Genre.GetName (89));
		Assert.AreEqual ("Acoustic", Id3v1Genre.GetName (99));
		Assert.AreEqual ("Heavy Metal", Id3v1Genre.GetName (137));
		Assert.AreEqual ("Black Metal", Id3v1Genre.GetName (138));
		Assert.AreEqual ("JPop", Id3v1Genre.GetName (146));
		Assert.AreEqual ("Synthpop", Id3v1Genre.GetName (147));
		Assert.AreEqual ("Dubstep", Id3v1Genre.GetName (189));
		Assert.AreEqual ("Psybient", Id3v1Genre.GetName (191));
	}

	[TestMethod]
	public void GetName_Index192_ReturnsNull ()
	{
		// Index 192 is out of range
		Assert.IsNull (Id3v1Genre.GetName (192));
	}

	[TestMethod]
	public void GetName_Index255_ReturnsNull ()
	{
		// Index 255 typically means "undefined" in ID3v1
		Assert.IsNull (Id3v1Genre.GetName (255));
	}

	[TestMethod]
	public void GetIndex_Blues_Returns0 ()
	{
		Assert.AreEqual ((byte)0, Id3v1Genre.GetIndex ("Blues"));
	}

	[TestMethod]
	public void GetIndex_CaseInsensitive_ReturnsCorrectIndex ()
	{
		Assert.AreEqual ((byte)0, Id3v1Genre.GetIndex ("blues"));
		Assert.AreEqual ((byte)0, Id3v1Genre.GetIndex ("BLUES"));
		Assert.AreEqual ((byte)0, Id3v1Genre.GetIndex ("BlUeS"));
		Assert.AreEqual ((byte)17, Id3v1Genre.GetIndex ("rock"));
		Assert.AreEqual ((byte)17, Id3v1Genre.GetIndex ("ROCK"));
	}

	[TestMethod]
	public void GetIndex_StandardGenres_ReturnsCorrectIndices ()
	{
		Assert.AreEqual ((byte)1, Id3v1Genre.GetIndex ("Classic Rock"));
		Assert.AreEqual ((byte)8, Id3v1Genre.GetIndex ("Jazz"));
		Assert.AreEqual ((byte)9, Id3v1Genre.GetIndex ("Metal"));
		Assert.AreEqual ((byte)13, Id3v1Genre.GetIndex ("Pop"));
		Assert.AreEqual ((byte)32, Id3v1Genre.GetIndex ("Classical"));
	}

	[TestMethod]
	public void GetIndex_WinampExtensions_ReturnsCorrectIndices ()
	{
		Assert.AreEqual ((byte)80, Id3v1Genre.GetIndex ("Folk"));
		Assert.AreEqual ((byte)137, Id3v1Genre.GetIndex ("Heavy Metal"));
		Assert.AreEqual ((byte)146, Id3v1Genre.GetIndex ("JPop"));
		Assert.AreEqual ((byte)189, Id3v1Genre.GetIndex ("Dubstep"));
	}

	[TestMethod]
	public void GetIndex_NotFound_Returns255 ()
	{
		Assert.AreEqual ((byte)255, Id3v1Genre.GetIndex ("Not A Real Genre"));
		Assert.AreEqual ((byte)255, Id3v1Genre.GetIndex ("xyz"));
	}

	[TestMethod]
	public void GetIndex_Null_Returns255 ()
	{
		Assert.AreEqual ((byte)255, Id3v1Genre.GetIndex (null));
	}

	[TestMethod]
	public void GetIndex_EmptyString_Returns255 ()
	{
		Assert.AreEqual ((byte)255, Id3v1Genre.GetIndex (string.Empty));
	}

	[TestMethod]
	public void Count_ReturnsCorrectValue ()
	{
		// ID3v1 has 80 standard genres (0-79) plus 112 Winamp extensions (80-191) = 192 total
		Assert.AreEqual (192, Id3v1Genre.Count);
	}

	[TestMethod]
	public void GetName_AllIndices_DoNotThrow ()
	{
		// Verify all valid indices return a name without throwing
		for (byte i = 0; i < 192; i++) {
			var name = Id3v1Genre.GetName (i);
			Assert.IsNotNull (name, $"Genre at index {i} should not be null");
			Assert.IsFalse (string.IsNullOrEmpty (name), $"Genre at index {i} should not be empty");
		}
	}

	[TestMethod]
	public void GetIndex_AllGenres_RoundTrip ()
	{
		// Verify all genres can round-trip: index -> name -> index
		for (byte i = 0; i < 192; i++) {
			var name = Id3v1Genre.GetName (i);
			var index = Id3v1Genre.GetIndex (name);
			Assert.AreEqual (i, index, $"Round-trip failed for genre '{name}' at index {i}");
		}
	}

	[TestMethod]
	public void GetName_GenresWithSpecialCharacters_ReturnCorrectNames ()
	{
		// Test genres with special characters
		Assert.AreEqual ("R&B", Id3v1Genre.GetName (14));
		Assert.AreEqual ("Jazz+Funk", Id3v1Genre.GetName (29));
		Assert.AreEqual ("Pop/Funk", Id3v1Genre.GetName (62));
		Assert.AreEqual ("Rock & Roll", Id3v1Genre.GetName (78));
		Assert.AreEqual ("Drum & Bass", Id3v1Genre.GetName (127));
	}

	[TestMethod]
	public void GetIndex_GenresWithSpecialCharacters_ReturnsCorrectIndices ()
	{
		Assert.AreEqual ((byte)14, Id3v1Genre.GetIndex ("R&B"));
		Assert.AreEqual ((byte)29, Id3v1Genre.GetIndex ("Jazz+Funk"));
		Assert.AreEqual ((byte)78, Id3v1Genre.GetIndex ("Rock & Roll"));
	}

	[TestMethod]
	public void GetName_LastValidIndex_ReturnsPsybient ()
	{
		Assert.AreEqual ("Psybient", Id3v1Genre.GetName (191));
	}

	[TestMethod]
	public void GetName_BoundaryConditions ()
	{
		// First valid
		Assert.IsNotNull (Id3v1Genre.GetName (0));

		// Last valid
		Assert.IsNotNull (Id3v1Genre.GetName (191));

		// First invalid
		Assert.IsNull (Id3v1Genre.GetName (192));

		// Max byte value
		Assert.IsNull (Id3v1Genre.GetName (255));
	}
}
