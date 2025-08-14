using System;

namespace Crestron.SimplSharp
{
  public class InitialParametersClass
  {
    public static string ApplicationDirectory { get; set; } = "/User/";
    public static string ProgramIDTag { get; set; } = "MockProgram";
    public static string ApplicationName { get; set; } = "MockApplication";
    public static string FirmwareVersion { get; set; } = "1.0.0.0";
    public static uint ProgramNumber { get; set; } = 1;
    public static eDevicePlatform DevicePlatform { get; set; } = eDevicePlatform.Appliance;
    public static eCrestronSeries ControllerSeries { get; set; } = eCrestronSeries.FourSeries;
  }

  public enum eDevicePlatform
  {
    Appliance = 0,
    Server = 1,
    ControlSystem = 2
  }

  public enum eCrestronSeries
  {
    TwoSeries = 2,
    ThreeSeries = 3,
    FourSeries = 4
  }

  public enum eRuntimeEnvironment
  {
    SimplSharpPro = 0,
    SimplSharp = 1
  }
}
