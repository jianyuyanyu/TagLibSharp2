// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Aiff;
using TagLibSharp2.Mpeg;
using TagLibSharp2.Ogg;
using TagLibSharp2.Riff;
using TagLibSharp2.Xiph;

namespace TagLibSharp2.Core;

/// <summary>
/// Provides factory methods to open media files with automatic format detection.
/// </summary>
/// <remarks>
/// <para>
/// This class auto-detects the file format and returns the appropriate file object.
/// Supported formats include:
/// </para>
/// <list type="bullet">
/// <item>MP3 (ID3v1, ID3v2)</item>
/// <item>FLAC (Vorbis Comment, FLAC metadata)</item>
/// <item>Ogg Vorbis (Vorbis Comment)</item>
/// <item>WAV (RIFF INFO, ID3v2)</item>
/// <item>AIFF (ID3 chunk)</item>
/// </list>
/// </remarks>
/// <example>
/// <code>
/// var result = MediaFile.Open("song.flac");
/// if (result.IsSuccess)
/// {
///     Console.WriteLine($"Title: {result.Tag?.Title}");
/// }
/// </code>
/// </example>
public static class MediaFile
{
	// Magic bytes for format detection
	static readonly byte[] FlacMagic = { 0x66, 0x4C, 0x61, 0x43 }; // "fLaC"
	static readonly byte[] OggMagic = { 0x4F, 0x67, 0x67, 0x53 }; // "OggS"
	static readonly byte[] Id3Magic = { 0x49, 0x44, 0x33 }; // "ID3"
	static readonly byte[] RiffMagic = { 0x52, 0x49, 0x46, 0x46 }; // "RIFF"
	static readonly byte[] WaveId = { 0x57, 0x41, 0x56, 0x45 }; // "WAVE"
	static readonly byte[] FormMagic = { 0x46, 0x4F, 0x52, 0x4D }; // "FORM"
	static readonly byte[] AiffId = { 0x41, 0x49, 0x46, 0x46 }; // "AIFF"
	static readonly byte[] AifcId = { 0x41, 0x49, 0x46, 0x43 }; // "AIFC"

	/// <summary>
	/// Opens a media file and returns a unified result.
	/// </summary>
	/// <param name="path">The path to the media file.</param>
	/// <param name="fileSystem">Optional file system abstraction for testing.</param>
	/// <returns>A result containing the opened file or an error.</returns>
	public static MediaFileResult Open (string path, IFileSystem? fileSystem = null)
	{
#if NETSTANDARD2_0 || NETSTANDARD2_1
		if (path is null)
			throw new ArgumentNullException (nameof (path));
#else
		ArgumentNullException.ThrowIfNull (path);
#endif

		var readResult = FileHelper.SafeReadAllBytes (path, fileSystem);
		if (!readResult.IsSuccess)
			return MediaFileResult.Failure (readResult.Error!);

		return OpenFromData (readResult.Data!, path);
	}

	/// <summary>
	/// Opens a media file asynchronously and returns a unified result.
	/// </summary>
	/// <param name="path">The path to the media file.</param>
	/// <param name="fileSystem">Optional file system abstraction for testing.</param>
	/// <param name="cancellationToken">Cancellation token.</param>
	/// <returns>A result containing the opened file or an error.</returns>
	public static async Task<MediaFileResult> OpenAsync (
		string path,
		IFileSystem? fileSystem = null,
		CancellationToken cancellationToken = default)
	{
#if NETSTANDARD2_0 || NETSTANDARD2_1
		if (path is null)
			throw new ArgumentNullException (nameof (path));
#else
		ArgumentNullException.ThrowIfNull (path);
#endif

		var readResult = await FileHelper.SafeReadAllBytesAsync (path, fileSystem, cancellationToken)
			.ConfigureAwait (false);
		if (!readResult.IsSuccess)
			return MediaFileResult.Failure (readResult.Error!);

		return OpenFromData (readResult.Data!, path);
	}

