using System;
using System.Threading;

namespace Crestron.SimplSharp
{
  public delegate void ProgramStatusEventHandler(eProgramStatusEventType eventType);

  public class InitialParametersClass
  {
    public static string ApplicationDirectory { get; set; } = "/User/";
    public static string ProgramIDTag { get; set; } = "MockProgram";
    public static string ApplicationName { get; set; } = "MockApplication";
    public static string FirmwareVersion { get; set; } = "1.0.0.0";
    public static uint ProgramNumber { get; set; } = 1;
    public static eDevicePlatform DevicePlatform { get; set; } = eDevicePlatform.Appliance;
    public static eCrestronSeries ControllerSeries { get; set; } = eCrestronSeries.FourSeries;

    // Additional properties needed by PepperDash.Core
    public static string RoomId { get; set; } = "Room001";
    public static string RoomName { get; set; } = "Conference Room";
    public static uint ApplicationNumber { get; set; } = 1;
    public static string ControllerPromptName { get; set; } = "TestController";
    public static string ProgramDirectory { get; set; } = "/User/";
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
    FourSeries = 4,
    // Alias names used in some contexts
    Series2 = 2,
    Series3 = 3,
    Series4 = 4
  }

  public enum eRuntimeEnvironment
  {
    SimplSharpPro = 0,
    SimplSharp = 1
  }

  public enum eProgramCompatibility
  {
    Series3And4 = 0,
    Series3Only = 1,
    Series4Only = 2
  }

  public static class Timeout
  {
    public const int Infinite = -1;
  }
}
