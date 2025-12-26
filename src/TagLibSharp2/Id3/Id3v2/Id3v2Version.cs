// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace TagLibSharp2.Id3.Id3v2;

/// <summary>
/// ID3v2 tag versions.
/// </summary>
[SuppressMessage ("Design", "CA1008:Enums should have zero value", Justification = "ID3v2 versions 2, 3, 4 are the valid values per spec")]
public enum Id3v2Version
{
	/// <summary>
	/// ID3v2.2 - Uses 3-character frame IDs.
	/// </summary>
	V22 = 2,

	/// <summary>
	/// ID3v2.3 - Uses 4-character frame IDs, big-endian sizes.
	/// </summary>
	V23 = 3,

	/// <summary>
	/// ID3v2.4 - Uses 4-character frame IDs, syncsafe sizes, UTF-8 support.
	/// </summary>
	V24 = 4
}
