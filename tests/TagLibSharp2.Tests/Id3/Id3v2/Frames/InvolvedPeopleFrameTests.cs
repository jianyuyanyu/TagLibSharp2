// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Id3.Id3v2;
using TagLibSharp2.Id3.Id3v2.Frames;

namespace TagLibSharp2.Tests.Id3.Id3v2.Frames;

/// <summary>
/// Tests for InvolvedPeopleFrame (TIPL, TMCL, IPLS).
/// </summary>
[TestClass]
[TestCategory ("Unit")]
[TestCategory ("Id3")]
[TestCategory ("Id3v2")]
[TestCategory ("Frame")]
public class InvolvedPeopleFrameTests
{
	// Basic Construction Tests

	[TestMethod]
	public void Constructor_WithEmptyPairs_CreatesEmptyFrame ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);

		Assert.AreEqual ("TMCL", frame.Id);
		Assert.AreEqual (0, frame.Count);
	}

	[TestMethod]
	public void Constructor_WithTipl_CreatesCorrectFrameId ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.InvolvedPeople);

		Assert.AreEqual ("TIPL", frame.Id);
	}

	[TestMethod]
	public void Constructor_WithTmcl_CreatesCorrectFrameId ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);

		Assert.AreEqual ("TMCL", frame.Id);
	}

	// Add/Get Tests

	[TestMethod]
	public void Add_SinglePair_Works ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);

		frame.Add ("guitar", "John Smith");

		Assert.AreEqual (1, frame.Count);
		Assert.AreEqual ("John Smith", frame.GetPerson ("guitar"));
	}

	[TestMethod]
	public void Add_MultiplePairs_Works ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);

		frame.Add ("guitar", "John Smith");
		frame.Add ("drums", "Jane Doe");
		frame.Add ("bass", "Bob Wilson");

		Assert.AreEqual (3, frame.Count);
		Assert.AreEqual ("John Smith", frame.GetPerson ("guitar"));
		Assert.AreEqual ("Jane Doe", frame.GetPerson ("drums"));
		Assert.AreEqual ("Bob Wilson", frame.GetPerson ("bass"));
	}

	[TestMethod]
	public void Add_DuplicateRole_AllowsMultiplePeople ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);

		frame.Add ("guitar", "John Smith");
		frame.Add ("guitar", "Jane Doe"); // Add another guitarist

		Assert.AreEqual (2, frame.Count);
		// GetPerson returns the first match
		Assert.AreEqual ("John Smith", frame.GetPerson ("guitar"));
		// GetPeopleForRole returns all matches
		var guitarists = frame.GetPeopleForRole ("guitar");
		Assert.HasCount (2, guitarists);
		Assert.AreEqual ("John Smith", guitarists[0]);
		Assert.AreEqual ("Jane Doe", guitarists[1]);
	}

	[TestMethod]
	public void Add_NullOrEmptyRole_ThrowsArgumentException ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);

		Assert.ThrowsExactly<ArgumentException> (() => frame.Add ("", "John"));
		Assert.ThrowsExactly<ArgumentException> (() => frame.Add (null!, "John"));
	}

	[TestMethod]
	public void Add_NullPerson_TreatedAsEmptyString ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);

		frame.Add ("guitar", null!);

		Assert.AreEqual (1, frame.Count);
		Assert.AreEqual ("", frame.GetPerson ("guitar"));
	}

	// Set Tests

	[TestMethod]
	public void Set_NewRole_AddsPair ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);

		frame.Set ("guitar", "John Smith");

		Assert.AreEqual (1, frame.Count);
		Assert.AreEqual ("John Smith", frame.GetPerson ("guitar"));
	}

	[TestMethod]
	public void Set_ExistingRole_ReplacesPerson ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);
		frame.Add ("guitar", "John Smith");

		frame.Set ("guitar", "Jane Doe");

		Assert.AreEqual (1, frame.Count);
		Assert.AreEqual ("Jane Doe", frame.GetPerson ("guitar"));
	}

	[TestMethod]
	public void Set_CaseInsensitiveRoleMatch ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);
		frame.Add ("Guitar", "John Smith");

		frame.Set ("GUITAR", "Jane Doe");

		Assert.AreEqual (1, frame.Count);
		Assert.AreEqual ("Jane Doe", frame.GetPerson ("guitar"));
	}

	// GetPerson Tests

	[TestMethod]
	public void GetPerson_NonExistentRole_ReturnsNull ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);
		frame.Add ("guitar", "John Smith");

		var result = frame.GetPerson ("piano");

		Assert.IsNull (result);
	}

	[TestMethod]
	public void GetPerson_CaseInsensitiveMatch ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);
		frame.Add ("Guitar", "John Smith");

		Assert.AreEqual ("John Smith", frame.GetPerson ("guitar"));
		Assert.AreEqual ("John Smith", frame.GetPerson ("GUITAR"));
	}

	// GetRoles/GetPeople Tests

	[TestMethod]
	public void GetRoles_ReturnsUniqueRoles ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);
		frame.Add ("guitar", "John Smith");
		frame.Add ("guitar", "Jane Doe"); // Duplicate role
		frame.Add ("drums", "Bob Wilson");

		var roles = frame.GetRoles ();

		Assert.HasCount (2, roles);
		Assert.IsTrue (roles.Contains ("guitar"));
		Assert.IsTrue (roles.Contains ("drums"));
	}

	[TestMethod]
	public void GetPeople_ReturnsAllPeople ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);
		frame.Add ("guitar", "John Smith");
		frame.Add ("drums", "Jane Doe");

		var people = frame.GetPeople ();

		Assert.HasCount (2, people);
		Assert.IsTrue (people.Contains ("John Smith"));
		Assert.IsTrue (people.Contains ("Jane Doe"));
	}

	[TestMethod]
	public void Pairs_ReturnsAllPairs ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);
		frame.Add ("guitar", "John Smith");
		frame.Add ("guitar", "Jane Doe");
		frame.Add ("drums", "Bob Wilson");

		var pairs = frame.Pairs;

		Assert.HasCount (3, pairs);
		Assert.AreEqual (("guitar", "John Smith"), pairs[0]);
		Assert.AreEqual (("guitar", "Jane Doe"), pairs[1]);
		Assert.AreEqual (("drums", "Bob Wilson"), pairs[2]);
	}

	// Clear/Remove Tests

	[TestMethod]
	public void Clear_RemovesAllPairs ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);
		frame.Add ("guitar", "John Smith");
		frame.Add ("drums", "Jane Doe");

		frame.Clear ();

		Assert.AreEqual (0, frame.Count);
	}

	[TestMethod]
	public void Remove_ExistingRole_RemovesAllMatchingPairs ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);
		frame.Add ("guitar", "John Smith");
		frame.Add ("guitar", "Jane Doe");
		frame.Add ("drums", "Bob Wilson");

		var removedCount = frame.Remove ("guitar");

		Assert.AreEqual (2, removedCount);
		Assert.AreEqual (1, frame.Count);
		Assert.IsNull (frame.GetPerson ("guitar"));
		Assert.AreEqual ("Bob Wilson", frame.GetPerson ("drums"));
	}

	[TestMethod]
	public void Remove_NonExistentRole_ReturnsZero ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);
		frame.Add ("guitar", "John Smith");

		var removedCount = frame.Remove ("piano");

		Assert.AreEqual (0, removedCount);
		Assert.AreEqual (1, frame.Count);
	}

	[TestMethod]
	public void RemovePair_ExistingPair_RemovesSpecificPair ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);
		frame.Add ("guitar", "John Smith");
		frame.Add ("guitar", "Jane Doe");

		var removed = frame.RemovePair ("guitar", "John Smith");

		Assert.IsTrue (removed);
		Assert.AreEqual (1, frame.Count);
		Assert.AreEqual ("Jane Doe", frame.GetPerson ("guitar"));
	}

	[TestMethod]
	public void RemovePair_NonExistentPair_ReturnsFalse ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);
		frame.Add ("guitar", "John Smith");

		var removed = frame.RemovePair ("guitar", "Jane Doe");

		Assert.IsFalse (removed);
		Assert.AreEqual (1, frame.Count);
	}

	// Read/Parse Tests

	[TestMethod]
	public void Read_ValidTmclFrame_ParsesPairs ()
	{
		// UTF-8 encoding, "guitar\0John Smith\0drums\0Jane Doe"
		var data = new byte[] {
			0x03, // UTF-8 encoding
			(byte)'g', (byte)'u', (byte)'i', (byte)'t', (byte)'a', (byte)'r', 0x00,
			(byte)'J', (byte)'o', (byte)'h', (byte)'n', (byte)' ', (byte)'S', (byte)'m', (byte)'i', (byte)'t', (byte)'h', 0x00,
			(byte)'d', (byte)'r', (byte)'u', (byte)'m', (byte)'s', 0x00,
			(byte)'J', (byte)'a', (byte)'n', (byte)'e', (byte)' ', (byte)'D', (byte)'o', (byte)'e'
		};

		var result = InvolvedPeopleFrame.Read ("TMCL", data, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (2, result.Frame!.Count);
		Assert.AreEqual ("John Smith", result.Frame.GetPerson ("guitar"));
		Assert.AreEqual ("Jane Doe", result.Frame.GetPerson ("drums"));
	}

	[TestMethod]
	public void Read_ValidTiplFrame_ParsesPairs ()
	{
		// UTF-8 encoding, "producer\0Phil Spector"
		var data = new byte[] {
			0x03, // UTF-8 encoding
			(byte)'p', (byte)'r', (byte)'o', (byte)'d', (byte)'u', (byte)'c', (byte)'e', (byte)'r', 0x00,
			(byte)'P', (byte)'h', (byte)'i', (byte)'l', (byte)' ', (byte)'S', (byte)'p', (byte)'e', (byte)'c', (byte)'t', (byte)'o', (byte)'r'
		};

		var result = InvolvedPeopleFrame.Read ("TIPL", data, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (1, result.Frame!.Count);
		Assert.AreEqual ("Phil Spector", result.Frame.GetPerson ("producer"));
	}

	[TestMethod]
	public void Read_ValidIplsFrame_ParsesPairs ()
	{
		// UTF-8 encoding for IPLS (ID3v2.3 equivalent)
		var data = new byte[] {
			0x03, // UTF-8 encoding
			(byte)'m', (byte)'i', (byte)'x', (byte)'e', (byte)'r', 0x00,
			(byte)'B', (byte)'o', (byte)'b', (byte)' ', (byte)'C', (byte)'l', (byte)'e', (byte)'a', (byte)'r', (byte)'m', (byte)'o', (byte)'u', (byte)'n', (byte)'t', (byte)'a', (byte)'i', (byte)'n'
		};

		var result = InvolvedPeopleFrame.Read ("IPLS", data, Id3v2Version.V23);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (1, result.Frame!.Count);
		Assert.AreEqual ("Bob Clearmountain", result.Frame.GetPerson ("mixer"));
	}

	[TestMethod]
	public void Read_EmptyData_ReturnsFailure ()
	{
		var result = InvolvedPeopleFrame.Read ("TMCL", ReadOnlySpan<byte>.Empty, Id3v2Version.V24);

		Assert.IsFalse (result.IsSuccess);
		Assert.AreEqual ("Frame data is empty", result.Error);
	}

	[TestMethod]
	public void Read_NullFrameId_ReturnsFailure ()
	{
		var data = new byte[] { 0x03 };

		var result = InvolvedPeopleFrame.Read (null!, data, Id3v2Version.V24);

		Assert.IsFalse (result.IsSuccess);
		Assert.AreEqual ("Frame ID cannot be null or empty", result.Error);
	}

	[TestMethod]
	public void Read_EmptyFrameId_ReturnsFailure ()
	{
		var data = new byte[] { 0x03 };

		var result = InvolvedPeopleFrame.Read ("", data, Id3v2Version.V24);

		Assert.IsFalse (result.IsSuccess);
		Assert.AreEqual ("Frame ID cannot be null or empty", result.Error);
	}

	[TestMethod]
	public void Read_InvalidEncoding_ReturnsFailure ()
	{
		var data = new byte[] { 0x05 }; // Invalid encoding (> 3)

		var result = InvolvedPeopleFrame.Read ("TMCL", data, Id3v2Version.V24);

		Assert.IsFalse (result.IsSuccess);
		Assert.Contains ("Invalid text encoding", result.Error!);
	}

	[TestMethod]
	public void Read_OnlyEncodingByte_ReturnsEmptyFrame ()
	{
		var data = new byte[] { 0x03 }; // Only UTF-8 encoding byte

		var result = InvolvedPeopleFrame.Read ("TMCL", data, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (0, result.Frame!.Count);
	}

	[TestMethod]
	public void Read_OddNumberOfStrings_SkipsIncompletePair ()
	{
		// UTF-8 encoding, "guitar\0John Smith\0drums" (missing person for drums)
		var data = new byte[] {
			0x03,
			(byte)'g', (byte)'u', (byte)'i', (byte)'t', (byte)'a', (byte)'r', 0x00,
			(byte)'J', (byte)'o', (byte)'h', (byte)'n', (byte)' ', (byte)'S', (byte)'m', (byte)'i', (byte)'t', (byte)'h', 0x00,
			(byte)'d', (byte)'r', (byte)'u', (byte)'m', (byte)'s'
		};

		var result = InvolvedPeopleFrame.Read ("TMCL", data, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (1, result.Frame!.Count); // Only complete pair
		Assert.AreEqual ("John Smith", result.Frame.GetPerson ("guitar"));
		Assert.IsNull (result.Frame.GetPerson ("drums"));
	}

	[TestMethod]
	public void Read_Latin1Encoding_ParsesCorrectly ()
	{
		// Latin1 encoding (0x00), "role\0name"
		var data = new byte[] {
			0x00, // Latin1 encoding
			(byte)'r', (byte)'o', (byte)'l', (byte)'e', 0x00,
			(byte)'n', (byte)'a', (byte)'m', (byte)'e'
		};

		var result = InvolvedPeopleFrame.Read ("TMCL", data, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (1, result.Frame!.Count);
		Assert.AreEqual ("name", result.Frame.GetPerson ("role"));
	}

	// Render Tests

	[TestMethod]
	public void RenderContent_EmptyFrame_ReturnsEncodingByteOnly ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);

		var content = frame.RenderContent ();

		Assert.AreEqual (1, content.Length);
		Assert.AreEqual (0x03, content.Span[0]); // UTF-8 encoding
	}

	[TestMethod]
	public void RenderContent_WithPairs_CreatesNullSeparatedData ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);
		frame.Add ("guitar", "John");

		var content = frame.RenderContent ();

		Assert.IsGreaterThan (1, content.Length);
		Assert.AreEqual (0x03, content.Span[0]); // UTF-8 encoding
	}

	// Round-trip Tests

	[TestMethod]
	public void RoundTrip_SinglePair_PreservesData ()
	{
		var original = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);
		original.Add ("guitar", "John Smith");

		var content = original.RenderContent ();
		var result = InvolvedPeopleFrame.Read ("TMCL", content.Span, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (1, result.Frame!.Count);
		Assert.AreEqual ("John Smith", result.Frame.GetPerson ("guitar"));
	}

	[TestMethod]
	public void RoundTrip_MultiplePairs_PreservesData ()
	{
		var original = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);
		original.Add ("lead vocals", "Freddie Mercury");
		original.Add ("guitar", "Brian May");
		original.Add ("bass", "John Deacon");
		original.Add ("drums", "Roger Taylor");

		var content = original.RenderContent ();
		var result = InvolvedPeopleFrame.Read ("TMCL", content.Span, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (4, result.Frame!.Count);
		Assert.AreEqual ("Freddie Mercury", result.Frame.GetPerson ("lead vocals"));
		Assert.AreEqual ("Brian May", result.Frame.GetPerson ("guitar"));
		Assert.AreEqual ("John Deacon", result.Frame.GetPerson ("bass"));
		Assert.AreEqual ("Roger Taylor", result.Frame.GetPerson ("drums"));
	}

	[TestMethod]
	public void RoundTrip_DuplicateRoles_PreservesAll ()
	{
		var original = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);
		original.Add ("guitar", "Brian May");
		original.Add ("guitar", "John Deacon"); // Two guitarists

		var content = original.RenderContent ();
		var result = InvolvedPeopleFrame.Read ("TMCL", content.Span, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (2, result.Frame!.Count);
		var guitarists = result.Frame.GetPeopleForRole ("guitar");
		Assert.HasCount (2, guitarists);
	}

	[TestMethod]
	public void RoundTrip_Unicode_PreservesCharacters ()
	{
		var original = new InvolvedPeopleFrame (InvolvedPeopleFrameType.MusicianCredits);
		original.Add ("ボーカル", "田中太郎"); // Japanese

		var content = original.RenderContent ();
		var result = InvolvedPeopleFrame.Read ("TMCL", content.Span, Id3v2Version.V24);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (1, result.Frame!.Count);
		Assert.AreEqual ("田中太郎", result.Frame.GetPerson ("ボーカル"));
	}

	// TIPL-specific tests (production credits)

	[TestMethod]
	public void InvolvedPeopleFrame_Tipl_StoresProductionCredits ()
	{
		var frame = new InvolvedPeopleFrame (InvolvedPeopleFrameType.InvolvedPeople);
		frame.Add ("producer", "Phil Spector");
		frame.Add ("engineer", "Eddie Kramer");
		frame.Add ("mixing", "Bob Clearmountain");

		Assert.AreEqual ("TIPL", frame.Id);
		Assert.AreEqual (3, frame.Count);
		Assert.AreEqual ("Phil Spector", frame.GetPerson ("producer"));
		Assert.AreEqual ("Eddie Kramer", frame.GetPerson ("engineer"));
		Assert.AreEqual ("Bob Clearmountain", frame.GetPerson ("mixing"));
	}
}
