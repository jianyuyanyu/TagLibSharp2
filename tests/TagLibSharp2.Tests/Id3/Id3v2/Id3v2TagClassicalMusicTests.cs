// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Id3.Id3v2;

namespace TagLibSharp2.Tests.Id3.Id3v2;

/// <summary>
/// Tests for classical music metadata (Work, Movement, MovementNumber, MovementTotal) in ID3v2 tags.
/// </summary>
[TestClass]
[TestCategory ("Unit")]
public sealed class Id3v2TagClassicalMusicTests
{
	[TestMethod]
	public void Work_GetSet_RoundTrips ()
	{
		var tag = new Id3v2Tag ();

		tag.Work = "Symphony No. 9 in D minor, Op. 125";

		Assert.AreEqual ("Symphony No. 9 in D minor, Op. 125", tag.Work);
	}

	[TestMethod]
	public void Work_Render_PreservesValue ()
	{
		var tag = new Id3v2Tag ();
		tag.Work = "Piano Concerto No. 5";

		var rendered = tag.Render ();
		var parsed = Id3v2Tag.Read (rendered.Span);

		Assert.IsTrue (parsed.IsSuccess);
		Assert.AreEqual ("Piano Concerto No. 5", parsed.Tag!.Work);
	}

	[TestMethod]
	public void Movement_GetSet_RoundTrips ()
	{
		var tag = new Id3v2Tag ();

		tag.Movement = "Allegro con brio";

		Assert.AreEqual ("Allegro con brio", tag.Movement);
	}

	[TestMethod]
	public void Movement_Render_PreservesValue ()
	{
		var tag = new Id3v2Tag ();
		tag.Movement = "Adagio sostenuto";

		var rendered = tag.Render ();
		var parsed = Id3v2Tag.Read (rendered.Span);

		Assert.IsTrue (parsed.IsSuccess);
		Assert.AreEqual ("Adagio sostenuto", parsed.Tag!.Movement);
	}

	[TestMethod]
	public void MovementNumber_GetSet_RoundTrips ()
	{
		var tag = new Id3v2Tag ();

		tag.MovementNumber = 3;

		Assert.AreEqual (3u, tag.MovementNumber);
	}

	[TestMethod]
	public void MovementNumber_Render_PreservesValue ()
	{
		var tag = new Id3v2Tag ();
		tag.MovementNumber = 2;

		var rendered = tag.Render ();
		var parsed = Id3v2Tag.Read (rendered.Span);

		Assert.IsTrue (parsed.IsSuccess);
		Assert.AreEqual (2u, parsed.Tag!.MovementNumber);
	}

	[TestMethod]
	public void MovementTotal_GetSet_RoundTrips ()
	{
		var tag = new Id3v2Tag ();

		tag.MovementTotal = 4;

		Assert.AreEqual (4u, tag.MovementTotal);
	}

	[TestMethod]
	public void MovementTotal_Render_PreservesValue ()
	{
		var tag = new Id3v2Tag ();
		tag.MovementTotal = 5;

		var rendered = tag.Render ();
		var parsed = Id3v2Tag.Read (rendered.Span);

		Assert.IsTrue (parsed.IsSuccess);
		Assert.AreEqual (5u, parsed.Tag!.MovementTotal);
	}

	[TestMethod]
	public void AllClassicalMetadata_RoundTrip_PreservesAll ()
	{
		var tag = new Id3v2Tag ();
		tag.Work = "The Planets, Op. 32";
		tag.Movement = "Mars, the Bringer of War";
		tag.MovementNumber = 1;
		tag.MovementTotal = 7;

		var rendered = tag.Render ();
		var parsed = Id3v2Tag.Read (rendered.Span);

		Assert.IsTrue (parsed.IsSuccess);
		Assert.AreEqual ("The Planets, Op. 32", parsed.Tag!.Work);
		Assert.AreEqual ("Mars, the Bringer of War", parsed.Tag.Movement);
		Assert.AreEqual (1u, parsed.Tag.MovementNumber);
		Assert.AreEqual (7u, parsed.Tag.MovementTotal);
	}

	[TestMethod]
	public void Work_SetNull_ClearsValue ()
	{
		var tag = new Id3v2Tag ();
		tag.Work = "Symphony No. 5";

		tag.Work = null;

		Assert.IsNull (tag.Work);
	}

	[TestMethod]
	public void Movement_SetNull_ClearsValue ()
	{
		var tag = new Id3v2Tag ();
		tag.Movement = "Allegro";

		tag.Movement = null;

		Assert.IsNull (tag.Movement);
	}

	[TestMethod]
	public void MovementNumber_SetNull_ClearsValue ()
	{
		var tag = new Id3v2Tag ();
		tag.MovementNumber = 1;

		tag.MovementNumber = null;

		Assert.IsNull (tag.MovementNumber);
	}

	[TestMethod]
	public void MovementTotal_SetNull_ClearsValue ()
	{
		var tag = new Id3v2Tag ();
		tag.MovementTotal = 4;

		tag.MovementTotal = null;

		Assert.IsNull (tag.MovementTotal);
	}
}
