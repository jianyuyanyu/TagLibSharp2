// Copyright (c) 2025 Stephen Shaw and contributors
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using System.Text;
using TagLibSharp2.Core;
using TagLibSharp2.Id3.Id3v2;
using TagLibSharp2.Id3.Id3v2.Frames;
using TagLibSharp2.Ogg;
using TagLibSharp2.Xiph;

namespace TagLibSharp2.Tests;

/// <summary>
/// Centralized test data builders to reduce duplication and ensure consistency.
/// </summary>
/// <remarks>
/// Each format has its own nested builder class. Use these instead of duplicating
/// helper methods in individual test files.
/// </remarks>
public static class TestBuilders
{
	/// <summary>
	/// Builders for ID3v2 tag test data.
	/// </summary>
	public static class Id3v2
	{
		/// <summary>
		/// Creates a minimal valid ID3v2 header.
		/// </summary>
		/// <param name="version">Major version (3 or 4).</param>
		/// <param name="size">Tag body size (syncsafe encoded).</param>
		/// <returns>10-byte ID3v2 header.</returns>
		public static byte[] CreateHeader (byte version, uint size)
		{
			var data = new byte[TestConstants.Id3v2.HeaderSize];
			TestConstants.Magic.Id3.CopyTo (data, 0);
			data[3] = version;
			data[4] = TestConstants.Id3v2.MinorVersion;
			data[5] = 0; // flags

			// Syncsafe size
			data[6] = (byte)((size >> 21) & 0x7F);
			data[7] = (byte)((size >> 14) & 0x7F);
			data[8] = (byte)((size >> 7) & 0x7F);
			data[9] = (byte)(size & 0x7F);

			return data;
		}

		/// <summary>
		/// Creates a text frame (TIT2, TPE1, etc.) with Latin-1 encoding.
		/// </summary>
		/// <param name="frameId">4-character frame ID.</param>
		/// <param name="text">Text content.</param>
		/// <param name="version">ID3v2 major version (3 or 4).</param>
		/// <returns>Complete frame bytes (header + content).</returns>
		public static byte[] CreateTextFrame (string frameId, string text, byte version)
		{
			var textBytes = Encoding.Latin1.GetBytes (text);
			var frameContent = new byte[1 + textBytes.Length];
			frameContent[0] = TestConstants.Id3v2.EncodingLatin1;
			Array.Copy (textBytes, 0, frameContent, 1, textBytes.Length);

			return CreateFrame (frameId, frameContent, version);
		}

		/// <summary>
		/// Creates a raw frame with the given content.
		/// </summary>
		/// <param name="frameId">4-character frame ID.</param>
		/// <param name="content">Frame content bytes.</param>
		/// <param name="version">ID3v2 major version (3 or 4).</param>
		/// <returns>Complete frame bytes (header + content).</returns>
		public static byte[] CreateFrame (string frameId, byte[] content, byte version)
		{
			var frameSize = content.Length;
			var frame = new byte[TestConstants.Id3v2.FrameHeaderSize + frameSize];

			// Frame ID
			Encoding.ASCII.GetBytes (frameId).CopyTo (frame, 0);

			// Size (syncsafe for v2.4, big-endian for v2.3)
			if (version == TestConstants.Id3v2.Version4) {
				frame[4] = (byte)((frameSize >> 21) & 0x7F);
				frame[5] = (byte)((frameSize >> 14) & 0x7F);
				frame[6] = (byte)((frameSize >> 7) & 0x7F);
				frame[7] = (byte)(frameSize & 0x7F);
			} else {
				frame[4] = (byte)((frameSize >> 24) & 0xFF);
				frame[5] = (byte)((frameSize >> 16) & 0xFF);
				frame[6] = (byte)((frameSize >> 8) & 0xFF);
				frame[7] = (byte)(frameSize & 0xFF);
			}

			// Flags (2 bytes, zeroes)
			frame[8] = 0;
			frame[9] = 0;

			// Content
			Array.Copy (content, 0, frame, TestConstants.Id3v2.FrameHeaderSize, frameSize);

			return frame;
		}

		/// <summary>
		/// Creates a complete ID3v2 tag with a single text frame.
		/// </summary>
		/// <param name="frameId">4-character frame ID.</param>
		/// <param name="text">Text content.</param>
		/// <param name="version">ID3v2 major version (3 or 4).</param>
		/// <returns>Complete tag bytes (header + frame).</returns>
		public static byte[] CreateTagWithTextFrame (string frameId, string text, byte version)
		{
			var frame = CreateTextFrame (frameId, text, version);
			var header = CreateHeader (version, (uint)frame.Length);

			var result = new byte[header.Length + frame.Length];
			Array.Copy (header, result, header.Length);
			Array.Copy (frame, 0, result, header.Length, frame.Length);

			return result;
		}

		/// <summary>
		/// Creates a complete ID3v2 tag with multiple text frames.
		/// </summary>
		/// <param name="version">ID3v2 major version (3 or 4).</param>
		/// <param name="frames">Dictionary of frame ID to text value.</param>
		/// <returns>Complete tag bytes.</returns>
		public static byte[] CreateTagWithFrames (byte version, params (string frameId, string text)[] frames)
		{
			var allFrames = frames.SelectMany (f => CreateTextFrame (f.frameId, f.text, version)).ToArray ();
			var header = CreateHeader (version, (uint)allFrames.Length);

			var result = new byte[header.Length + allFrames.Length];
			Array.Copy (header, result, header.Length);
			Array.Copy (allFrames, 0, result, header.Length, allFrames.Length);

			return result;
		}

