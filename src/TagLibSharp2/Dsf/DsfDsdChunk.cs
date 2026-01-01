// DSF DSD Chunk parsing
// First chunk in DSF file: "DSD " + chunk_size(8) + file_size(8) + metadata_offset(8)

using System;
using System.Buffers.Binary;

#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning disable CA2231 // Overload operator equals on overriding value type Equals
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace TagLibSharp2.Dsf;

/// <summary>
/// Represents the result of parsing a DSD chunk.
/// </summary>
public readonly struct DsfDsdChunkParseResult : IEquatable<DsfDsdChunkParseResult>
{
    public DsfDsdChunk? Chunk { get; }
    public string? Error { get; }
    public bool IsSuccess => Chunk is not null && Error is null;

    private DsfDsdChunkParseResult(DsfDsdChunk? chunk, string? error)
    {
        Chunk = chunk;
        Error = error;
    }

    public static DsfDsdChunkParseResult Success(DsfDsdChunk chunk) => new(chunk, null);
    public static DsfDsdChunkParseResult Failure(string error) => new(null, error);

    public bool Equals(DsfDsdChunkParseResult other) =>
        Equals(Chunk, other.Chunk) && Error == other.Error;

    public override bool Equals(object? obj) =>
        obj is DsfDsdChunkParseResult other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Chunk, Error);
}

/// <summary>
/// Represents the DSD chunk header in a DSF file.
/// This is always the first chunk and contains file-level metadata.
/// </summary>
public sealed class DsfDsdChunk
{
    /// <summary>
    /// DSD chunk magic bytes: "DSD ".
    /// </summary>
    public static ReadOnlySpan<byte> Magic => "DSD "u8;

    /// <summary>
    /// Standard size of the DSD chunk (28 bytes).
    /// </summary>
    public const int Size = 28;

    /// <summary>
    /// Gets the chunk size (should always be 28 for DSD chunk).
    /// </summary>
    public ulong ChunkSize { get; }

    /// <summary>
    /// Gets the total file size in bytes.
    /// </summary>
    public ulong FileSize { get; }

    /// <summary>
    /// Gets the offset to ID3v2 metadata, or 0 if no metadata.
    /// </summary>
    public ulong MetadataOffset { get; }

    /// <summary>
    /// Gets whether the file contains ID3v2 metadata.
    /// </summary>
    public bool HasMetadata => MetadataOffset > 0;

    private DsfDsdChunk(ulong chunkSize, ulong fileSize, ulong metadataOffset)
    {
        ChunkSize = chunkSize;
        FileSize = fileSize;
        MetadataOffset = metadataOffset;
    }

    /// <summary>
    /// Parses a DSD chunk from binary data.
    /// </summary>
    public static DsfDsdChunkParseResult Parse(ReadOnlySpan<byte> data)
    {
        if (data.Length < Size)
        {
            return DsfDsdChunkParseResult.Failure(
                $"Data too short for DSD chunk: {data.Length} bytes, need {Size}");
        }

        // Validate magic bytes
        if (!data[..4].SequenceEqual(Magic))
        {
            return DsfDsdChunkParseResult.Failure(
                "Invalid DSD chunk magic bytes: expected 'DSD '");
        }

        var chunkSize = BinaryPrimitives.ReadUInt64LittleEndian(data[4..]);
        var fileSize = BinaryPrimitives.ReadUInt64LittleEndian(data[12..]);
        var metadataOffset = BinaryPrimitives.ReadUInt64LittleEndian(data[20..]);

        // Basic validation
        if (chunkSize != Size)
        {
            // Some implementations may have different chunk sizes
            // We'll accept it but log it could be non-standard
        }

        return DsfDsdChunkParseResult.Success(
            new DsfDsdChunk(chunkSize, fileSize, metadataOffset));
    }

    /// <summary>
    /// Renders the DSD chunk to binary data.
    /// </summary>
    public byte[] Render()
    {
        var data = new byte[Size];
        Magic.CopyTo(data);
        BinaryPrimitives.WriteUInt64LittleEndian(data.AsSpan(4), ChunkSize);
        BinaryPrimitives.WriteUInt64LittleEndian(data.AsSpan(12), FileSize);
        BinaryPrimitives.WriteUInt64LittleEndian(data.AsSpan(20), MetadataOffset);
        return data;
    }

    /// <summary>
    /// Creates a new DSD chunk with the specified properties.
    /// </summary>
    public static DsfDsdChunk Create(ulong fileSize, ulong metadataOffset = 0)
    {
        return new DsfDsdChunk(Size, fileSize, metadataOffset);
    }
}
