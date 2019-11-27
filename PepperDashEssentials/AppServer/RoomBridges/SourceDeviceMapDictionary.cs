using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Room.MobileControl
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

                {"num0", 130},
                {"num1", 131},
                {"num2", 132},
                {"num3", 133},
                {"num4", 134},
                {"num5", 135},
                {"num6", 136},
                {"num7", 137},
                {"num8", 138},
                {"num9", 139},
                {"numDash", 140},
                {"numEnter", 141},
                {"chanUp", 142},
                {"chanDown", 143},
                {"lastChan", 144},
                {"exit", 145},
                {"powerToggle", 146},
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
                {"up", 159},
                {"down", 160},
                {"left", 161},
                {"right", 162},
                {"settings", 163},
                {"info", 164},
                {"return", 165},
                {"guide", 166},
                {"reboot", 167},
                {"dvrList", 168},
                {"replay", 169},           
                {"play", 170},
                {"select", 171},
                {"record", 172},
                {"menu", 173},
                {"topMenu", 174},
                {"prevTrack", 175},
                {"nextTrack", 176},
                {"powerOn", 177},
                {"powerOff", 178},
                {"dot", 179}

            };

            foreach (var item in dictionary)
            {
                this.Add(item.Key, item.Value);
            }
        }
    }
}