		/// <summary>
		/// Creates raw text frame content with specified encoding.
		/// Used for testing text frame parsing with different encodings.
		/// </summary>
		/// <param name="encoding">Text encoding type.</param>
		/// <param name="text">Text content.</param>
		/// <returns>Frame content bytes (encoding byte + encoded text).</returns>
		public static byte[] CreateTextFrameData (TextEncodingType encoding, string text) => encoding switch
		{
			TextEncodingType.Latin1 => [(byte)encoding, .. Encoding.Latin1.GetBytes (text)],
			TextEncodingType.Utf16WithBom => [(byte)encoding, 0xFF, 0xFE, .. Encoding.Unicode.GetBytes (text)],
			TextEncodingType.Utf16BE => [(byte)encoding, .. Encoding.BigEndianUnicode.GetBytes (text)],
			TextEncodingType.Utf8 => [(byte)encoding, .. Encoding.UTF8.GetBytes (text)],
			_ => throw new ArgumentException ($"Unsupported encoding: {encoding}", nameof (encoding))
		};

		/// <summary>
		/// Creates a minimal empty ID3v2 tag (header only, size = 0).
		/// </summary>
		public static byte[] CreateEmptyTag (byte version = 4) => CreateHeader (version, 0);

		/// <summary>
		/// Creates an ID3v2 tag with padding.
		/// </summary>
		public static byte[] CreateTagWithPadding (byte version, int paddingSize)
		{
			var header = CreateHeader (version, (uint)paddingSize);
			var result = new byte[header.Length + paddingSize];
			Array.Copy (header, result, header.Length);
			return result;
		}

		/// <summary>
		/// Creates a minimal valid ID3v2 header with flags.
		/// </summary>
		public static byte[] CreateHeaderWithFlags (byte version, uint size, byte flags)
		{
			var data = CreateHeader (version, size);
			data[5] = flags;
			return data;
		}

		/// <summary>
		/// Creates an ID3v2.2 text frame (3-byte ID, 3-byte size).
		/// </summary>
		/// <param name="frameId">3-character frame ID (e.g., "TT2").</param>
		/// <param name="text">Text content.</param>
		/// <returns>Complete frame bytes (6-byte header + content).</returns>
		public static byte[] CreateTextFrameV22 (string frameId, string text)
		{
			var textBytes = Encoding.Latin1.GetBytes (text);
			var frameContent = new byte[1 + textBytes.Length];
			frameContent[0] = TestConstants.Id3v2.EncodingLatin1;
			Array.Copy (textBytes, 0, frameContent, 1, textBytes.Length);

			return CreateFrameV22 (frameId, frameContent);
		}

		/// <summary>
		/// Creates a raw ID3v2.2 frame (6-byte header).
		/// </summary>
		public static byte[] CreateFrameV22 (string frameId, byte[] content)
		{
			var frameSize = content.Length;
			var frame = new byte[TestConstants.Id3v2.FrameHeaderSizeV22 + frameSize];

			// Frame ID (3 bytes)
			Encoding.ASCII.GetBytes (frameId).CopyTo (frame, 0);

			// Size (3-byte big-endian)
			frame[3] = (byte)((frameSize >> 16) & 0xFF);
			frame[4] = (byte)((frameSize >> 8) & 0xFF);
			frame[5] = (byte)(frameSize & 0xFF);

			// Content
			Array.Copy (content, 0, frame, TestConstants.Id3v2.FrameHeaderSizeV22, frameSize);

			return frame;
		}

		/// <summary>
		/// Creates a complete ID3v2.2 tag with a single text frame.
		/// </summary>
		public static byte[] CreateTagWithTextFrameV22 (string frameId, string text)
		{
			var frame = CreateTextFrameV22 (frameId, text);
			var header = CreateHeader (TestConstants.Id3v2.Version2, (uint)frame.Length);

			var result = new byte[header.Length + frame.Length];
			Array.Copy (header, result, header.Length);
			Array.Copy (frame, 0, result, header.Length, frame.Length);

			return result;
		}

		/// <summary>
		/// Applies unsynchronization to data (inserts 0x00 after each 0xFF).
		/// </summary>
		public static byte[] ApplyUnsynchronization (byte[] data)
		{
			// Count how many 0xFF bytes need unsync
			var count = 0;
			foreach (var b in data) {
				if (b == 0xFF)
					count++;
			}

			if (count == 0)
				return data;

			// Create unsynchronized output
			var output = new byte[data.Length + count];
			var outIndex = 0;
			foreach (var b in data) {
				output[outIndex++] = b;
				if (b == 0xFF)
					output[outIndex++] = 0x00;
			}

			return output;
		}

		/// <summary>
		/// Creates an ID3v2 tag with unsynchronization applied.
		/// </summary>
		public static byte[] CreateTagWithUnsynchronization (byte version, byte[] frameData)
		{
			var unsyncData = ApplyUnsynchronization (frameData);
			var header = CreateHeaderWithFlags (version, (uint)unsyncData.Length, TestConstants.Id3v2.FlagUnsynchronization);

			var result = new byte[header.Length + unsyncData.Length];
			Array.Copy (header, result, header.Length);
			Array.Copy (unsyncData, 0, result, header.Length, unsyncData.Length);

			return result;
		}
	}

