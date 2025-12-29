// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Globalization;
using TagLibSharp2.Ogg;
using TagLibSharp2.Tests.Core;

namespace TagLibSharp2.Tests.Ogg;

/// <summary>
/// Integration tests that require real Ogg Vorbis files.
/// </summary>
/// <remarks>
/// These tests are skipped by default. To run them:
/// 1. Set the TAGLIB_TEST_OGG environment variable to a path to an Ogg Vorbis file
/// 2. Run with: dotnet test --filter "TestCategory=Integration"
///
/// Example: TAGLIB_TEST_OGG=/path/to/song.ogg dotnet test --filter "TestCategory=Integration"
/// </remarks>
[TestClass]
[TestCategory ("Integration")]
[TestCategory ("Manual")]
[TestCategory ("Ogg")]
public class OggIntegrationTests : FileFormatTestBase
{
	const string TestFileEnvVar = "TAGLIB_TEST_OGG";

	[TestMethod]
	public void Read_RealOgg_ParsesMetadata ()
	{
		var path = SkipIfNoTestFile (TestFileEnvVar, "Ogg Vorbis");

		var fileData = File.ReadAllBytes (path);
		var result = OggVorbisFile.Read (fileData);

		Console.WriteLine ($"Ogg Vorbis File Parsed: {result.IsSuccess}");

		if (!result.IsSuccess) {
			Console.WriteLine ($"  Error: {result.Error}");
			Assert.Fail ($"Failed to parse Ogg Vorbis file: {result.Error}");
			return;
		}

		var file = result.File!;
		Console.WriteLine ($"  Title: {file.Title ?? "(none)"}");
		Console.WriteLine ($"  Artist: {file.Artist ?? "(none)"}");
		Console.WriteLine ($"  Album: {file.Album ?? "(none)"}");
		Console.WriteLine ($"  Year: {file.Year ?? "(none)"}");
		Console.WriteLine ($"  Genre: {file.Genre ?? "(none)"}");
		Console.WriteLine ($"  Track: {file.Track?.ToString (CultureInfo.InvariantCulture) ?? "(none)"}");

		if (file.VorbisComment is not null) {
			Console.WriteLine ($"  Vendor String: {file.VorbisComment.VendorString}");
			Console.WriteLine ($"  Field Count: {file.VorbisComment.Fields.Count}");
			Console.WriteLine ("  All Fields:");
			foreach (var field in file.VorbisComment.Fields)
				Console.WriteLine ($"    {field.Name}={field.Value}");
		}

		Assert.IsNotNull (file.VorbisComment, "Expected Vorbis Comment to be parsed");
	}

	[TestMethod]
	public void Read_RealOgg_ValidatesCrcOnRequest ()
	{
		var path = SkipIfNoTestFile (TestFileEnvVar, "Ogg Vorbis");

		var fileData = File.ReadAllBytes (path);

		// Parse first Ogg page with CRC validation
		var result = OggPage.Read (fileData, validateCrc: true);

		Console.WriteLine ($"First Ogg Page CRC Valid: {result.IsSuccess}");

		if (!result.IsSuccess) {
			Console.WriteLine ($"  Error: {result.Error}");
		} else {
			Console.WriteLine ($"  Page Flags: {result.Page.Flags}");
			Console.WriteLine ($"  Serial Number: {result.Page.SerialNumber}");
			Console.WriteLine ($"  Sequence Number: {result.Page.SequenceNumber}");
			Console.WriteLine ($"  Data Length: {result.Page.Data.Length}");
		}

		Assert.IsTrue (result.IsSuccess, "Expected first page CRC to be valid");
	}

	[TestMethod]
	public void Read_RealOgg_FirstPageIsBOS ()
	{
		var path = SkipIfNoTestFile (TestFileEnvVar, "Ogg Vorbis");

		var fileData = File.ReadAllBytes (path);
		var result = OggPage.Read (fileData);

		Assert.IsTrue (result.IsSuccess, "Expected to parse first page");
		Assert.IsTrue (result.Page.IsBeginOfStream, "First page should have BOS flag");
		Assert.IsFalse (result.Page.IsEndOfStream, "First page should not have EOS flag");
		Assert.IsFalse (result.Page.IsContinuation, "First page should not be a continuation");
	}

	[TestMethod]
	public void Read_RealOgg_ContainsVorbisIdentificationHeader ()
	{
		var path = SkipIfNoTestFile (TestFileEnvVar, "Ogg Vorbis");

		var fileData = File.ReadAllBytes (path);
		var result = OggPage.Read (fileData);

		Assert.IsTrue (result.IsSuccess, "Expected to parse first page");

		// First page data should start with Vorbis identification header
		var data = result.Page.Data.Span;
		Assert.IsGreaterThanOrEqualTo (7, data.Length, "First page should have at least 7 bytes");

		// Packet type 1 + "vorbis"
		Assert.AreEqual ((byte)1, data[0], "Expected packet type 1 (identification)");
		Assert.AreEqual ((byte)'v', data[1], "Expected 'v' in vorbis magic");
		Assert.AreEqual ((byte)'o', data[2], "Expected 'o' in vorbis magic");
		Assert.AreEqual ((byte)'r', data[3], "Expected 'r' in vorbis magic");
		Assert.AreEqual ((byte)'b', data[4], "Expected 'b' in vorbis magic");
		Assert.AreEqual ((byte)'i', data[5], "Expected 'i' in vorbis magic");
		Assert.AreEqual ((byte)'s', data[6], "Expected 's' in vorbis magic");

		Console.WriteLine ("Verified Vorbis identification header in first page");
	}
}
