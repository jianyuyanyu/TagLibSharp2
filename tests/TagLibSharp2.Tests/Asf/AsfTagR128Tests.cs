// Copyright (c) 2025-2026 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using TagLibSharp2.Asf;

namespace TagLibSharp2.Tests.Asf;

[TestClass]
public class AsfTagR128Tests
{
	[TestMethod]
	public void R128TrackGain_GetSet ()
	{
		var tag = new AsfTag ();

		tag.R128TrackGain = "-512";

		Assert.AreEqual ("-512", tag.R128TrackGain);
	}

	[TestMethod]
	public void R128AlbumGain_GetSet ()
	{
		var tag = new AsfTag ();

		tag.R128AlbumGain = "256";

		Assert.AreEqual ("256", tag.R128AlbumGain);
	}

	[TestMethod]
	public void R128TrackGainDb_Conversion ()
	{
		var tag = new AsfTag ();

		// Set using dB property (should convert to Q7.8)
		tag.R128TrackGainDb = -2.0;

		// Q7.8: -2 * 256 = -512
		Assert.AreEqual ("-512", tag.R128TrackGain);
		Assert.AreEqual (-2.0, tag.R128TrackGainDb);
	}

	[TestMethod]
	public void R128AlbumGainDb_Conversion ()
	{
		var tag = new AsfTag ();

		// Set using dB property
		tag.R128AlbumGainDb = 1.5;

		// Q7.8: 1.5 * 256 = 384
		Assert.AreEqual ("384", tag.R128AlbumGain);
		Assert.AreEqual (1.5, tag.R128AlbumGainDb);
	}

	[TestMethod]
	public void R128Gain_ReturnsNull_WhenNotSet ()
	{
		var tag = new AsfTag ();

		Assert.IsNull (tag.R128TrackGain);
		Assert.IsNull (tag.R128AlbumGain);
		Assert.IsNull (tag.R128TrackGainDb);
		Assert.IsNull (tag.R128AlbumGainDb);
	}

	[TestMethod]
	public void R128Gain_SetToNull_Clears ()
	{
		var tag = new AsfTag ();
		tag.R128TrackGain = "-256";

		tag.R128TrackGain = null;

		Assert.IsNull (tag.R128TrackGain);
	}
}
