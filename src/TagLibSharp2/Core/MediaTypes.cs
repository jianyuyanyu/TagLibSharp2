// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace TagLibSharp2.Core;

/// <summary>
/// Specifies the types of media content present in a file.
/// </summary>
/// <remarks>
/// This is a flags enum, allowing multiple media types to be combined.
/// For example, a video file with audio would have both Audio and Video flags set.
/// </remarks>
[Flags]
public enum MediaTypes
{
	/// <summary>
	/// No media type detected.
	/// </summary>
	None = 0,

	/// <summary>
	/// The file contains audio content.
	/// </summary>
	Audio = 1 << 0,

	/// <summary>
	/// The file contains video content.
	/// </summary>
	Video = 1 << 1,

	/// <summary>
	/// The file contains image content.
	/// </summary>
	Image = 1 << 2
}
