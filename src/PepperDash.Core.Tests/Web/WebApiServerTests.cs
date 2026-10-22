using FluentAssertions;
using PepperDash.Core.Web;
using Xunit;

namespace PepperDash.Core.Tests.Web;

/// <summary>
/// Tests for <see cref="WebApiServer.IsStreamPoll"/>, which keeps the stream poll routes out of the
/// request log so a debug poll doesn't wake itself.
/// </summary>
public class WebApiServerTests
{
    [Theory]
    [InlineData("/app01/api/debugSession/messages")]
    [InlineData("/app10/api/routingFeedbackSession/messages/")]
    [InlineData("/api/debugsession/MESSAGES")]
    public void IsStreamPoll_MatchesThePollRoutes(string path)
    {
        WebApiServer.IsStreamPoll(path).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("/app01/api/debugSession")]
    [InlineData("/app01/api/routingFeedbackSession")]
    [InlineData("/app01/api/versions")]
    public void IsStreamPoll_LeavesOtherRoutesLogged(string? path)
    {
        WebApiServer.IsStreamPoll(path!).Should().BeFalse();
    }
}
