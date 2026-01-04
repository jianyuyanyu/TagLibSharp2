// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Core;
using TagLibSharp2.Riff;

namespace TagLibSharp2.Tests.Riff;

/// <summary>
/// Tests for high-resolution WAV audio format support.
/// Validates WAVEFORMATEXTENSIBLE parsing for high sample rates and surround sound.
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
	public void Wav_51Surround_ChannelMask_ParsesCorrectly ()
	{
		// Arrange - 5.1 surround (FL, FR, FC, LFE, BL, BR)
		const uint channelMask51 =
			WavChannelMask.FrontLeft |
			WavChannelMask.FrontRight |
			WavChannelMask.FrontCenter |
			WavChannelMask.LowFrequency |
			WavChannelMask.BackLeft |
			WavChannelMask.BackRight;

		var fmtData = CreateWaveFormatExtensible (
			channels: 6,
			sampleRate: 48000,
			bitsPerSample: 24,
			validBitsPerSample: 24,
			channelMask: channelMask51,
			subFormat: WavSubFormat.Pcm);

		// Act
		var extended = WavAudioPropertiesParser.ParseExtended (fmtData);

		// Assert
		Assert.IsNotNull (extended);
		Assert.AreEqual (6, extended.Value.Channels);
		Assert.AreEqual (48000, extended.Value.SampleRate);
		Assert.AreEqual (24, extended.Value.BitsPerSample);
		Assert.AreEqual (channelMask51, extended.Value.ChannelMask);
		Assert.AreEqual (WavSubFormat.Pcm, extended.Value.SubFormat);
	}

	[TestMethod]
	public void Wav_71Surround_ChannelMask_ParsesCorrectly ()
	{
		// Arrange - 7.1 surround (FL, FR, FC, LFE, BL, BR, SL, SR)
		const uint channelMask71 =
			WavChannelMask.FrontLeft |
			WavChannelMask.FrontRight |
			WavChannelMask.FrontCenter |
			WavChannelMask.LowFrequency |
			WavChannelMask.BackLeft |
			WavChannelMask.BackRight |
			WavChannelMask.SideLeft |
			WavChannelMask.SideRight;

		var fmtData = CreateWaveFormatExtensible (
			channels: 8,
			sampleRate: 48000,
			bitsPerSample: 24,
			validBitsPerSample: 24,
			channelMask: channelMask71,
			subFormat: WavSubFormat.Pcm);

		// Act
		var extended = WavAudioPropertiesParser.ParseExtended (fmtData);

		// Assert
		Assert.IsNotNull (extended);
		Assert.AreEqual (8, extended.Value.Channels);
		Assert.AreEqual (channelMask71, extended.Value.ChannelMask);
	}

	[TestMethod]
	public void Wav_24bit192kHz_ParsesCorrectly ()
	{
		// Arrange - 24-bit/192kHz stereo WAV (high-res PCM)
		var fmtData = CreateWaveFormatExtensible (
			channels: 2,
			sampleRate: 192000,
			bitsPerSample: 24,
			validBitsPerSample: 24,
			channelMask: WavChannelMask.FrontLeft | WavChannelMask.FrontRight,
			subFormat: WavSubFormat.Pcm);

		// Act
		var extended = WavAudioPropertiesParser.ParseExtended (fmtData);

		// Assert
		Assert.IsNotNull (extended);
		Assert.AreEqual (192000, extended.Value.SampleRate);
		Assert.AreEqual (24, extended.Value.BitsPerSample);
		Assert.AreEqual (24, extended.Value.ValidBitsPerSample);
	}

	[TestMethod]
	public void Wav_32bitFloat_ParsesCorrectly ()
	{
		// Arrange - 32-bit IEEE float (common in DAWs)
		var fmtData = CreateWaveFormatExtensible (
			channels: 2,
			sampleRate: 96000,
			bitsPerSample: 32,
			validBitsPerSample: 32,
			channelMask: WavChannelMask.FrontLeft | WavChannelMask.FrontRight,
			subFormat: WavSubFormat.IeeeFloat);

		// Act
		var extended = WavAudioPropertiesParser.ParseExtended (fmtData);

		// Assert
		Assert.IsNotNull (extended);
		Assert.AreEqual (32, extended.Value.BitsPerSample);
		Assert.AreEqual (WavSubFormat.IeeeFloat, extended.Value.SubFormat);
	}

	/// <summary>
	/// Creates a WAVEFORMATEXTENSIBLE fmt chunk for testing.
	/// </summary>
	static BinaryData CreateWaveFormatExtensible (
		int channels,
		int sampleRate,
		int bitsPerSample,
		int validBitsPerSample,
		uint channelMask,
		WavSubFormat subFormat)
	{
		using var builder = new BinaryDataBuilder (40);

		var blockAlign = channels * bitsPerSample / 8;
		var byteRate = sampleRate * blockAlign;

		// WAVEFORMAT (16 bytes)
		builder.AddUInt16LE (WavAudioPropertiesParser.FormatExtensible); // 0xFFFE
		builder.AddUInt16LE ((ushort)channels);
		builder.AddUInt32LE ((uint)sampleRate);
		builder.AddUInt32LE ((uint)byteRate);
		builder.AddUInt16LE ((ushort)blockAlign);
		builder.AddUInt16LE ((ushort)bitsPerSample);

		// WAVEFORMATEX extension (2 bytes)
		builder.AddUInt16LE (22); // cbSize

		// WAVEFORMATEXTENSIBLE extension (22 bytes)
		builder.AddUInt16LE ((ushort)validBitsPerSample);
		builder.AddUInt32LE (channelMask);

		// SubFormat GUID (16 bytes) - PCM or Float GUID
		if (subFormat == WavSubFormat.Pcm) {
			// KSDATAFORMAT_SUBTYPE_PCM: 00000001-0000-0010-8000-00aa00389b71
			ReadOnlySpan<byte> pcmGuid = [0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00,
			                              0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71];
			builder.Add (pcmGuid);
		} else if (subFormat == WavSubFormat.IeeeFloat) {
			// KSDATAFORMAT_SUBTYPE_IEEE_FLOAT: 00000003-0000-0010-8000-00aa00389b71
			ReadOnlySpan<byte> floatGuid = [0x03, 0x00, 0x00, 0x00, 0x00, 0x00, 0x10, 0x00,
			                                0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71];
			builder.Add (floatGuid);
		} else {
			builder.AddZeros (16);
		}

		return builder.ToBinaryData ();
	}
}
