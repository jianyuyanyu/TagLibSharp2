# TagLibSharp2 Roadmap

> Generated: January 2026
> Last Updated: January 2026
> Status: Active development - ASF/WMA complete, edge cases in progress

## Executive Summary

TagLibSharp2 currently supports **14 of 20** required formats with comprehensive tag support. The library has mature read/write capabilities for major formats (MP3, FLAC, MP4, Ogg, WAV, AIFF, DSD, ASF/WMA, Musepack). Remaining gaps are Speex, TrueAudio, and tracker formats.

---

## Implementation Status Matrix

### Format Support

| Format | Required | Implemented | Read | Write | Priority | Notes |
|--------|----------|-------------|------|-------|----------|-------|
| MP3 | P0 | ✅ | ✅ | ✅ | - | Production ready |
| FLAC | P0 | ✅ | ✅ | ✅ | - | Production ready |
| MP4/M4A | P0 | ✅ | ✅ | ✅ | - | Production ready |
| WAV | P0 | ✅ | ✅ | ✅ | - | Production ready |
| AIFF | P0 | ✅ | ✅ | ✅ | - | Production ready |
| DSF | P0 | ✅ | ✅ | ✅ | - | Recently added |
| Ogg Vorbis | P0 | ✅ | ✅ | ✅ | - | Production ready |
| Opus | P0 | ✅ | ✅ | ✅ | - | Production ready |
| WMA/ASF | P1 | ✅ | ✅ | ❌ | - | Read complete, write pending |
| DFF | P1 | ✅ | ✅ | ✅ | - | Recently added |
| WavPack | P1 | ✅ | ✅ | ✅ | - | Recently added |
| Musepack | P1 | ✅ | ✅ | ✅ | - | SV7 and SV8 support |
| Ogg FLAC | P1 | ✅ | ✅ | ✅ | - | Recently added |
| Speex | P1 | ❌ | ❌ | ❌ | LOW | Uses Vorbis Comment |
| TrueAudio | P2 | ❌ | ❌ | ❌ | LOW | Uses ID3v2 |
| Monkey's Audio | P2 | ✅ | ✅ | ✅ | - | Recently added |
| MOD | P2 | ❌ | ❌ | ❌ | LOW | Tracker format |
| S3M | P2 | ❌ | ❌ | ❌ | LOW | Tracker format |
| IT | P2 | ❌ | ❌ | ❌ | LOW | Tracker format |
| XM | P2 | ❌ | ❌ | ❌ | LOW | Tracker format |

### Tag Format Support

