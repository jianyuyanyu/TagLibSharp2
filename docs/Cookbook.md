# TagLibSharp2 Cookbook

Practical recipes for common audio tagging tasks.

## Table of Contents

1. [Reading Tags](#reading-tags)
2. [Writing Tags](#writing-tags)
3. [Album Art](#album-art)
4. [Batch Operations](#batch-operations)
5. [Format Conversion](#format-conversion)
6. [Advanced Patterns](#advanced-patterns)

---

## Reading Tags

### Read Any File (Auto-Detect Format)

```csharp
var result = MediaFile.Open("music.mp3");
if (result.IsSuccess)
{
    var tag = result.Tag!;
    Console.WriteLine($"Title: {tag.Title}");
    Console.WriteLine($"Artist: {tag.Artist}");
    Console.WriteLine($"Album: {tag.Album}");
    Console.WriteLine($"Track: {tag.Track}/{tag.TotalTracks}");
    Console.WriteLine($"Year: {tag.Year}");
    Console.WriteLine($"Format: {result.Format}");
}
```

### Read MP3 with Full Access

```csharp
var result = Mp3File.ReadFromFile("song.mp3");
if (result.IsSuccess)
{
    var mp3 = result.File!;

    // Access ID3v2 tag (preferred)
    if (mp3.Id3v2Tag is { } id3v2)
    {
        Console.WriteLine($"Title: {id3v2.Title}");
        Console.WriteLine($"BPM: {id3v2.BeatsPerMinute}");
        Console.WriteLine($"ISRC: {id3v2.Isrc}");

        // Get custom TXXX frames
        var customValue = id3v2.GetUserText("MY_CUSTOM_FIELD");
    }

    // Fallback to ID3v1 for legacy files
    if (mp3.Id3v1Tag is { } id3v1)
    {
        Console.WriteLine($"Title: {id3v1.Title}");
    }
}
```

### Read FLAC Metadata

```csharp
var result = FlacFile.ReadFromFile("album.flac");
if (result.IsSuccess)
{
    var flac = result.File!;
    var vorbis = flac.VorbisComment;

    Console.WriteLine($"Title: {vorbis?.Title}");
    Console.WriteLine($"ReplayGain: {vorbis?.ReplayGainTrackGain}");

    // Access stream info
    var streamInfo = flac.StreamInfo;
    Console.WriteLine($"Sample Rate: {streamInfo.SampleRate} Hz");
    Console.WriteLine($"Channels: {streamInfo.Channels}");
    Console.WriteLine($"Bits: {streamInfo.BitsPerSample}");
}
```

### Read Custom Fields

```csharp
// ID3v2 - User-defined text (TXXX frames)
var id3v2 = mp3.Id3v2Tag!;
var acoustId = id3v2.GetUserText("ACOUSTID_ID");
var customField = id3v2.GetUserText("MY_APP_DATA");

// Vorbis Comment - Any field
var vorbis = flac.VorbisComment!;
var acoustId = vorbis.GetValue("ACOUSTID_ID");
var customField = vorbis.GetValue("MY_APP_DATA");
```

---

## Writing Tags

### Basic Tag Writing

```csharp
var result = Mp3File.ReadFromFile("song.mp3");
if (result.IsSuccess)
{
    var mp3 = result.File!;

    mp3.Title = "New Title";
    mp3.Artist = "New Artist";
    mp3.Album = "New Album";
    mp3.Year = "2024";
    mp3.Track = 1;
    mp3.TotalTracks = 12;
    mp3.Genre = "Rock";

    var originalData = File.ReadAllBytes("song.mp3");
    mp3.SaveToFile("song.mp3", originalData);
}
```

### Write Extended Metadata

```csharp
var mp3 = result.File!;

// Disc information
mp3.DiscNumber = 1;
mp3.TotalDiscs = 2;

// Credits
mp3.Composer = "Composer Name";
mp3.Conductor = "Conductor Name";
mp3.AlbumArtist = "Various Artists";

// Production
mp3.Copyright = "2024 Record Label";
mp3.Publisher = "Record Label";
mp3.Isrc = "USRC12345678";
mp3.BeatsPerMinute = 120;

// Classification
mp3.Grouping = "Holiday";
mp3.Subtitle = "Radio Edit";
mp3.InitialKey = "Am";

// Compilation flag
mp3.IsCompilation = true;
```

### Write MusicBrainz IDs

```csharp
var tag = mp3.Id3v2Tag!;

tag.MusicBrainzTrackId = "f4e7c9d8-1234-5678-9abc-def012345678";
tag.MusicBrainzReleaseId = "a1b2c3d4-5678-90ab-cdef-1234567890ab";
tag.MusicBrainzArtistId = "12345678-90ab-cdef-1234-567890abcdef";
tag.MusicBrainzReleaseGroupId = "abcdef12-3456-7890-abcd-ef1234567890";
tag.MusicBrainzAlbumArtistId = "fedcba98-7654-3210-fedc-ba9876543210";
tag.MusicBrainzReleaseStatus = "official";
tag.MusicBrainzReleaseType = "album";
```

### Write AcoustID Fingerprint

```csharp
var tag = mp3.Id3v2Tag!;

// After running fpcalc/chromaprint
tag.AcoustIdId = "f4e7c9d8-1234-5678-9abc-def012345678";
tag.AcoustIdFingerprint = "AQADtNQyRUkSRZEGAAAAAA...";
```

### Write ReplayGain and R128

```csharp
var tag = mp3.Id3v2Tag!;

// ReplayGain (decibel strings)
tag.ReplayGainTrackGain = "-6.50 dB";
tag.ReplayGainTrackPeak = "0.988547";
tag.ReplayGainAlbumGain = "-5.20 dB";
tag.ReplayGainAlbumPeak = "1.000000";

// R128 (Q7.8 format integers)
tag.R128TrackGain = "-512";  // -2 dB
tag.R128AlbumGain = "256";   // +1 dB
```

### Write Custom Fields

```csharp
// ID3v2 - User-defined text (TXXX frames)
var id3v2 = mp3.Id3v2Tag!;
id3v2.SetUserText("MY_APP_DATA", "custom value");
id3v2.SetUserText("CATALOG_ID", "ABC-12345");

// Vorbis Comment - Any field
var vorbis = flac.VorbisComment!;
vorbis.SetValue("MY_APP_DATA", "custom value");
vorbis.SetValue("CATALOG_ID", "ABC-12345");
```

---

## Album Art

### Read Album Art

```csharp
var result = Mp3File.ReadFromFile("song.mp3");
if (result.IsSuccess && result.File!.Pictures.Length > 0)
{
    foreach (var picture in result.File.Pictures)
    {
        Console.WriteLine($"Type: {picture.PictureType}");
        Console.WriteLine($"MIME: {picture.MimeType}");
        Console.WriteLine($"Size: {picture.PictureData.Length} bytes");

        // Save to file
        var extension = picture.MimeType switch {
            "image/jpeg" => ".jpg",
            "image/png" => ".png",
            _ => ".bin"
        };
        File.WriteAllBytes($"cover{extension}", picture.PictureData.ToArray());
    }
}
```

### Add Album Art to MP3

```csharp
using TagLibSharp2.Id3.Id3v2.Frames;

var mp3 = result.File!;
var imageBytes = File.ReadAllBytes("cover.jpg");

var picture = new PictureFrame(
    "image/jpeg",
    PictureType.FrontCover,
    "Album Cover",
    imageBytes);

mp3.Id3v2Tag?.AddPicture(picture);

// Add back cover
var backCover = new PictureFrame(
    "image/jpeg",
    PictureType.BackCover,
    "Back Cover",
    File.ReadAllBytes("back.jpg"));
mp3.Id3v2Tag?.AddPicture(backCover);
```

### Add Album Art to FLAC

```csharp
using TagLibSharp2.Xiph;

var flac = result.File!;
var imageBytes = File.ReadAllBytes("cover.jpg");

// Get image dimensions (simplified - use actual image library)
var picture = new FlacPicture(
    PictureType.FrontCover,
    "image/jpeg",
    "Album Cover",
    width: 500,
    height: 500,
    colorDepth: 24,
    colorCount: 0,
    new BinaryData(imageBytes));

flac.VorbisComment?.AddPicture(picture);
```

### Remove Album Art

```csharp
// Clear all pictures
mp3.Id3v2Tag!.Pictures = [];

// Or remove specific type
var pictures = mp3.Id3v2Tag!.Pictures.ToList();
pictures.RemoveAll(p => p.PictureType == PictureType.BackCover);
mp3.Id3v2Tag!.Pictures = pictures.ToArray();
```

---

## Batch Operations

### Process Multiple Files

```csharp
var files = Directory.GetFiles("music/", "*.mp3", SearchOption.AllDirectories);

var results = await BatchProcessor.ProcessAsync(
    files,
    async (path, ct) =>
    {
        var result = await Mp3File.ReadFromFileAsync(path, ct: ct);
        if (!result.IsSuccess) return null;

        return new {
            Path = path,
            Title = result.File!.Title,
            Artist = result.File.Artist
        };
    },
    maxDegreeOfParallelism: 4,
    progress: new Progress<BatchProgress>(p =>
        Console.WriteLine($"Processing: {p.PercentComplete:F1}%"))
);

// Analyze results
var successful = results.WhereSucceeded().ToList();
var failed = results.WhereFailed().ToList();

Console.WriteLine($"Processed: {successful.Count}, Failed: {failed.Count}");
```

### Bulk Update Tags

```csharp
var files = Directory.GetFiles("album/", "*.mp3");

var results = await BatchProcessor.ProcessAsync(
    files,
    async (path, ct) =>
    {
        var result = await Mp3File.ReadFromFileAsync(path, ct: ct);
        if (!result.IsSuccess) return false;

        var mp3 = result.File!;
        mp3.Album = "Corrected Album Name";
        mp3.AlbumArtist = "Various Artists";

        var data = await File.ReadAllBytesAsync(path, ct);
        await mp3.SaveToFileAsync(path, data, ct);
        return true;
    });

Console.WriteLine($"Updated {results.SuccessCount()} files");
```

### Find Files Missing Metadata

```csharp
var files = Directory.GetFiles("music/", "*.mp3", SearchOption.AllDirectories);

var missingData = await BatchProcessor.ProcessAsync(
    files,
    async (path, ct) =>
    {
        var result = await Mp3File.ReadFromFileAsync(path, ct: ct);
        if (!result.IsSuccess) return null;

        var mp3 = result.File!;
        var issues = new List<string>();

        if (string.IsNullOrEmpty(mp3.Title)) issues.Add("Missing title");
        if (string.IsNullOrEmpty(mp3.Artist)) issues.Add("Missing artist");
        if (mp3.Pictures.Length == 0) issues.Add("Missing album art");

        return issues.Count > 0 ? new { Path = path, Issues = issues } : null;
    });

foreach (var item in missingData.WhereSucceeded().Where(r => r.Value is not null))
{
    Console.WriteLine($"{item.Value!.Path}:");
    foreach (var issue in item.Value.Issues)
        Console.WriteLine($"  - {issue}");
}
```

---

## Format Conversion

### Copy Tags from MP3 to FLAC

```csharp
var mp3Result = Mp3File.ReadFromFile("song.mp3");
var flacResult = FlacFile.ReadFromFile("song.flac");

if (mp3Result.IsSuccess && flacResult.IsSuccess)
{
    var sourceTag = mp3Result.File!.Id3v2Tag;
    var targetTag = flacResult.File!.VorbisComment ?? new VorbisComment();

    if (sourceTag is not null)
    {
        // Copy all metadata
        sourceTag.CopyTo(targetTag);

        // Or copy selectively
        sourceTag.CopyTo(targetTag,
            TagCopyOptions.Basic |
            TagCopyOptions.Extended |
            TagCopyOptions.Pictures);
    }

    var data = File.ReadAllBytes("song.flac");
    flacResult.File.SaveToFile("song.flac", data);
}
```

### Copy Tags Preserving Existing Values

```csharp
// CopyTo only overwrites non-null source values
var sourceTag = new Id3v2Tag { Title = "New Title" };
var targetTag = new Id3v2Tag {
    Title = "Old Title",
    Artist = "Existing Artist"  // Will be preserved
};

sourceTag.CopyTo(targetTag);

Console.WriteLine(targetTag.Title);   // "New Title"
Console.WriteLine(targetTag.Artist);  // "Existing Artist"
```

---

## Advanced Patterns

### Validate Tags Before Saving

```csharp
var tag = mp3.Id3v2Tag!;
var validation = tag.Validate();

if (!validation.IsValid)
{
    Console.WriteLine("Validation issues found:");
    foreach (var issue in validation.AllIssues)
    {
        var icon = issue.Severity switch {
            ValidationSeverity.Error => "[ERROR]",
            ValidationSeverity.Warning => "[WARN]",
            _ => "[INFO]"
        };
        Console.WriteLine($"{icon} {issue.Field}: {issue.Message}");
    }

    if (validation.HasErrors)
    {
        Console.WriteLine("Cannot save - fix errors first");
        return;
    }
}

// Safe to save
mp3.SaveToFile(path, originalData);
```

### Work with Chapters (Podcasts/Audiobooks)

```csharp
using TagLibSharp2.Id3.Id3v2.Frames;

var tag = mp3.Id3v2Tag!;

// Read chapters
var chapters = tag.GetChapters();
foreach (var chapter in chapters)
{
    Console.WriteLine($"Chapter: {chapter.ElementId}");
    Console.WriteLine($"  Start: {chapter.StartTimeMs}ms");
    Console.WriteLine($"  End: {chapter.EndTimeMs}ms");
    if (chapter.Title is not null)
        Console.WriteLine($"  Title: {chapter.Title}");
}

// Add a chapter
var newChapter = new ChapterFrame(
    elementId: "chp01",
    startTimeMs: 0,
    endTimeMs: 300000);  // 5 minutes

// Add title to chapter
newChapter.AddSubFrame(new TextFrame("TIT2", "Introduction"));
tag.AddFrame(newChapter);
```

### Work with Synchronized Lyrics

```csharp
using TagLibSharp2.Id3.Id3v2.Frames;

var tag = mp3.Id3v2Tag!;

// Read synchronized lyrics
var syncLyrics = tag.GetSyncLyrics();
if (syncLyrics is not null)
{
    foreach (var item in syncLyrics.SyncItems)
    {
        Console.WriteLine($"[{item.Timestamp}ms] {item.Text}");
    }
}

// Add synchronized lyrics
var newLyrics = new SyncLyricsFrame
{
    Encoding = TextEncodingType.Utf8,
    Language = "eng",
    ContentType = SyncLyricsType.Lyrics,
    Description = "Lyrics"
};

newLyrics.AddSyncItem("First line", 0);
newLyrics.AddSyncItem("Second line", 3000);
tag.AddFrame(newLyrics);
```

### Access Popularimeter (Ratings/Play Counts)

```csharp
using TagLibSharp2.Id3.Id3v2.Frames;

var tag = mp3.Id3v2Tag!;

// Read ratings
var popm = tag.GetPOPM("my-email@example.com");
if (popm is not null)
{
    Console.WriteLine($"Rating: {popm.Rating}/255");
    Console.WriteLine($"Play Count: {popm.PlayCount}");
}

// Simple rating access (uses any email)
Console.WriteLine($"Rating: {tag.Rating}");

// Add/update rating
tag.Rating = 200;  // ~4 stars
```

### Safe Async Pattern

```csharp
async Task ProcessFileAsync(string path, CancellationToken ct)
{
    try
    {
        var result = await Mp3File.ReadFromFileAsync(path, ct: ct);
        if (!result.IsSuccess)
        {
            Console.WriteLine($"Failed to read: {result.Error}");
            return;
        }

        var mp3 = result.File!;
        mp3.Title = "Updated";

        var data = await File.ReadAllBytesAsync(path, ct);
        await mp3.SaveToFileAsync(path, data, ct);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Operation cancelled");
    }
}

// Usage with timeout
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
await ProcessFileAsync("song.mp3", cts.Token);
```

### Testable Code with IFileSystem

```csharp
// Production code uses default file system
public class TagService
{
    readonly IFileSystem _fileSystem;

    public TagService(IFileSystem? fileSystem = null)
    {
        _fileSystem = fileSystem ?? DefaultFileSystem.Instance;
    }

    public async Task<string?> GetTitleAsync(string path)
    {
        var result = await Mp3File.ReadFromFileAsync(path, _fileSystem);
        return result.IsSuccess ? result.File!.Title : null;
    }
}

// Test code uses mock
[TestMethod]
public async Task GetTitle_ValidFile_ReturnsTitle()
{
    var mockFs = new MockFileSystem();
    mockFs.AddFile("test.mp3", CreateTestMp3("Test Title"));

    var service = new TagService(mockFs);

    var title = await service.GetTitleAsync("test.mp3");

    Assert.AreEqual("Test Title", title);
}
```
