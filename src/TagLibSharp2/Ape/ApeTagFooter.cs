// APE Tag Footer/Header parsing
// APEv2 specification: 32-byte structure at end of tag (footer) or start (header)

using System;
using System.Buffers.Binary;

#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning disable CA2231 // Overload operator equals on overriding value type Equals
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace TagLibSharp2.Ape;

/// <summary>
/// Represents the result of parsing an APE tag footer.
/// </summary>
public readonly struct ApeTagFooterParseResult : IEquatable<ApeTagFooterParseResult>
{
	public ApeTagFooter? Footer { get; }
	public string? Error { get; }
	public bool IsSuccess => Footer is not null && Error is null;

	private ApeTagFooterParseResult (ApeTagFooter? footer, string? error)
	{
		Footer = footer;
		Error = error;
	}

	public static ApeTagFooterParseResult Success (ApeTagFooter footer) => new (footer, null);
	public static ApeTagFooterParseResult Failure (string error) => new (null, error);

	public bool Equals (ApeTagFooterParseResult other) =>
		Equals (Footer, other.Footer) && Error == other.Error;

	public override bool Equals (object? obj) =>
		obj is ApeTagFooterParseResult other && Equals (other);

	public override int GetHashCode () => HashCode.Combine (Footer, Error);
}

/// <summary>
/// Represents an APE tag footer or header structure (32 bytes).
/// </summary>
public sealed class ApeTagFooter
{
	/// <summary>
	/// APE tag magic bytes: "APETAGEX"
	/// </summary>
	public static ReadOnlySpan<byte> Magic => "APETAGEX"u8;

	/// <summary>
	/// Size of the footer/header structure in bytes.
	/// </summary>
	public const int Size = 32;

	/// <summary>
	/// APE tag version (1000 = v1, 2000 = v2).
	/// </summary>
	public uint Version { get; }

	/// <summary>
	/// Total tag size in bytes (includes footer, excludes header for v2 compatibility).
	/// </summary>
	public uint TagSize { get; }

	/// <summary>
	/// Number of items in the tag.
	/// </summary>
	public uint ItemCount { get; }

	/// <summary>
	/// Raw flags value.
	/// </summary>
	public uint Flags { get; }

	/// <summary>
	/// True if this structure is a header (bit 29 set).
	/// </summary>
	public bool IsHeader => (Flags & 0x20000000) != 0;

	/// <summary>
	/// True if the tag contains a header (bit 31 set).
	/// </summary>
	public bool HasHeader => (Flags & 0x80000000) != 0;

	/// <summary>
	/// True if the tag contains a footer (bit 30 NOT set).
	/// </summary>
	public bool HasFooter => (Flags & 0x40000000) == 0;

	/// <summary>
	/// True if the tag is read-only (bit 0 set).
	/// </summary>
	public bool IsReadOnly => (Flags & 0x01) != 0;

	private ApeTagFooter (uint version, uint tagSize, uint itemCount, uint flags)
	{
		Version = version;
		TagSize = tagSize;
		ItemCount = itemCount;
		Flags = flags;
	}

	/// <summary>
	/// Parses an APE tag footer from binary data.
	/// </summary>
	public static ApeTagFooterParseResult Parse (ReadOnlySpan<byte> data)
	{
		if (data.Length < Size) {
			return ApeTagFooterParseResult.Failure (
				$"Data too short for APE footer: {data.Length} bytes, need {Size}");
		}

		// Validate magic bytes
		if (!data[..8].SequenceEqual (Magic)) {
			return ApeTagFooterParseResult.Failure (
				"Invalid APE tag magic bytes: expected 'APETAGEX'");
		}

		var version = BinaryPrimitives.ReadUInt32LittleEndian (data[8..]);
		var tagSize = BinaryPrimitives.ReadUInt32LittleEndian (data[12..]);
		var itemCount = BinaryPrimitives.ReadUInt32LittleEndian (data[16..]);
		var flags = BinaryPrimitives.ReadUInt32LittleEndian (data[20..]);

		// Validate version
		if (version != 1000 && version != 2000) {
			return ApeTagFooterParseResult.Failure (
				$"Unsupported APE tag version: {version}");
		}

		// Validate reserved bytes are zero
		var reserved = data[24..32];
		foreach (var b in reserved) {
			if (b != 0) {
				return ApeTagFooterParseResult.Failure (
					"APE tag reserved bytes must be zero");
			}
		}

		return ApeTagFooterParseResult.Success (
			new ApeTagFooter (version, tagSize, itemCount, flags));
	}

	/// <summary>
	/// Renders the footer to binary data.
	/// </summary>
	public byte[] Render ()
	{
		var data = new byte[Size];
		Magic.CopyTo (data);
		BinaryPrimitives.WriteUInt32LittleEndian (data.AsSpan (8), Version);
		BinaryPrimitives.WriteUInt32LittleEndian (data.AsSpan (12), TagSize);
		BinaryPrimitives.WriteUInt32LittleEndian (data.AsSpan (16), ItemCount);
		BinaryPrimitives.WriteUInt32LittleEndian (data.AsSpan (20), Flags);
		// Reserved bytes 24-31 stay zero
		return data;
	}

	/// <summary>
	/// Creates a new footer with the specified properties.
	/// </summary>
	public static ApeTagFooter Create (uint tagSize, uint itemCount, bool isHeader = false, bool hasHeader = false, bool isReadOnly = false)
	{
		uint flags = 0;
		if (hasHeader) flags |= 0x80000000;
		if (isHeader) flags |= 0x20000000;
		if (isReadOnly) flags |= 0x01;

		return new ApeTagFooter (2000, tagSize, itemCount, flags);
	}
}
