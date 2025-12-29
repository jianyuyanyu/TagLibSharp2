// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Ogg;
using TagLibSharp2.Xiph;

namespace TagLibSharp2.Tests.Ogg;

[TestClass]
[TestCategory ("Unit")]
[TestCategory ("Ogg")]
public class OggVorbisFileTests
{
	[TestMethod]
	public void Read_ValidOggVorbis_ParsesVorbisComment ()
	{
		var data = TestBuilders.Ogg.CreateMinimalFile ("Test Title", "Test Artist", calculateCrc: false);

		var result = OggVorbisFile.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.IsNotNull (result.File);
		Assert.IsNotNull (result.File!.VorbisComment);
		Assert.AreEqual ("Test Title", result.File.VorbisComment.Title);
		Assert.AreEqual ("Test Artist", result.File.VorbisComment.Artist);
	}

	[TestMethod]
	public void Read_InvalidMagic_ReturnsFailure ()
	{
		var data = new byte[100]; // All zeros, no valid Ogg pages

		var result = OggVorbisFile.Read (data);

		Assert.IsFalse (result.IsSuccess);
		Assert.Contains ("Ogg", result.Error!);
	}

	[TestMethod]
	public void Read_TooShort_ReturnsFailure ()
	{
		var data = new byte[] { 0x4F, 0x67, 0x67, 0x53 }; // Just "OggS"

		var result = OggVorbisFile.Read (data);

		Assert.IsFalse (result.IsSuccess);
	}

	[TestMethod]
	public void Read_NotVorbisCodec_ReturnsFailure ()
	{
		var data = TestBuilders.Ogg.CreatePageWithNonVorbisData ();

		var result = OggVorbisFile.Read (data);

		Assert.IsFalse (result.IsSuccess);
		Assert.Contains ("Vorbis", result.Error!);
	}

	[TestMethod]
	public void Title_DelegatesToVorbisComment ()
	{
		var data = TestBuilders.Ogg.CreateMinimalFile ("My Song", "", calculateCrc: false);

		var result = OggVorbisFile.Read (data);

		Assert.AreEqual ("My Song", result.File!.Title);
	}

	[TestMethod]
	public void Title_Set_UpdatesVorbisComment ()
	{
		var data = TestBuilders.Ogg.CreateMinimalFile ("", "", calculateCrc: false);
		var result = OggVorbisFile.Read (data);
		var file = result.File!;

		file.Title = "New Title";

		Assert.AreEqual ("New Title", file.Title);
		Assert.IsNotNull (file.VorbisComment);
	}

	[TestMethod]
	public void Artist_DelegatesToVorbisComment ()
	{
		var data = TestBuilders.Ogg.CreateMinimalFile ("", "Test Artist", calculateCrc: false);

		var result = OggVorbisFile.Read (data);

		Assert.AreEqual ("Test Artist", result.File!.Artist);
	}

	[TestMethod]
	public void Read_InvalidFramingBit_ReturnsFailure ()
	{
		var data = TestBuilders.Ogg.CreateFileWithInvalidFramingBit ();

		var result = OggVorbisFile.Read (data);

		Assert.IsFalse (result.IsSuccess);
		Assert.Contains ("framing", result.Error!.ToLowerInvariant ());
	}

	[TestMethod]
	public void Read_ValidFramingBit_Succeeds ()
	{
		var data = TestBuilders.Ogg.CreateMinimalFile ("Title", "Artist", calculateCrc: false);

		var result = OggVorbisFile.Read (data);

		Assert.IsTrue (result.IsSuccess);
	}

	[TestMethod]
	public void Read_LargeCommentSpanningMultiplePages_ParsesCorrectly ()
	{
		var data = TestBuilders.Ogg.CreateFileWithMultiPageComment ();

		var result = OggVorbisFile.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.IsNotNull (result.File!.VorbisComment);
		Assert.AreEqual ("Multi-Page Title", result.File.VorbisComment.Title);
		var longValue = result.File.VorbisComment.GetValue ("LONGFIELD");
		Assert.IsNotNull (longValue);
		Assert.IsGreaterThan (60000, longValue!.Length);
	}

