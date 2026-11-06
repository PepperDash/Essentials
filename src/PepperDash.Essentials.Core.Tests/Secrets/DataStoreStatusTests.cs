using FluentAssertions;
using PepperDash.Essentials.Core;
using Xunit;

namespace PepperDash.Essentials.Core.Tests.Secrets;

/// <summary>
/// Covers parsing of the DATASTORESTATUS console command. The samples are verbatim output from an
/// RMC4 (firmware 2.8006.00110), tabs and CRLF included.
/// </summary>
public class DataStoreStatusTests
{
    // Two programs with different names; note the global rows put a space before each tab.
    private const string TwoPrograms =
        "CrestronDataStore: SIMPLLOCAL\r\n" +
        "\tPepperDashEssentials\t  9 records\t 3661 bytes\t 36% used\r\n" +
        "\tCdsScopeProbe\t  2 records\t 289 bytes\t 2% used\r\n" +
        "CrestronDataStore: SIMPLGLOBAL\r\n" +
        "\tPepperDashEssentials\t  3 records \t 742 bytes \t 7% used\r\n" +
        "\tCdsScopeProbe\t  1 records \t 98 bytes \t 0% used\r\n" +
        "RMC4>";

    [Fact]
    public void Parses_each_program_in_each_store()
    {
        var rows = DataStoreStatus.Parse(TwoPrograms);

        rows.Should().HaveCount(4);
        rows[0].Should().BeEquivalentTo(new DataStoreUsage
        {
            Store = "local", Owner = "PepperDashEssentials", Records = 9, Bytes = 3661, PercentUsed = 36
        });
        rows[1].Should().BeEquivalentTo(new DataStoreUsage
        {
            Store = "local", Owner = "CdsScopeProbe", Records = 2, Bytes = 289, PercentUsed = 2
        });
        rows[2].Should().BeEquivalentTo(new DataStoreUsage
        {
            Store = "global", Owner = "PepperDashEssentials", Records = 3, Bytes = 742, PercentUsed = 7
        });
        rows[3].Store.Should().Be("global");
        rows[3].PercentUsed.Should().Be(0);
    }

    [Fact]
    public void Reports_usage_over_one_hundred_percent()
    {
        var rows = DataStoreStatus.Parse(
            "CrestronDataStore: SIMPLLOCAL\r\n\tPepperDashEssentials\t  7 records\t 11421 bytes\t 114% used\r\nCrestronDataStore: SIMPLGLOBAL\r\n");

        rows.Should().ContainSingle().Which.PercentUsed.Should().Be(114);
    }

    [Fact]
    public void An_empty_store_has_no_rows()
    {
        DataStoreStatus.Parse("CrestronDataStore: SIMPLLOCAL\r\nCrestronDataStore: SIMPLGLOBAL\r\nRMC4>")
            .Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("DataStores not Available")]
    [InlineData("ERROR: Bad or Incomplete Command")]
    public void Unrecognised_output_has_no_rows(string output)
    {
        DataStoreStatus.Parse(output).Should().BeEmpty();
    }

    [Fact]
    public void Ignores_usage_lines_before_any_store_header()
    {
        DataStoreStatus.Parse("\tPepperDashEssentials\t  6 records\t 2921 bytes\t 29% used\r\n")
            .Should().BeEmpty();
    }
}
