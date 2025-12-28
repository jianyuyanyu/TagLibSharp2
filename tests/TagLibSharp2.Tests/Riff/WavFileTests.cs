// Copyright (c) 2025 Stephen Shaw and contributors

using TagLibSharp2.Core;
using TagLibSharp2.Id3.Id3v2;
using TagLibSharp2.Riff;
using TagLibSharp2.Tests.Core;

namespace TagLibSharp2.Tests.Riff;

[TestClass]
[TestCategory ("Unit")]
[TestCategory ("Riff")]
public class WavFileTests
{
	static BinaryData CreateMinimalWav ()
	{
		using var builder = new BinaryDataBuilder (512);

		builder.AddStringLatin1 ("RIFF");
		builder.AddUInt32LE (36);
		builder.AddStringLatin1 ("WAVE");

		builder.AddStringLatin1 ("fmt ");
		builder.AddUInt32LE (16);
		builder.AddUInt16LE (1);
		builder.AddUInt16LE (2);
		builder.AddUInt32LE (44100);
		builder.AddUInt32LE (176400);
		builder.AddUInt16LE (4);
		builder.AddUInt16LE (16);

		builder.AddStringLatin1 ("data");
		builder.AddUInt32LE (0);

		return builder.ToBinaryData ();
	}

	[TestMethod]
	public void ReadFromData_ValidWav_ReturnsWavFile ()
	{
		var wav = WavFile.ReadFromData (CreateMinimalWav ());

		Assert.IsNotNull (wav);
		Assert.IsTrue (wav.IsValid);
	}

	[TestMethod]
	public void ReadFromData_InvalidData_ReturnsNull ()
	{
		var wav = WavFile.ReadFromData (new BinaryData ([1, 2, 3, 4, 5]));
		Assert.IsNull (wav);
	}

	[TestMethod]
	public void ReadFromData_ParsesAudioProperties ()
	{
		var wav = WavFile.ReadFromData (CreateMinimalWav ());

		Assert.IsNotNull (wav?.Properties);
		Assert.AreEqual (44100, wav.Properties.SampleRate);
		Assert.AreEqual (2, wav.Properties.Channels);
		Assert.AreEqual (16, wav.Properties.BitsPerSample);
	}

	[TestMethod]
	public void Title_PrefersId3v2OverInfo ()
	{
		var wav = WavFile.ReadFromData (CreateMinimalWav ());
		Assert.IsNotNull (wav);

		wav.InfoTag = new RiffInfoTag { Title = "Info Title" };
		wav.Id3v2Tag = new Id3v2Tag { Title = "ID3 Title" };

		Assert.AreEqual ("ID3 Title", wav.Title);
	}

	[TestMethod]
	public void Title_FallsBackToInfo ()
	{
		var wav = WavFile.ReadFromData (CreateMinimalWav ());
		Assert.IsNotNull (wav);

		wav.InfoTag = new RiffInfoTag { Title = "Info Title" };

		Assert.AreEqual ("Info Title", wav.Title);
	}

	[TestMethod]
	public void Render_ProducesValidWav ()
	{
		var wav = WavFile.ReadFromData (CreateMinimalWav ());
		Assert.IsNotNull (wav);

		var rendered = wav.Render ();

		Assert.AreEqual ((byte)'R', rendered[0]);
		Assert.AreEqual ((byte)'W', rendered[8]);
	}

	[TestMethod]
	public void Render_WithInfoTag_IncludesListChunk ()
	{
		var wav = WavFile.ReadFromData (CreateMinimalWav ());
		Assert.IsNotNull (wav);

		wav.InfoTag = new RiffInfoTag { Title = "Test" };

		var rendered = wav.Render ();
		var roundTripped = WavFile.ReadFromData (rendered);

		Assert.IsNotNull (roundTripped?.InfoTag);
		Assert.AreEqual ("Test", roundTripped.InfoTag.Title);
	}

	[TestMethod]
	public void Render_WithId3v2Tag_IncludesId3Chunk ()
	{
		var wav = WavFile.ReadFromData (CreateMinimalWav ());
		Assert.IsNotNull (wav);

		wav.Id3v2Tag = new Id3v2Tag { Title = "ID3 Title" };

		var rendered = wav.Render ();
		var roundTripped = WavFile.ReadFromData (rendered);

		Assert.IsNotNull (roundTripped?.Id3v2Tag);
		Assert.AreEqual ("ID3 Title", roundTripped.Id3v2Tag.Title);
	}

	[TestMethod]
	public void ReadFromFile_NonExistentFile_ReturnsNull ()
	{
		var wav = WavFile.ReadFromFile ("/nonexistent/path/file.wav");
		Assert.IsNull (wav);
	}

	[TestMethod]
	public async Task ReadFromFileAsync_NonExistentFile_ReturnsNull ()
	{
		var wav = await WavFile.ReadFromFileAsync ("/nonexistent/path/file.wav");
		Assert.IsNull (wav);
	}

	[TestMethod]
	public void SaveToFile_WithMockFileSystem_WritesData ()
	{
		var wav = WavFile.ReadFromData (CreateMinimalWav ());
		Assert.IsNotNull (wav);

		var mockFs = new MockFileSystem ();
		var result = wav.SaveToFile ("/test/output.wav", mockFs);

		Assert.IsTrue (result.IsSuccess);
		Assert.IsTrue (mockFs.FileExists ("/test/output.wav"));
	}

	[TestMethod]
	public async Task SaveToFileAsync_WithMockFileSystem_WritesData ()
	{
		var wav = WavFile.ReadFromData (CreateMinimalWav ());
		Assert.IsNotNull (wav);

		var mockFs = new MockFileSystem ();
		var result = await wav.SaveToFileAsync ("/test/output.wav", mockFs);

		Assert.IsTrue (result.IsSuccess);
		Assert.IsTrue (mockFs.FileExists ("/test/output.wav"));
	}
}
