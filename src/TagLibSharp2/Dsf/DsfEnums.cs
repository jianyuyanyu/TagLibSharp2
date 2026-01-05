// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

#pragma warning disable CA1008 // Enums should have zero value
#pragma warning disable CA1028 // Enum storage should be Int32

namespace TagLibSharp2.Dsf;

/// <summary>
/// Standard DSD sample rates.
/// </summary>
public enum DsfSampleRate
{
	/// <summary>Unknown or non-standard sample rate.</summary>
	Unknown = 0,

	/// <summary>DSD64 - 2.8224 MHz (64x CD sample rate).</summary>
	DSD64 = 2822400,

	/// <summary>DSD128 - 5.6448 MHz (128x CD sample rate).</summary>
	DSD128 = 5644800,

	/// <summary>DSD256 - 11.2896 MHz (256x CD sample rate).</summary>
	DSD256 = 11289600,

	/// <summary>DSD512 - 22.5792 MHz (512x CD sample rate).</summary>
	DSD512 = 22579200,

	/// <summary>DSD1024 - 45.1584 MHz (1024x CD sample rate).</summary>
	DSD1024 = 45158400
}

/// <summary>
/// DSF channel type identifiers.
/// </summary>
public enum DsfChannelType
{
	/// <summary>Mono (1 channel).</summary>
	Mono = 1,

	/// <summary>Stereo (2 channels).</summary>
	Stereo = 2,

	/// <summary>3 channels.</summary>
	ThreeChannels = 3,

	/// <summary>Quad (4 channels).</summary>
	Quad = 4,

	/// <summary>5 channels (5.0 surround).</summary>
	Surround50 = 5,

	/// <summary>5.1 surround (6 channels).</summary>
	Surround51 = 6
}

/// <summary>
/// DSF format identifiers.
/// </summary>
public enum DsfFormatId
{
	/// <summary>Raw DSD data.</summary>
	DsdRaw = 0
}
