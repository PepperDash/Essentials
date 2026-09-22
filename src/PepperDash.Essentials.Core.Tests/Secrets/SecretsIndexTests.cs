using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using PepperDash.Essentials.Core;
using Xunit;

namespace PepperDash.Essentials.Core.Tests.Secrets;

/// <summary>
/// Covers the parts of the secrets index that do not need a processor: chunking, the torn-write
/// checksum, sanitizing, and stale detection.
/// </summary>
public class SecretsIndexTests
{
    private static SecretsIndexEntry Entry(string key, string? description = null) => new()
    {
        Key = key,
        Description = description,
        CreatedUtc = new DateTime(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc),
        UpdatedUtc = new DateTime(2026, 9, 16, 12, 0, 0, DateTimeKind.Utc)
    };

    private static SecretsIndexDocument Document(params SecretsIndexEntry[] entries) => new()
    {
        Version = SecretsIndex.Version,
        Entries = entries.ToList()
    };

    // ─── Reserved keys ───────────────────────────────────────────────────────

    [Theory]
    [InlineData("__essSecretsIdx", true)]
    [InlineData("__essSecretsIdx00", true)]
    [InlineData("__essSecretsIdx15", true)]
    [InlineData("displayPassword", false)]
    [InlineData("__other", false)]
    [InlineData("", false)]
    public void IsReservedKey_identifies_the_index_records(string key, bool expected)
    {
        SecretsIndex.IsReservedKey(key).Should().Be(expected);
    }

    [Fact]
    public void Chunk_and_header_keys_fit_the_32_character_data_store_limit()
    {
        SecretsIndex.HeaderKey.Length.Should().BeLessThanOrEqualTo(32);

        for (var i = 0; i < SecretsIndex.MaxChunks; i++)
        {
            SecretsIndex.ChunkKey(i).Length.Should().BeLessThanOrEqualTo(32);
        }
    }

    [Fact]
    public void ChunkKey_is_zero_padded_so_keys_sort_and_parse_predictably()
    {
        SecretsIndex.ChunkKey(0).Should().Be("__essSecretsIdx00");
        SecretsIndex.ChunkKey(9).Should().Be("__essSecretsIdx09");
        SecretsIndex.ChunkKey(15).Should().Be("__essSecretsIdx15");
    }

    // ─── Chunking ────────────────────────────────────────────────────────────

    [Fact]
    public void Split_returns_nothing_for_an_empty_payload()
    {
        SecretsIndex.Split("").Should().BeEmpty();
        SecretsIndex.Split(null!).Should().BeEmpty();
    }

    [Fact]
    public void Split_keeps_every_chunk_within_the_data_store_value_limit()
    {
        var payload = new string('x', SecretsIndex.ChunkSize * 3 + 17);

        var chunks = SecretsIndex.Split(payload);

        chunks.Should().HaveCount(4);
        chunks.Should().OnlyContain(chunk => chunk.Length <= 1600);
        chunks.Take(3).Should().OnlyContain(chunk => chunk.Length == SecretsIndex.ChunkSize);
        chunks.Last().Length.Should().Be(17);
    }

    [Fact]
    public void Split_round_trips_exactly()
    {
        var payload = string.Concat(Enumerable.Range(0, 5000).Select(i => (char)('a' + i % 26)));

        string.Concat(SecretsIndex.Split(payload)).Should().Be(payload);
    }

    [Fact]
    public void Serialize_escapes_non_ascii_so_character_count_equals_byte_count()
    {
        // The chunk arithmetic counts characters; if the store counts bytes, a multi-byte character
        // would silently push a chunk over the limit. Escaping removes the ambiguity.
        var payload = SecretsIndex.Serialize(Document(Entry("k", "café ☕ naïve")));

        payload.Should().NotContain("é");
        payload.Should().Contain("\\u");
        payload.All(c => c < 128).Should().BeTrue();
    }

