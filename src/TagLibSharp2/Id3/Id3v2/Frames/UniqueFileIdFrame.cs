// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Core;

namespace TagLibSharp2.Id3.Id3v2.Frames;

/// <summary>
/// Represents an ID3v2 UFID (Unique File Identifier) frame.
/// </summary>
/// <remarks>
/// <para>
/// UFID frames provide a way to identify audio files using external databases.
/// Multiple UFID frames with different owners are allowed.
/// </para>
/// <para>
/// Frame format:
/// </para>
/// <code>
/// Offset  Size  Field
/// 0       n     Owner identifier (null-terminated ASCII string)
/// n+1     m     Identifier (binary data, up to 64 bytes recommended)
/// </code>
/// <para>
/// Common owners:
/// </para>
/// <list type="bullet">
/// <item>http://musicbrainz.org - MusicBrainz Recording ID</item>
/// <item>http://www.cddb.com - CDDB/FreeDB ID</item>
/// </list>
/// </remarks>
public sealed class UniqueFileIdFrame
{
	/// <summary>
	/// The frame ID (always "UFID").
	/// </summary>
	public const string FrameId = "UFID";

	/// <summary>
	/// The owner URL for MusicBrainz IDs.
	/// </summary>
	public const string MusicBrainzOwner = "http://musicbrainz.org";

	/// <summary>
	/// Gets the owner identifier (typically a URL).
	/// </summary>
	/// <remarks>
	/// The owner identifies the database or system that issued the identifier.
	/// </remarks>
	public string Owner { get; }

	/// <summary>
	/// Gets the unique identifier data.
	/// </summary>
	/// <remarks>
	/// The identifier format depends on the owner. For MusicBrainz, this is
	/// typically an ASCII-encoded UUID string.
	/// </remarks>
	public BinaryData Identifier { get; }

