using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core;
using PepperDash.Core.Web.RequestHandlers;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
  /// <summary>
  /// Handles HTTP requests to retrieve routing devices and tielines information
  /// </summary>
  public class GetRoutingDevicesAndTieLinesHandler : WebApiBaseRequestHandler
  {
    public GetRoutingDevicesAndTieLinesHandler() : base(true) { }

    protected override void HandleGet(HttpCwsContext context)
    {
      var devices = new List<RoutingDeviceInfo>();

      // Get all devices from DeviceManager
      foreach (var device in DeviceManager.AllDevices)
      {
        var deviceInfo = new RoutingDeviceInfo
        {
          Key = device.Key,
          Name = (device as IKeyName)?.Name ?? device.Key
        };

        // Check if device implements IRoutingInputs
        if (device is IRoutingInputs inputDevice)
        {
          deviceInfo.HasInputs = true;
          deviceInfo.InputPorts = inputDevice.InputPorts.Select(p => new PortInfo
          {
            Key = p.Key,
            SignalType = p.Type.ToString(),
            ConnectionType = p.ConnectionType.ToString(),
            IsInternal = p.IsInternal
          }).ToList();
        }

        // Check if device implements IRoutingOutputs
        if (device is IRoutingOutputs outputDevice)
        {
          deviceInfo.HasOutputs = true;
          deviceInfo.OutputPorts = outputDevice.OutputPorts.Select(p => new PortInfo
          {
            Key = p.Key,
            SignalType = p.Type.ToString(),
            ConnectionType = p.ConnectionType.ToString(),
            IsInternal = p.IsInternal
          }).ToList();
        }

        // Check if device implements IRoutingInputsOutputs
        if (device is IRoutingInputsOutputs)
        {
          deviceInfo.HasInputsAndOutputs = true;
        }

        // Only include devices that have routing capabilities
        if (deviceInfo.HasInputs || deviceInfo.HasOutputs)
        {
          devices.Add(deviceInfo);
        }
      }

      // Get all tielines
      var tielines = TieLineCollection.Default.Select(tl => new TieLineInfo
      {
        SourceDeviceKey = tl.SourcePort.ParentDevice.Key,
        SourcePortKey = tl.SourcePort.Key,
        DestinationDeviceKey = tl.DestinationPort.ParentDevice.Key,
        DestinationPortKey = tl.DestinationPort.Key,
        SignalType = tl.Type.ToString(),
        IsInternal = tl.IsInternal
      }).ToList();

      var response = new RoutingSystemInfo
      {
        Devices = devices,
        TieLines = tielines
      };

      var jsonResponse = JsonConvert.SerializeObject(response, Formatting.Indented);

      context.Response.StatusCode = 200;
      context.Response.StatusDescription = "OK";
      context.Response.ContentType = "application/json";
      context.Response.ContentEncoding = Encoding.UTF8;
      context.Response.Write(jsonResponse, false);
      context.Response.End();
    }
  }

  /// <summary>
  /// Represents the complete routing system information including devices and tielines
  /// </summary>
  public class RoutingSystemInfo
  {
    [JsonProperty("devices")]
    public List<RoutingDeviceInfo> Devices { get; set; }

    [JsonProperty("tieLines")]
    public List<TieLineInfo> TieLines { get; set; }
  }

  /// <summary>
  /// Represents a routing device with its ports information
  /// </summary>
  public class RoutingDeviceInfo
  {
    [JsonProperty("key")]
    public string Key { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("hasInputs")]
    public bool HasInputs { get; set; }

    [JsonProperty("hasOutputs")]
    public bool HasOutputs { get; set; }

    [JsonProperty("hasInputsAndOutputs")]
    public bool HasInputsAndOutputs { get; set; }

    [JsonProperty("inputPorts", NullValueHandling = NullValueHandling.Ignore)]
    public List<PortInfo> InputPorts { get; set; }

    [JsonProperty("outputPorts", NullValueHandling = NullValueHandling.Ignore)]
    public List<PortInfo> OutputPorts { get; set; }
  }

  /// <summary>
  /// Represents a routing port with its properties
  /// </summary>
  public class PortInfo
  {
    [JsonProperty("key")]
    public string Key { get; set; }

    [JsonProperty("signalType")]
    public string SignalType { get; set; }

    [JsonProperty("connectionType")]
    public string ConnectionType { get; set; }

    [JsonProperty("isInternal")]
    public bool IsInternal { get; set; }
  }

  /// <summary>
  /// Represents a tieline connection between two ports
  /// </summary>
  public class TieLineInfo
  {
    [JsonProperty("sourceDeviceKey")]
    public string SourceDeviceKey { get; set; }

    [JsonProperty("sourcePortKey")]
    public string SourcePortKey { get; set; }

    [JsonProperty("destinationDeviceKey")]
    public string DestinationDeviceKey { get; set; }

    [JsonProperty("destinationPortKey")]
    public string DestinationPortKey { get; set; }

    [JsonProperty("signalType")]
    public string SignalType { get; set; }

    [JsonProperty("isInternal")]
    public bool IsInternal { get; set; }
  }
}
