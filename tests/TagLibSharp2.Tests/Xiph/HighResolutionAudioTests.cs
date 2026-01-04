// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Xiph;

namespace TagLibSharp2.Tests.Xiph;

/// <summary>
/// Tests for high-resolution FLAC audio format support.
/// Validates proper handling of high sample rates, bit depths, and surround configurations.
/// </summary>
/// <remarks>
/// Added per audiophile review recommendation to validate edge cases
/// at extreme audio resolutions before v0.5.0 release.
/// </remarks>
[TestClass]
[TestCategory ("Unit")]
[TestCategory ("HighResolution")]
public class HighResolutionAudioTests
{
	[TestMethod]
	public void Flac_24bit96kHz_ParsesCorrectly ()
	{
		// Arrange - 24-bit/96kHz stereo FLAC
		var data = TestBuilders.Flac.CreateWithStreamInfo (
			sampleRate: 96000,
			channels: 2,
			bitsPerSample: 24,
			totalSamples: 96000 * 60); // 1 minute

		// Act
		var result = FlacFile.Read (data);

		// Assert
		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (96000, result.File!.Properties.SampleRate);
		Assert.AreEqual (2, result.File.Properties.Channels);
		Assert.AreEqual (24, result.File.Properties.BitsPerSample);
	}

	[TestMethod]
	public void Flac_24bit192kHz_ParsesCorrectly ()
	{
		// Arrange - 24-bit/192kHz stereo FLAC (common high-res format)
		var data = TestBuilders.Flac.CreateWithStreamInfo (
			sampleRate: 192000,
			channels: 2,
			bitsPerSample: 24,
			totalSamples: 192000 * 60); // 1 minute

		// Act
		var result = FlacFile.Read (data);

		// Assert
		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (192000, result.File!.Properties.SampleRate);
		Assert.AreEqual (2, result.File.Properties.Channels);
		Assert.AreEqual (24, result.File.Properties.BitsPerSample);
	}

	[TestMethod]
	public void Flac_24bit192kHz_RoundTrip_PreservesMetadata ()
	{
		// Arrange - 24-bit/192kHz with metadata
		var data = TestBuilders.Flac.CreateWithStreamInfo (
			sampleRate: 192000,
			channels: 2,
			bitsPerSample: 24,
			totalSamples: 192000 * 120); // 2 minutes

		var result = FlacFile.Read (data);
		Assert.IsTrue (result.IsSuccess);

		var file = result.File!;
		file.Title = "High-Res Test";
		file.Artist = "Test Artist";
		file.Album = "24-bit/192kHz Album";

		// Act - Round trip
		var rendered = file.Render (data);
		var reparsed = FlacFile.Read (rendered.Span);

		// Assert
		Assert.IsTrue (reparsed.IsSuccess);
		Assert.AreEqual (192000, reparsed.File!.Properties.SampleRate);
		Assert.AreEqual (24, reparsed.File.Properties.BitsPerSample);
		Assert.AreEqual ("High-Res Test", reparsed.File.Title);
		Assert.AreEqual ("Test Artist", reparsed.File.Artist);
	}

	[TestMethod]
	public void Flac_32bit384kHz_ParsesCorrectly ()
	{
		// Arrange - 32-bit/384kHz (extreme high-res)
		var data = TestBuilders.Flac.CreateWithStreamInfo (
			sampleRate: 384000,
			channels: 2,
			bitsPerSample: 32,
			totalSamples: 384000 * 30); // 30 seconds

		// Act
		var result = FlacFile.Read (data);

		// Assert
		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (384000, result.File!.Properties.SampleRate);
		Assert.AreEqual (32, result.File.Properties.BitsPerSample);
	}

	[TestMethod]
	public void Flac_Multichannel_51Surround_ParsesCorrectly ()
	{
		// Arrange - 24-bit/48kHz 5.1 surround (6 channels)
		var data = TestBuilders.Flac.CreateWithStreamInfo (
			sampleRate: 48000,
			channels: 6,
			bitsPerSample: 24,
			totalSamples: 48000 * 60);

		// Act
		var result = FlacFile.Read (data);

		// Assert
		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (6, result.File!.Properties.Channels);
		Assert.AreEqual (48000, result.File.Properties.SampleRate);
	}

	[TestMethod]
	public void Flac_Multichannel_71Surround_ParsesCorrectly ()
	{
		// Arrange - 24-bit/48kHz 7.1 surround (8 channels)
		var data = TestBuilders.Flac.CreateWithStreamInfo (
			sampleRate: 48000,
			channels: 8,
			bitsPerSample: 24,
			totalSamples: 48000 * 60);

		// Act
		var result = FlacFile.Read (data);

		// Assert
		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (8, result.File!.Properties.Channels);
	}
}
