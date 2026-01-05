// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace TagLibSharp2.Id3.Id3v2;

/// <summary>
/// Flags present in the ID3v2 header byte.
/// </summary>
[Flags]
[SuppressMessage ("Naming", "CA1711:Identifiers should not have incorrect suffix", Justification = "This is a flags enum")]
[SuppressMessage ("Design", "CA1028:Enum storage should be Int32", Justification = "Matches ID3v2 spec byte layout")]
public enum Id3v2HeaderFlags : byte
{
	/// <summary>
	/// No flags set.
	/// </summary>
	None = 0,

	/// <summary>
	/// Bit 7: Unsynchronization applied to all frames.
	/// </summary>
	Unsynchronization = 0x80,

	/// <summary>
	/// Bit 6: Extended header follows the main header.
	/// </summary>
	ExtendedHeader = 0x40,

	/// <summary>
	/// Bit 5: Tag is experimental (v2.3+).
	/// </summary>
	Experimental = 0x20,

	/// <summary>
	/// Bit 4: Footer present at end of tag (v2.4 only).
	/// </summary>
	Footer = 0x10
}
