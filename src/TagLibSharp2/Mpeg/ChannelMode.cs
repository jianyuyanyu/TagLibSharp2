// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace TagLibSharp2.Mpeg;

/// <summary>
/// MPEG audio channel mode.
/// </summary>
public enum ChannelMode
{
	/// <summary>
	/// Stereo (left and right channels encoded independently).
	/// </summary>
	Stereo = 0,

	/// <summary>
	/// Joint stereo (uses correlations between channels for compression).
	/// </summary>
	JointStereo = 1,

	/// <summary>
	/// Dual channel (two independent mono channels).
	/// </summary>
	DualChannel = 2,

	/// <summary>
	/// Single channel (mono).
	/// </summary>
	Mono = 3
}
