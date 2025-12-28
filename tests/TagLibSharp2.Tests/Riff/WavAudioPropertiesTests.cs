// Copyright (c) 2025 Stephen Shaw and contributors

using TagLibSharp2.Core;
using TagLibSharp2.Riff;

namespace TagLibSharp2.Tests.Riff;

[TestClass]
[TestCategory ("Unit")]
[TestCategory ("Riff")]
public class WavAudioPropertiesTests
{
	static BinaryData CreateFmtChunk (
		ushort formatCode = 1,
		ushort channels = 2,
		uint sampleRate = 44100,
		ushort bitsPerSample = 16)
	{
		var blockAlign = (ushort)(channels * bitsPerSample / 8);
		var byteRate = sampleRate * blockAlign;

		using var builder = new BinaryDataBuilder (16);
		builder.AddUInt16LE (formatCode);
		builder.AddUInt16LE (channels);
		builder.AddUInt32LE (sampleRate);
		builder.AddUInt32LE (byteRate);
		builder.AddUInt16LE (blockAlign);
		builder.AddUInt16LE (bitsPerSample);

		return builder.ToBinaryData ();
	}

	[TestMethod]
	public void Parse_ValidPcmFmt_ReturnsProperties ()
	{
		var props = WavAudioPropertiesParser.Parse (CreateFmtChunk ());

		Assert.IsNotNull (props);
		Assert.AreEqual (44100, props.SampleRate);
		Assert.AreEqual (2, props.Channels);
		Assert.AreEqual (16, props.BitsPerSample);
	}

	[TestMethod]
	public void Parse_TooShort_ReturnsNull ()
	{
		var data = new BinaryData ([1, 2, 3, 4, 5]);
		Assert.IsNull (WavAudioPropertiesParser.Parse (data));
	}

	[TestMethod]
	public void Parse_ZeroChannels_ReturnsNull ()
	{
		var data = CreateFmtChunk (channels: 0);
		Assert.IsNull (WavAudioPropertiesParser.Parse (data));
	}

	[TestMethod]
	public void Parse_WithDataSize_CalculatesDuration ()
	{
		var fmtData = CreateFmtChunk ();
		var props = WavAudioPropertiesParser.Parse (fmtData, 176400); // 1 second

		Assert.IsNotNull (props);
		Assert.AreEqual (1.0, props.Duration.TotalSeconds, 0.001);
	}

	[TestMethod]
	public void Parse_CalculatesBitrate ()
	{
		var props = WavAudioPropertiesParser.Parse (CreateFmtChunk ());

		Assert.IsNotNull (props);
		Assert.AreEqual (1411, props.Bitrate);
	}

	[TestMethod]
	public void Parse_MonoAudio_SingleChannel ()
	{
		var props = WavAudioPropertiesParser.Parse (CreateFmtChunk (channels: 1));

		Assert.IsNotNull (props);
		Assert.AreEqual (1, props.Channels);
	}

	[TestMethod]
	public void GetFormatDescription_Pcm_ReturnsPcm ()
	{
		Assert.AreEqual ("PCM", WavAudioPropertiesParser.GetFormatDescription (1));
	}

	[TestMethod]
	public void GetFormatDescription_IeeeFloat_ReturnsIeeeFloat ()
	{
		Assert.AreEqual ("IEEE Float", WavAudioPropertiesParser.GetFormatDescription (3));
	}

	[TestMethod]
	public void GetFormatDescription_Unknown_ReturnsWavWithCode ()
	{
		var desc = WavAudioPropertiesParser.GetFormatDescription (99);
		Assert.IsTrue (desc.StartsWith ("WAV"));
	}

	[TestMethod]
	public void Parse_IeeeFloat_ReturnsCorrectCodec ()
	{
		var props = WavAudioPropertiesParser.Parse (CreateFmtChunk (formatCode: 3));

		Assert.IsNotNull (props);
		Assert.AreEqual ("IEEE Float", props.Codec);
	}
}