    [Fact]
    public void A_full_index_of_realistic_entries_needs_more_than_one_chunk()
    {
        // The reason the index is chunked at all: a single record caps at 1600 characters, which is
        // only a handful of entries.
        var entries = Enumerable.Range(0, 20)
            .Select(i => Entry($"secretNumber{i:00}", $"A description for secret number {i}"))
            .ToArray();

        var payload = SecretsIndex.Serialize(Document(entries));

        payload.Length.Should().BeGreaterThan(1600);
        SecretsIndex.Split(payload).Count.Should().BeGreaterThan(1);
    }

    // ─── Checksum ────────────────────────────────────────────────────────────

    [Fact]
    public void Checksum_is_stable_and_sensitive_to_change()
    {
        var a = SecretsIndex.Checksum("hello world");
        var b = SecretsIndex.Checksum("hello world");
        var c = SecretsIndex.Checksum("hello worlds");

        a.Should().Be(b);
        a.Should().NotBe(c);
        a.Should().MatchRegex("^[0-9a-f]{8}$");
    }

    [Fact]
    public void Checksum_handles_empty_and_null_without_throwing()
    {
        SecretsIndex.Checksum("").Should().NotBeNullOrEmpty();
        SecretsIndex.Checksum(null!).Should().Be(SecretsIndex.Checksum(""));
    }

    // ─── Header parsing ──────────────────────────────────────────────────────

