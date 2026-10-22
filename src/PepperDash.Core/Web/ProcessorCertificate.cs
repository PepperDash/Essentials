using System.IO;

namespace PepperDash.Core.Web;

/// <summary>
/// The self-signed certificate Essentials' secure servers share: the debug console and routing feedback
/// WebSockets, and the Mobile Control direct server when it runs secure.
/// </summary>
/// <remarks>
/// The certificate lives in the program's <c>user</c> folder under the application root, the same place
/// as the debug settings. On a 4-series appliance the application root is empty, so this is
/// <c>/user/selfCres.pfx</c> as it always has been. On Virtual Control (VC-4) it is the room folder, so
/// this is <c>{room}/user/selfCres.pfx</c>, where VC-4 links <c>user</c> to the room's <c>User</c> folder.
/// A hard-coded <c>/user/selfCres.pfx</c> does not exist on VC-4, and the room can't create it, so none of
/// those servers could start there.
/// </remarks>
public static class ProcessorCertificate
{
    /// <summary>The certificate file's name, without the extension.</summary>
    public const string Name = "selfCres";

    /// <summary>The PFX password.</summary>
    public const string Password = "cres12345";

    /// <summary>
    /// The certificate's path for a given application root, e.g. <c>""</c> on an appliance or
    /// <c>/opt/crestron/virtualcontrol/RunningPrograms/DEMOV3</c> on VC-4.
    /// </summary>
    public static string GetPath(string applicationRoot)
    {
        var root = (applicationRoot ?? string.Empty).TrimEnd('/', '\\');
        var separator = Path.DirectorySeparatorChar;
        return $"{root}{separator}user{separator}{Name}.pfx";
    }

    /// <summary>The certificate's path for the running program.</summary>
    public static string FilePath =>
        GetPath(Crestron.SimplSharp.CrestronIO.Directory.GetApplicationRootDirectory());
}
