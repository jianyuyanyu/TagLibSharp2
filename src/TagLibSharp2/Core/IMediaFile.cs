// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace TagLibSharp2.Core;

/// <summary>
/// Represents a media file that can be read, modified, and saved.
/// </summary>
/// <remarks>
/// <para>
/// This interface provides a common abstraction for all media file types,
/// enabling polymorphic code that works with any supported format.
/// </para>
/// <para>
/// All file classes (FlacFile, Mp3File, Mp4File, etc.) implement this interface.
/// </para>
/// <para>
/// Use <see cref="MediaTypes"/> to determine what kind of content the file contains,
/// then access the appropriate properties (<see cref="AudioProperties"/>,
/// <see cref="VideoProperties"/>, or <see cref="ImageProperties"/>).
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Work with any media file type
/// IMediaFile file = result.File!;
/// Console.WriteLine($"Title: {file.Tag?.Title}");
/// Console.WriteLine($"Format: {file.Format}");
/// Console.WriteLine($"Media Types: {file.MediaTypes}");
///
/// if (file.AudioProperties is { } audio)
/// {
///     Console.WriteLine($"Duration: {audio.Duration}");
///     Console.WriteLine($"Sample Rate: {audio.SampleRate}Hz");
/// }
/// </code>
/// </example>
public interface IMediaFile : IDisposable
{
	/// <summary>
	/// Gets the metadata tag for this file.
	/// </summary>
	/// <remarks>
	/// The returned tag type depends on the file format. For example:
	/// <list type="bullet">
	/// <item>FLAC files return VorbisComment</item>
	/// <item>MP3 files return a combined tag (Id3v2Tag preferred, Id3v1Tag fallback)</item>
	/// <item>MP4 files return Mp4Tag</item>
	/// </list>
	/// </remarks>
	Tag? Tag { get; }

	/// <summary>
	/// Gets the audio properties (duration, bitrate, sample rate, channels) for this file.
	/// </summary>
	/// <remarks>
	/// Returns null if the file doesn't contain audio or if parsing failed.
	/// Check <see cref="MediaTypes"/> to determine if audio is present.
	/// </remarks>
	IMediaProperties? AudioProperties { get; }

	/// <summary>
	/// Gets the video properties (resolution, frame rate, codec) for this file.
	/// </summary>
	/// <remarks>
	/// Returns null if the file doesn't contain video or if parsing failed.
	/// Check <see cref="MediaTypes"/> to determine if video is present.
	/// </remarks>
	VideoProperties? VideoProperties { get; }

	/// <summary>
	/// Gets the image properties (dimensions, color depth) for this file.
	/// </summary>
	/// <remarks>
	/// Returns null if the file doesn't contain image data or if parsing failed.
	/// Check <see cref="MediaTypes"/> to determine if image data is present.
	/// </remarks>
	ImageProperties? ImageProperties { get; }

	/// <summary>
	/// Gets the types of media content present in this file.
	/// </summary>
	/// <remarks>
	/// This is a flags enum - a file may contain multiple media types.
	/// For example, a video file typically has both <see cref="Core.MediaTypes.Audio"/>
	/// and <see cref="Core.MediaTypes.Video"/> flags set.
	/// </remarks>
	MediaTypes MediaTypes { get; }

	/// <summary>
	/// Gets the path this file was read from, if applicable.
	/// </summary>
	/// <remarks>
	/// This is only set when the file was loaded using ReadFromFile or ReadFromFileAsync.
	/// Files parsed from byte data will have null SourcePath.
	/// </remarks>
	string? SourcePath { get; }

	/// <summary>
	/// Gets the detected format of this media file.
	/// </summary>
	MediaFormat Format { get; }
}