    [Fact]
    public void ParseHeader_reads_a_well_formed_header()
    {
        var header = SecretsIndex.ParseHeader(
            """{"v":1,"chunks":2,"len":100,"sum":"abcd1234","updatedUtc":"2026-09-16T18:22:05Z"}""");

        header.Should().NotBeNull();
        header!.Chunks.Should().Be(2);
        header.Length.Should().Be(100);
        header.Checksum.Should().Be("abcd1234");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not json at all")]
    [InlineData("""{"v":2,"chunks":1,"len":10,"sum":"x"}""")]   // wrong version
    [InlineData("""{"v":1,"chunks":-1,"len":10,"sum":"x"}""")]  // impossible chunk count
    [InlineData("""{"v":1,"chunks":99,"len":10,"sum":"x"}""")]  // beyond MaxChunks
    public void ParseHeader_rejects_anything_it_cannot_trust(string? json)
    {
        SecretsIndex.ParseHeader(json!).Should().BeNull();
    }

    // ─── Document parsing and corruption ─────────────────────────────────────

    private static (SecretsIndexHeader Header, string Payload) Build(params SecretsIndexEntry[] entries)
    {
        var payload = SecretsIndex.Serialize(Document(entries));
        var header = new SecretsIndexHeader
        {
            Version = SecretsIndex.Version,
            Chunks = SecretsIndex.Split(payload).Count,
            Length = payload.Length,
            Checksum = SecretsIndex.Checksum(payload),
            UpdatedUtc = DateTime.UtcNow
        };
        return (header, payload);
    }

    [Fact]
    public void ParseDocument_round_trips_a_valid_index()
    {
        var (header, payload) = Build(Entry("displayPassword", "Display admin"));

        var document = SecretsIndex.ParseDocument(header, payload, out var status);

        status.Should().BeNull();
        document.Should().NotBeNull();
        document!.Entries.Should().ContainSingle();
        document.Entries[0].Key.Should().Be("displayPassword");
        document.Entries[0].Description.Should().Be("Display admin");
    }

    [Fact]
    public void ParseDocument_reports_a_missing_header()
    {
        SecretsIndex.ParseDocument(null!, "{}", out var status).Should().BeNull();
        status.Should().Be("corrupt:header");
    }

    [Fact]
    public void ParseDocument_reports_a_missing_chunk()
    {
        var (header, _) = Build(Entry("k"));

        SecretsIndex.ParseDocument(header, null!, out var status).Should().BeNull();
        status.Should().Be("corrupt:missingChunk");
    }

    // This is what a torn write looks like: the header survived from the previous write but the
    // chunks are from the new one, so the two disagree. Detecting it is the whole point of writing
    // the header last.
    [Fact]
    public void ParseDocument_detects_a_header_that_disagrees_with_its_chunks()
    {
        var (header, payload) = Build(Entry("k"));
        var (_, differentPayload) = Build(Entry("k"), Entry("k2"));

        SecretsIndex.ParseDocument(header, differentPayload, out var status).Should().BeNull();
        status.Should().Be("corrupt:checksum");

        // Same length, different content, so only the checksum can catch it.
        var tampered = payload.Substring(0, payload.Length - 2) + "ZZ";
        tampered.Length.Should().Be(payload.Length);
        SecretsIndex.ParseDocument(header, tampered, out var tamperedStatus).Should().BeNull();
        tamperedStatus.Should().Be("corrupt:checksum");
    }

    [Fact]
    public void ParseDocument_reports_unparseable_content()
    {
        var payload = "this is not json";
        var header = new SecretsIndexHeader
        {
            Version = SecretsIndex.Version,
            Chunks = 1,
            Length = payload.Length,
            Checksum = SecretsIndex.Checksum(payload),
            UpdatedUtc = DateTime.UtcNow
        };

        SecretsIndex.ParseDocument(header, payload, out var status).Should().BeNull();
        status.Should().Be("corrupt:unparseable");
    }

    // ─── Sanitizing ──────────────────────────────────────────────────────────

    [Fact]
    public void Sanitize_drops_keyless_entries_rather_than_rejecting_the_whole_index()
    {
        var warnings = new List<string>();

        var result = SecretsIndex.Sanitize(
            new[] { Entry("good"), Entry(""), Entry("   "), null!, Entry("alsoGood") },
            warnings);

        result.Select(e => e.Key).Should().Equal("good", "alsoGood");
        warnings.Should().HaveCount(3);
    }

    [Fact]
    public void Sanitize_keeps_the_first_of_a_duplicate_pair()
    {
        var warnings = new List<string>();

        var result = SecretsIndex.Sanitize(
            new[] { Entry("dup", "first"), Entry("DUP", "second") },
            warnings);

        result.Should().ContainSingle();
        result[0].Description.Should().Be("first");
        warnings.Should().ContainSingle(w => w.Contains("DUP"));
    }

    [Fact]
    public void Sanitize_tolerates_a_null_collection()
    {
        SecretsIndex.Sanitize(null!, new List<string>()).Should().BeEmpty();
    }

    // ─── Descriptions ────────────────────────────────────────────────────────

    [Fact]
    public void NormalizeDescription_trims_truncates_and_nulls_out_blanks()
    {
        SecretsIndex.NormalizeDescription(null).Should().BeNull();
        SecretsIndex.NormalizeDescription("   ").Should().BeNull();
        SecretsIndex.NormalizeDescription("  hello  ").Should().Be("hello");

        SecretsIndex.NormalizeDescription(new string('x', 500))
            .Should().HaveLength(SecretsIndex.MaxDescriptionLength);
    }

    // ─── Stale detection ─────────────────────────────────────────────────────

    [Fact]
    public void FindStale_returns_entries_with_no_matching_record()
    {
        var stale = SecretsIndex.FindStale(
            new[] { Entry("present"), Entry("gone"), Entry("alsoGone") },
            new[] { "present", "somethingElse" });

        stale.Select(e => e.Key).Should().Equal("gone", "alsoGone");
    }

    [Fact]
    public void FindStale_matches_keys_case_insensitively()
    {
        SecretsIndex.FindStale(new[] { Entry("Present") }, new[] { "present" }).Should().BeEmpty();
    }

    [Fact]
    public void FindStale_treats_an_empty_store_as_everything_being_stale()
    {
        // Correct in isolation - and exactly why the caller must refuse to prune unless the walk
        // completed. Against a truncated enumeration this would report live secrets as stale.
        SecretsIndex.FindStale(new[] { Entry("a"), Entry("b") }, Array.Empty<string>())
            .Should().HaveCount(2);
    }

    [Fact]
    public void FindStale_tolerates_nulls()
    {
        SecretsIndex.FindStale(null!, new[] { "a" }).Should().BeEmpty();
        SecretsIndex.FindStale(new[] { Entry("a") }, null!).Should().ContainSingle();
    }
}
