// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Core;
using TagLibSharp2.Id3.Id3v2;
using TagLibSharp2.Id3.Id3v2.Frames;

namespace TagLibSharp2.Tests.Id3.Id3v2;

/// <summary>
/// Tests for Apple proprietary ID3v2.3 frames.
/// </summary>
/// <remarks>
/// Apple uses several non-standard frames in iTunes:
/// - WFED: Podcast feed URL
/// - MVNM: Movement name (classical music)
/// - MVIN: Movement index/total (e.g., "2/4")
/// </remarks>
[TestClass]
public class Id3v2AppleFrameTests
{
	#region WFED (Podcast Feed URL) Tests

	[TestMethod]
	public void UrlFrame_IsUrlFrameId_RecognizesWfed ()
	{
		Assert.IsTrue (UrlFrame.IsUrlFrameId ("WFED"), "WFED should be recognized as a URL frame ID");
	}

	[TestMethod]
	public void Read_WfedFrame_ParsesCorrectly ()
	{
		var url = "https://example.com/podcast.xml";
		var data = TestBuilders.Id3v2.CreateTagWithUrlFrame ("WFED", url, TestConstants.Id3v2.Version3);

		var result = Id3v2Tag.Read (data);

		Assert.IsTrue (result.IsSuccess);
		var wfedFrame = result.Tag!.UrlFrames.FirstOrDefault (f => f.Id == "WFED");
		Assert.IsNotNull (wfedFrame, "WFED frame should be parsed");
		Assert.AreEqual (url, wfedFrame.Url);
	}

	[TestMethod]
	public void Tag_PodcastFeedUrl_ReturnsWfedValue ()
	{
		var url = "https://example.com/podcast.rss";
		var data = TestBuilders.Id3v2.CreateTagWithUrlFrame ("WFED", url, TestConstants.Id3v2.Version3);

		var result = Id3v2Tag.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (url, result.Tag!.PodcastFeedUrl, "PodcastFeedUrl should return WFED value");
	}

	#endregion

	#region MVNM (Movement Name) Tests

	[TestMethod]
	public void Read_MvnmFrame_ParsesCorrectly ()
	{
		var movementName = "Allegro con brio";
		var data = TestBuilders.Id3v2.CreateTagWithTextFrame ("MVNM", movementName, TestConstants.Id3v2.Version3);

		var result = Id3v2Tag.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (movementName, result.Tag!.GetTextFrame ("MVNM"));
	}

	[TestMethod]
	public void Tag_Movement_ReturnsMvnmValue ()
	{
		var movementName = "Adagio sostenuto";
		var data = TestBuilders.Id3v2.CreateTagWithTextFrame ("MVNM", movementName, TestConstants.Id3v2.Version3);

		var result = Id3v2Tag.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (movementName, result.Tag!.Movement, "Movement should return MVNM value");
	}

	#endregion

	#region MVIN (Movement Index/Total) Tests

	[TestMethod]
	public void Read_MvinFrame_ParsesCorrectly ()
	{
		// MVIN uses the same format as TRCK: "index/total" or just "index"
		var data = TestBuilders.Id3v2.CreateTagWithTextFrame ("MVIN", "2/4", TestConstants.Id3v2.Version3);

		var result = Id3v2Tag.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual ("2/4", result.Tag!.GetTextFrame ("MVIN"));
	}

	[TestMethod]
	public void Tag_MovementNumber_ReturnsMvinIndex ()
	{
		var data = TestBuilders.Id3v2.CreateTagWithTextFrame ("MVIN", "3/6", TestConstants.Id3v2.Version3);

		var result = Id3v2Tag.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (3u, result.Tag!.MovementNumber, "MovementNumber should return index from MVIN");
	}

	[TestMethod]
	public void Tag_MovementTotal_ReturnsMvinTotal ()
	{
		var data = TestBuilders.Id3v2.CreateTagWithTextFrame ("MVIN", "3/6", TestConstants.Id3v2.Version3);

		var result = Id3v2Tag.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (6u, result.Tag!.MovementTotal, "MovementTotal should return total from MVIN");
	}

	[TestMethod]
	public void Tag_MovementNumber_WithoutTotal_ReturnsIndex ()
	{
		var data = TestBuilders.Id3v2.CreateTagWithTextFrame ("MVIN", "5", TestConstants.Id3v2.Version3);

		var result = Id3v2Tag.Read (data);

		Assert.IsTrue (result.IsSuccess);
		Assert.AreEqual (5u, result.Tag!.MovementNumber, "MovementNumber should return index even without total");
		Assert.IsNull (result.Tag!.MovementTotal, "MovementTotal should be null when not specified");
	}

	#endregion
}
