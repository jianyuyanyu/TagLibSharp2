// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace TagLibSharp2.Id3;

/// <summary>
/// ID3v1 genre lookup table containing 80 standard genres plus Winamp extensions.
/// </summary>
public static class Id3v1Genre
{
	/// <summary>
	/// Standard ID3v1 genres (0-79) plus Winamp extensions (80-191).
	/// </summary>
	static readonly string[] Genres =
	[
		// 0-9: Original ID3v1
		"Blues",
		"Classic Rock",
		"Country",
		"Dance",
		"Disco",
		"Funk",
		"Grunge",
		"Hip-Hop",
		"Jazz",
		"Metal",

		// 10-19
		"New Age",
		"Oldies",
		"Other",
		"Pop",
		"R&B",
		"Rap",
		"Reggae",
		"Rock",
		"Techno",
		"Industrial",

		// 20-29
		"Alternative",
		"Ska",
		"Death Metal",
		"Pranks",
		"Soundtrack",
		"Euro-Techno",
		"Ambient",
		"Trip-Hop",
		"Vocal",
		"Jazz+Funk",

		// 30-39
		"Fusion",
		"Trance",
		"Classical",
		"Instrumental",
		"Acid",
		"House",
		"Game",
		"Sound Clip",
		"Gospel",
		"Noise",

		// 40-49
		"AlternRock",
		"Bass",
		"Soul",
		"Punk",
		"Space",
		"Meditative",
		"Instrumental Pop",
		"Instrumental Rock",
		"Ethnic",
		"Gothic",

		// 50-59
		"Darkwave",
		"Techno-Industrial",
		"Electronic",
		"Pop-Folk",
		"Eurodance",
		"Dream",
		"Southern Rock",
		"Comedy",
		"Cult",
		"Gangsta",

		// 60-69
		"Top 40",
		"Christian Rap",
		"Pop/Funk",
		"Jungle",
		"Native American",
		"Cabaret",
		"New Wave",
		"Psychadelic",
		"Rave",
		"Showtunes",

		// 70-79
		"Trailer",
		"Lo-Fi",
		"Tribal",
		"Acid Punk",
		"Acid Jazz",
		"Polka",
		"Retro",
		"Musical",
		"Rock & Roll",
		"Hard Rock",

		// 80-89: Winamp extensions
		"Folk",
		"Folk-Rock",
		"National Folk",
		"Swing",
		"Fast Fusion",
		"Bebop",
		"Latin",
		"Revival",
		"Celtic",
		"Bluegrass",

		// 90-99
		"Avantgarde",
		"Gothic Rock",
		"Progressive Rock",
		"Psychedelic Rock",
		"Symphonic Rock",
		"Slow Rock",
		"Big Band",
		"Chorus",
		"Easy Listening",
		"Acoustic",

		// 100-109
		"Humour",
		"Speech",
		"Chanson",
		"Opera",
		"Chamber Music",
		"Sonata",
		"Symphony",
		"Booty Bass",
		"Primus",
		"Porn Groove",

		// 110-119
		"Satire",
		"Slow Jam",
		"Club",
		"Tango",
		"Samba",
		"Folklore",
		"Ballad",
		"Power Ballad",
		"Rhythmic Soul",
		"Freestyle",

		// 120-129
		"Duet",
		"Punk Rock",
		"Drum Solo",
		"A capella",
		"Euro-House",
		"Dance Hall",
		"Goa",
		"Drum & Bass",
		"Club-House",
		"Hardcore",

		// 130-139
		"Terror",
		"Indie",
		"BritPop",
		"Negerpunk",
		"Polsk Punk",
		"Beat",
		"Christian Gangsta Rap",
		"Heavy Metal",
		"Black Metal",
		"Crossover",

		// 140-149
		"Contemporary Christian",
		"Christian Rock",
		"Merengue",
		"Salsa",
		"Thrash Metal",
		"Anime",
		"JPop",
		"Synthpop",
		"Abstract",
		"Art Rock",

		// 150-159
		"Baroque",
		"Bhangra",
		"Big Beat",
		"Breakbeat",
		"Chillout",
		"Downtempo",
		"Dub",
		"EBM",
		"Eclectic",
		"Electro",

		// 160-169
		"Electroclash",
		"Emo",
		"Experimental",
		"Garage",
		"Global",
		"IDM",
		"Illbient",
		"Industro-Goth",
		"Jam Band",
		"Krautrock",

		// 170-179
		"Leftfield",
		"Lounge",
		"Math Rock",
		"New Romantic",
		"Nu-Breakz",
		"Post-Punk",
		"Post-Rock",
		"Psytrance",
		"Shoegaze",
		"Space Rock",

		// 180-191
		"Trop Rock",
		"World Music",
		"Neoclassical",
		"Audiobook",
		"Audio Theatre",
		"Neue Deutsche Welle",
		"Podcast",
		"Indie Rock",
		"G-Funk",
		"Dubstep",
		"Garage Rock",
		"Psybient"
	];

	/// <summary>
	/// Gets the genre name for the specified index.
	/// </summary>
	/// <param name="index">The genre index (0-191).</param>
	/// <returns>The genre name, or null if the index is out of range or undefined (255).</returns>
	public static string? GetName (byte index) =>
		index < Genres.Length ? Genres[index] : null;

	/// <summary>
	/// Gets the genre index for the specified name.
	/// </summary>
	/// <param name="name">The genre name (case-insensitive).</param>
	/// <returns>The genre index, or 255 if not found.</returns>
	public static byte GetIndex (string? name)
	{
		if (string.IsNullOrEmpty (name))
			return 255;

		for (var i = 0; i < Genres.Length; i++) {
			if (string.Equals (Genres[i], name, StringComparison.OrdinalIgnoreCase))
				return (byte)i;
		}

		return 255;
	}

	/// <summary>
	/// Gets the total number of defined genres.
	/// </summary>
	public static int Count => Genres.Length;
}
