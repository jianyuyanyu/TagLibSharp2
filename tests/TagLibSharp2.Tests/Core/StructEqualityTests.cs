// Copyright (c) 2025 Stephen Shaw and contributors
// Tests for struct equality methods to ensure complete coverage

using TagLibSharp2.Core;
using TagLibSharp2.Id3.Id3v2.Frames;
using TagLibSharp2.Riff;
using TagLibSharp2.Xiph;

namespace TagLibSharp2.Tests.Core;

/// <summary>
/// Tests equality implementations for structs.
/// </summary>
[TestClass]
public class StructEqualityTests
{
	// ========== SyncLyricsItem ==========

	[TestMethod]
	public void SyncLyricsItem_Equals_SameValues_ReturnsTrue ()
	{
		var item1 = new SyncLyricsItem ("Hello", 1000);
		var item2 = new SyncLyricsItem ("Hello", 1000);

		Assert.IsTrue (item1.Equals (item2));
		Assert.IsTrue (item1 == item2);
		Assert.IsFalse (item1 != item2);
	}

	[TestMethod]
	public void SyncLyricsItem_Equals_DifferentText_ReturnsFalse ()
	{
		var item1 = new SyncLyricsItem ("Hello", 1000);
		var item2 = new SyncLyricsItem ("World", 1000);

		Assert.IsFalse (item1.Equals (item2));
		Assert.IsFalse (item1 == item2);
		Assert.IsTrue (item1 != item2);
	}

	[TestMethod]
	public void SyncLyricsItem_Equals_DifferentTimestamp_ReturnsFalse ()
	{
		var item1 = new SyncLyricsItem ("Hello", 1000);
		var item2 = new SyncLyricsItem ("Hello", 2000);

		Assert.IsFalse (item1.Equals (item2));
		Assert.IsFalse (item1 == item2);
		Assert.IsTrue (item1 != item2);
	}

	[TestMethod]
	public void SyncLyricsItem_Equals_Object_ReturnsCorrectly ()
	{
		var item = new SyncLyricsItem ("Hello", 1000);

		Assert.IsFalse (item.Equals (null));
		Assert.IsFalse (item.Equals ("not an item"));
		Assert.IsTrue (item.Equals ((object)new SyncLyricsItem ("Hello", 1000)));
	}

	[TestMethod]
	public void SyncLyricsItem_GetHashCode_SameValues_SameHash ()
	{
		var item1 = new SyncLyricsItem ("Hello", 1000);
		var item2 = new SyncLyricsItem ("Hello", 1000);

		Assert.AreEqual (item1.GetHashCode (), item2.GetHashCode ());
	}

	// ========== FlacPreservedBlock ==========

	[TestMethod]
	public void FlacPreservedBlock_Equals_SameValues_ReturnsTrue ()
	{
		var data = new byte[] { 1, 2, 3 };
		var block1 = new FlacPreservedBlock (FlacBlockType.SeekTable, data);
		var block2 = new FlacPreservedBlock (FlacBlockType.SeekTable, data);

		Assert.IsTrue (block1.Equals (block2));
		Assert.IsTrue (block1 == block2);
		Assert.IsFalse (block1 != block2);
	}

	[TestMethod]
	public void FlacPreservedBlock_Equals_DifferentType_ReturnsFalse ()
	{
		var data = new byte[] { 1, 2, 3 };
		var block1 = new FlacPreservedBlock (FlacBlockType.SeekTable, data);
		var block2 = new FlacPreservedBlock (FlacBlockType.Application, data);

		Assert.IsFalse (block1.Equals (block2));
		Assert.IsFalse (block1 == block2);
		Assert.IsTrue (block1 != block2);
	}

	[TestMethod]
	public void FlacPreservedBlock_Equals_DifferentData_ReturnsFalse ()
	{
		var block1 = new FlacPreservedBlock (FlacBlockType.SeekTable, new byte[] { 1, 2, 3 });
		var block2 = new FlacPreservedBlock (FlacBlockType.SeekTable, new byte[] { 4, 5, 6 });

		Assert.IsFalse (block1.Equals (block2));
		Assert.IsFalse (block1 == block2);
		Assert.IsTrue (block1 != block2);
	}

	[TestMethod]
	public void FlacPreservedBlock_Equals_Object_ReturnsCorrectly ()
	{
		var data = new byte[] { 1, 2, 3 };
		var block = new FlacPreservedBlock (FlacBlockType.SeekTable, data);

		Assert.IsFalse (block.Equals (null));
		Assert.IsFalse (block.Equals ("not a block"));
		Assert.IsTrue (block.Equals ((object)new FlacPreservedBlock (FlacBlockType.SeekTable, data)));
	}

	[TestMethod]
	public void FlacPreservedBlock_GetHashCode_SameValues_SameHash ()
	{
		var data = new byte[] { 1, 2, 3 };
		var block1 = new FlacPreservedBlock (FlacBlockType.SeekTable, data);
		var block2 = new FlacPreservedBlock (FlacBlockType.SeekTable, data);

		Assert.AreEqual (block1.GetHashCode (), block2.GetHashCode ());
	}

	// ========== WavExtendedProperties ==========

	[TestMethod]
	public void WavExtendedProperties_Equals_SameValues_ReturnsTrue ()
	{
		var props1 = new WavExtendedProperties (2, 44100, 24, 24, 3, WavSubFormat.Pcm);
		var props2 = new WavExtendedProperties (2, 44100, 24, 24, 3, WavSubFormat.Pcm);

		Assert.IsTrue (props1.Equals (props2));
		Assert.IsTrue (props1 == props2);
		Assert.IsFalse (props1 != props2);
	}

