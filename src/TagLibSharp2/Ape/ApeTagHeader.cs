// APE Tag Header parsing
// Header has identical structure to footer, only differs in flag bits

using System;

#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning disable CA2231 // Overload operator equals on overriding value type Equals
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace TagLibSharp2.Ape;

/// <summary>
/// Represents the result of parsing an APE tag header.
/// </summary>
public readonly struct ApeTagHeaderParseResult : IEquatable<ApeTagHeaderParseResult>
{
    public ApeTagHeader? Header { get; }
    public string? Error { get; }
    public bool IsSuccess => Header is not null && Error is null;

    private ApeTagHeaderParseResult(ApeTagHeader? header, string? error)
    {
        Header = header;
        Error = error;
    }

    public static ApeTagHeaderParseResult Success(ApeTagHeader header) => new(header, null);
    public static ApeTagHeaderParseResult Failure(string error) => new(null, error);

    public bool Equals(ApeTagHeaderParseResult other) =>
        Equals(Header, other.Header) && Error == other.Error;

    public override bool Equals(object? obj) =>
        obj is ApeTagHeaderParseResult other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Header, Error);
}

/// <summary>
/// Represents an APE tag header structure (32 bytes).
/// Structurally identical to footer, differs only in flag interpretation.
/// </summary>
public sealed class ApeTagHeader
{
    /// <summary>
    /// Size of the header structure in bytes.
    /// </summary>
    public const int Size = ApeTagFooter.Size;

    /// <summary>
    /// APE tag version.
    /// </summary>
    public uint Version { get; }

    /// <summary>
    /// Total tag size (items + footer, excludes header).
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
    /// True if the tag is read-only (bit 0 set).
    /// </summary>
    public bool IsReadOnly => (Flags & 0x01) != 0;

    private ApeTagHeader(uint version, uint tagSize, uint itemCount, uint flags)
    {
        Version = version;
        TagSize = tagSize;
        ItemCount = itemCount;
        Flags = flags;
    }

    /// <summary>
    /// Parses an APE tag header from binary data.
    /// </summary>
    public static ApeTagHeaderParseResult Parse(ReadOnlySpan<byte> data)
    {
        // Use footer parser since structure is identical
        var footerResult = ApeTagFooter.Parse(data);

        if (!footerResult.IsSuccess)
        {
            return ApeTagHeaderParseResult.Failure(footerResult.Error!);
        }

        var footer = footerResult.Footer!;

        // Validate this is actually a header (bit 29 should be set)
        if (!footer.IsHeader)
        {
            return ApeTagHeaderParseResult.Failure(
                "Structure is not marked as a header (bit 29 not set)");
        }

        return ApeTagHeaderParseResult.Success(
            new ApeTagHeader(footer.Version, footer.TagSize, footer.ItemCount, footer.Flags));
    }

    /// <summary>
    /// Renders the header to binary data.
    /// </summary>
    public byte[] Render()
    {
        var footer = ApeTagFooter.Create(TagSize, ItemCount, isHeader: true, hasHeader: HasHeader, isReadOnly: IsReadOnly);
        return footer.Render();
    }

    /// <summary>
    /// Creates a new header with the specified properties.
    /// </summary>
    public static ApeTagHeader Create(uint tagSize, uint itemCount, bool isReadOnly = false)
    {
        uint flags = 0x80000000; // Has header
        flags |= 0x20000000;    // This is a header
        if (isReadOnly) flags |= 0x01;

        return new ApeTagHeader(2000, tagSize, itemCount, flags);
    }
}
