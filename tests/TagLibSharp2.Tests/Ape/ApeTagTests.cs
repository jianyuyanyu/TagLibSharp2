// APE Tag v2 Tests - TDD approach
// Experts consulted: SDET, C# expert, APE Tag expert, QA Manager
//
// Test strategy:
// 1. Unit tests for parsing individual components (header, footer, items)
// 2. Integration tests for full tag parsing
// 3. Round-trip tests for read → modify → write → read
// 4. Edge cases: empty tags, binary items, large items, malformed data

using Microsoft.VisualStudio.TestTools.UnitTesting;
using TagLibSharp2.Ape;
using TagLibSharp2.Core;

namespace TagLibSharp2.Tests.Ape;

[TestClass]
[TestCategory("Unit")]
[TestCategory("Ape")]
public class ApeTagTests
{
    // APE Tag magic bytes: "APETAGEX"
    private static readonly byte[] ApeMagic = "APETAGEX"u8.ToArray();

    #region Header/Footer Structure Tests

    [TestMethod]
    public void ParseFooter_ValidFooter_ReturnsSuccess()
    {
        // Arrange - minimal valid APEv2 footer (32 bytes)
        var footer = CreateValidFooter(version: 2000, tagSize: 32, itemCount: 0, flags: 0);

        // Act
        var result = ApeTagFooter.Parse(footer);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(2000u, result.Footer!.Version);
        Assert.AreEqual(32u, result.Footer.TagSize);
        Assert.AreEqual(0u, result.Footer.ItemCount);
    }

