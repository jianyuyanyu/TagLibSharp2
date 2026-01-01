// Copyright (c) 2025 Stephen Shaw and contributors
// Tests for IDisposable implementations across all file types

using TagLibSharp2.Aiff;
using TagLibSharp2.Mp4;
using TagLibSharp2.Ogg;
using TagLibSharp2.Riff;
using TagLibSharp2.Xiph;

namespace TagLibSharp2.Tests.Core;

[TestClass]
[TestCategory ("Unit")]
[TestCategory ("Dispose")]
public class DisposeTests
{
	#region FlacFile Dispose Tests

	[TestMethod]
	public void FlacFile_Dispose_ClearsReferences ()
	{
		// Arrange
		var data = TestBuilders.Flac.CreateMinimal ();
		var result = FlacFile.Read (data);
		Assert.IsTrue (result.IsSuccess);
		var file = result.File!;

		// Act
		file.Dispose ();

		// Assert - properties should be cleared
		Assert.IsNull (file.VorbisComment);
	}

	[TestMethod]
	public void FlacFile_DoubleDispose_DoesNotThrow ()
	{
		// Arrange
		var data = TestBuilders.Flac.CreateMinimal ();
		var result = FlacFile.Read (data);
		var file = result.File!;

		// Act
		file.Dispose ();
		file.Dispose (); // Should not throw
	}

	#endregion

	#region OggVorbisFile Dispose Tests

	[TestMethod]
	public void OggVorbisFile_Dispose_ClearsReferences ()
	{
		// Arrange
		var data = TestBuilders.Ogg.CreateMinimalFile ();
		var result = OggVorbisFile.Read (data);
		Assert.IsTrue (result.IsSuccess);
		var file = result.File!;

		// Act
		file.Dispose ();

		// Assert - VorbisComment should be cleared
		Assert.IsNull (file.VorbisComment);
	}

	[TestMethod]
	public void OggVorbisFile_DoubleDispose_DoesNotThrow ()
	{
		// Arrange
		var data = TestBuilders.Ogg.CreateMinimalFile ();
		var result = OggVorbisFile.Read (data);
		var file = result.File!;

		// Act
		file.Dispose ();
		file.Dispose ();
	}

	#endregion

	#region OggOpusFile Dispose Tests

	[TestMethod]
	public void OggOpusFile_Dispose_ClearsReferences ()
	{
		// Arrange
		var data = TestBuilders.Opus.CreateMinimalFile ();
		var result = OggOpusFile.Read (data);
		Assert.IsTrue (result.IsSuccess);
		var file = result.File!;

		// Act
		file.Dispose ();

		// Assert - VorbisComment should be cleared
		Assert.IsNull (file.VorbisComment);
	}

	[TestMethod]
	public void OggOpusFile_DoubleDispose_DoesNotThrow ()
	{
		// Arrange
		var data = TestBuilders.Opus.CreateMinimalFile ();
		var result = OggOpusFile.Read (data);
		var file = result.File!;

		// Act
		file.Dispose ();
		file.Dispose ();
	}

	#endregion

	#region Mp4File Dispose Tests

	[TestMethod]
	public void Mp4File_Dispose_ClearsReferences ()
	{
		// Arrange
		var data = TestBuilders.Mp4.CreateMinimalM4a (Mp4CodecType.Aac);
		var result = Mp4File.Read (data);
		Assert.IsTrue (result.IsSuccess);
		var file = result.File!;

		// Act
		file.Dispose ();

		// Assert - Tag should be cleared
		Assert.IsNull (file.Tag);
	}

	[TestMethod]
	public void Mp4File_DoubleDispose_DoesNotThrow ()
	{
		// Arrange
		var data = TestBuilders.Mp4.CreateMinimalM4a (Mp4CodecType.Aac);
		var result = Mp4File.Read (data);
		var file = result.File!;

		// Act
		file.Dispose ();
		file.Dispose ();
	}

	#endregion

	#region WavFile Dispose Tests

	[TestMethod]
	public void WavFile_Dispose_ClearsReferences ()
	{
		// Arrange
		var data = TestBuilders.Wav.CreateMinimal ();
		var result = WavFile.Read (data);
		Assert.IsTrue (result.IsSuccess);
		var file = result.File!;

		// Act
		file.Dispose ();

		// Assert - IsValid should return false after dispose
		Assert.IsFalse (file.IsValid);
	}

	[TestMethod]
	public void WavFile_DoubleDispose_DoesNotThrow ()
	{
		// Arrange
		var data = TestBuilders.Wav.CreateMinimal ();
		var result = WavFile.Read (data);
		var file = result.File!;

		// Act
		file.Dispose ();
		file.Dispose ();
	}

	#endregion

	#region AiffFile Dispose Tests

	[TestMethod]
	public void AiffFile_Dispose_ClearsReferences ()
	{
		// Arrange
		var data = TestBuilders.Aiff.CreateMinimal ();
		var result = AiffFile.Read (data);
		Assert.IsTrue (result.IsSuccess);
		var file = result.File!;

		// Act
		file.Dispose ();

		// Assert - Tag and AudioProperties should be cleared
		Assert.IsNull (file.Tag);
		Assert.IsNull (file.AudioProperties);
	}

	[TestMethod]
	public void AiffFile_DoubleDispose_DoesNotThrow ()
	{
		// Arrange
		var data = TestBuilders.Aiff.CreateMinimal ();
		var result = AiffFile.Read (data);
		var file = result.File!;

		// Act
		file.Dispose ();
		file.Dispose ();
	}

	#endregion
}
