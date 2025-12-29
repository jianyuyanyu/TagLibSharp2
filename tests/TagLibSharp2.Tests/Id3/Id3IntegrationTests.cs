// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Globalization;
using TagLibSharp2.Id3;
using TagLibSharp2.Id3.Id3v2;
using TagLibSharp2.Tests.Core;

namespace TagLibSharp2.Tests.Id3;

/// <summary>
/// Integration tests that require real MP3 files.
/// </summary>
/// <remarks>
/// These tests are skipped by default. To run them:
/// 1. Set the TAGLIB_TEST_MP3 environment variable to a path to an MP3 file with ID3 tags
/// 2. Run with: dotnet test --filter "TestCategory=Integration"
///
/// Example: TAGLIB_TEST_MP3=/path/to/song.mp3 dotnet test --filter "TestCategory=Integration"
/// </remarks>
[TestClass]
[TestCategory ("Integration")]
[TestCategory ("Manual")]
[TestCategory ("Id3")]
public class Id3IntegrationTests : FileFormatTestBase
{
	const string TestFileEnvVar = "TAGLIB_TEST_MP3";

	[TestMethod]
	public void Read_RealMp3_ParsesId3v2Header ()
	{
		var path = SkipIfNoTestFile (TestFileEnvVar, "MP3");

		var buffer = new byte[10];
		using var fs = File.OpenRead (path);
		_ = fs.Read (buffer, 0, 10);

		var result = Id3v2Header.Read (buffer);

		Console.WriteLine ($"ID3v2 Header Found: {result.IsSuccess}");
		if (result.IsSuccess) {
			Console.WriteLine ($"  Version: 2.{result.Header.MajorVersion}.{result.Header.MinorVersion}");
			Console.WriteLine ($"  Tag Size: {result.Header.TagSize} bytes");
			Console.WriteLine ($"  Has Extended Header: {result.Header.HasExtendedHeader}");
			Console.WriteLine ($"  Has Footer: {result.Header.HasFooter}");
		}

		Assert.IsTrue (result.IsSuccess, "Expected to find ID3v2 header in test file");
	}

	[TestMethod]
	public void Read_RealMp3_ParsesId3v2Tag ()
	{
		var path = SkipIfNoTestFile (TestFileEnvVar, "MP3");

		byte[] buffer;
		using (var fs = File.OpenRead (path)) {
			var header = new byte[10];
			_ = fs.Read (header, 0, 10);
			var headerResult = Id3v2Header.Read (header);

			if (!headerResult.IsSuccess) {
				Assert.Fail ("No ID3v2 header found in test file");
				return;
			}

			var totalSize = (int)headerResult.Header.TotalSize;
			fs.Position = 0;
			buffer = new byte[totalSize];
			_ = fs.Read (buffer, 0, totalSize);
		}

		var result = Id3v2Tag.Read (buffer);

		Console.WriteLine ($"ID3v2 Tag Parsed: {result.IsSuccess}");
		if (result.IsSuccess) {
			var tag = result.Tag!;
			Console.WriteLine ($"  Title: {tag.Title ?? "(none)"}");
			Console.WriteLine ($"  Artist: {tag.Artist ?? "(none)"}");
			Console.WriteLine ($"  Album: {tag.Album ?? "(none)"}");
			Console.WriteLine ($"  Year: {tag.Year ?? "(none)"}");
			Console.WriteLine ($"  Genre: {tag.Genre ?? "(none)"}");
			Console.WriteLine ($"  Track: {tag.Track?.ToString (CultureInfo.InvariantCulture) ?? "(none)"}");
			Console.WriteLine ($"  Frame Count: {tag.Frames.Count}");

			Console.WriteLine ("  All Frames:");
			foreach (var frame in tag.Frames)
				Console.WriteLine ($"    {frame.Id}: {frame.Text}");
		}

		Assert.IsTrue (result.IsSuccess, "Expected to parse ID3v2 tag from test file");
	}

	[TestMethod]
	public void Read_RealMp3_ChecksForId3v1 ()
	{
		var path = SkipIfNoTestFile (TestFileEnvVar, "MP3");

		var buffer = new byte[128];
		using (var fs = File.OpenRead (path)) {
			fs.Seek (-128, SeekOrigin.End);
			_ = fs.Read (buffer, 0, 128);
		}

		var result = Id3v1Tag.Read (buffer);

		Console.WriteLine ($"ID3v1 Tag Found: {!result.IsNotFound}");
		if (result.IsSuccess) {
			var tag = result.Tag!;
			Console.WriteLine ($"  Title: {tag.Title ?? "(none)"}");
			Console.WriteLine ($"  Artist: {tag.Artist ?? "(none)"}");
			Console.WriteLine ($"  Album: {tag.Album ?? "(none)"}");
			Console.WriteLine ($"  Year: {tag.Year ?? "(none)"}");
			Console.WriteLine ($"  Genre: {tag.Genre ?? "(none)"}");
			Console.WriteLine ($"  Track: {tag.Track?.ToString (CultureInfo.InvariantCulture) ?? "(none)"}");
			Console.WriteLine ($"  Is v1.1: {tag.IsVersion11}");
		} else if (result.IsNotFound) {
			Console.WriteLine ("  No ID3v1 tag present (common for modern files)");
		}

		// Don't fail if no ID3v1 - many modern files only have ID3v2
	}

	[TestMethod]
	public void Read_RealMp3_ParsesAlbumArt ()
	{
		var path = SkipIfNoTestFile (TestFileEnvVar, "MP3");

		byte[] buffer;
		using (var fs = File.OpenRead (path)) {
			var header = new byte[10];
			_ = fs.Read (header, 0, 10);
			var headerResult = Id3v2Header.Read (header);

			if (!headerResult.IsSuccess) {
				Assert.Fail ("No ID3v2 header found in test file");
				return;
			}

			var totalSize = (int)headerResult.Header.TotalSize;
			fs.Position = 0;
			buffer = new byte[totalSize];
			_ = fs.Read (buffer, 0, totalSize);
		}

		var result = Id3v2Tag.Read (buffer);

		Assert.IsTrue (result.IsSuccess, "Expected to parse ID3v2 tag from test file");

		var pictures = result.Tag!.Pictures;
		Console.WriteLine ($"Album Art Frames: {pictures.Length}");

		if (pictures.Length > 0) {
			foreach (var pic in pictures) {
				Console.WriteLine ($"  Picture Type: {pic.PictureType}");
				Console.WriteLine ($"  MIME Type: {pic.MimeType}");
				Console.WriteLine ($"  Description: {pic.Description}");
				Console.WriteLine ($"  Data Size: {pic.PictureData.Length} bytes");
			}

			var first = pictures[0];
			Assert.IsFalse (string.IsNullOrEmpty (first.MimeType), "Picture should have MIME type");
			Assert.IsGreaterThan (0, first.PictureData.Length, "Picture should have data");
		} else {
			Console.WriteLine ("  No album art found in this file");
		}
	}
}