    [TestMethod]
    public void ParseFooter_InvalidMagic_ReturnsFailure()
    {
        // Arrange - invalid magic bytes
        var footer = new byte[32];
        "NOTAPETX"u8.CopyTo(footer);

        // Act
        var result = ApeTagFooter.Parse(footer);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Error!.Contains("magic"));
    }

    [TestMethod]
    public void ParseFooter_TooShort_ReturnsFailure()
    {
        // Arrange - data too short for footer
        var footer = new byte[20];

        // Act
        var result = ApeTagFooter.Parse(footer);

        // Assert
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public void ParseFooter_Version1000_ParsesAsV1()
    {
        // Arrange - APEv1 footer
        var footer = CreateValidFooter(version: 1000, tagSize: 32, itemCount: 0, flags: 0);

        // Act
        var result = ApeTagFooter.Parse(footer);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1000u, result.Footer!.Version);
    }

    [TestMethod]
    public void ParseHeader_ValidHeader_ReturnsSuccess()
    {
        // Arrange - valid APEv2 header with header flag set (bit 31)
        var flags = 0x80000000u; // Header present flag
        flags |= 0x20000000u; // This is a header (bit 29)
        var header = CreateValidFooter(version: 2000, tagSize: 64, itemCount: 1, flags: flags);

        // Act
        var result = ApeTagHeader.Parse(header);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Header!.IsHeader);
        Assert.IsTrue(result.Header.HasHeader);
    }

    [TestMethod]
    public void Footer_FlagsDecoding_CorrectlyIdentifiesTextType()
    {
        // Arrange - footer with text type flag (bits 2-1 = 0)
        var footer = CreateValidFooter(version: 2000, tagSize: 32, itemCount: 0, flags: 0);

        // Act
        var result = ApeTagFooter.Parse(footer);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsFalse(result.Footer!.IsReadOnly);
    }

    [TestMethod]
    public void Footer_FlagsDecoding_CorrectlyIdentifiesReadOnly()
    {
        // Arrange - footer with read-only flag (bit 0 = 1)
        var footer = CreateValidFooter(version: 2000, tagSize: 32, itemCount: 0, flags: 1);

        // Act
        var result = ApeTagFooter.Parse(footer);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.IsTrue(result.Footer!.IsReadOnly);
    }

    #endregion

    #region Item Parsing Tests

    [TestMethod]
    public void ParseItem_ValidTextItem_ReturnsSuccess()
    {
        // Arrange - text item: "Title" = "Test Song"
        var item = CreateTextItem("Title", "Test Song");

        // Act
        var result = ApeTagItem.Parse(item);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Title", result.Item!.Key);
        Assert.AreEqual("Test Song", result.Item.ValueAsString);
        Assert.AreEqual(ApeItemType.Text, result.Item.ItemType);
    }

    [TestMethod]
    public void ParseItem_ValidBinaryItem_ReturnsSuccess()
    {
        // Arrange - binary item with cover art pattern
        var imageData = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // JPEG magic
        var item = CreateBinaryItem("Cover Art (Front)", "cover.jpg", imageData);

        // Act
        var result = ApeTagItem.Parse(item);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Cover Art (Front)", result.Item!.Key);
        Assert.AreEqual(ApeItemType.Binary, result.Item.ItemType);
        Assert.IsNotNull(result.Item.BinaryValue);
    }

    [TestMethod]
    public void ParseItem_EmptyKey_ReturnsFailure()
    {
        // Arrange - item with empty key (invalid per spec: 2-255 chars)
        var item = CreateTextItem("", "value");

        // Act
        var result = ApeTagItem.Parse(item);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Error!.Contains("key"));
    }

    [TestMethod]
    public void ParseItem_KeyTooLong_ReturnsFailure()
    {
        // Arrange - key > 255 characters
        var longKey = new string('A', 256);
        var item = CreateTextItem(longKey, "value");

        // Act
        var result = ApeTagItem.Parse(item);

        // Assert
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public void ParseItem_ReservedKey_ReturnsFailure()
    {
        // Arrange - reserved key "ID3"
        var item = CreateTextItem("ID3", "value");

        // Act
        var result = ApeTagItem.Parse(item);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Error!.Contains("reserved"));
    }

    [TestMethod]
    public void ParseItem_KeyWithControlCharacter_ReturnsFailure()
    {
        // Arrange - key with control character (< 0x20)
        var item = CreateTextItem("Key\x01Name", "value");

        // Act
        var result = ApeTagItem.Parse(item);

        // Assert
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public void ParseItem_ExternalLocator_ParsesCorrectly()
    {
        // Arrange - external locator type (flags bits 2-1 = 2)
        var item = CreateExternalLocatorItem("Lyrics", "http://example.com/lyrics.txt");

        // Act
        var result = ApeTagItem.Parse(item);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(ApeItemType.ExternalLocator, result.Item!.ItemType);
        Assert.AreEqual("http://example.com/lyrics.txt", result.Item.ValueAsString);
    }

    #endregion

    #region Full Tag Parsing Tests

    [TestMethod]
    public void Parse_EmptyTag_ReturnsEmptyTag()
    {
        // Arrange - tag with 0 items
        var tag = CreateCompleteTag(new Dictionary<string, string>());

        // Act
        var result = ApeTag.Parse(tag);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(0, result.Tag!.ItemCount);
    }

    [TestMethod]
    public void Parse_SingleItem_ReturnsTagWithItem()
    {
        // Arrange
        var tag = CreateCompleteTag(new Dictionary<string, string>
        {
            ["Title"] = "Test Song"
        });

        // Act
        var result = ApeTag.Parse(tag);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1, result.Tag!.ItemCount);
        Assert.AreEqual("Test Song", result.Tag.Title);
    }

    [TestMethod]
    public void Parse_MultipleItems_ReturnsAllItems()
    {
        // Arrange
        var tag = CreateCompleteTag(new Dictionary<string, string>
        {
            ["Title"] = "Test Song",
            ["Artist"] = "Test Artist",
            ["Album"] = "Test Album",
            ["Year"] = "2025",
            ["Track"] = "1/10"
        });

        // Act
        var result = ApeTag.Parse(tag);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(5, result.Tag!.ItemCount);
        Assert.AreEqual("Test Song", result.Tag.Title);
        Assert.AreEqual("Test Artist", result.Tag.Artist);
        Assert.AreEqual("Test Album", result.Tag.Album);
        Assert.AreEqual("2025", result.Tag.Year);
        Assert.AreEqual(1u, result.Tag.Track);
    }

    [TestMethod]
    public void Parse_CaseInsensitiveKeyLookup_FindsItem()
    {
        // Arrange - keys stored as "Title" but queried as "title"
        var tag = CreateCompleteTag(new Dictionary<string, string>
        {
            ["Title"] = "Test Song"
        });

        // Act
        var result = ApeTag.Parse(tag);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Test Song", result.Tag!.GetValue("title")); // lowercase query
        Assert.AreEqual("Test Song", result.Tag!.GetValue("TITLE")); // uppercase query
    }

    [TestMethod]
    public void Parse_TrackWithTotal_ParsesBothParts()
    {
        // Arrange - track format "3/12"
        var tag = CreateCompleteTag(new Dictionary<string, string>
        {
            ["Track"] = "3/12"
        });

        // Act
        var result = ApeTag.Parse(tag);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(3u, result.Tag!.Track);
        Assert.AreEqual(12u, result.Tag.TotalTracks);
    }

    [TestMethod]
    public void Parse_DiscWithTotal_ParsesBothParts()
    {
        // Arrange - disc format "1/2"
        var tag = CreateCompleteTag(new Dictionary<string, string>
        {
            ["Disc"] = "1/2"
        });

        // Act
        var result = ApeTag.Parse(tag);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual(1u, result.Tag!.Disc);
        Assert.AreEqual(2u, result.Tag.TotalDiscs);
    }

    #endregion

    #region Tag Rendering Tests

    [TestMethod]
    public void Render_EmptyTag_ProducesValidFooter()
    {
        // Arrange
        var tag = new ApeTag();

        // Act
        var rendered = tag.Render();

        // Assert
        Assert.IsNotNull(rendered);
        Assert.IsTrue(rendered.Length >= 32); // At least footer size

        // Verify we can parse it back
        var parseResult = ApeTag.Parse(rendered.Span);
        Assert.IsTrue(parseResult.IsSuccess);
    }

    [TestMethod]
    public void Render_WithItems_ProducesValidTag()
    {
        // Arrange
        var tag = new ApeTag
        {
            Title = "Test Song",
            Artist = "Test Artist"
        };

        // Act
        var rendered = tag.Render();

        // Assert
        Assert.IsNotNull(rendered);

        // Round-trip validation
        var parseResult = ApeTag.Parse(rendered.Span);
        Assert.IsTrue(parseResult.IsSuccess);
        Assert.AreEqual("Test Song", parseResult.Tag!.Title);
        Assert.AreEqual("Test Artist", parseResult.Tag.Artist);
    }

    [TestMethod]
    public void Render_WithBinaryItem_PreservesBinaryData()
    {
        // Arrange
        var imageData = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; // PNG magic
        var tag = new ApeTag();
        tag.SetBinaryItem("Cover Art (Front)", "cover.png", imageData);

        // Act
        var rendered = tag.Render();

        // Assert - round-trip
        var parseResult = ApeTag.Parse(rendered.Span);
        Assert.IsTrue(parseResult.IsSuccess);
        var coverArt = parseResult.Tag!.GetBinaryItem("Cover Art (Front)");
        Assert.IsNotNull(coverArt);
        CollectionAssert.AreEqual(imageData, coverArt.Data);
    }

    [TestMethod]
    public void Render_IncludesHeader_WhenRequested()
    {
        // Arrange
        var tag = new ApeTag { Title = "Test" };

        // Act
        var rendered = tag.RenderWithOptions(includeHeader: true);

        // Assert
        Assert.IsTrue(rendered.Length >= 64); // Header (32) + items + Footer (32)

        // Verify header magic at start
        Assert.AreEqual((byte)'A', rendered[0]);
        Assert.AreEqual((byte)'P', rendered[1]);
        Assert.AreEqual((byte)'E', rendered[2]);
        Assert.AreEqual((byte)'T', rendered[3]);
    }

    #endregion

    #region Round-Trip Tests (Critical for data integrity)

    [TestMethod]
    public void RoundTrip_AllStandardFields_PreservesData()
    {
        // Arrange - comprehensive tag with all standard fields
        var original = new ApeTag
        {
            Title = "Symphony No. 5",
            Artist = "Ludwig van Beethoven",
            Album = "Complete Symphonies",
            Year = "1808",
            Genre = "Classical",
            Comment = "First movement",
            Track = 5,
            TotalTracks = 9,
            Disc = 1,
            TotalDiscs = 3
        };

        // Act - render and parse
        var rendered = original.Render();
        var parseResult = ApeTag.Parse(rendered.Span);

        // Assert
        Assert.IsTrue(parseResult.IsSuccess);
        var parsed = parseResult.Tag!;
        Assert.AreEqual(original.Title, parsed.Title);
        Assert.AreEqual(original.Artist, parsed.Artist);
        Assert.AreEqual(original.Album, parsed.Album);
        Assert.AreEqual(original.Year, parsed.Year);
        Assert.AreEqual(original.Genre, parsed.Genre);
        Assert.AreEqual(original.Comment, parsed.Comment);
        Assert.AreEqual(original.Track, parsed.Track);
        Assert.AreEqual(original.TotalTracks, parsed.TotalTracks);
        Assert.AreEqual(original.Disc, parsed.Disc);
        Assert.AreEqual(original.TotalDiscs, parsed.TotalDiscs);
    }

    [TestMethod]
    public void RoundTrip_UnicodeCharacters_PreservesData()
    {
        // Arrange - Unicode in multiple scripts
        var original = new ApeTag
        {
            Title = "日本語タイトル",
            Artist = "Артист",
            Album = "مقطوعة موسيقية"
        };

        // Act
        var rendered = original.Render();
        var parseResult = ApeTag.Parse(rendered.Span);

        // Assert
        Assert.IsTrue(parseResult.IsSuccess);
        Assert.AreEqual("日本語タイトル", parseResult.Tag!.Title);
        Assert.AreEqual("Артист", parseResult.Tag.Artist);
        Assert.AreEqual("مقطوعة موسيقية", parseResult.Tag.Album);
    }

    [TestMethod]
    public void RoundTrip_ExtendedMetadata_PreservesData()
    {
        // Arrange - MusicBrainz and ReplayGain
        var original = new ApeTag();
        original.SetValue("MUSICBRAINZ_TRACKID", "12345678-1234-1234-1234-123456789abc");
        original.SetValue("REPLAYGAIN_TRACK_GAIN", "-6.5 dB");
        original.SetValue("REPLAYGAIN_TRACK_PEAK", "0.987654");

        // Act
        var rendered = original.Render();
        var parseResult = ApeTag.Parse(rendered.Span);

        // Assert
        Assert.IsTrue(parseResult.IsSuccess);
        Assert.AreEqual("12345678-1234-1234-1234-123456789abc",
            parseResult.Tag!.GetValue("MUSICBRAINZ_TRACKID"));
        Assert.AreEqual("-6.5 dB", parseResult.Tag.GetValue("REPLAYGAIN_TRACK_GAIN"));
    }

    #endregion

    #region Edge Cases and Security Tests

    [TestMethod]
    public void Parse_TruncatedData_ReturnsFailure()
    {
        // Arrange - data claims 100 items but only has footer
        var footer = CreateValidFooter(version: 2000, tagSize: 1000, itemCount: 100, flags: 0);

        // Act
        var result = ApeTag.Parse(footer);

        // Assert
        Assert.IsFalse(result.IsSuccess);
    }

    [TestMethod]
    public void Parse_OverflowTagSize_ReturnsFailure()
    {
        // Arrange - tag size larger than int.MaxValue
        var footer = CreateValidFooter(version: 2000, tagSize: uint.MaxValue, itemCount: 1, flags: 0);

        // Act
        var result = ApeTag.Parse(footer);

        // Assert
        Assert.IsFalse(result.IsSuccess);
        Assert.IsTrue(result.Error!.Contains("size") || result.Error.Contains("overflow"));
    }

    [TestMethod]
    public void Parse_ZeroByteValue_HandlesCorrectly()
    {
        // Arrange - item with empty value (0 bytes)
        var tag = CreateCompleteTag(new Dictionary<string, string>
        {
            ["EmptyField"] = ""
        });

        // Act
        var result = ApeTag.Parse(tag);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("", result.Tag!.GetValue("EmptyField"));
    }

    [TestMethod]
    public void Parse_MaxKeyLength_Succeeds()
    {
        // Arrange - key at max length (255 chars)
        var maxKey = new string('K', 255);
        var tag = CreateCompleteTag(new Dictionary<string, string>
        {
            [maxKey] = "value"
        });

        // Act
        var result = ApeTag.Parse(tag);

        // Assert
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("value", result.Tag!.GetValue(maxKey));
    }

    #endregion

    #region TagTypes Integration Tests

    [TestMethod]
    public void TagType_ReturnsApe()
    {
        // Arrange
        var tag = new ApeTag();

        // Act & Assert
        Assert.AreEqual(TagTypes.Ape, tag.TagType);
    }

    #endregion

    #region Test Helpers

    private static byte[] CreateValidFooter(uint version, uint tagSize, uint itemCount, uint flags)
    {
        var footer = new byte[32];

        // Preamble: "APETAGEX"
        ApeMagic.CopyTo(footer, 0);

        // Version (little-endian)
        BitConverter.GetBytes(version).CopyTo(footer, 8);

        // Tag size (little-endian)
        BitConverter.GetBytes(tagSize).CopyTo(footer, 12);

        // Item count (little-endian)
        BitConverter.GetBytes(itemCount).CopyTo(footer, 16);

        // Flags (little-endian)
        BitConverter.GetBytes(flags).CopyTo(footer, 20);

        // Reserved (8 bytes of zero) - already zero from new byte[32]

        return footer;
    }

    private static byte[] CreateTextItem(string key, string value)
    {
        var keyBytes = System.Text.Encoding.ASCII.GetBytes(key);
        var valueBytes = System.Text.Encoding.UTF8.GetBytes(value);

        // Item structure: valueSize(4) + flags(4) + key + null + value
        var item = new byte[8 + keyBytes.Length + 1 + valueBytes.Length];

        // Value size (little-endian)
        BitConverter.GetBytes((uint)valueBytes.Length).CopyTo(item, 0);

        // Flags = 0 (text type, read-write)
        // Already zero from new byte[]

        // Key
        keyBytes.CopyTo(item, 8);

        // Null terminator (already zero)

        // Value
        valueBytes.CopyTo(item, 8 + keyBytes.Length + 1);

        return item;
    }

    private static byte[] CreateBinaryItem(string key, string filename, byte[] data)
    {
        var keyBytes = System.Text.Encoding.ASCII.GetBytes(key);
        var filenameBytes = System.Text.Encoding.ASCII.GetBytes(filename);

        // Binary format: filename + null + binary data
        var valueLength = filenameBytes.Length + 1 + data.Length;

        var item = new byte[8 + keyBytes.Length + 1 + valueLength];

        // Value size
        BitConverter.GetBytes((uint)valueLength).CopyTo(item, 0);

        // Flags = 2 (binary type, bits 2-1 = 1)
        BitConverter.GetBytes(2u).CopyTo(item, 4);

        // Key
        keyBytes.CopyTo(item, 8);

        // Key null terminator (already zero)

        // Value: filename + null + data
        filenameBytes.CopyTo(item, 8 + keyBytes.Length + 1);
        // Null separator already zero
        data.CopyTo(item, 8 + keyBytes.Length + 1 + filenameBytes.Length + 1);

        return item;
    }

    private static byte[] CreateExternalLocatorItem(string key, string url)
    {
        var keyBytes = System.Text.Encoding.ASCII.GetBytes(key);
        var valueBytes = System.Text.Encoding.UTF8.GetBytes(url);

        var item = new byte[8 + keyBytes.Length + 1 + valueBytes.Length];

        // Value size
        BitConverter.GetBytes((uint)valueBytes.Length).CopyTo(item, 0);

        // Flags = 4 (external locator, bits 2-1 = 2)
        BitConverter.GetBytes(4u).CopyTo(item, 4);

        // Key
        keyBytes.CopyTo(item, 8);

        // Value
        valueBytes.CopyTo(item, 8 + keyBytes.Length + 1);

        return item;
    }

    private static byte[] CreateCompleteTag(Dictionary<string, string> items)
    {
        using var ms = new MemoryStream();

        // Write items
        foreach (var kvp in items)
        {
            var item = CreateTextItem(kvp.Key, kvp.Value);
            ms.Write(item);
        }

        var itemsData = ms.ToArray();

        // Calculate tag size (items + footer, excludes header for APEv2 compat)
        var tagSize = (uint)(itemsData.Length + 32);

        // Create footer
        var footer = CreateValidFooter(
            version: 2000,
            tagSize: tagSize,
            itemCount: (uint)items.Count,
            flags: 0);

        // Combine items + footer
        var result = new byte[itemsData.Length + 32];
        itemsData.CopyTo(result, 0);
        footer.CopyTo(result, itemsData.Length);

        return result;
    }

    #endregion
}
