// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Core;
using TagLibSharp2.Mp4;

namespace TagLibSharp2.Tests.Mp4;

[TestClass]
public class Mp4ResultTypeTests
{
	[TestMethod]
	public void Mp4BoxReadResult_Success_HasCorrectProperties ()
	{
		var box = new Mp4Box ("test", BinaryData.Empty, []);
		var result = new Mp4BoxReadResult (box, 100);

		Assert.IsTrue (result.IsSuccess);
		Assert.IsNotNull (result.Box);
		Assert.AreEqual ("test", result.Box.Type);
		Assert.AreEqual (100, result.BytesConsumed);
	}

	[TestMethod]
	public void Mp4BoxReadResult_Failure_HasCorrectProperties ()
	{
		var result = Mp4BoxReadResult.Failure ();

		Assert.IsFalse (result.IsSuccess);
		Assert.IsNull (result.Box);
		Assert.AreEqual (0, result.BytesConsumed);
	}

	[TestMethod]
	public void Mp4BoxReadResult_Equals_SameFailures_ReturnsTrue ()
	{
		var result1 = Mp4BoxReadResult.Failure ();
		var result2 = Mp4BoxReadResult.Failure ();

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void Mp4BoxReadResult_Equals_SuccessVsFailure_ReturnsFalse ()
	{
		var box = new Mp4Box ("test", BinaryData.Empty, []);
		var result1 = new Mp4BoxReadResult (box, 100);
		var result2 = Mp4BoxReadResult.Failure ();

		Assert.IsFalse (result1.Equals (result2));
		Assert.IsFalse (result1 == result2);
		Assert.IsTrue (result1 != result2);
	}

	[TestMethod]
	public void Mp4BoxReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = Mp4BoxReadResult.Failure ();

		Assert.IsTrue (result.Equals ((object)Mp4BoxReadResult.Failure ()));
		Assert.IsFalse (result.Equals ("not a result"));
		Assert.IsFalse (result.Equals (null));
	}

	[TestMethod]
	public void Mp4BoxReadResult_GetHashCode_SameValues_ReturnsSameHash ()
	{
		var result1 = Mp4BoxReadResult.Failure ();
		var result2 = Mp4BoxReadResult.Failure ();

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}

	[TestMethod]
	public void Mp4FileReadResult_Success_HasCorrectProperties ()
	{
		var data = Mp4TestBuilder.CreateMinimalM4a ();
		var fileResult = Mp4File.Read (data);

		Assert.IsTrue (fileResult.IsSuccess);
		Assert.IsNotNull (fileResult.File);
		Assert.IsNull (fileResult.Error);
		Assert.IsTrue (fileResult.BytesConsumed > 0);
	}

	[TestMethod]
	public void Mp4FileReadResult_Failure_HasCorrectProperties ()
	{
		var result = Mp4FileReadResult.Failure ("Test error");

		Assert.IsFalse (result.IsSuccess);
		Assert.IsNull (result.File);
		Assert.AreEqual ("Test error", result.Error);
		Assert.AreEqual (0, result.BytesConsumed);
	}

	[TestMethod]
	public void Mp4FileReadResult_Equals_SameFailures_ReturnsTrue ()
	{
		var result1 = Mp4FileReadResult.Failure ("error");
		var result2 = Mp4FileReadResult.Failure ("error");

		Assert.IsTrue (result1.Equals (result2));
		Assert.IsTrue (result1 == result2);
		Assert.IsFalse (result1 != result2);
	}

	[TestMethod]
	public void Mp4FileReadResult_Equals_DifferentErrors_ReturnsFalse ()
	{
		var result1 = Mp4FileReadResult.Failure ("error1");
		var result2 = Mp4FileReadResult.Failure ("error2");

		Assert.IsFalse (result1.Equals (result2));
		Assert.IsFalse (result1 == result2);
		Assert.IsTrue (result1 != result2);
	}

	[TestMethod]
	public void Mp4FileReadResult_Equals_Object_ReturnsCorrectly ()
	{
		var result = Mp4FileReadResult.Failure ("error");

		Assert.IsTrue (result.Equals ((object)Mp4FileReadResult.Failure ("error")));
		Assert.IsFalse (result.Equals ((object)Mp4FileReadResult.Failure ("other")));
		Assert.IsFalse (result.Equals ("not a result"));
		Assert.IsFalse (result.Equals (null));
	}

	[TestMethod]
	public void Mp4FileReadResult_GetHashCode_SameValues_ReturnsSameHash ()
	{
		var result1 = Mp4FileReadResult.Failure ("error");
		var result2 = Mp4FileReadResult.Failure ("error");

		Assert.AreEqual (result1.GetHashCode (), result2.GetHashCode ());
	}
}
