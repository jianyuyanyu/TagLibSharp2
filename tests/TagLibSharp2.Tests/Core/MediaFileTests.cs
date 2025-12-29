// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Aiff;
using TagLibSharp2.Core;
using TagLibSharp2.Mpeg;
using TagLibSharp2.Ogg;
using TagLibSharp2.Riff;
using TagLibSharp2.Xiph;

namespace TagLibSharp2.Tests.Core;

[TestClass]
[TestCategory ("Unit")]
public class MediaFileTests
{
	[TestMethod]
	public void DetectFormat_FlacMagic_ReturnsFlac ()
	{
		var data = new byte[8];
		TestConstants.Magic.Flac.CopyTo (data, 0);
		data[4] = 0x00; data[5] = 0x00; data[6] = 0x00; data[7] = TestConstants.Flac.StreamInfoSize;

		var format = MediaFile.DetectFormat (data);

		Assert.AreEqual (MediaFormat.Flac, format);
	}

	[TestMethod]
	public void DetectFormat_OggMagic_ReturnsOggVorbis ()
	{
		var data = new byte[8];
		TestConstants.Magic.Ogg.CopyTo (data, 0);
		data[4] = 0x00;
		data[5] = TestConstants.Ogg.FlagBos;

		var format = MediaFile.DetectFormat (data);

		Assert.AreEqual (MediaFormat.OggVorbis, format);
	}

	[TestMethod]
	public void DetectFormat_Id3v2Magic_ReturnsMp3 ()
	{
		var data = new byte[8];
		TestConstants.Magic.Id3.CopyTo (data, 0);
		data[3] = TestConstants.Id3v2.Version4;

		var format = MediaFile.DetectFormat (data);

		Assert.AreEqual (MediaFormat.Mp3, format);
	}

	[TestMethod]
	public void DetectFormat_Mp3FrameSync_ReturnsMp3 ()
	{
		// MP3 frame sync (0xFF 0xFB)
		var data = new byte[] { 0xFF, 0xFB, 0x90, 0x00 };

		var format = MediaFile.DetectFormat (data);

		Assert.AreEqual (MediaFormat.Mp3, format);
	}

	[TestMethod]
	public void DetectFormat_RiffWaveMagic_ReturnsWav ()
	{
		var data = new byte[12];
		TestConstants.Magic.Riff.CopyTo (data, 0);
		// bytes 4-7: size placeholder (zeros)
		TestConstants.Magic.Wave.CopyTo (data, 8);

		var format = MediaFile.DetectFormat (data);

		Assert.AreEqual (MediaFormat.Wav, format);
	}

	[TestMethod]
	public void DetectFormat_FormAiffMagic_ReturnsAiff ()
	{
		var data = new byte[12];
		TestConstants.Magic.Form.CopyTo (data, 0);
		// bytes 4-7: size placeholder (zeros)
		TestConstants.Magic.Aiff.CopyTo (data, 8);

		var format = MediaFile.DetectFormat (data);

		Assert.AreEqual (MediaFormat.Aiff, format);
	}

	[TestMethod]
	public void DetectFormat_FormAifcMagic_ReturnsAiff ()
	{
		var data = new byte[12];
		TestConstants.Magic.Form.CopyTo (data, 0);
		// bytes 4-7: size placeholder (zeros)
		TestConstants.Magic.Aifc.CopyTo (data, 8);

		var format = MediaFile.DetectFormat (data);

		Assert.AreEqual (MediaFormat.Aiff, format);
	}

	[TestMethod]
	public void DetectFormat_UnknownMagic_UsesExtension ()
	{
		var data = new byte[] { 0x00, 0x00, 0x00, 0x00 };

		Assert.AreEqual (MediaFormat.Flac, MediaFile.DetectFormat (data, "song.flac"));
		Assert.AreEqual (MediaFormat.Mp3, MediaFile.DetectFormat (data, "song.mp3"));
		Assert.AreEqual (MediaFormat.OggVorbis, MediaFile.DetectFormat (data, "song.ogg"));
		Assert.AreEqual (MediaFormat.Wav, MediaFile.DetectFormat (data, "song.wav"));
		Assert.AreEqual (MediaFormat.Aiff, MediaFile.DetectFormat (data, "song.aiff"));
		Assert.AreEqual (MediaFormat.Aiff, MediaFile.DetectFormat (data, "song.aif"));
	}