	/// <summary>
	/// Builders for ID3v1 tag test data.
	/// </summary>
	public static class Id3v1
	{
		/// <summary>
		/// Creates a complete ID3v1.0 tag.
		/// </summary>
		public static byte[] CreateTag (
			string title = "",
			string artist = "",
			string album = "",
			string year = "",
			string comment = "",
			byte genre = 0xFF)
		{
			var data = new byte[TestConstants.Id3v1.TagSize];
			TestConstants.Id3v1.Signature.CopyTo (data, 0);

			WriteFixedString (data, 3, title, TestConstants.Id3v1.TitleLength);
			WriteFixedString (data, 33, artist, TestConstants.Id3v1.ArtistLength);
			WriteFixedString (data, 63, album, TestConstants.Id3v1.AlbumLength);
			WriteFixedString (data, 93, year, TestConstants.Id3v1.YearLength);
			WriteFixedString (data, 97, comment, TestConstants.Id3v1.CommentLength);
			data[127] = genre;

			return data;
		}

		/// <summary>
		/// Creates a complete ID3v1.1 tag with track number.
		/// </summary>
		public static byte[] CreateTagV11 (
			string title = "",
			string artist = "",
			string album = "",
			string year = "",
			string comment = "",
			byte track = 0,
			byte genre = 0xFF)
		{
			var data = CreateTag (title, artist, album, year, comment, genre);

			// ID3v1.1: byte 125 = 0, byte 126 = track number
			data[125] = 0;
			data[126] = track;

			return data;
		}

		static void WriteFixedString (byte[] data, int offset, string value, int length)
		{
			var bytes = Encoding.Latin1.GetBytes (value);
			var copyLength = Math.Min (bytes.Length, length);
			Array.Copy (bytes, 0, data, offset, copyLength);
		}
	}

	/// <summary>
	/// Builders for FLAC file test data.
	/// </summary>
	public static class Flac
	{
		// Static readonly arrays to avoid allocations on each call
		static readonly byte[] StreamInfoHeaderLast = [0x80, 0x00, 0x00, 0x22];
		static readonly byte[] StreamInfoHeaderNotLast = [0x00, 0x00, 0x00, 0x22];
		static readonly byte[] DefaultBlockSizes = [0x10, 0x00, 0x10, 0x00]; // 4096/4096
		static readonly byte[] ZeroFrameSizes = [0x00, 0x00, 0x00, 0x00, 0x00, 0x00];
		static readonly byte[] Default44100StreamInfo = [0x0A, 0xC4, 0x42, 0xF0, 0x00, 0x00, 0x00, 0x00];

		/// <summary>
		/// Creates a minimal valid FLAC file with STREAMINFO.
		/// Default properties: 44100Hz, 2 channels, 16-bit, 0 samples.
		/// </summary>
		public static byte[] CreateMinimal ()
		{
			return CreateWithStreamInfo (44100, 2, 16, 0);
		}

		/// <summary>
		/// Creates a FLAC file with custom STREAMINFO properties.
		/// </summary>
		/// <param name="sampleRate">Sample rate in Hz.</param>
		/// <param name="channels">Number of channels (1-8).</param>
		/// <param name="bitsPerSample">Bits per sample (4-32).</param>
		/// <param name="totalSamples">Total number of samples.</param>
		public static byte[] CreateWithStreamInfo (int sampleRate, int channels, int bitsPerSample, ulong totalSamples)
		{
			var builder = new BinaryDataBuilder ();

			// Magic: "fLaC"
			builder.Add (TestConstants.Magic.Flac);

			// STREAMINFO block header: last=true, type=0, size=34
			builder.Add (StreamInfoHeaderLast);

			// min/max block size: 4096
			builder.Add (DefaultBlockSizes);
			// min/max frame size: 0 (unknown)
			builder.Add (ZeroFrameSizes);

			// Bytes 10-17: sample rate (20 bits), channels-1 (3 bits), bps-1 (5 bits), total samples (36 bits)
			var sr = sampleRate & 0xFFFFF;
			var ch = (channels - 1) & 0x07;
			var bps = (bitsPerSample - 1) & 0x1F;
			var samplesUpper = (int)((totalSamples >> 32) & 0x0F);
			var samplesLower = (uint)(totalSamples & 0xFFFFFFFF);

			builder.Add ((byte)((sr >> 12) & 0xFF));
			builder.Add ((byte)((sr >> 4) & 0xFF));
			builder.Add ((byte)(((sr & 0x0F) << 4) | ((ch & 0x07) << 1) | ((bps >> 4) & 0x01)));
			builder.Add ((byte)(((bps & 0x0F) << 4) | (samplesUpper & 0x0F)));
			builder.Add ((byte)((samplesLower >> 24) & 0xFF));
			builder.Add ((byte)((samplesLower >> 16) & 0xFF));
			builder.Add ((byte)((samplesLower >> 8) & 0xFF));
			builder.Add ((byte)(samplesLower & 0xFF));

			// MD5 signature (16 bytes - all zeros)
			builder.AddZeros (16);

			return builder.ToArray ();
		}

		/// <summary>
		/// Creates a FLAC file with a specific MD5 signature.
		/// </summary>
		/// <param name="md5">16-byte MD5 signature, or null for all zeros.</param>
		public static byte[] CreateWithMd5 (byte[]? md5 = null)
		{
			var builder = new BinaryDataBuilder ();

			// Magic: "fLaC"
			builder.Add (TestConstants.Magic.Flac);

			// STREAMINFO block header: last=true, type=0, size=34
			builder.Add (StreamInfoHeaderLast);

			// min/max block size: 4096
			builder.Add (DefaultBlockSizes);
			// min/max frame size: 0 (unknown)
			builder.Add (ZeroFrameSizes);

			// Sample rate, channels, bps, total samples (using defaults)
			builder.Add ((byte)0xAC); // 44100 Hz upper
			builder.Add ((byte)0x44);
			builder.Add ((byte)0xF0); // 44100 lower + 2 channels
			builder.Add ((byte)0x00); // 16 bits + 0 samples upper
			builder.AddZeros (4); // total samples lower

			// MD5 signature (16 bytes)
			if (md5 is { Length: 16 })
				builder.Add (md5);
			else
				builder.AddZeros (16);

			return builder.ToArray ();
		}

