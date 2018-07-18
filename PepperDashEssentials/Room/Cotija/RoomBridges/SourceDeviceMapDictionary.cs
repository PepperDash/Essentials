using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Room.Cotija
{

    /// <summary>
    /// Contains all of the default joins that map to API funtions
    /// </summary>
    public class SourceDeviceMapDictionary : Dictionary<string, uint>
    {

        public SourceDeviceMapDictionary(): base()
        {
            var dictionary = new Dictionary<string, uint>
            {
                {"preset01", 101},
                {"preset02", 102},
                {"preset03", 103},
                {"preset04", 104},
                {"preset05", 105},
                {"preset06", 106},
                {"preset07", 107},
                {"preset08", 108},
                {"preset09", 109},
                {"preset10", 110},
                {"preset11", 111},
                {"preset12", 112},
                {"preset13", 113},
                {"preset14", 114},
                {"preset15", 115},
                {"preset16", 116},
                {"preset17", 117},
                {"preset18", 118},
                {"preset19", 119},
                {"preset20", 120},
                {"preset21", 121},
                {"preset22", 122},
                {"preset23", 123},
                {"preset24", 124},

                {"key0", 130},
                {"key1", 131},
                {"key2", 132},
                {"key3", 133},
                {"key4", 134},
                {"key5", 135},
                {"key6", 136},
                {"key7", 137},
                {"key8", 138},
                {"key9", 139},
                {"keyDash", 140},
                {"keyEnter", 141},
                {"channelUp", 142},
                {"channelDown", 143},
                {"channelLast", 144},
                {"exit", 145},
                {"power", 146},
                {"red", 147},
                {"green", 148},
                {"yellow", 149},
                {"blue", 150},
                {"video", 151},
                {"previous", 152},
                {"next", 153},
                {"rewind", 154},
                {"ffwd", 155},
                {"closedCaption", 156},
                {"stop", 157},
                {"pause", 158},
                {"cursorUp", 159},
                {"cursorDown", 160},
                {"cursorLeft", 161},
                {"cursorRight", 162},
                {"settings", 163},
                {"info", 164},
                {"return", 165},
                {"guide", 166},
                {"reboot", 167},
             
                {"play", 170},
                {"cursorOk", 171},
                {"record", 172},
                {"menu", 173},
                {"topMenu", 174}
            };

            
        }
    }
}