	/// <summary>
	/// Opens a media file from raw bytes.
	/// </summary>
	/// <param name="data">The file data.</param>
	/// <param name="pathHint">Optional file path hint for extension-based detection.</param>
	/// <returns>A result containing the opened file or an error.</returns>
	public static MediaFileResult OpenFromData (ReadOnlyMemory<byte> data, string? pathHint = null)
	{
		var format = DetectFormat (data.Span, pathHint);

		return format switch {
			MediaFormat.Flac => OpenFlac (data),
			MediaFormat.OggVorbis => OpenOggVorbis (data),
			MediaFormat.Mp3 => OpenMp3 (data),
			MediaFormat.Wav => OpenWav (data),
			MediaFormat.Aiff => OpenAiff (data),
			_ => MediaFileResult.Failure ($"Unknown or unsupported file format{(pathHint is not null ? $": {pathHint}" : "")}")
		};
	}

	/// <summary>
	/// Detects the format of a media file.
	/// </summary>
	/// <param name="data">The file data (at least first 12 bytes).</param>
	/// <param name="pathHint">Optional file path for extension fallback.</param>
	/// <returns>The detected format.</returns>
	public static MediaFormat DetectFormat (ReadOnlySpan<byte> data, string? pathHint = null)
	{
		// Check magic bytes first
		if (data.Length >= 4) {
			// FLAC: starts with "fLaC"
			if (data[0] == FlacMagic[0] && data[1] == FlacMagic[1] &&
				data[2] == FlacMagic[2] && data[3] == FlacMagic[3])
				return MediaFormat.Flac;

			// Ogg: starts with "OggS"
			if (data[0] == OggMagic[0] && data[1] == OggMagic[1] &&
				data[2] == OggMagic[2] && data[3] == OggMagic[3])
				return MediaFormat.OggVorbis;

			// RIFF: starts with "RIFF" + 4 bytes size + "WAVE"
			if (data.Length >= 12 &&
				data[0] == RiffMagic[0] && data[1] == RiffMagic[1] &&
				data[2] == RiffMagic[2] && data[3] == RiffMagic[3] &&
				data[8] == WaveId[0] && data[9] == WaveId[1] &&
				data[10] == WaveId[2] && data[11] == WaveId[3])
				return MediaFormat.Wav;

			// AIFF/AIFC: starts with "FORM" + 4 bytes size + "AIFF" or "AIFC"
			if (data.Length >= 12 &&
				data[0] == FormMagic[0] && data[1] == FormMagic[1] &&
				data[2] == FormMagic[2] && data[3] == FormMagic[3] &&
				((data[8] == AiffId[0] && data[9] == AiffId[1] &&
				  data[10] == AiffId[2] && data[11] == AiffId[3]) ||
				 (data[8] == AifcId[0] && data[9] == AifcId[1] &&
				  data[10] == AifcId[2] && data[11] == AifcId[3])))
				return MediaFormat.Aiff;
		}

		if (data.Length >= 3) {
			// ID3v2: starts with "ID3"
			if (data[0] == Id3Magic[0] && data[1] == Id3Magic[1] && data[2] == Id3Magic[2])
				return MediaFormat.Mp3;
		}

		// Check for MP3 frame sync (0xFF 0xFB, 0xFF 0xFA, 0xFF 0xF3, 0xFF 0xF2)
		if (data.Length >= 2 && data[0] == 0xFF && (data[1] & 0xE0) == 0xE0)
			return MediaFormat.Mp3;

		// Fall back to extension
		if (!string.IsNullOrEmpty (pathHint)) {
			var ext = Path.GetExtension (pathHint).ToUpperInvariant ();
			return ext switch {
				".FLAC" => MediaFormat.Flac,
				".OGG" => MediaFormat.OggVorbis,
				".MP3" => MediaFormat.Mp3,
				".WAV" => MediaFormat.Wav,
				".AIF" or ".AIFF" or ".AIFC" => MediaFormat.Aiff,
				_ => MediaFormat.Unknown
			};
		}

		return MediaFormat.Unknown;
	}

	static MediaFileResult OpenFlac (ReadOnlyMemory<byte> data)
	{
		var result = FlacFile.Read (data.Span);
		if (!result.IsSuccess)
			return MediaFileResult.Failure (result.Error!);

		return MediaFileResult.Success (result.File!, result.File!.VorbisComment, MediaFormat.Flac);
	}