		/// <summary>
		/// Creates a FLAC file with a Vorbis Comment block.
		/// </summary>
		public static byte[] CreateWithVorbisComment (string? title = null, string? artist = null)
		{
			var builder = new BinaryDataBuilder ();

			// Magic
			builder.Add (TestConstants.Magic.Flac);

			// STREAMINFO block (not last)
			builder.Add (StreamInfoHeaderNotLast);
			AddDefaultStreamInfoData (builder);

			// Vorbis Comment block (last)
			var comment = new TagLibSharp2.Xiph.VorbisComment (TestConstants.Vendors.TagLibSharp2);
			if (!string.IsNullOrEmpty (title))
				comment.Title = title;
			if (!string.IsNullOrEmpty (artist))
				comment.Artist = artist;

			var commentData = comment.Render ().ToArray ();
			builder.Add ((byte)(TestConstants.Flac.LastBlockFlag | TestConstants.Flac.BlockTypeVorbisComment));
			builder.AddUInt24BE ((uint)commentData.Length);
			builder.Add (commentData);

			return builder.ToArray ();
		}

		/// <summary>
		/// Creates a FLAC file with a picture block.
		/// </summary>
		public static byte[] CreateWithPicture (PictureType pictureType = PictureType.FrontCover)
		{
			var builder = new BinaryDataBuilder ();

			// Magic
			builder.Add (TestConstants.Magic.Flac);

			// STREAMINFO (not last)
			builder.Add (new byte[] { 0x00, 0x00, 0x00, 0x22 });
			AddDefaultStreamInfoData (builder);

			// PICTURE block
			var jpegData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10 };
			var picture = new FlacPicture ("image/jpeg", pictureType, "", new BinaryData (jpegData), 100, 100, 24, 0);
			var pictureData = picture.RenderContent ();

			builder.Add ((byte)(TestConstants.Flac.LastBlockFlag | TestConstants.Flac.BlockTypePicture));
			builder.AddUInt24BE ((uint)pictureData.Length);
			builder.Add (pictureData);

			return builder.ToArray ();
		}

		/// <summary>
		/// Creates a FLAC file with multiple pictures.
		/// </summary>
		public static byte[] CreateWithMultiplePictures ()
		{
			var builder = new BinaryDataBuilder ();

			// Magic
			builder.Add (TestConstants.Magic.Flac);

			// STREAMINFO (not last)
			builder.Add (new byte[] { 0x00, 0x00, 0x00, 0x22 });
			AddDefaultStreamInfoData (builder);

			// Picture 1 (front cover, not last)
			var pic1Data = new FlacPicture ("image/jpeg", PictureType.FrontCover, "",
				new BinaryData (new byte[] { 0xFF, 0xD8 }), 100, 100, 24, 0).RenderContent ();
			builder.Add (TestConstants.Flac.BlockTypePicture);
			builder.AddUInt24BE ((uint)pic1Data.Length);
			builder.Add (pic1Data);

			// Picture 2 (back cover, last)
			var pic2Data = new FlacPicture ("image/jpeg", PictureType.BackCover, "",
				new BinaryData (new byte[] { 0xFF, 0xD9 }), 200, 200, 24, 0).RenderContent ();
			builder.Add ((byte)(TestConstants.Flac.LastBlockFlag | TestConstants.Flac.BlockTypePicture));
			builder.AddUInt24BE ((uint)pic2Data.Length);
			builder.Add (pic2Data);

			return builder.ToArray ();
		}

		/// <summary>
		/// Creates a FLAC file with audio data following the metadata.
		/// </summary>
		public static byte[] CreateWithAudioData (byte[] audioData)
		{
			var builder = new BinaryDataBuilder ();

			// Magic
			builder.Add (TestConstants.Magic.Flac);

			// STREAMINFO (last)
			builder.Add (new byte[] { 0x80, 0x00, 0x00, 0x22 });
			AddDefaultStreamInfoData (builder);

			// Audio data
			builder.Add (audioData);

			return builder.ToArray ();
		}

