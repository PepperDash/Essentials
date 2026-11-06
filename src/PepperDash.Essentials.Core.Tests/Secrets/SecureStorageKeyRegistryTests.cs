using System.Collections.Generic;
using FluentAssertions;
using PepperDash.Essentials.Core;
using Xunit;

namespace PepperDash.Essentials.Core.Tests.Secrets;

/// <summary>
/// Covers the key list the secure storage provider keeps, since secure storage cannot list its own
/// tags.
/// </summary>
public class SecureStorageKeyRegistryTests
{
    [Fact]
    public void RecordKey_is_reserved_so_the_secrets_api_refuses_to_touch_it()
    {
        SecretsIndex.IsReservedKey(SecureStorageKeyRegistry.RecordKey).Should().BeTrue();
    }

    [Fact]
    public void RecordKey_does_not_collide_with_an_index_chunk()
    {
        for (var i = 0; i < SecretsIndex.MaxChunks; i++)
        {
            SecureStorageKeyRegistry.RecordKey.Should().NotBe(SecretsIndex.ChunkKey(i));
        }
        SecureStorageKeyRegistry.RecordKey.Should().NotBe(SecretsIndex.HeaderKey);
    }

    [Fact]
    public void Serialize_then_TryParse_round_trips()
    {
        var json = SecureStorageKeyRegistry.Serialize(new[] { "iotCert", "iotKey" });

        SecureStorageKeyRegistry.TryParse(json, out var keys).Should().BeTrue();
        keys.Should().Equal("iotCert", "iotKey");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("not json")]
    [InlineData("{\"v\":99,\"keys\":[\"a\"]}")]
    [InlineData("{\"v\":1}")]
    public void TryParse_rejects_a_missing_or_unreadable_document(string? json)
    {
        SecureStorageKeyRegistry.TryParse(json!, out var keys).Should().BeFalse();
        keys.Should().BeEmpty();
    }

    [Fact]
    public void TryParse_drops_blank_and_duplicate_keys()
    {
        SecureStorageKeyRegistry.TryParse("{\"v\":1,\"keys\":[\"a\",\"\",\" \",\"a\",\"b\"]}", out var keys)
            .Should().BeTrue();
        keys.Should().Equal("a", "b");
    }

    [Fact]
    public void WithKey_adds_a_new_key()
    {
        SecureStorageKeyRegistry.WithKey(new[] { "a" }, "b").Should().Equal("a", "b");
    }

    [Fact]
    public void WithKey_returns_null_when_the_key_is_already_listed()
    {
        SecureStorageKeyRegistry.WithKey(new[] { "a" }, "a").Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("  ")]
    public void WithKey_ignores_a_blank_key(string? key)
    {
        SecureStorageKeyRegistry.WithKey(new[] { "a" }, key!).Should().BeNull();
    }

    [Fact]
    public void WithKey_starts_a_list_from_nothing()
    {
        SecureStorageKeyRegistry.WithKey(null!, "a").Should().Equal("a");
    }

    [Fact]
    public void WithoutKey_removes_a_listed_key()
    {
        SecureStorageKeyRegistry.WithoutKey(new[] { "a", "b" }, "a").Should().Equal("b");
    }

    [Fact]
    public void WithoutKey_returns_null_when_the_key_is_not_listed()
    {
        SecureStorageKeyRegistry.WithoutKey(new List<string> { "a" }, "b").Should().BeNull();
    }
}
