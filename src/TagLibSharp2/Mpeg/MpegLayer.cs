// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace TagLibSharp2.Mpeg;

/// <summary>
/// MPEG audio layer.
/// </summary>
public enum MpegLayer
{
	/// <summary>
	/// Invalid layer value (per MPEG specification).
	/// </summary>
	Invalid = 0,

	/// <summary>
	/// Layer III (MP3).
	/// </summary>
	Layer3 = 1,

	/// <summary>
	/// Layer II.
	/// </summary>
	Layer2 = 2,

	/// <summary>
	/// Layer I.
	/// </summary>
	Layer1 = 3
}