		/// <summary>
		/// Creates a FLAC file with a SEEKTABLE block.
		/// </summary>
		public static byte[] CreateWithSeekTable ()
		{
			var builder = new BinaryDataBuilder ();

			// Magic
			builder.Add (TestConstants.Magic.Flac);

			// STREAMINFO (not last)
			builder.Add (new byte[] { 0x00, 0x00, 0x00, 0x22 });
			AddDefaultStreamInfoData (builder);

			// SEEKTABLE block (last, type=3)
			const int seekTableSize = 36; // 2 seek points * 18 bytes each
			builder.Add ((byte)(TestConstants.Flac.LastBlockFlag | TestConstants.Flac.BlockTypeSeekTable));
			builder.AddUInt24BE (seekTableSize);

			// Seek point 1: sample 0 at offset 0, 4096 samples
			builder.AddZeros (8);
			builder.AddZeros (8);
			builder.Add (new byte[] { 0x10, 0x00 });

			// Seek point 2: sample 44100 at offset 12345
			builder.Add (new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xAC, 0x44 });
			builder.Add (new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x30, 0x39 });
			builder.Add (new byte[] { 0x10, 0x00 });

			return builder.ToArray ();
		}

		/// <summary>
		/// Creates a FLAC file with an APPLICATION block.
		/// </summary>
		public static byte[] CreateWithApplicationBlock (string appId = "TEST")
		{
			var builder = new BinaryDataBuilder ();

			// Magic
			builder.Add (TestConstants.Magic.Flac);

			// STREAMINFO (not last)
			builder.Add (new byte[] { 0x00, 0x00, 0x00, 0x22 });
			AddDefaultStreamInfoData (builder);

			// APPLICATION block (last, type=2)
			var appIdBytes = Encoding.ASCII.GetBytes (appId.PadRight (4)[..4]);
			var appContent = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 };

			builder.Add ((byte)(TestConstants.Flac.LastBlockFlag | TestConstants.Flac.BlockTypeApplication));
			builder.AddUInt24BE ((uint)(appIdBytes.Length + appContent.Length));
			builder.Add (appIdBytes);
			builder.Add (appContent);

			return builder.ToArray ();
		}

		/// <summary>
		/// Creates a FLAC file with both SEEKTABLE and APPLICATION blocks.
		/// </summary>
		public static byte[] CreateWithSeekTableAndApplication ()
		{
			var builder = new BinaryDataBuilder ();

			// Magic
			builder.Add (TestConstants.Magic.Flac);

			// STREAMINFO (not last)
			builder.Add (new byte[] { 0x00, 0x00, 0x00, 0x22 });
			AddDefaultStreamInfoData (builder);

			// SEEKTABLE block (not last)
			const int seekTableSize = 18; // 1 seek point
			builder.Add (TestConstants.Flac.BlockTypeSeekTable);
			builder.AddUInt24BE (seekTableSize);
			builder.AddZeros (18);

			// APPLICATION block (last)
			var appId = Encoding.ASCII.GetBytes ("APPL");
			var appContent = new byte[] { 0xAA, 0xBB };
			builder.Add ((byte)(TestConstants.Flac.LastBlockFlag | TestConstants.Flac.BlockTypeApplication));
			builder.AddUInt24BE ((uint)(appId.Length + appContent.Length));
			builder.Add (appId);
			builder.Add (appContent);

			return builder.ToArray ();
		}

		/// <summary>
		/// Creates a FLAC file with an invalid STREAMINFO size for error testing.
		/// </summary>
		public static byte[] CreateWithInvalidStreamInfoSize (int size)
		{
			var builder = new BinaryDataBuilder ();

			// Magic
			builder.Add (TestConstants.Magic.Flac);

			// STREAMINFO block header: last=true, type=0, with custom size
			builder.Add ((byte)0x80);
			builder.Add ((byte)((size >> 16) & 0xFF));
			builder.Add ((byte)((size >> 8) & 0xFF));
			builder.Add ((byte)(size & 0xFF));

			// Add data with requested size
			builder.AddZeros (size);

			return builder.ToArray ();
		}

		/// <summary>
		/// Adds default STREAMINFO data (34 bytes) for 44100Hz/2ch/16-bit.
		/// </summary>
		static void AddDefaultStreamInfoData (BinaryDataBuilder builder)
		{
			// min/max block size: 4096
			builder.Add (DefaultBlockSizes);
			// min/max frame size: 0
			builder.Add (ZeroFrameSizes);
			// 44100Hz, 2ch, 16-bit, 0 samples
			builder.Add (Default44100StreamInfo);
			// MD5 signature
			builder.AddZeros (16);
		}
	}

	/// <summary>
	/// Builders for Ogg Vorbis file test data.
	/// </summary>
	public static class Ogg
	{
		/// <summary>
		/// Creates a minimal Ogg page with the given data.
		/// </summary>
		/// <param name="data">Page data (will be segmented automatically).</param>
		/// <param name="sequenceNumber">Page sequence number.</param>
		/// <param name="flags">Page flags (BOS, EOS, continuation).</param>
		/// <param name="serialNumber">Stream serial number.</param>
		/// <param name="calculateCrc">If true, calculate proper CRC; if false, use zeroes (for simpler testing).</param>
		/// <returns>Complete Ogg page bytes.</returns>
		public static byte[] CreatePage (
			byte[] data,
			uint sequenceNumber,
			OggPageFlags flags,
			uint serialNumber = 1,
			bool calculateCrc = true)
		{
			var builder = new BinaryDataBuilder ();

			// Magic
			builder.Add (TestConstants.Magic.Ogg);

			// Version
			builder.Add ((byte)0);

			// Flags
			builder.Add ((byte)flags);

			// Granule position (8 bytes)
			builder.AddUInt64LE (0);

			// Serial number
			builder.AddUInt32LE (serialNumber);

			// Sequence number
			builder.AddUInt32LE (sequenceNumber);

			// CRC placeholder
			builder.AddUInt32LE (0);

			// Segment count and table
			var segments = CalculateSegments (data.Length);
			builder.Add ((byte)segments.Length);
			builder.Add (segments);

			// Page data
			builder.Add (data);

			var page = builder.ToArray ();

			// Calculate and insert CRC if requested
			if (calculateCrc) {
				var crc = OggCrc.Calculate (page);
				page[22] = (byte)(crc & 0xFF);
				page[23] = (byte)((crc >> 8) & 0xFF);
				page[24] = (byte)((crc >> 16) & 0xFF);
				page[25] = (byte)((crc >> 24) & 0xFF);
			}

			return page;
		}

		/// <summary>
		/// Creates a Vorbis identification packet.
		/// </summary>
		public static byte[] CreateIdentificationPacket (
			int sampleRate = TestConstants.AudioProperties.SampleRate44100,
			int channels = TestConstants.AudioProperties.ChannelsStereo,
			int bitrateNominal = 128000)
		{
			var builder = new BinaryDataBuilder ();

			// Packet type + "vorbis"
			builder.Add (TestConstants.Ogg.PacketTypeIdentification);
			builder.Add (TestConstants.Magic.Vorbis);

			// Version (0)
			builder.AddUInt32LE (0);

			// Channels
			builder.Add ((byte)channels);

			// Sample rate
			builder.AddUInt32LE ((uint)sampleRate);

			// Bitrate (max, nominal, min)
			builder.AddUInt32LE (0); // max
			builder.AddUInt32LE ((uint)bitrateNominal);
			builder.AddUInt32LE (0); // min

			// Block sizes (4 bits each)
			builder.Add ((byte)0xB8); // blocksize_0 = 8, blocksize_1 = 11

			// Framing bit
			builder.Add ((byte)0x01);

			return builder.ToArray ();
		}

		/// <summary>
		/// Creates a Vorbis comment packet with framing bit.
		/// </summary>
		public static byte[] CreateCommentPacket (TagLibSharp2.Xiph.VorbisComment comment, bool validFramingBit = true)
		{
			var commentData = comment.Render ().ToArray ();
			var builder = new BinaryDataBuilder ();

			// Packet type + "vorbis"
			builder.Add (TestConstants.Ogg.PacketTypeComment);
			builder.Add (TestConstants.Magic.Vorbis);

			// Comment data
			builder.Add (commentData);

			// Framing bit (1 = valid, 0 = invalid)
			builder.Add ((byte)(validFramingBit ? 0x01 : 0x00));

			return builder.ToArray ();
		}

		/// <summary>
		/// Creates a minimal Vorbis setup packet.
		/// </summary>
		public static byte[] CreateSetupPacket ()
		{
			var builder = new BinaryDataBuilder ();
			builder.Add (TestConstants.Ogg.PacketTypeSetup);
			builder.Add (TestConstants.Magic.Vorbis);
			// Minimal setup data placeholder
			builder.Add (new byte[10]);
			return builder.ToArray ();
		}

		/// <summary>
		/// Creates a minimal valid Ogg Vorbis file with metadata.
		/// </summary>
		/// <param name="title">Title metadata (null or empty to skip).</param>
		/// <param name="artist">Artist metadata (null or empty to skip).</param>
		/// <param name="calculateCrc">If true, pages have valid CRCs.</param>
		public static byte[] CreateMinimalFile (string title = "Test", string artist = "Artist", bool calculateCrc = true)
		{
			var comment = new TagLibSharp2.Xiph.VorbisComment (TestConstants.Vendors.TagLibSharp2);
			if (!string.IsNullOrEmpty (title))
				comment.Title = title;
			if (!string.IsNullOrEmpty (artist))
				comment.Artist = artist;

			return CreateFile (comment, calculateCrc: calculateCrc);
		}

		/// <summary>
		/// Creates an Ogg Vorbis file with custom audio properties.
		/// </summary>
		public static byte[] CreateFileWithProperties (
			int sampleRate = TestConstants.AudioProperties.SampleRate44100,
			int channels = TestConstants.AudioProperties.ChannelsStereo,
			int bitrateNominal = 128000,
			bool calculateCrc = true)
		{
			var comment = new TagLibSharp2.Xiph.VorbisComment (TestConstants.Vendors.TagLibSharp2);
			return CreateFile (comment, sampleRate, channels, bitrateNominal, calculateCrc: calculateCrc);
		}

		/// <summary>
		/// Creates an Ogg Vorbis file with an invalid framing bit for error testing.
		/// </summary>
		public static byte[] CreateFileWithInvalidFramingBit (bool calculateCrc = false)
		{
			var comment = new TagLibSharp2.Xiph.VorbisComment (TestConstants.Vendors.TagLibSharp2);
			return CreateFile (comment, validFramingBit: false, calculateCrc: calculateCrc);
		}

		/// <summary>
		/// Creates an Ogg Vorbis file with a multi-page comment spanning 3+ pages.
		/// </summary>
		public static byte[] CreateFileWithMultiPageComment (bool calculateCrc = false)
		{
			var comment = new TagLibSharp2.Xiph.VorbisComment (TestConstants.Vendors.TagLibSharp2);
			comment.Title = "Multi-Page Title";
			// Create a 70KB+ field to span multiple pages
			comment.SetValue ("LONGFIELD", new string ('X', 70000));

			return CreateFile (comment, splitCommentAcrossPages: true, calculateCrc: calculateCrc);
		}

		/// <summary>
		/// Creates an Ogg page with non-Vorbis data for error testing.
		/// </summary>
		public static byte[] CreatePageWithNonVorbisData (bool calculateCrc = false)
		{
			var data = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
			return CreatePage (data, 0, OggPageFlags.BeginOfStream, calculateCrc: calculateCrc);
		}

		/// <summary>
		/// Creates a complete Ogg Vorbis file with full control over parameters.
		/// </summary>
		public static byte[] CreateFile (
			TagLibSharp2.Xiph.VorbisComment comment,
			int sampleRate = TestConstants.AudioProperties.SampleRate44100,
			int channels = TestConstants.AudioProperties.ChannelsStereo,
			int bitrateNominal = 128000,
			bool validFramingBit = true,
			bool splitCommentAcrossPages = false,
			bool calculateCrc = true)
		{
			var builder = new BinaryDataBuilder ();

			var idPacket = CreateIdentificationPacket (sampleRate, channels, bitrateNominal);
			var commentPacket = CreateCommentPacket (comment, validFramingBit);
			var setupPacket = CreateSetupPacket ();

			// Page 1: BOS with identification header
			builder.Add (CreatePage (idPacket, 0, OggPageFlags.BeginOfStream, calculateCrc: calculateCrc));

			if (splitCommentAcrossPages) {
				// Split comment packet across multiple pages using multi-page aware builder
				var sequenceNum = 1u;
				var pages = CreateMultiPagePacket (commentPacket, sequenceNum, calculateCrc);
				foreach (var page in pages)
					builder.Add (page);
				sequenceNum += (uint)pages.Length;

				// Final page: Setup + EOS
				builder.Add (CreatePage (setupPacket, sequenceNum, OggPageFlags.EndOfStream, calculateCrc: calculateCrc));
			} else {
				// Page 2: Comment header
				builder.Add (CreatePage (commentPacket, 1, OggPageFlags.None, calculateCrc: calculateCrc));

				// Page 3: Setup + EOS
				builder.Add (CreatePage (setupPacket, 2, OggPageFlags.EndOfStream, calculateCrc: calculateCrc));
			}

			return builder.ToArray ();
		}

		/// <summary>
		/// Creates multiple Ogg pages for a packet that spans pages.
		/// Handles segment table correctly: continuation pages have all 255 segments.
		/// </summary>
		static byte[][] CreateMultiPagePacket (byte[] packet, uint startSequence, bool calculateCrc)
		{
			var pages = new List<byte[]> ();
			var offset = 0;
			var sequenceNum = startSequence;

			// Max segments per page: 255, max bytes per segment: 255
			// But we use a more reasonable chunk size for multi-page spanning
			const int maxSegmentsPerPage = 255;
			const int bytesPerSegment = 255;
			const int maxBytesPerPage = maxSegmentsPerPage * bytesPerSegment; // 65025

			while (offset < packet.Length) {
				var remaining = packet.Length - offset;
				var isFirstPage = offset == 0;
				var flags = isFirstPage ? OggPageFlags.None : OggPageFlags.Continuation;

				// For continuation: take up to maxBytesPerPage
				// Make sure we fill exactly maxBytesPerPage for non-final pages
				int pageDataLen;
				bool isFinalPage;
				if (remaining > maxBytesPerPage) {
					pageDataLen = maxBytesPerPage;
					isFinalPage = false;
				} else {
					pageDataLen = remaining;
					isFinalPage = true;
				}

				var pageData = new byte[pageDataLen];
				Array.Copy (packet, offset, pageData, 0, pageDataLen);

				// Create page with proper segment table
				var page = CreatePageWithSegmentTable (pageData, sequenceNum, flags, isFinalPage, calculateCrc);
				pages.Add (page);

				offset += pageDataLen;
				sequenceNum++;
			}

			return pages.ToArray ();
		}

		/// <summary>
		/// Creates an Ogg page with proper segment table for multi-page spanning.
		/// </summary>
		static byte[] CreatePageWithSegmentTable (byte[] data, uint sequenceNumber, OggPageFlags flags, bool isPacketEnd, bool calculateCrc)
		{
			var builder = new BinaryDataBuilder ();

			// Magic
			builder.Add (TestConstants.Magic.Ogg);

			// Version
			builder.Add ((byte)0);

			// Flags
			builder.Add ((byte)flags);

			// Granule position (8 bytes)
			builder.AddUInt64LE (0);

			// Serial number
			builder.AddUInt32LE (1);

			// Sequence number
			builder.AddUInt32LE (sequenceNumber);

			// CRC placeholder
			builder.AddUInt32LE (0);

			// Build segment table
			// For multi-page spanning: use 255-byte segments
			// If not end of packet: all segments must be 255 (indicating continuation)
			// If end of packet: last segment can be < 255
			var segments = new List<byte> ();
			var remaining = data.Length;
			while (remaining > 0) {
				if (remaining >= 255) {
					segments.Add (255);
					remaining -= 255;
				} else {
					segments.Add ((byte)remaining);
					remaining = 0;
				}
			}
			// If this is end of packet and data length was exactly divisible by 255,
			// we need to add a 0-length segment to indicate packet end
			if (isPacketEnd && data.Length > 0 && data.Length % 255 == 0)
				segments.Add (0);

			// If NOT end of packet, ensure last segment is 255 to indicate continuation
			if (!isPacketEnd && segments.Count > 0 && segments[^1] != 255) {
				// This shouldn't happen if we take maxBytesPerPage, but safety check
				// Pad with zeros if needed (shouldn't be needed with our chunking)
			}

			builder.Add ((byte)segments.Count);
			foreach (var seg in segments)
				builder.Add (seg);

			// Data
			builder.Add (data);

			var page = builder.ToArray ();

			// Calculate and insert CRC if requested
			if (calculateCrc) {
				var crc = OggCrc.Calculate (page);
				page[22] = (byte)(crc & 0xFF);
				page[23] = (byte)((crc >> 8) & 0xFF);
				page[24] = (byte)((crc >> 16) & 0xFF);
				page[25] = (byte)((crc >> 24) & 0xFF);
			}

			return page;
		}

		/// <summary>
		/// Calculates segment table for Ogg page.
		/// </summary>
		static byte[] CalculateSegments (int dataLength)
		{
			var count = (dataLength / 255) + 1;
			var segments = new byte[count];
			for (int i = 0; i < count - 1; i++)
				segments[i] = 255;
			segments[count - 1] = (byte)(dataLength % 255);
			return segments;
		}
	}

	/// <summary>
	/// Builders for Vorbis Comment test data.
	/// </summary>
	public static class VorbisComment
	{
		/// <summary>
		/// Creates a minimal valid Vorbis Comment.
		/// </summary>
		public static byte[] CreateMinimal (string vendor = "")
		{
			var vendorBytes = Encoding.UTF8.GetBytes (vendor);
			var builder = new BinaryDataBuilder ();

			// Vendor length + string
			builder.AddUInt32LE ((uint)vendorBytes.Length);
			builder.Add (vendorBytes);

			// Field count = 0
			builder.AddUInt32LE (0);

			return builder.ToArray ();
		}

		/// <summary>
		/// Creates a Vorbis Comment with the given fields.
		/// </summary>
		public static byte[] CreateWithFields (string vendor, params (string key, string value)[] fields)
		{
			var vendorBytes = Encoding.UTF8.GetBytes (vendor);
			var builder = new BinaryDataBuilder ();

			// Vendor length + string
			builder.AddUInt32LE ((uint)vendorBytes.Length);
			builder.Add (vendorBytes);

			// Field count
			builder.AddUInt32LE ((uint)fields.Length);

			// Fields
			foreach (var (key, value) in fields) {
				var field = $"{key}={value}";
				var fieldBytes = Encoding.UTF8.GetBytes (field);
				builder.AddUInt32LE ((uint)fieldBytes.Length);
				builder.Add (fieldBytes);
			}

			return builder.ToArray ();
		}
	}

	/// <summary>
	/// Builders for RIFF/WAV file test data.
	/// </summary>
	public static class Wav
	{
		/// <summary>
		/// Creates a minimal valid WAV file header.
		/// </summary>
		public static byte[] CreateMinimal (
			int sampleRate = TestConstants.AudioProperties.SampleRate44100,
			int channels = TestConstants.AudioProperties.ChannelsStereo,
			int bitsPerSample = TestConstants.AudioProperties.BitsPerSample16)
		{
			var builder = new BinaryDataBuilder ();

			// RIFF header
			builder.Add (TestConstants.Magic.Riff);
			builder.AddUInt32LE (36); // File size - 8 (placeholder)
			builder.Add (TestConstants.Magic.Wave);

			// fmt chunk
			builder.AddStringLatin1 (TestConstants.Riff.FormatChunkId);
			builder.AddUInt32LE (16); // Chunk size
			builder.AddUInt16LE (TestConstants.Riff.FormatPcm);
			builder.AddUInt16LE ((ushort)channels);
			builder.AddUInt32LE ((uint)sampleRate);
			builder.AddUInt32LE ((uint)(sampleRate * channels * bitsPerSample / 8)); // Byte rate
			builder.AddUInt16LE ((ushort)(channels * bitsPerSample / 8)); // Block align
			builder.AddUInt16LE ((ushort)bitsPerSample);

			// data chunk (empty)
			builder.AddStringLatin1 (TestConstants.Riff.DataChunkId);
			builder.AddUInt32LE (0);

			return builder.ToArray ();
		}
	}

	/// <summary>
	/// Builders for AIFF file test data.
	/// </summary>
	public static class Aiff
	{
		/// <summary>
		/// Creates a minimal valid AIFF file header.
		/// </summary>
		public static byte[] CreateMinimal (
			int sampleRate = TestConstants.AudioProperties.SampleRate44100,
			int channels = TestConstants.AudioProperties.ChannelsStereo,
			int bitsPerSample = TestConstants.AudioProperties.BitsPerSample16)
		{
			var builder = new BinaryDataBuilder ();

			// FORM header
			builder.Add (TestConstants.Magic.Form);
			builder.AddUInt32BE (30); // File size - 8 (placeholder)
			builder.Add (TestConstants.Magic.Aiff);

			// COMM chunk
			builder.AddStringLatin1 (TestConstants.Aiff.CommonChunkId);
			builder.AddUInt32BE (TestConstants.Aiff.CommChunkSize);
			builder.AddUInt16BE ((ushort)channels);
			builder.AddUInt32BE (0); // Sample frames
			builder.AddUInt16BE ((ushort)bitsPerSample);
			builder.Add (ConvertToExtended (sampleRate));

			// SSND chunk (empty)
			builder.AddStringLatin1 (TestConstants.Aiff.SoundDataChunkId);
			builder.AddUInt32BE (8);
			builder.AddUInt32BE (0); // Offset
			builder.AddUInt32BE (0); // Block size

			return builder.ToArray ();
		}

		/// <summary>
		/// Converts a sample rate to 80-bit extended precision format.
		/// </summary>
		static byte[] ConvertToExtended (int sampleRate)
		{
			// Simplified conversion for common sample rates
			var result = new byte[10];
			if (sampleRate == 44100) {
				result[0] = 0x40; result[1] = 0x0E;
				result[2] = 0xAC; result[3] = 0x44;
			} else if (sampleRate == 48000) {
				result[0] = 0x40; result[1] = 0x0E;
				result[2] = 0xBB; result[3] = 0x80;
			}
			return result;
		}
	}
}
