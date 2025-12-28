// Copyright (c) 2025 Stephen Shaw and contributors

using TagLibSharp2.Core;
using TagLibSharp2.Mpeg;

namespace TagLibSharp2.Tests.Mpeg;

[TestClass]
[TestCategory ("Unit")]
[TestCategory ("Mpeg")]
public class MpegFrameTests
{
	// MPEG frame header is 4 bytes:
	// Bits 0-10:  Sync word (0x7FF = 11 ones)
	// Bits 11-12: Version (00=2.5, 01=reserved, 10=2, 11=1)
	// Bits 13-14: Layer (00=reserved, 01=III, 10=II, 11=I)
	// Bit 15:     Protection bit (0=CRC, 1=no CRC)
	// Bits 16-19: Bitrate index
	// Bits 20-21: Sample rate index
	// Bit 22:     Padding
	// Bit 23:     Private
	// Bits 24-25: Channel mode
	// Bits 26-27: Mode extension
	// Bit 28:     Copyright
	// Bit 29:     Original
	// Bits 30-31: Emphasis

	[TestMethod]
	public void TryParse_ValidMpeg1Layer3Frame_ReturnsTrue ()
	{
		// MPEG1 Layer 3, 128 kbps, 44100 Hz, stereo
		// 0xFF 0xFB = sync + MPEG1 + Layer3 + no CRC
		// 0x90 = 128 kbps (index 9) + 44100 Hz (index 0) + no padding
		// 0x00 = stereo + mode ext 0 + no copyright + no original + emphasis none
		byte[] data = [0xFF, 0xFB, 0x90, 0x00];

		var result = MpegFrame.TryParse (new BinaryData (data), 0, out var frame);

		Assert.IsTrue (result);
		Assert.IsNotNull (frame);
		Assert.AreEqual (MpegVersion.Version1, frame.Version);
		Assert.AreEqual (MpegLayer.Layer3, frame.Layer);
		Assert.AreEqual (128, frame.Bitrate);
		Assert.AreEqual (44100, frame.SampleRate);
		Assert.AreEqual (ChannelMode.Stereo, frame.ChannelMode);
	}

	[TestMethod]
	public void TryParse_ValidMpeg2Layer3Frame_ReturnsTrue ()
	{
		// MPEG2 Layer 3, 64 kbps, 22050 Hz
		// 0xFF 0xF3 = sync + MPEG2 + Layer3 + no CRC
		// 0x80 = 64 kbps (index 8 for MPEG2 L3) + 22050 Hz (index 0 for MPEG2)
		byte[] data = [0xFF, 0xF3, 0x80, 0x00];

		var result = MpegFrame.TryParse (new BinaryData (data), 0, out var frame);

		Assert.IsTrue (result);
		Assert.IsNotNull (frame);
		Assert.AreEqual (MpegVersion.Version2, frame.Version);
		Assert.AreEqual (MpegLayer.Layer3, frame.Layer);
		Assert.AreEqual (64, frame.Bitrate);
		Assert.AreEqual (22050, frame.SampleRate);
	}

	[TestMethod]
	public void TryParse_NoSyncWord_ReturnsFalse ()
	{
		byte[] data = [0x00, 0x00, 0x00, 0x00];

		var result = MpegFrame.TryParse (new BinaryData (data), 0, out _);

		Assert.IsFalse (result);
	}

	[TestMethod]
	public void TryParse_TooShort_ReturnsFalse ()
	{
		byte[] data = [0xFF, 0xFB, 0x90]; // Only 3 bytes

		var result = MpegFrame.TryParse (new BinaryData (data), 0, out _);

		Assert.IsFalse (result);
	}

	[TestMethod]
	public void TryParse_InvalidVersion_ReturnsFalse ()
	{
		// Byte 1 format: [SSS VV LL P] where VV=01 is invalid
		// 0xEB = 1110 1011 = sync(111) + version(01 = invalid) + layer(01 = L3) + prot(1)
		byte[] data = [0xFF, 0xEB, 0x90, 0x00];

		var result = MpegFrame.TryParse (new BinaryData (data), 0, out _);

		Assert.IsFalse (result);
	}

	[TestMethod]
	public void TryParse_InvalidLayer_ReturnsFalse ()
	{
		// Byte 1 format: [SSS VV LL P] where LL=00 is invalid
		// 0xF9 = 1111 1001 = sync(111) + version(11 = V1) + layer(00 = invalid) + prot(1)
		byte[] data = [0xFF, 0xF9, 0x90, 0x00];

		var result = MpegFrame.TryParse (new BinaryData (data), 0, out _);

		Assert.IsFalse (result);
	}

	[TestMethod]
	public void TryParse_InvalidBitrate_ReturnsFalse ()
	{
		// 0xFF 0xFB 0xF0 = 128kbps header but bitrate index 15 (invalid)
		byte[] data = [0xFF, 0xFB, 0xF0, 0x00];

		var result = MpegFrame.TryParse (new BinaryData (data), 0, out _);

		Assert.IsFalse (result);
	}

