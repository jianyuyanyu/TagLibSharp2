// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Core;
using TagLibSharp2.Mp4;
using TagLibSharp2.Tests.Core;

namespace TagLibSharp2.Tests.Mp4;

[TestClass]
public class Mp4FileAsyncTests
{
	[TestMethod]
	public async Task ReadFromFileAsync_ValidFile_ReturnsSuccess ()
	{
		// Arrange
		var mockFs = new MockFileSystem ();
		var data = Mp4TestBuilder.CreateMinimalM4a ();
		mockFs.AddFile ("/test.m4a", data);

		// Act
		var result = await Mp4File.ReadFromFileAsync ("/test.m4a", mockFs);

		// Assert
		Assert.IsTrue (result.IsSuccess);
		Assert.IsNotNull (result.File);
		Assert.AreEqual ("/test.m4a", result.File.SourcePath);
	}

	[TestMethod]
	public async Task ReadFromFileAsync_NonExistentFile_ReturnsFailure ()
	{
		var mockFs = new MockFileSystem ();

		var result = await Mp4File.ReadFromFileAsync ("/missing.m4a", mockFs);

		Assert.IsFalse (result.IsSuccess);
		Assert.IsNotNull (result.Error);
	}

	[TestMethod]
	public async Task ReadFromFileAsync_WithCancellation_ReturnsFailure ()
	{
		var mockFs = new MockFileSystem ();
		var data = Mp4TestBuilder.CreateMinimalM4a ();
		mockFs.AddFile ("/test.m4a", data);

		using var cts = new CancellationTokenSource ();
		cts.Cancel ();

		var result = await Mp4File.ReadFromFileAsync ("/test.m4a", mockFs, cts.Token);

		Assert.IsFalse (result.IsSuccess);
		Assert.IsNotNull (result.Error);
		StringAssert.Contains (result.Error, "cancelled");
	}

	[TestMethod]
	public async Task SaveToFileAsync_WithOriginalData_SavesSuccessfully ()
	{
		// Arrange
		var mockFs = new MockFileSystem ();
		var originalData = Mp4TestBuilder.CreateMinimalM4a ();
		var readResult = Mp4File.Read (originalData);
		Assert.IsTrue (readResult.IsSuccess);

		var file = readResult.File!;
		file.Title = "Async Test Title";

		// Act
		var saveResult = await file.SaveToFileAsync ("/output.m4a", originalData, mockFs);

		// Assert
		Assert.IsTrue (saveResult.IsSuccess);
		Assert.IsTrue (mockFs.FileExists ("/output.m4a"));

		// Verify the saved file has the new title
		var verifyResult = await Mp4File.ReadFromFileAsync ("/output.m4a", mockFs);
		Assert.IsTrue (verifyResult.IsSuccess);
		Assert.AreEqual ("Async Test Title", verifyResult.File!.Title);
	}

	[TestMethod]
	public async Task SaveToFileAsync_NoSourcePath_ReturnsFailure ()
	{
		// Create a file without source path
		var data = Mp4TestBuilder.CreateMinimalM4a ();
		var result = Mp4File.Read (data);
		var file = result.File!;

		var mockFs = new MockFileSystem ();
		var saveResult = await file.SaveToFileAsync (mockFs);

		Assert.IsFalse (saveResult.IsSuccess);
		Assert.IsTrue (saveResult.Error!.Contains ("source path"));
	}

	[TestMethod]
	public async Task SaveToFileAsync_WithSourcePath_ReReadsAndSaves ()
	{
		// Arrange
		var mockFs = new MockFileSystem ();
		var originalData = Mp4TestBuilder.CreateMinimalM4a ();
		mockFs.AddFile ("/source.m4a", originalData);

		var readResult = await Mp4File.ReadFromFileAsync ("/source.m4a", mockFs);
		Assert.IsTrue (readResult.IsSuccess);

		var file = readResult.File!;
		file.Title = "Updated Title";

		// Act - save to different path, re-reading from source
		var saveResult = await file.SaveToFileAsync ("/dest.m4a", mockFs);

		// Assert
		Assert.IsTrue (saveResult.IsSuccess);
		Assert.IsTrue (mockFs.FileExists ("/dest.m4a"));

		var verifyResult = await Mp4File.ReadFromFileAsync ("/dest.m4a", mockFs);
		Assert.AreEqual ("Updated Title", verifyResult.File!.Title);
	}

	[TestMethod]
	public async Task SaveToFileAsync_BackToSource_Succeeds ()
	{
		// Arrange
		var mockFs = new MockFileSystem ();
		var originalData = Mp4TestBuilder.CreateMinimalM4a ();
		mockFs.AddFile ("/test.m4a", originalData);

		var readResult = await Mp4File.ReadFromFileAsync ("/test.m4a", mockFs);
		var file = readResult.File!;
		file.Title = "Overwritten Title";

		// Act - save back to same file
		var saveResult = await file.SaveToFileAsync (mockFs);

		// Assert
		Assert.IsTrue (saveResult.IsSuccess);

		var verifyResult = await Mp4File.ReadFromFileAsync ("/test.m4a", mockFs);
		Assert.AreEqual ("Overwritten Title", verifyResult.File!.Title);
	}

	[TestMethod]
	public async Task SaveToFileAsync_EmptyRender_ReturnsFailure ()
	{
		var mockFs = new MockFileSystem ();
		var originalData = Mp4TestBuilder.CreateMinimalM4a ();
		var result = Mp4File.Read (originalData);
		var file = result.File!;

		// Try to save with invalid (too short) original data
		var saveResult = await file.SaveToFileAsync ("/test.m4a", new byte[4], mockFs);

		Assert.IsFalse (saveResult.IsSuccess);
		Assert.IsTrue (saveResult.Error!.Contains ("render"));
	}
}
