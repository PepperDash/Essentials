using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;

namespace PepperDash.Essentials
{

    //public class TPConfig : DeviceConfig
    //{
    //    new public TPConfigProperties Properties { get; set; }
    //}

    //public class TPConfigProperties
    //{
    //    /* 
    //    "properties": {
    //      "ipId": "aa",
    //      "defaultSystemKey": "system1",
    //      "sgdPath": "\\NVRAM\\Program1\\Sgds\\PepperDash Essentials TSW1050_v0.9.sgd",
    //      "usesSplashPage": true,
    //      "showDate": true,
    //      "showTime": false 
    //    }
    //    */
    //    public uint IpId { get; set; }
    //    public string deafultSystemKey { get; set; }
    //    public string SgdPath { get; set; }
    //    public bool UsesSplashPage { get; set; }
    //    public bool ShowDate { get; set; }
    //    public bool ShowTime { get; set; }

    //}




    ///// <summary>
    ///// The gist of this converter: The comspec JSON comes in with normal values that need to be converted
    ///// into enum names.  This converter takes the value and applies the appropriate enum's name prefix to the value
    ///// and then returns the enum value using Enum.Parse
    ///// </summary>
    //public class TPPropertiesConverter : JsonConverter
    //{
    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        return JObject.Load(reader);
    //    }

    //    /// <summary>
    //    /// This will be hit with every value in the ComPortConfig class.  We only need to
    //    /// do custom conversion on the comspec items.
    //    /// </summary>
    //    public override bool CanConvert(Type objectType) 
    //    {
    //        return true;
    //    }

    //    public override bool CanRead { get { return true; } }
    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}		
}