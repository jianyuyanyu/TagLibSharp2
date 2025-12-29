// Copyright (c) 2025 Stephen Shaw and contributors

using TagLibSharp2.Core;
using TagLibSharp2.Mpeg;

namespace TagLibSharp2.Tests.Mpeg;

[TestClass]
[TestCategory ("Unit")]
[TestCategory ("Mpeg")]
public class MpegAudioPropertiesTests
{
	[TestMethod]
	public void Duration_VbrWithXingHeader_AccurateCalculation ()
	{
		// Create a minimal MP3 with Xing header
		// 4096 frames at 44100 Hz, 1152 samples per frame
		// Duration = 4096 * 1152 / 44100 = 107.0 seconds
		var data = CreateMp3WithXingHeader (4096, 44100, 1152);

		MpegAudioProperties.TryParse (data, 0, out var props);

		Assert.IsNotNull (props);
		var expectedDuration = TimeSpan.FromSeconds (4096.0 * 1152 / 44100);
		Assert.AreEqual (expectedDuration.TotalSeconds, props.Duration.TotalSeconds, 0.1);
	}

	[TestMethod]
	public void Duration_VbrWithVbriHeader_AccurateCalculation ()
	{
		// Create MP3 with VBRI header (8192 frames)
		var data = CreateMp3WithVbriHeader (8192, 5000000, 44100, 1152);

		MpegAudioProperties.TryParse (data, 0, out var props);

		Assert.IsNotNull (props);
		var expectedDuration = TimeSpan.FromSeconds (8192.0 * 1152 / 44100);
		Assert.AreEqual (expectedDuration.TotalSeconds, props.Duration.TotalSeconds, 0.1);
	}

	[TestMethod]
	public void Duration_CbrNoVbrHeader_EstimatedFromFileSize ()
	{
		// Create CBR MP3 (no Xing or VBRI header)
		// 128 kbps, 44100 Hz, file size ~1MB = ~65 seconds
		var data = CreateCbrMp3 (128, 44100, 1024 * 1024);

		MpegAudioProperties.TryParse (data, 0, out var props);

		Assert.IsNotNull (props);
		// CBR: duration = (file_size * 8) / bitrate
		var expectedSeconds = (1024.0 * 1024 * 8) / (128 * 1000);
		Assert.AreEqual (expectedSeconds, props.Duration.TotalSeconds, 0.5);
	}

	[TestMethod]
	public void SampleRate_ParsedFromFrame ()
	{
		var data = CreateMp3WithXingHeader (1000, 48000, 1152);

		MpegAudioProperties.TryParse (data, 0, out var props);

		Assert.AreEqual (48000, props!.SampleRate);
	}

	[TestMethod]
	public void Channels_Stereo_Returns2 ()
	{
		var data = CreateMp3WithXingHeader (1000, 44100, 1152, ChannelMode.Stereo);

		MpegAudioProperties.TryParse (data, 0, out var props);

		Assert.AreEqual (2, props!.Channels);
	}

	[TestMethod]
	public void Channels_Mono_Returns1 ()
	{
		var data = CreateMp3WithXingHeader (1000, 44100, 1152, ChannelMode.Mono);

		MpegAudioProperties.TryParse (data, 0, out var props);

		Assert.AreEqual (1, props!.Channels);
	}

	[TestMethod]
	public void IsVbr_WithXingHeader_ReturnsTrue ()
	{
		var data = CreateMp3WithXingHeader (1000, 44100, 1152);

		MpegAudioProperties.TryParse (data, 0, out var props);

		Assert.IsTrue (props!.IsVbr);
	}

	[TestMethod]
	public void IsVbr_CbrFile_ReturnsFalse ()
	{
		var data = CreateCbrMp3 (128, 44100, 10000);

		MpegAudioProperties.TryParse (data, 0, out var props);

		Assert.IsFalse (props!.IsVbr);
	}

	[TestMethod]
	public void Bitrate_Vbr_CalculatesAverage ()
	{
		// 4096 frames, 500000 bytes audio data
		// Average bitrate = (bytes * 8) / duration
		var data = CreateMp3WithXingHeader (4096, 44100, 1152, byteCount: 500000);

		MpegAudioProperties.TryParse (data, 0, out var props);

		// duration = 4096 * 1152 / 44100 = 107.0 sec
		// avg bitrate = 500000 * 8 / 107.0 = ~37.4 kbps
		Assert.IsTrue (props!.Bitrate > 30 && props.Bitrate < 50);
	}

	[TestMethod]
	public void Bitrate_Cbr_ReturnsFrameBitrate ()
	{
		var data = CreateCbrMp3 (192, 44100, 10000);

		MpegAudioProperties.TryParse (data, 0, out var props);

		Assert.AreEqual (192, props!.Bitrate);
	}

	[TestMethod]
	public void TryParse_NoAudioFrame_ReturnsFalse ()
	{
		// Just random data, no MPEG frame
		byte[] data = new byte[1000];

		var result = MpegAudioProperties.TryParse (new BinaryData (data), 0, out _);

		Assert.IsFalse (result);
	}

