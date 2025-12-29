// Copyright (c) 2025 Stephen Shaw and contributors
// Tests for result type equality methods to ensure complete coverage

using TagLibSharp2.Aiff;
using TagLibSharp2.Core;
using TagLibSharp2.Id3.Id3v2;
using TagLibSharp2.Id3.Id3v2.Frames;
using TagLibSharp2.Mpeg;
using TagLibSharp2.Ogg;
using TagLibSharp2.Riff;
using TagLibSharp2.Xiph;

namespace TagLibSharp2.Tests.Core;

/// <summary>
/// Tests equality implementations for all result types.
/// These tests ensure IEquatable implementations work correctly.
/// </summary>
[TestClass]
public class ResultTypeEqualityTests
{
	// ========== WavFileReadResult ==========

	[TestMethod]
	public void WavFileReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = WavFileReadResult.Failure ("error");
		var result2 = WavFileReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void WavFileReadResult_Equals_DifferentError_ReturnsFalse ()
	{
		var result1 = WavFileReadResult.Failure ("error1");
		var result2 = WavFileReadResult.Failure ("error2");

		Assert.IsFalse (result1.Equals (result2));
		Assert.IsFalse (result1 == result2);
		Assert.IsTrue (result1 != result2);
	}

	[TestMethod]
	public void WavFileReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = WavFileReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
		Assert.IsTrue (result.Equals ((object)WavFileReadResult.Failure ("error")));
	}

	[TestMethod]
	public void WavFileReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = WavFileReadResult.Failure ("error");
		var result2 = WavFileReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== AiffFileReadResult ==========

	[TestMethod]
	public void AiffFileReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = AiffFileReadResult.Failure ("error");
		var result2 = AiffFileReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void AiffFileReadResult_Equals_DifferentError_ReturnsFalse ()
	{
		var result1 = AiffFileReadResult.Failure ("error1");
		var result2 = AiffFileReadResult.Failure ("error2");

		Assert.IsFalse (result1.Equals (result2));
		Assert.IsFalse (result1 == result2);
		Assert.IsTrue (result1 != result2);
	}

	[TestMethod]
	public void AiffFileReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = AiffFileReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
		Assert.IsTrue (result.Equals ((object)AiffFileReadResult.Failure ("error")));
	}

	[TestMethod]
	public void AiffFileReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = AiffFileReadResult.Failure ("error");
		var result2 = AiffFileReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== Mp3FileReadResult ==========

	[TestMethod]
	public void Mp3FileReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = Mp3FileReadResult.Failure ("error");
		var result2 = Mp3FileReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void Mp3FileReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = Mp3FileReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void Mp3FileReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = Mp3FileReadResult.Failure ("error");
		var result2 = Mp3FileReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== FlacFileReadResult ==========

	[TestMethod]
	public void FlacFileReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = FlacFileReadResult.Failure ("error");
		var result2 = FlacFileReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void FlacFileReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = FlacFileReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void FlacFileReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = FlacFileReadResult.Failure ("error");
		var result2 = FlacFileReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== OggVorbisFileReadResult ==========

	[TestMethod]
	public void OggVorbisFileReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = OggVorbisFileReadResult.Failure ("error");
		var result2 = OggVorbisFileReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void OggVorbisFileReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = OggVorbisFileReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void OggVorbisFileReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = OggVorbisFileReadResult.Failure ("error");
		var result2 = OggVorbisFileReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== OggPageReadResult ==========

	[TestMethod]
	public void OggPageReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = OggPageReadResult.Failure ("error");
		var result2 = OggPageReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void OggPageReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = OggPageReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void OggPageReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = OggPageReadResult.Failure ("error");
		var result2 = OggPageReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== VorbisCommentReadResult ==========

	[TestMethod]
	public void VorbisCommentReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = VorbisCommentReadResult.Failure ("error");
		var result2 = VorbisCommentReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void VorbisCommentReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = VorbisCommentReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void VorbisCommentReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = VorbisCommentReadResult.Failure ("error");
		var result2 = VorbisCommentReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== FlacPictureReadResult ==========

	[TestMethod]
	public void FlacPictureReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = FlacPictureReadResult.Failure ("error");
		var result2 = FlacPictureReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void FlacPictureReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = FlacPictureReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void FlacPictureReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = FlacPictureReadResult.Failure ("error");
		var result2 = FlacPictureReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== FlacCueSheetReadResult ==========

	[TestMethod]
	public void FlacCueSheetReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = FlacCueSheetReadResult.Failure ("error");
		var result2 = FlacCueSheetReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void FlacCueSheetReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = FlacCueSheetReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void FlacCueSheetReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = FlacCueSheetReadResult.Failure ("error");
		var result2 = FlacCueSheetReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== FlacMetadataBlockHeaderReadResult ==========

	[TestMethod]
	public void FlacMetadataBlockHeaderReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = FlacMetadataBlockHeaderReadResult.Failure ("error");
		var result2 = FlacMetadataBlockHeaderReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void FlacMetadataBlockHeaderReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = FlacMetadataBlockHeaderReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void FlacMetadataBlockHeaderReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = FlacMetadataBlockHeaderReadResult.Failure ("error");
		var result2 = FlacMetadataBlockHeaderReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== Id3v2HeaderReadResult ==========

	[TestMethod]
	public void Id3v2HeaderReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = Id3v2HeaderReadResult.Failure ("error");
		var result2 = Id3v2HeaderReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void Id3v2HeaderReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = Id3v2HeaderReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void Id3v2HeaderReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = Id3v2HeaderReadResult.Failure ("error");
		var result2 = Id3v2HeaderReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== TextFrameReadResult ==========

	[TestMethod]
	public void TextFrameReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = TextFrameReadResult.Failure ("error");
		var result2 = TextFrameReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void TextFrameReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = TextFrameReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void TextFrameReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = TextFrameReadResult.Failure ("error");
		var result2 = TextFrameReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== PictureFrameReadResult ==========

	[TestMethod]
	public void PictureFrameReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = PictureFrameReadResult.Failure ("error");
		var result2 = PictureFrameReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void PictureFrameReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = PictureFrameReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void PictureFrameReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = PictureFrameReadResult.Failure ("error");
		var result2 = PictureFrameReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== CommentFrameReadResult ==========

	[TestMethod]
	public void CommentFrameReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = CommentFrameReadResult.Failure ("error");
		var result2 = CommentFrameReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void CommentFrameReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = CommentFrameReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void CommentFrameReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = CommentFrameReadResult.Failure ("error");
		var result2 = CommentFrameReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== UserTextFrameReadResult ==========

	[TestMethod]
	public void UserTextFrameReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = UserTextFrameReadResult.Failure ("error");
		var result2 = UserTextFrameReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void UserTextFrameReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = UserTextFrameReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void UserTextFrameReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = UserTextFrameReadResult.Failure ("error");
		var result2 = UserTextFrameReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== UrlFrameReadResult ==========

	[TestMethod]
	public void UrlFrameReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = UrlFrameReadResult.Failure ("error");
		var result2 = UrlFrameReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void UrlFrameReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = UrlFrameReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void UrlFrameReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = UrlFrameReadResult.Failure ("error");
		var result2 = UrlFrameReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== UserUrlFrameReadResult ==========

	[TestMethod]
	public void UserUrlFrameReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = UserUrlFrameReadResult.Failure ("error");
		var result2 = UserUrlFrameReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void UserUrlFrameReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = UserUrlFrameReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void UserUrlFrameReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = UserUrlFrameReadResult.Failure ("error");
		var result2 = UserUrlFrameReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== LyricsFrameReadResult ==========

	[TestMethod]
	public void LyricsFrameReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = LyricsFrameReadResult.Failure ("error");
		var result2 = LyricsFrameReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void LyricsFrameReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = LyricsFrameReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void LyricsFrameReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = LyricsFrameReadResult.Failure ("error");
		var result2 = LyricsFrameReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== SyncLyricsFrameReadResult ==========

	[TestMethod]
	public void SyncLyricsFrameReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = SyncLyricsFrameReadResult.Failure ("error");
		var result2 = SyncLyricsFrameReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void SyncLyricsFrameReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = SyncLyricsFrameReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void SyncLyricsFrameReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = SyncLyricsFrameReadResult.Failure ("error");
		var result2 = SyncLyricsFrameReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== UniqueFileIdFrameReadResult ==========

	[TestMethod]
	public void UniqueFileIdFrameReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = UniqueFileIdFrameReadResult.Failure ("error");
		var result2 = UniqueFileIdFrameReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void UniqueFileIdFrameReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = UniqueFileIdFrameReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void UniqueFileIdFrameReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = UniqueFileIdFrameReadResult.Failure ("error");
		var result2 = UniqueFileIdFrameReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== PopularimeterFrameReadResult ==========

	[TestMethod]
	public void PopularimeterFrameReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = PopularimeterFrameReadResult.Failure ("error");
		var result2 = PopularimeterFrameReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void PopularimeterFrameReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = PopularimeterFrameReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void PopularimeterFrameReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = PopularimeterFrameReadResult.Failure ("error");
		var result2 = PopularimeterFrameReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== PrivateFrameReadResult ==========

	[TestMethod]
	public void PrivateFrameReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = PrivateFrameReadResult.Failure ("error");
		var result2 = PrivateFrameReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void PrivateFrameReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = PrivateFrameReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void PrivateFrameReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = PrivateFrameReadResult.Failure ("error");
		var result2 = PrivateFrameReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== GeneralObjectFrameReadResult ==========

	[TestMethod]
	public void GeneralObjectFrameReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = GeneralObjectFrameReadResult.Failure ("error");
		var result2 = GeneralObjectFrameReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void GeneralObjectFrameReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = GeneralObjectFrameReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void GeneralObjectFrameReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = GeneralObjectFrameReadResult.Failure ("error");
		var result2 = GeneralObjectFrameReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== ChapterFrameReadResult ==========

	[TestMethod]
	public void ChapterFrameReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = ChapterFrameReadResult.Failure ("error");
		var result2 = ChapterFrameReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void ChapterFrameReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = ChapterFrameReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void ChapterFrameReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = ChapterFrameReadResult.Failure ("error");
		var result2 = ChapterFrameReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== TableOfContentsFrameReadResult ==========

	[TestMethod]
	public void TableOfContentsFrameReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = TableOfContentsFrameReadResult.Failure ("error");
		var result2 = TableOfContentsFrameReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void TableOfContentsFrameReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = TableOfContentsFrameReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void TableOfContentsFrameReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = TableOfContentsFrameReadResult.Failure ("error");
		var result2 = TableOfContentsFrameReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== InvolvedPeopleFrameReadResult ==========

	[TestMethod]
	public void InvolvedPeopleFrameReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = InvolvedPeopleFrameReadResult.Failure ("error");
		var result2 = InvolvedPeopleFrameReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void InvolvedPeopleFrameReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = InvolvedPeopleFrameReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void InvolvedPeopleFrameReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = InvolvedPeopleFrameReadResult.Failure ("error");
		var result2 = InvolvedPeopleFrameReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== FileReadResult ==========

	[TestMethod]
	public void FileReadResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = FileReadResult.Failure ("error");
		var result2 = FileReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void FileReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = FileReadResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void FileReadResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = FileReadResult.Failure ("error");
		var result2 = FileReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	// ========== VorbisCommentFieldParseResult ==========

	[TestMethod]
	public void VorbisCommentFieldParseResult_Equals_SameFailure_ReturnsTrue ()
	{
		var result1 = VorbisCommentFieldParseResult.Failure ("error");
		var result2 = VorbisCommentFieldParseResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void VorbisCommentFieldParseResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = VorbisCommentFieldParseResult.Failure ("error");

		Assert.IsFalse (result.Equals (null));
		Assert.IsFalse (result.Equals ("not a result"));
	}

	[TestMethod]
	public void VorbisCommentFieldParseResult_GetHashCode_SameValues_SameHash ()
	{
		var result1 = VorbisCommentFieldParseResult.Failure ("error");
		var result2 = VorbisCommentFieldParseResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}
}
