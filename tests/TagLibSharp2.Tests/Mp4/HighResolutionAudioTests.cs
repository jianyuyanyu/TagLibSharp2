// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Mp4;

namespace TagLibSharp2.Tests.Mp4;

/// <summary>
/// Tests for high-resolution ALAC audio format support.
/// Validates proper handling of various bit depths (16, 20, 24, 32-bit).
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
	public void Alac_24bit_ParsesCorrectly ()
	{
		// Arrange - 24-bit ALAC
		var data = TestBuilders.Mp4.CreateWithBitsPerSample (Mp4CodecType.Alac, 24);

		// Act
		var result = Mp4File.Read (data);

		// Assert
		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (24, result.File!.Properties.BitsPerSample);
	}

	[TestMethod]
	public void Alac_32bit_ParsesCorrectly ()
	{
		// Arrange - 32-bit ALAC (highest supported)
		var data = TestBuilders.Mp4.CreateWithBitsPerSample (Mp4CodecType.Alac, 32);

		// Act
		var result = Mp4File.Read (data);

		// Assert
		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (32, result.File!.Properties.BitsPerSample);
	}

	[TestMethod]
	public void Alac_16bit_ParsesCorrectly ()
	{
		// Arrange - 16-bit ALAC (CD quality baseline)
		var data = TestBuilders.Mp4.CreateWithBitsPerSample (Mp4CodecType.Alac, 16);

		// Act
		var result = Mp4File.Read (data);

		// Assert
		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (16, result.File!.Properties.BitsPerSample);
	}

	[TestMethod]
	public void Alac_20bit_ParsesCorrectly ()
	{
		// Arrange - 20-bit ALAC (DVD-Audio compatible)
		var data = TestBuilders.Mp4.CreateWithBitsPerSample (Mp4CodecType.Alac, 20);

		// Act
		var result = Mp4File.Read (data);

		// Assert
		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (20, result.File!.Properties.BitsPerSample);
	}
}