| Tag Format | Required | Implemented | Notes |
|------------|----------|-------------|-------|
| ID3v1 | ✅ | ✅ | Complete |
| ID3v2 (v2.2/v2.3/v2.4) | ✅ | ✅ | Comprehensive frame support |
| Vorbis Comment | ✅ | ✅ | Full field mapping |
| MP4/iTunes | ✅ | ✅ | Apple atom support |
| APE Tag | ✅ | ✅ | v2 with binary items |
| ASF Attributes | ✅ | ✅ | Complete with WM/* mappings |
| RIFF INFO | ✅ | ✅ | For WAV files |

---

## Phase 1: Critical Gaps (HIGH Priority)

### 1.1 WMA/ASF Format Support ✅ COMPLETE
**Status: Implemented January 2026**

Full read support implemented with:
- ASF container parsing (GUID-based object structure)
- Content Description (Title, Author, Copyright, Description, Rating)
- Extended Content Description (WM/* attributes)
- File Properties (duration, bitrate, file size)
- Stream Properties (sample rate, channels, codec detection)
- Security hardening with bounds checking and overflow protection

**Files created:**
```
src/TagLibSharp2/Asf/
├── AsfFile.cs                      # Container parser
├── AsfTag.cs                       # Tag implementation
├── AsfGuid.cs                      # 128-bit GUID struct
├── AsfGuids.cs                     # Well-known GUID constants
├── AsfDescriptor.cs                # Typed attribute values
├── AsfContentDescription.cs        # 5 fixed fields
├── AsfExtendedContentDescription.cs # Key-value attributes
├── AsfFileProperties.cs            # Duration, bitrate
└── AsfStreamProperties.cs          # Audio codec info
```

---

### 1.2 Edge Case Handling Improvements
**Effort: Medium | Business Value: High**

The Roon docs identify specific edge cases that must be handled robustly:

| Edge Case | Status | Action |
|-----------|--------|--------|
| iTunes ID3v2.3 with syncsafe sizes | ✅ | Syncsafe fallback when big-endian fails |
| UTF-16 without BOM fallback | ✅ | Falls back to little-endian (Windows default) |
| Duplicate ID3v2 tags | ❌ | Handle gracefully |
| Apple proprietary v2.3 frames | ⚠️ Partial | Add TSOA, TSOT, TSOP, WFED, MVNM, MVIN |
| MP4 duplicate atoms | ⚠️ Partial | Merge list types |
| ID3v2.4 genre separator | ⚠️ Partial | Already handles both null and "/" |

---

### 1.3 Cross-Tagger Compatibility Tests
**Status: Mostly Complete**

Cross-tagger compatibility tests exist in `CrossTaggerCompatibilityTests.cs` and `Mp4CompatibilityTests.cs`:
- [x] MusicBrainz Picard field names (TXXX descriptions)
- [x] ReplayGain format (foobar2000, Mp3tag compatible)
- [x] iTunes compilation flag ("1" value)
- [x] Classical music fields (WORK, MOVEMENT)
- [x] AcoustID fields
- [x] R128 gain normalization
- [x] Standard frame IDs and Vorbis Comment uppercase names

Integration tests with real files (requires env vars):
- [ ] foobar2000 (TEST_MP4_FOOBAR2000)
- [ ] Mp3tag
- [ ] iTunes
- [ ] VLC
- [ ] MediaMonkey
- [ ] Kid3

---

## Phase 2: Format Expansion (MEDIUM Priority)

### 2.1 Musepack Support
**Effort: Small | Business Value: Medium**

Musepack uses APE tags (already implemented). Primarily need:
- Magic byte detection for SV7/SV8 streams
- Audio properties parsing from stream headers
- Registration in MediaFile factory

**Files to create:**
```
src/TagLibSharp2/Musepack/
├── MusepackFile.cs
└── MusepackProperties.cs
```

### 2.2 Speex Support
**Effort: Small | Business Value: Low**

Speex is an Ogg-encapsulated format using Vorbis Comments (already implemented).

**Requirements:**
- Speex header parsing for audio properties
- Registration in Ogg container detection

### 2.3 TrueAudio Support
**Effort: Small | Business Value: Low**

TrueAudio uses ID3v1/ID3v2 tags (already implemented).

**Requirements:**
- TTA header parsing for audio properties
- Magic byte detection

---

## Phase 3: Tracker Formats (LOW Priority)

Tracker formats (MOD, S3M, IT, XM) have limited tagging capabilities and are lower priority.

### Implementation Notes:
- MOD: Metadata at offset 1080, 31 sample names (20 chars each)
- S3M: Similar to MOD but with SCRM signature
- IT: IMPM signature, extended instrument names
- XM: Extended Module format with pattern data

**Recommendation:** Implement as read-only with minimal tag support initially.

---

## Phase 4: Quality & Polish

### 4.1 Performance Optimization
- [ ] Benchmark suite for regression testing
- [ ] Memory profiling for large file handling
- [ ] Parallel read safety verification

### 4.2 Documentation
- [ ] XML documentation coverage audit
- [ ] Migration guide from TagLib#
- [ ] API reference generation

### 4.3 Missing Metadata Fields

Per Roon requirements, verify these fields are properly mapped:

| Field | ID3v2 | Vorbis | MP4 | Status |
|-------|-------|--------|-----|--------|
| RoonId | TSID | ROONID | ©sid | Verify |
| UserTags | TTAG | TAGS | ©tag | Verify |
| Version | TVER | VERSION | ©ver | Verify |
| RoonRadioBan | TXXX:ROONRADIOBAN | ROONRADIOBAN | - | Verify |
| RecordingDate | - | PERFORMANCEDATE | - | Add |
| Section | TXXX:SECTION | SECTION | - | Verify |

---

## Backlog: Not Planned

These items from the requirements are explicitly not planned:

| Item | Reason |
|------|--------|
| Matroska/MKV | No audio-only use case |
| XMP metadata | Out of scope for audio files |
| DVD/BD audio | Not standard audio files |

---

## Work Breakdown

### Sprint 1: ASF/WMA Foundation ✅ COMPLETE
1. ✅ Create ASF container parser with GUID detection
2. ✅ Implement ASF attribute reading
3. ✅ Add audio properties extraction
4. ✅ Create AsfTag implementing Tag interface
5. ✅ Add comprehensive test suite (12 security tests)
6. ✅ Register in MediaFile factory

### Sprint 2: Edge Cases & Compatibility (IN PROGRESS)
1. ✅ Fix UTF-16 BOM fallback in TextFrame and LyricsFrame
2. ✅ Add iTunes syncsafe fallback for v2.3 tags
3. Handle duplicate tags gracefully
4. Add cross-tagger compatibility tests
5. Add remaining Apple proprietary frames

### Sprint 3: Format Expansion (1-2 weeks effort)
1. ✅ Add Musepack support (SV7 and SV8)
2. Add Speex support
3. Add TrueAudio support
4. Verify all Roon-specific fields

### Sprint 4: Tracker Formats (1 week effort)
1. MOD format (read-only)
2. S3M format (read-only)
3. IT format (read-only)
4. XM format (read-only)

---

## Success Metrics

| Metric | Current | Target |
|--------|---------|--------|
| Format coverage | 14/20 (70%) | 20/20 (100%) |
| Test coverage | ~85% | >90% |
| Round-trip tests | Partial | All formats |
| Cross-tagger compat | Untested | 7 major taggers |
| Edge case handling | Partial | All documented cases |

---

## Appendix: Specification References

- **ASF:** [Microsoft ASF Specification](https://docs.microsoft.com/en-us/windows/win32/wmformat/overview-of-the-asf-format)
- **Musepack:** [Musepack SV8 Specification](https://wiki.hydrogenaud.io/index.php?title=Musepack)
- **Speex:** [Speex Manual](https://speex.org/docs/manual/speex-manual/)
- **TrueAudio:** [TTA Specification](http://tausoft.org/wiki/True_Audio_Codec_Format)
- **Tracker Formats:** [ModPlug Tracker Documentation](https://wiki.openmpt.org/Manual:_Module_formats)
