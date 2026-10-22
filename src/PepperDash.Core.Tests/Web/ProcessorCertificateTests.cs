using System.IO;
using FluentAssertions;
using PepperDash.Core.Web;
using Xunit;

namespace PepperDash.Core.Tests.Web;

public class ProcessorCertificateTests
{
    private static readonly char Sep = Path.DirectorySeparatorChar;

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("/")]
    public void Appliance_KeepsTheProcessorUserFolder(string? applicationRoot)
    {
        // On a 4-series the application root is empty, so the certificate stays at /user/selfCres.pfx
        ProcessorCertificate.GetPath(applicationRoot!).Should().Be($"{Sep}user{Sep}selfCres.pfx");
    }

    [Theory]
    [InlineData("/opt/crestron/virtualcontrol/RunningPrograms/DEMOV3")]
    [InlineData("/opt/crestron/virtualcontrol/RunningPrograms/DEMOV3/")]
    public void VirtualControl_UsesTheRoomsUserFolder(string applicationRoot)
    {
        ProcessorCertificate.GetPath(applicationRoot)
            .Should().Be($"/opt/crestron/virtualcontrol/RunningPrograms/DEMOV3{Sep}user{Sep}selfCres.pfx");
    }
}
