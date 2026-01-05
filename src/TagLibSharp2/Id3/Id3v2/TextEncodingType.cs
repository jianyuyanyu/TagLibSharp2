// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Diagnostics.CodeAnalysis;

namespace TagLibSharp2.Id3.Id3v2;

/// <summary>
/// Text encoding types used in ID3v2 frames.
/// </summary>
[SuppressMessage ("Design", "CA1028:Enum storage should be Int32", Justification = "Matches ID3v2 spec single-byte encoding field")]
public enum TextEncodingType : byte
{
	/// <summary>
	/// ISO-8859-1 (Latin-1) encoding. Null terminator is 1 byte.
	/// </summary>
	Latin1 = 0,

	/// <summary>
	/// UTF-16 with BOM. Null terminator is 2 bytes.
	/// </summary>
	Utf16WithBom = 1,

	/// <summary>
	/// UTF-16 Big Endian without BOM (ID3v2.4 only). Null terminator is 2 bytes.
	/// </summary>
	Utf16BE = 2,

	/// <summary>
	/// UTF-8 encoding (ID3v2.4 only). Null terminator is 1 byte.
	/// </summary>
	Utf8 = 3
}
