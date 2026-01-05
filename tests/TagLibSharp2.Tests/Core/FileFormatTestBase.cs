// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace TagLibSharp2.Tests.Core;

/// <summary>
/// Base class for integration tests that require real media files.
/// </summary>
public abstract class FileFormatTestBase
{
	/// <summary>
	/// Skips the test if the specified environment variable test file is not available.
	/// </summary>
	protected static string SkipIfNoTestFile (string envVar, string fileType)
	{
		var path = Environment.GetEnvironmentVariable (envVar);
		if (string.IsNullOrEmpty (path) || !File.Exists (path))
			Assert.Inconclusive (
				$"Integration test skipped. Set {envVar} environment variable to a {fileType} file path.");
		return path!;
	}
}
