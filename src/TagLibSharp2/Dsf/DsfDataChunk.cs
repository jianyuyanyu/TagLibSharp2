// DSF Data Chunk parsing
// Third chunk: "data" + chunk_size(8) + audio_data

using System;
using System.Buffers.Binary;

#pragma warning disable CA1815 // Override equals and operator equals on value types
#pragma warning disable CA2231 // Overload operator equals on overriding value type Equals
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace TagLibSharp2.Dsf;

/// <summary>
/// Represents the result of parsing a data chunk.
/// </summary>
public readonly struct DsfDataChunkParseResult : IEquatable<DsfDataChunkParseResult>
{
	public DsfDataChunk? Chunk { get; }
	public string? Error { get; }
	public bool IsSuccess => Chunk is not null && Error is null;

	private DsfDataChunkParseResult (DsfDataChunk? chunk, string? error)
	{
		Chunk = chunk;
		Error = error;
	}

	public static DsfDataChunkParseResult Success (DsfDataChunk chunk) => new (chunk, null);
	public static DsfDataChunkParseResult Failure (string error) => new (null, error);

	public bool Equals (DsfDataChunkParseResult other) =>
		Equals (Chunk, other.Chunk) && Error == other.Error;

	public override bool Equals (object? obj) =>
		obj is DsfDataChunkParseResult other && Equals (other);

	public override int GetHashCode () => HashCode.Combine (Chunk, Error);
}

/// <summary>
/// Represents the data chunk in a DSF file.
/// Contains the actual DSD audio data.
/// </summary>
public sealed class DsfDataChunk
{
	/// <summary>
	/// Data chunk magic bytes: "data".
	/// </summary>
	public static ReadOnlySpan<byte> Magic => "data"u8;

	/// <summary>
	/// Size of the data chunk header (12 bytes).
	/// </summary>
	public const int HeaderSize = 12;

	/// <summary>
	/// Gets the total chunk size including header and audio data.
	/// </summary>
	public ulong ChunkSize { get; }

	/// <summary>
	/// Gets the size of just the header portion.
	/// </summary>
	public static int HeaderSizeValue => HeaderSize;

	/// <summary>
	/// Gets the size of the audio data (ChunkSize - HeaderSize).
	/// </summary>
	public ulong AudioDataSize => ChunkSize > HeaderSize ? ChunkSize - HeaderSize : 0;

	private DsfDataChunk (ulong chunkSize)
	{
		ChunkSize = chunkSize;
	}

	/// <summary>
	/// Parses a data chunk header from binary data.
	/// </summary>
	public static DsfDataChunkParseResult Parse (ReadOnlySpan<byte> data)
	{
		if (data.Length < HeaderSize) {
			return DsfDataChunkParseResult.Failure (
				$"Data too short for data chunk header: {data.Length} bytes, need {HeaderSize}");
		}

		// Validate magic bytes
		if (!data[..4].SequenceEqual (Magic)) {
			return DsfDataChunkParseResult.Failure (
				"Invalid data chunk magic bytes: expected 'data'");
		}

		var chunkSize = BinaryPrimitives.ReadUInt64LittleEndian (data[4..]);

		return DsfDataChunkParseResult.Success (new DsfDataChunk (chunkSize));
	}

	/// <summary>
	/// Renders the data chunk header to binary data.
	/// </summary>
	public byte[] RenderHeader ()
	{
		var data = new byte[HeaderSize];
		Magic.CopyTo (data);
		BinaryPrimitives.WriteUInt64LittleEndian (data.AsSpan (4), ChunkSize);
		return data;
	}

	/// <summary>
	/// Creates a new data chunk with the specified size.
	/// </summary>
	public static DsfDataChunk Create (ulong totalChunkSize)
	{
		return new DsfDataChunk (totalChunkSize);
	}
}