	[TestMethod]
	public void TryParse_FreeBitrate_ReturnsTrue ()
	{
		// Bitrate index 0 = "free" format (valid but bitrate = 0)
		byte[] data = [0xFF, 0xFB, 0x00, 0x00];

		var result = MpegFrame.TryParse (new BinaryData (data), 0, out var frame);

		Assert.IsTrue (result);
		Assert.AreEqual (0, frame!.Bitrate);
	}

	[TestMethod]
	public void TryParse_AtOffset_WorksCorrectly ()
	{
		byte[] data = [0x00, 0x00, 0x00, 0xFF, 0xFB, 0x90, 0x00];

		var result = MpegFrame.TryParse (new BinaryData (data), 3, out var frame);

		Assert.IsTrue (result);
		Assert.AreEqual (MpegVersion.Version1, frame!.Version);
	}

	[TestMethod]
	public void FrameSize_CalculatedCorrectly ()
	{
		// MPEG1 Layer 3, 128 kbps, 44100 Hz, no padding
		// Frame size = 144 * bitrate / sample_rate
		// = 144 * 128000 / 44100 = 417.something, truncated = 417 bytes
		byte[] data = [0xFF, 0xFB, 0x90, 0x00];

		MpegFrame.TryParse (new BinaryData (data), 0, out var frame);

		Assert.AreEqual (417, frame!.FrameSize);
	}

	[TestMethod]
	public void FrameSize_WithPadding_CalculatedCorrectly ()
	{
		// MPEG1 Layer 3, 128 kbps, 44100 Hz, WITH padding
		// 0x92 = 128 kbps + 44100 Hz + padding bit set
		byte[] data = [0xFF, 0xFB, 0x92, 0x00];

		MpegFrame.TryParse (new BinaryData (data), 0, out var frame);

		Assert.AreEqual (418, frame!.FrameSize); // 417 + 1 padding
	}

	[TestMethod]
	public void SamplesPerFrame_Mpeg1Layer3_Returns1152 ()
	{
		byte[] data = [0xFF, 0xFB, 0x90, 0x00];

		MpegFrame.TryParse (new BinaryData (data), 0, out var frame);

		Assert.AreEqual (1152, frame!.SamplesPerFrame);
	}

	[TestMethod]
	public void SamplesPerFrame_Mpeg2Layer3_Returns576 ()
	{
		byte[] data = [0xFF, 0xF3, 0x80, 0x00];

		MpegFrame.TryParse (new BinaryData (data), 0, out var frame);

		Assert.AreEqual (576, frame!.SamplesPerFrame);
	}

	[TestMethod]
	public void TryParse_ChannelModeMono_ParsedCorrectly ()
	{
		// 0xFF 0xFB 0x90 0xC0 = mono channel mode (bits 24-25 = 11)
		byte[] data = [0xFF, 0xFB, 0x90, 0xC0];

		MpegFrame.TryParse (new BinaryData (data), 0, out var frame);

		Assert.AreEqual (ChannelMode.Mono, frame!.ChannelMode);
	}

	[TestMethod]
	public void TryParse_ChannelModeJointStereo_ParsedCorrectly ()
	{
		// 0xFF 0xFB 0x90 0x40 = joint stereo (bits 24-25 = 01)
		byte[] data = [0xFF, 0xFB, 0x90, 0x40];

		MpegFrame.TryParse (new BinaryData (data), 0, out var frame);

		Assert.AreEqual (ChannelMode.JointStereo, frame!.ChannelMode);
	}

	[TestMethod]
	public void TryParse_HasProtection_ParsedCorrectly ()
	{
		// 0xFF 0xFA = protection bit = 0 (CRC present)
		byte[] data = [0xFF, 0xFA, 0x90, 0x00];

		MpegFrame.TryParse (new BinaryData (data), 0, out var frame);

		Assert.IsTrue (frame!.HasCrc);
	}

	[TestMethod]
	public void HeaderSize_ReturnsCorrectValue ()
	{
		Assert.AreEqual (4, MpegFrame.HeaderSize);
	}

	// Common sample rates for testing
	// Byte 2: [BBBB RR PP] where BBBB=bitrate, RR=sample rate, PP=padding+private
	[TestMethod]
	[DataRow (0x90, 44100)] // Sample rate index 0 (bits 3-2 = 00)
	[DataRow (0x94, 48000)] // Sample rate index 1 (bits 3-2 = 01)
	[DataRow (0x98, 32000)] // Sample rate index 2 (bits 3-2 = 10)
	public void SampleRate_Mpeg1_AllIndexes (int byte2, int expectedRate)
	{
		byte[] data = [0xFF, 0xFB, (byte)byte2, 0x00];

		MpegFrame.TryParse (new BinaryData (data), 0, out var frame);

		Assert.AreEqual (expectedRate, frame!.SampleRate);
	}
}