	[TestMethod]
	public void TryParse_AtOffset_SkipsId3v2Tag ()
	{
		// Create MP3 with fake ID3v2 header
		var id3Size = 1000;
		var mp3Data = CreateMp3WithXingHeader (2000, 44100, 1152);
		var fullData = CreateDataWithId3v2Prefix (id3Size, mp3Data);

		MpegAudioProperties.TryParse (fullData, id3Size, out var props);

		Assert.IsNotNull (props);
		Assert.AreEqual (44100, props.SampleRate);
	}

	[TestMethod]
	public void Version_Mpeg1Layer3_ReturnsCorrectly ()
	{
		var data = CreateMp3WithXingHeader (1000, 44100, 1152);

		MpegAudioProperties.TryParse (data, 0, out var props);

		Assert.AreEqual (MpegVersion.Version1, props!.Version);
		Assert.AreEqual (MpegLayer.Layer3, props.Layer);
	}

	// Helper methods

	static BinaryData CreateMp3WithXingHeader (
		uint frameCount,
		int sampleRate,
		int samplesPerFrame,
		ChannelMode channelMode = ChannelMode.Stereo,
		uint? byteCount = null)
	{
		var builder = new BinaryDataBuilder ();

		// Create MPEG frame header (MPEG1 Layer3)
		// 0xFF 0xFB = MPEG1 Layer3 no CRC
		byte byte2 = sampleRate switch {
			48000 => 0x94, // 128kbps, 48000Hz
			32000 => 0x98, // 128kbps, 32000Hz
			_ => 0x90      // 128kbps, 44100Hz
		};
		byte byte3 = channelMode switch {
			ChannelMode.Mono => 0xC0,
			ChannelMode.JointStereo => 0x40,
			ChannelMode.DualChannel => 0x80,
			_ => 0x00
		};

		builder.Add (0xFF, 0xFB, byte2, byte3);

		// Add padding to reach Xing header position (32 bytes for stereo, 17 for mono)
		var sideInfoSize = channelMode == ChannelMode.Mono ? 17 : 32;
		builder.Add (new byte[sideInfoSize]);

		// Xing header
		builder.Add (0x58, 0x69, 0x6E, 0x67); // "Xing"
		var flags = (byteCount.HasValue ? 0x03u : 0x01u); // frames + optional bytes
		builder.AddUInt32BE (flags);
		builder.AddUInt32BE (frameCount);
		if (byteCount.HasValue)
			builder.AddUInt32BE (byteCount.Value);

		// Pad to frame size
		while (builder.Length < 417) // Typical MPEG1 L3 128kbps frame size
			builder.Add (0);

		return builder.ToBinaryData ();
	}

	static BinaryData CreateMp3WithVbriHeader (
		uint frameCount,
		uint byteCount,
		int sampleRate,
		int samplesPerFrame)
	{
		var builder = new BinaryDataBuilder ();

		// MPEG frame header
		byte byte2 = sampleRate switch {
			48000 => 0x94,
			32000 => 0x98,
			_ => 0x90
		};
		builder.Add (0xFF, 0xFB, byte2, 0x00);

		// Pad 32 bytes to VBRI position
		builder.Add (new byte[32]);

		// VBRI header
		builder.Add (0x56, 0x42, 0x52, 0x49); // "VBRI"
		builder.AddUInt16BE (1);               // Version
		builder.AddUInt16BE (0);               // Delay
		builder.AddUInt16BE (75);              // Quality
		builder.AddUInt32BE (byteCount);
		builder.AddUInt32BE (frameCount);
		builder.AddUInt16BE (0);               // TOC entries
		builder.AddUInt16BE (1);
		builder.AddUInt16BE (2);
		builder.AddUInt16BE (100);

		// Pad to frame size
		while (builder.Length < 417)
			builder.Add (0);

		return builder.ToBinaryData ();
	}

	static BinaryData CreateCbrMp3 (int bitrate, int sampleRate, int totalSize)
	{
		var builder = new BinaryDataBuilder ();

		// Find bitrate index (for 128kbps, index 9)
		int bitrateIndex = bitrate switch {
			32 => 1,
			40 => 2,
			48 => 3,
			56 => 4,
			64 => 5,
			80 => 6,
			96 => 7,
			112 => 8,
			128 => 9,
			160 => 10,
			192 => 11,
			224 => 12,
			256 => 13,
			320 => 14,
			_ => 9
		};

		byte byte2 = (byte)((bitrateIndex << 4) | (sampleRate switch {
			48000 => 0x04,
			32000 => 0x08,
			_ => 0x00
		}));

		builder.Add (0xFF, 0xFB, byte2, 0x00);

		// Pad side info
		builder.Add (new byte[32]);

		// No Xing/VBRI header - just pad to total size
		while (builder.Length < totalSize)
			builder.Add (0);

		return builder.ToBinaryData ();
	}

	static BinaryData CreateDataWithId3v2Prefix (int id3Size, BinaryData mp3Data)
	{
		var builder = new BinaryDataBuilder ();

		// ID3v2 header
		builder.Add (0x49, 0x44, 0x33);  // "ID3"
		builder.Add (0x04, 0x00);         // Version 2.4
		builder.Add (0x00);               // Flags
										  // Syncsafe size (without header)
		var payloadSize = id3Size - 10;
		builder.AddSyncSafeUInt32 ((uint)payloadSize);

		// Pad to id3Size
		while (builder.Length < id3Size)
			builder.Add (0);

		// Add MP3 data
		builder.Add (mp3Data);

		return builder.ToBinaryData ();
	}
}