	static MediaFileResult OpenOggVorbis (ReadOnlyMemory<byte> data)
	{
		var result = OggVorbisFile.Read (data.Span);
		if (!result.IsSuccess)
			return MediaFileResult.Failure (result.Error!);

		return MediaFileResult.Success (result.File!, result.File!.VorbisComment, MediaFormat.OggVorbis);
	}

	static MediaFileResult OpenMp3 (ReadOnlyMemory<byte> data)
	{
		var result = Mp3File.Read (data.Span);
		if (!result.IsSuccess)
			return MediaFileResult.Failure (result.Error!);

		// Prefer ID3v2 tag, fall back to ID3v1
		Tag? tag = result.File!.Id3v2Tag is not null
			? result.File.Id3v2Tag
			: result.File.Id3v1Tag;
		return MediaFileResult.Success (result.File, tag, MediaFormat.Mp3);
	}

	static MediaFileResult OpenWav (ReadOnlyMemory<byte> data)
	{
		var binaryData = new BinaryData (data.Span);
		var file = WavFile.ReadFromData (binaryData);
		if (file is null)
			return MediaFileResult.Failure ("Failed to parse WAV file");

		// Prefer ID3v2 tag, fall back to RIFF INFO tag
		Tag? tag = file.Id3v2Tag is not null
			? file.Id3v2Tag
			: file.InfoTag;
		return MediaFileResult.Success (file, tag, MediaFormat.Wav);
	}

	static MediaFileResult OpenAiff (ReadOnlyMemory<byte> data)
	{
		var binaryData = new BinaryData (data.Span);
		if (!AiffFile.TryParse (binaryData, out var file) || file is null)
			return MediaFileResult.Failure ("Failed to parse AIFF file");

		return MediaFileResult.Success (file, file.Tag, MediaFormat.Aiff);
	}
}

/// <summary>
/// Represents the result of opening a media file.
/// </summary>
public sealed class MediaFileResult
{
	/// <summary>
	/// Gets a value indicating whether the file was successfully opened.
	/// </summary>
	public bool IsSuccess { get; }

	/// <summary>
	/// Gets the error message if the operation failed.
	/// </summary>
	public string? Error { get; }

	/// <summary>
	/// Gets the underlying file object (FlacFile, Mp3File, etc.).
	/// </summary>
	public object? File { get; }

	/// <summary>
	/// Gets the tag from the file (VorbisComment, Id3v2Tag, etc.).
	/// </summary>
	public Tag? Tag { get; }

	/// <summary>
	/// Gets the detected media format.
	/// </summary>
	public MediaFormat Format { get; }

	MediaFileResult (bool isSuccess, string? error, object? file, Tag? tag, MediaFormat format)
	{
		IsSuccess = isSuccess;
		Error = error;
		File = file;
		Tag = tag;
		Format = format;
	}

	/// <summary>
	/// Creates a successful result.
	/// </summary>
	internal static MediaFileResult Success (object file, Tag? tag, MediaFormat format) =>
		new (true, null, file, tag, format);

	/// <summary>
	/// Creates a failed result.
	/// </summary>
	internal static MediaFileResult Failure (string error) =>
		new (false, error, null, null, MediaFormat.Unknown);

	/// <summary>
	/// Gets the file as a specific type.
	/// </summary>
	/// <typeparam name="T">The file type (FlacFile, Mp3File, etc.).</typeparam>
	/// <returns>The file if it matches the type, otherwise null.</returns>
	public T? GetFileAs<T> () where T : class => File as T;
}

/// <summary>
/// Represents supported media file formats.
/// </summary>
public enum MediaFormat
{
	/// <summary>
	/// Unknown or unsupported format.
	/// </summary>
	Unknown = 0,

	/// <summary>
	/// FLAC audio format.
	/// </summary>
	Flac,

	/// <summary>
	/// Ogg Vorbis audio format.
	/// </summary>
	OggVorbis,

	/// <summary>
	/// MP3 audio format.
	/// </summary>
	Mp3,

	/// <summary>
	/// WAV audio format (RIFF container).
	/// </summary>
	Wav,

	/// <summary>
	/// AIFF/AIFC audio format (IFF container).
	/// </summary>
	Aiff
}
