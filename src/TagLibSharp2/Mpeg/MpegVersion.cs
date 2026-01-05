// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace TagLibSharp2.Mpeg;

/// <summary>
/// MPEG audio version.
/// </summary>
public enum MpegVersion
{
	/// <summary>
	/// MPEG Version 2.5 (unofficial extension for lower sample rates).
	/// </summary>
	Version25 = 0,

	/// <summary>
	/// Invalid version value (per MPEG specification).
	/// </summary>
	Invalid = 1,

	/// <summary>
	/// MPEG Version 2 (ISO/IEC 13818-3).
	/// </summary>
	Version2 = 2,

	/// <summary>
	/// MPEG Version 1 (ISO/IEC 11172-3).
	/// </summary>
	Version1 = 3
}
