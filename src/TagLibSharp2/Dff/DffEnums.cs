// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace TagLibSharp2.Dff;

/// <summary>
/// DFF compression types.
/// </summary>
public enum DffCompressionType
{
	/// <summary>Uncompressed DSD audio.</summary>
	Dsd,

	/// <summary>DST (Direct Stream Transfer) compressed audio.</summary>
	Dst,

	/// <summary>Unknown compression type.</summary>
	Unknown
}