	[TestMethod]
	public void DetectFormat_UnknownWithNoHint_ReturnsUnknown ()
	{
		var data = new byte[] { 0x00, 0x00, 0x00, 0x00 };

		var format = MediaFile.DetectFormat (data);

		Assert.AreEqual (MediaFormat.Unknown, format);
	}

	[TestMethod]
	public void OpenFromData_ValidFlac_ReturnsFlacFile ()
	{
		// Build a minimal valid FLAC file
		var data = CreateMinimalFlac ();

		var result = MediaFile.OpenFromData (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (MediaFormat.Flac, result.Format);
		Assert.IsNotNull (result.File);
		Assert.IsInstanceOfType<FlacFile> (result.File);
	}

	[TestMethod]
	public void OpenFromData_ValidMp3WithId3v2_ReturnsMp3File ()
	{
		// Build a minimal valid MP3 with ID3v2 tag
		var data = CreateMinimalMp3WithId3v2 ();

		var result = MediaFile.OpenFromData (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (MediaFormat.Mp3, result.Format);
		Assert.IsNotNull (result.File);
		Assert.IsInstanceOfType<Mp3File> (result.File);
	}

	[TestMethod]
	public void OpenFromData_UnknownFormat_ReturnsFailure ()
	{
		var data = new byte[] { 0x00, 0x00, 0x00, 0x00 };

		var result = MediaFile.OpenFromData (data);

		Assert.IsFalse (result.IsSuccess);
		Assert.AreEqual (MediaFormat.Unknown, result.Format);
		Assert.IsNull (result.File);
		Assert.IsNotNull (result.Error);
	}

	[TestMethod]
	public void GetFileAs_CorrectType_ReturnsFile ()
	{
		var data = CreateMinimalFlac ();
		var result = MediaFile.OpenFromData (data);

		var flacFile = result.GetFileAs<FlacFile> ();

		Assert.IsNotNull (flacFile);
	}

	[TestMethod]
	public void GetFileAs_WrongType_ReturnsNull ()
	{
		var data = CreateMinimalFlac ();
		var result = MediaFile.OpenFromData (data);

		var mp3File = result.GetFileAs<Mp3File> ();

		Assert.IsNull (mp3File);
	}

	[TestMethod]
	public void MediaFileResult_FailedResult_HasCorrectProperties ()
	{
		var data = new byte[] { 0x00, 0x00, 0x00, 0x00 };

		var result = MediaFile.OpenFromData (data);

		Assert.IsFalse (result.IsSuccess);
		Assert.IsNull (result.File);
		Assert.IsNull (result.Tag);
		Assert.AreEqual (MediaFormat.Unknown, result.Format);
		StringAssert.Contains (result.Error!, "Unknown");
	}

	static byte[] CreateMinimalFlac ()
	{
		// Create minimal valid FLAC file:
		// fLaC magic (4) + STREAMINFO block header (4) + STREAMINFO data (34)
		var data = new byte[4 + 4 + 34];

		// Magic: "fLaC"
		data[0] = 0x66;
		data[1] = 0x4C;
		data[2] = 0x61;
		data[3] = 0x43;

		// STREAMINFO block header: type=0, is_last=true (0x80), size=34 (0x000022)
		data[4] = 0x80; // type 0 (STREAMINFO) + is_last flag
		data[5] = 0x00;
		data[6] = 0x00;
		data[7] = 0x22; // size = 34

		// STREAMINFO data (34 bytes) - minimal valid values
		// min/max block size (2+2), min/max frame size (3+3),
		// sample rate/channels/bits/samples (8), MD5 (16)
		data[8] = 0x00;
		data[9] = 0x10; // min block size
		data[10] = 0xFF;
		data[11] = 0xFF; // max block size

		return data;
	}

	static byte[] CreateMinimalMp3WithId3v2 ()
	{
		// Create minimal ID3v2.4 tag with empty content
		// Header: "ID3" (3) + version (2) + flags (1) + syncsafe size (4)
		var data = new byte[10];

		// Magic: "ID3"
		data[0] = 0x49;
		data[1] = 0x44;
		data[2] = 0x33;

		// Version: 2.4.0
		data[3] = 0x04;
		data[4] = 0x00;

		// Flags: none
		data[5] = 0x00;

		// Size: 0 (syncsafe)
		data[6] = 0x00;
		data[7] = 0x00;
		data[8] = 0x00;
		data[9] = 0x00;

		return data;
	}
}