	/// <summary>
	/// Gets the identifier as a string (ASCII encoding).
	/// </summary>
	/// <remarks>
	/// Most identifiers are ASCII strings (like UUIDs). Returns null if the
	/// identifier contains non-printable bytes.
	/// </remarks>
	public string? IdentifierString {
		get {
			if (Identifier.IsEmpty)
				return null;

			// Check if all bytes are printable ASCII
			var span = Identifier.Span;
			for (var i = 0; i < span.Length; i++) {
				if (span[i] < 0x20 || span[i] > 0x7E)
					return null;
			}

			return System.Text.Encoding.ASCII.GetString (span);
		}
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="UniqueFileIdFrame"/> class.
	/// </summary>
	/// <param name="owner">The owner identifier (typically a URL).</param>
	/// <param name="identifier">The unique identifier data.</param>
	public UniqueFileIdFrame (string owner, BinaryData identifier)
	{
		Owner = owner ?? "";
		Identifier = identifier;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="UniqueFileIdFrame"/> class.
	/// </summary>
	/// <param name="owner">The owner identifier (typically a URL).</param>
	/// <param name="identifier">The unique identifier bytes.</param>
	public UniqueFileIdFrame (string owner, byte[] identifier)
		: this (owner, identifier is null ? BinaryData.Empty : new BinaryData (identifier))
	{
	}

	/// <summary>
	/// Initializes a new instance with a string identifier (ASCII-encoded).
	/// </summary>
	/// <param name="owner">The owner identifier (typically a URL).</param>
	/// <param name="identifierString">The unique identifier as a string.</param>
	public UniqueFileIdFrame (string owner, string identifierString)
		: this (owner, BinaryData.FromStringLatin1 (identifierString ?? ""))
	{
	}

	/// <summary>
	/// Attempts to read a UFID frame from the provided data.
	/// </summary>
	/// <param name="data">The frame content data (excluding frame header).</param>
	/// <param name="version">The ID3v2 version.</param>
	/// <returns>A result indicating success or failure.</returns>
	public static UniqueFileIdFrameReadResult Read (ReadOnlySpan<byte> data, Id3v2Version version)
	{
		if (data.Length < 2) // Need at least owner null terminator + 1 byte identifier
			return UniqueFileIdFrameReadResult.Failure ("UFID frame data is too short");

		// Find the null terminator for owner string
		var nullIndex = data.IndexOf ((byte)0);
		if (nullIndex < 0)
			return UniqueFileIdFrameReadResult.Failure ("UFID frame missing null terminator for owner");

		// Extract owner (ASCII string)
		var owner = System.Text.Encoding.ASCII.GetString (data.Slice (0, nullIndex));

		// Extract identifier (remaining bytes after null)
		var identifierData = data.Slice (nullIndex + 1);
		var identifier = new BinaryData (identifierData.ToArray ());

		var frame = new UniqueFileIdFrame (owner, identifier);
		return UniqueFileIdFrameReadResult.Success (frame, data.Length);
	}

	/// <summary>
	/// Renders the frame content to binary data.
	/// </summary>
	/// <returns>The frame content including owner and identifier.</returns>
	public BinaryData RenderContent ()
	{
		var ownerBytes = System.Text.Encoding.ASCII.GetBytes (Owner);
		var totalSize = ownerBytes.Length + 1 + Identifier.Length; // owner + null + identifier

		using var builder = new BinaryDataBuilder (totalSize);

		// Owner (null-terminated ASCII)
		builder.Add (ownerBytes);
		builder.Add ((byte)0);

		// Identifier
		if (!Identifier.IsEmpty)
			builder.Add (Identifier);

		return builder.ToBinaryData ();
	}
}

/// <summary>
/// Represents the result of reading a UFID frame.
/// </summary>
public readonly struct UniqueFileIdFrameReadResult : IEquatable<UniqueFileIdFrameReadResult>
{
	/// <summary>
	/// Gets the parsed frame, or null if parsing failed.
	/// </summary>
	public UniqueFileIdFrame? Frame { get; }

	/// <summary>
	/// Gets a value indicating whether parsing succeeded.
	/// </summary>
	public bool IsSuccess => Frame is not null && Error is null;

	/// <summary>
	/// Gets the error message if parsing failed.
	/// </summary>
	public string? Error { get; }

	/// <summary>
	/// Gets the number of bytes consumed.
	/// </summary>
	public int BytesConsumed { get; }

	UniqueFileIdFrameReadResult (UniqueFileIdFrame? frame, string? error, int bytesConsumed)
	{
		Frame = frame;
		Error = error;
		BytesConsumed = bytesConsumed;
	}

	/// <summary>
	/// Creates a successful result.
	/// </summary>
	public static UniqueFileIdFrameReadResult Success (UniqueFileIdFrame frame, int bytesConsumed) =>
		new (frame, null, bytesConsumed);

	/// <summary>
	/// Creates a failure result.
	/// </summary>
	public static UniqueFileIdFrameReadResult Failure (string error) =>
		new (null, error, 0);

	/// <inheritdoc/>
	public bool Equals (UniqueFileIdFrameReadResult other) =>
		ReferenceEquals (Frame, other.Frame) &&
		Error == other.Error &&
		BytesConsumed == other.BytesConsumed;

	/// <inheritdoc/>
	public override bool Equals (object? obj) =>
		obj is UniqueFileIdFrameReadResult other && Equals (other);

	/// <inheritdoc/>
	public override int GetHashCode () =>
		HashCode.Combine (Frame, Error, BytesConsumed);

	/// <summary>
	/// Determines whether two results are equal.
	/// </summary>
	public static bool operator == (UniqueFileIdFrameReadResult left, UniqueFileIdFrameReadResult right) =>
		left.Equals (right);

	/// <summary>
	/// Determines whether two results are not equal.
	/// </summary>
	public static bool operator != (UniqueFileIdFrameReadResult left, UniqueFileIdFrameReadResult right) =>
		!left.Equals (right);
}
