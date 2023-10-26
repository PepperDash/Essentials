using Crestron.SimplSharpPro;

namespace PepperDash.Essentials.Devices.Common
{
    public static class AppleTvIrCommands
    {
        
        public static string Up = "+";
        public static string Down = "-";
        public static string Left = IROutputStandardCommands.IROut_TRACK_MINUS;
        public static string Right = IROutputStandardCommands.IROut_TRACK_PLUS;
        public static string Enter = IROutputStandardCommands.IROut_ENTER;
        public static string PlayPause = "PLAY/PAUSE";
        public static string Rewind = "REWIND";
        public static string Menu = "Menu";
        public static string FastForward = "FASTFORWARD";
    }
}