	[TestMethod]
	public void WavExtendedProperties_Equals_DifferentBits_ReturnsFalse ()
	{
		var props1 = new WavExtendedProperties (2, 44100, 24, 24, 3, WavSubFormat.Pcm);
		var props2 = new WavExtendedProperties (2, 44100, 16, 16, 3, WavSubFormat.Pcm);

		Assert.IsFalse (props1.Equals (props2));
		Assert.IsFalse (props1 == props2);
		Assert.IsTrue (props1 != props2);
	}

	[TestMethod]
	public void WavExtendedProperties_Equals_DifferentChannelMask_ReturnsFalse ()
	{
		var props1 = new WavExtendedProperties (2, 44100, 24, 24, 3, WavSubFormat.Pcm);
		var props2 = new WavExtendedProperties (2, 44100, 24, 24, 12, WavSubFormat.Pcm);

		Assert.IsFalse (props1.Equals (props2));
	}

	[TestMethod]
	public void WavExtendedProperties_Equals_DifferentSubFormat_ReturnsFalse ()
	{
		var props1 = new WavExtendedProperties (2, 44100, 24, 24, 3, WavSubFormat.Pcm);
		var props2 = new WavExtendedProperties (2, 44100, 24, 24, 3, WavSubFormat.IeeeFloat);

		Assert.IsFalse (props1.Equals (props2));
	}

	[TestMethod]
	public void WavExtendedProperties_Equals_Object_ReturnsCorrectly ()
	{
		var props = new WavExtendedProperties (2, 44100, 24, 24, 3, WavSubFormat.Pcm);

		Assert.IsFalse (props.Equals (null));
		Assert.IsFalse (props.Equals ("not props"));
		Assert.IsTrue (props.Equals ((object)new WavExtendedProperties (2, 44100, 24, 24, 3, WavSubFormat.Pcm)));
	}

	[TestMethod]
	public void WavExtendedProperties_GetHashCode_SameValues_SameHash ()
	{
		var props1 = new WavExtendedProperties (2, 44100, 24, 24, 3, WavSubFormat.Pcm);
		var props2 = new WavExtendedProperties (2, 44100, 24, 24, 3, WavSubFormat.Pcm);

		Assert.AreEqual (props1.GetHashCode (), props2.GetHashCode ());
	}

	// ========== BatchProgress ==========

	[TestMethod]
	public void BatchProgress_Equals_SameValues_ReturnsTrue ()
	{
		var progress1 = new BatchProgress (5, 10, "/path/file.mp3");
		var progress2 = new BatchProgress (5, 10, "/path/file.mp3");

		Assert.IsTrue (progress1.Equals (progress2));
		Assert.IsTrue (progress1 == progress2);
		Assert.IsFalse (progress1 != progress2);
	}

	[TestMethod]
	public void BatchProgress_Equals_DifferentCompleted_ReturnsFalse ()
	{
		var progress1 = new BatchProgress (5, 10, "/path/file.mp3");
		var progress2 = new BatchProgress (6, 10, "/path/file.mp3");

		Assert.IsFalse (progress1.Equals (progress2));
		Assert.IsFalse (progress1 == progress2);
		Assert.IsTrue (progress1 != progress2);
	}

	[TestMethod]
	public void BatchProgress_Equals_DifferentTotal_ReturnsFalse ()
	{
		var progress1 = new BatchProgress (5, 10, "/path/file.mp3");
		var progress2 = new BatchProgress (5, 20, "/path/file.mp3");

		Assert.IsFalse (progress1.Equals (progress2));
	}

	[TestMethod]
	public void BatchProgress_Equals_DifferentPath_ReturnsFalse ()
	{
		var progress1 = new BatchProgress (5, 10, "/path/file1.mp3");
		var progress2 = new BatchProgress (5, 10, "/path/file2.mp3");

		Assert.IsFalse (progress1.Equals (progress2));
	}

	[TestMethod]
	public void BatchProgress_Equals_Object_ReturnsCorrectly ()
	{
		var progress = new BatchProgress (5, 10, "/path/file.mp3");

		Assert.IsFalse (progress.Equals (null));
		Assert.IsFalse (progress.Equals ("not progress"));
		Assert.IsTrue (progress.Equals ((object)new BatchProgress (5, 10, "/path/file.mp3")));
	}

	[TestMethod]
	public void BatchProgress_GetHashCode_SameValues_SameHash ()
	{
		var progress1 = new BatchProgress (5, 10, "/path/file.mp3");
		var progress2 = new BatchProgress (5, 10, "/path/file.mp3");

		Assert.AreEqual (progress1.GetHashCode (), progress2.GetHashCode ());
	}

	[TestMethod]
	public void BatchProgress_PercentComplete_CalculatesCorrectly ()
	{
		var progress = new BatchProgress (5, 10, "/path/file.mp3");

		Assert.AreEqual (50.0, progress.PercentComplete);
	}

	[TestMethod]
	public void BatchProgress_PercentComplete_ZeroTotal_ReturnsZero ()
	{
		var progress = new BatchProgress (0, 0, "/path/file.mp3");

		Assert.AreEqual (0.0, progress.PercentComplete);
	}

}