	[TestMethod]
	[DataRow ("日本語タイトル 🎵", "Художник")]
	[DataRow ("Café", "Naïve")]
	[DataRow ("", "   ")]
	public void Read_Utf8EdgeCases_ParsesCorrectly (string title, string artist)
	{
		var data = TestBuilders.Ogg.CreateMinimalFile (title, artist, calculateCrc: false);

		var result = OggVorbisFile.Read (data);

		Assert.IsTrue (result.IsSuccess);
		if (string.IsNullOrEmpty (title))
			Assert.IsNull (result.File!.Title);
		else
			Assert.AreEqual (title, result.File!.Title);
		Assert.AreEqual (artist, result.File!.Artist);
	}

	[TestMethod]
	public void Properties_ParsesSampleRateFromIdentificationHeader ()
	{
		var data = TestBuilders.Ogg.CreateMinimalFile ("Test", "Artist", calculateCrc: false);

		var result = OggVorbisFile.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (44100, result.File!.Properties.SampleRate);
	}

	[TestMethod]
	public void Properties_ParsesChannelsFromIdentificationHeader ()
	{
		var data = TestBuilders.Ogg.CreateMinimalFile ("Test", "Artist", calculateCrc: false);

		var result = OggVorbisFile.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (2, result.File!.Properties.Channels);
	}

	[TestMethod]
	public void Properties_ParsesBitrateFromIdentificationHeader ()
	{
		var data = TestBuilders.Ogg.CreateMinimalFile ("Test", "Artist", calculateCrc: false);

		var result = OggVorbisFile.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (128, result.File!.Properties.Bitrate);
	}

	[TestMethod]
	public void Properties_CodecIsVorbis ()
	{
		var data = TestBuilders.Ogg.CreateMinimalFile ("Test", "Artist", calculateCrc: false);

		var result = OggVorbisFile.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("Vorbis", result.File!.Properties.Codec);
	}

	[TestMethod]
	public void Properties_BitsPerSampleIsZero ()
	{
		var data = TestBuilders.Ogg.CreateMinimalFile ("Test", "Artist", calculateCrc: false);

		var result = OggVorbisFile.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (0, result.File!.Properties.BitsPerSample);
	}

	[TestMethod]
	[DataRow (48000, 1, 192000, 192)]
	[DataRow (96000, 2, 320000, 320)]
	[DataRow (44100, 6, 128000, 128)]
	public void Properties_CustomValues_ParsesCorrectly (int sampleRate, int channels, int bitrateNominal, int expectedBitrateKbps)
	{
		var data = TestBuilders.Ogg.CreateFileWithProperties (sampleRate, channels, bitrateNominal, calculateCrc: false);

		var result = OggVorbisFile.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (sampleRate, result.File!.Properties.SampleRate);
		Assert.AreEqual (channels, result.File.Properties.Channels);
		Assert.AreEqual (expectedBitrateKbps, result.File.Properties.Bitrate);
	}

	[TestMethod]
	public void Read_WithValidateCrcFalse_AcceptsBadCrc ()
	{
		var data = TestBuilders.Ogg.CreateMinimalFile ("Test", "Artist", calculateCrc: false);

		var result = OggVorbisFile.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.IsNotNull (result.File);
	}

	[TestMethod]
	public void Read_WithValidateCrcTrue_RejectsBadCrc ()
	{
		var data = TestBuilders.Ogg.CreateMinimalFile ("Test", "Artist", calculateCrc: false);

		var result = OggVorbisFile.Read (data, validateCrc: true);

		Assert.IsFalse (result.IsSuccess);
		Assert.IsNotNull (result.Error);
		Assert.Contains ("CRC", result.Error!, StringComparison.OrdinalIgnoreCase);
	}

	[TestMethod]
	public void Read_WithValidateCrcTrue_AcceptsValidCrc ()
	{
		var data = TestBuilders.Ogg.CreateMinimalFile ("Test", "Artist", calculateCrc: true);

		var result = OggVorbisFile.Read (data, validateCrc: true);

		Assert.IsTrue (result.IsSuccess, $"Expected success but got: {result.Error}");
		Assert.IsNotNull (result.File);
		Assert.AreEqual ("Test", result.File!.Title);
	}
}
