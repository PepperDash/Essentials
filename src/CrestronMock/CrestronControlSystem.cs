using System;
using System.Collections.Generic;

namespace CrestronMock;

/// <summary>Mock Crestron signal base class</summary>
public class Sig { }

/// <summary>Mock UShort input signal</summary>
public class UShortInputSig { }

/// <summary>Mock UShort output signal</summary>
public class UShortOutputSig { }

/// <summary>Mock Boolean input signal</summary>
public class BoolInputSig { }

/// <summary>Mock String input signal</summary>
public class StringInputSig { }

/// <summary>Mock Boolean output signal</summary>
public class BoolOutputSig { }

/// <summary>Mock String output signal</summary>
public class StringOutputSig { }

/// <summary>Mock signal group</summary>
public class SigGroup { }

/// <summary>Mock COM port</summary>
public class ComPort { }

/// <summary>Mock relay</summary>
public class Relay { }

/// <summary>Mock IR output port</summary>
public class IROutputPort { }

/// <summary>Mock IO port</summary>
public class IOPort { }

/// <summary>Mock VersiPort</summary>
public class VersiPort { }

/// <summary>Mock IR input port</summary>
public class IRInputPort { }

/// <summary>Signal type enumeration</summary>
public enum eSigType
{
  Bool,
  UShort,
  String
}

/// <summary>Mock read-only collection</summary>
public class ReadOnlyCollection<TKey, TValue> : Dictionary<TKey, TValue> where TKey : notnull
{
}

/// <summary>Mock COM ports interface</summary>
public interface IComPorts
{
  ComPort[] ComPorts { get; }
}

/// <summary>Mock relay ports interface</summary>
public interface IRelayPorts
{
  Relay[] RelayPorts { get; }
}

/// <summary>Mock IR output ports interface</summary>
public interface IIROutputPorts
{
  IROutputPort[] IROutputPorts { get; }
}

/// <summary>Mock IO ports interface</summary>
public interface IIOPorts
{
  IOPort[] IOPorts { get; }
}

/// <summary>Mock digital input ports interface</summary>
public interface IDigitalInputPorts
{
  VersiPort[] DigitalInputPorts { get; }
}

/// <summary>Mock IR input port interface</summary>
public interface IIRInputPort
{
  IRInputPort IRInputPort { get; }
}

/// <summary>
/// Mock implementation of CrestronControlSystem for testing purposes
/// Base class for the CrestronControlSystem The Customer application is derived over this class
/// </summary>
public class CrestronControlSystem : IComPorts, IRelayPorts, IIROutputPorts, IIOPorts, IDigitalInputPorts, IIRInputPort
{
  // Static fields
  public static Sig NullCue { get; set; } = new Sig();
  public static UShortInputSig NullUShortInputSig { get; set; } = new UShortInputSig();
  public static UShortOutputSig NullUShortOutputSig { get; set; } = new UShortOutputSig();
  public static BoolInputSig NullBoolInputSig { get; set; } = new BoolInputSig();
  public static StringInputSig NullStringInputSig { get; set; } = new StringInputSig();
  public static BoolOutputSig NullBoolOutputSig { get; set; } = new BoolOutputSig();
  public static StringOutputSig NullStringOutputSig { get; set; } = new StringOutputSig();
  public static ReadOnlyCollection<int, SigGroup> SigGroups { get; set; } = new ReadOnlyCollection<int, SigGroup>();
  public static int MaxNumberOfEventsInQueue { get; set; } = 1000;

  // Constructor
  public CrestronControlSystem()
  {
    // Initialize collections and properties
    ComPorts = Array.Empty<ComPort>();
    RelayPorts = Array.Empty<Relay>();
    IROutputPorts = Array.Empty<IROutputPort>();
    IOPorts = Array.Empty<IOPort>();
    DigitalInputPorts = Array.Empty<VersiPort>();
    IRInputPort = new IRInputPort();
  }

  // Virtual methods that can be overridden
  public virtual void InitializeSystem()
  {
    // Override in derived classes
  }

  public virtual void SavePreset()
  {
    // Override in derived classes
  }

  public virtual void RecallPreset()
  {
    // Override in derived classes
  }

  public virtual void BassFlat()
  {
    // Override in derived classes
  }

  public virtual void TrebleFlat()
  {
    // Override in derived classes
  }

  public virtual void LimiterEnable()
  {
    // Override in derived classes
  }

  public virtual void LimiterDisable()
  {
    // Override in derived classes
  }

  public virtual void LimiterSoftKneeOn()
  {
    // Override in derived classes
  }

  public virtual void LimiterSoftKneeOff()
  {
    // Override in derived classes
  }

  public virtual void MasterMuteOn()
  {
    // Override in derived classes
  }

  public virtual void MasterMuteOff()
  {
    // Override in derived classes
  }

  public virtual void MicMasterMuteOn()
  {
    // Override in derived classes
  }

  public virtual void MicMasterMuteOff()
  {
    // Override in derived classes
  }

  public virtual void SourceMuteOn()
  {
    // Override in derived classes
  }

  public virtual void SourceMuteOff()
  {
    // Override in derived classes
  }

  // Non-virtual methods
  public void MicMuteOn(uint MicNumber)
  {
    // Implementation
  }

  public void MicMuteOff(uint MicNumber)
  {
    // Implementation
  }

  public void MonoOutput()
  {
    // Implementation
  }

  public void StereoOutput()
  {
    // Implementation
  }

  // Static methods for SigGroup management
  public static SigGroup CreateSigGroup(int groupID, params BoolInputSig[] boolInputSigs)
  {
    return new SigGroup();
  }

  public static SigGroup CreateSigGroup(int groupID, params UShortInputSig[] ushortInputSigs)
  {
    return new SigGroup();
  }

  public static SigGroup CreateSigGroup(int groupID, eSigType type)
  {
    return new SigGroup();
  }

  public static SigGroup CreateSigGroup(int groupID, params StringInputSig[] stringInputSigs)
  {
    return new SigGroup();
  }

  public static void RemoveSigGroup(int groupID)
  {
    // Implementation
  }

  public static void RemoveSigGroup(SigGroup sigGroupToRemove)
  {
    // Implementation
  }

  public static void ClearSigGroups()
  {
    // Implementation
  }

  // Interface implementations
  public ComPort[] ComPorts { get; set; }
  public Relay[] RelayPorts { get; set; }
  public IROutputPort[] IROutputPorts { get; set; }
  public IOPort[] IOPorts { get; set; }
  public VersiPort[] DigitalInputPorts { get; set; }
  public IRInputPort IRInputPort { get; set; }
}
