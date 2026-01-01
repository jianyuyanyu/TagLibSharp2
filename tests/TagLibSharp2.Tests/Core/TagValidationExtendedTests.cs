// Copyright (c) 2025 Stephen Shaw and contributors
// Extended validation tests for Tag

using TagLibSharp2.Core;
using TagLibSharp2.Id3.Id3v2;
using TagLibSharp2.Xiph;

namespace TagLibSharp2.Tests.Core;

[TestClass]
[TestCategory ("Unit")]
[TestCategory ("Validation")]
public class TagValidationExtendedTests
{
	[TestMethod]
	public void Validate_TrackExceedsTotal_HasWarning ()
	{
		var tag = new VorbisComment {
			Track = 15,
			TotalTracks = 10
		};

		var result = tag.Validate ();

		Assert.IsFalse (result.IsValid);
		Assert.IsTrue (result.HasWarnings);
		Assert.IsTrue (result.AllIssues.Any (i => i.Field == "Track"));
	}

	[TestMethod]
	public void Validate_DiscExceedsTotal_HasWarning ()
	{
		var tag = new VorbisComment {
			DiscNumber = 5,
			TotalDiscs = 3
		};

		var result = tag.Validate ();

		Assert.IsFalse (result.IsValid);
		Assert.IsTrue (result.HasWarnings);
		Assert.IsTrue (result.AllIssues.Any (i => i.Field == "DiscNumber"));
	}

	[TestMethod]
	public void Validate_YearTooOld_HasWarning ()
	{
		var tag = new VorbisComment { Year = "500" };

		var result = tag.Validate ();

		Assert.IsFalse (result.IsValid);
		Assert.IsTrue (result.HasWarnings);
		Assert.IsTrue (result.AllIssues.Any (i => i.Field == "Year"));
	}

	[TestMethod]
	public void Validate_YearTooFuture_HasWarning ()
	{
		var tag = new VorbisComment { Year = "2500" };

		var result = tag.Validate ();

		Assert.IsFalse (result.IsValid);
		Assert.IsTrue (result.AllIssues.Any (i => i.Field == "Year"));
	}

	[TestMethod]
	public void Validate_YearValid_NoWarning ()
	{
		var tag = new VorbisComment { Year = "2024" };

		var result = tag.Validate ();

		Assert.IsTrue (result.IsValid);
	}

	[TestMethod]
	public void Validate_IsrcWrongLength_HasError ()
	{
		var tag = new VorbisComment ();
		tag.Isrc = "ABC123"; // Too short

		var result = tag.Validate ();

		Assert.IsTrue (result.HasErrors);
		Assert.IsTrue (result.AllIssues.Any (i => i.Field == "ISRC" && i.Severity == ValidationSeverity.Error));
	}

	[TestMethod]
	public void Validate_IsrcWithDashes_ValidatesCorrectly ()
	{
		var tag = new VorbisComment ();
		tag.Isrc = "US-ABC-12-34567"; // Valid with dashes

		var result = tag.Validate ();

		// Should be valid - dashes are stripped for length check
		Assert.IsFalse (result.HasErrors);
	}

	[TestMethod]
	public void Validate_BpmTooLow_HasWarning ()
	{
		var tag = new VorbisComment { BeatsPerMinute = 10 };

		var result = tag.Validate ();

		Assert.IsTrue (result.HasWarnings);
		Assert.IsTrue (result.AllIssues.Any (i => i.Field == "BeatsPerMinute"));
	}

	[TestMethod]
	public void Validate_BpmTooHigh_HasWarning ()
	{
		var tag = new VorbisComment { BeatsPerMinute = 1500 };

		var result = tag.Validate ();

		Assert.IsTrue (result.HasWarnings);
		Assert.IsTrue (result.AllIssues.Any (i => i.Field == "BeatsPerMinute"));
	}

	[TestMethod]
	public void Validate_BpmValid_NoWarning ()
	{
		var tag = new VorbisComment { BeatsPerMinute = 120 };

		var result = tag.Validate ();

		Assert.IsFalse (result.AllIssues.Any (i => i.Field == "BeatsPerMinute"));
	}

	[TestMethod]
	public void Validate_AllValid_IsValid ()
	{
		var tag = new VorbisComment {
			Title = "Test",
			Track = 5,
			TotalTracks = 10,
			DiscNumber = 1,
			TotalDiscs = 2,
			Year = "2024"
		};
		tag.Isrc = "USABC1234567";
		tag.BeatsPerMinute = 128;

		var result = tag.Validate ();

		Assert.IsTrue (result.IsValid);
	}

	[TestMethod]
	public void IsEmpty_AllFieldsEmpty_ReturnsTrue ()
	{
		var tag = new VorbisComment ();

		Assert.IsTrue (tag.IsEmpty);
	}

	[TestMethod]
	public void IsEmpty_TitleSet_ReturnsFalse ()
	{
		var tag = new VorbisComment { Title = "Test" };

		Assert.IsFalse (tag.IsEmpty);
	}

	[TestMethod]
	public void IsEmpty_TrackSet_ReturnsFalse ()
	{
		var tag = new VorbisComment { Track = 1 };

		Assert.IsFalse (tag.IsEmpty);
	}

	[TestMethod]
	public void MusicBrainzReleaseArtistId_IsAliasForAlbumArtistId ()
	{
		var tag = new VorbisComment ();

		tag.MusicBrainzReleaseArtistId = "test-id";

		Assert.AreEqual ("test-id", tag.MusicBrainzAlbumArtistId);
		Assert.AreEqual (tag.MusicBrainzAlbumArtistId, tag.MusicBrainzReleaseArtistId);
	}

	[TestMethod]
	public void ValidationResult_GetIssues_FiltersBySeverity ()
	{
		var result = new ValidationResult ();
		result.AddWarning ("Field1", "Warning 1");
		result.AddError ("Field2", "Error 1");
		result.AddInfo ("Field3", "Info 1");

		var errors = result.GetIssues (ValidationSeverity.Error).ToList ();
		var warnings = result.GetIssues (ValidationSeverity.Warning).ToList ();
		var infos = result.GetIssues (ValidationSeverity.Info).ToList ();

		Assert.HasCount (1, errors);
		Assert.HasCount (1, warnings);
		Assert.HasCount (1, infos);
	}

	[TestMethod]
	public void ValidationResult_ErrorAndWarningCount_IsCorrect ()
	{
		var result = new ValidationResult ();
		result.AddWarning ("Field1", "Warning 1");
		result.AddWarning ("Field2", "Warning 2");
		result.AddError ("Field3", "Error 1");

		Assert.AreEqual (1, result.ErrorCount);
		Assert.AreEqual (2, result.WarningCount);
	}

	[TestMethod]
	public void ValidationIssue_ToString_IncludesSuggestedFix ()
	{
		var issue = new ValidationIssue ("Field", ValidationSeverity.Error, "Message", "Fix");

		var str = issue.ToString ();

		StringAssert.Contains (str, "Error");
		StringAssert.Contains (str, "Field");
		StringAssert.Contains (str, "Message");
		StringAssert.Contains (str, "Fix");
	}

	[TestMethod]
	public void ValidationResult_AddIssue_AddsToAllIssues ()
	{
		var result = new ValidationResult ();
		var issue = new ValidationIssue ("Field", ValidationSeverity.Warning, "Message");

		result.AddIssue (issue);

		Assert.HasCount (1, result.AllIssues);
		Assert.AreEqual (issue, result.AllIssues[0]);
	}
}
