using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.CiscoCodec
{
    public class xStatusSparkPlus
    {
        public class ConnectionStatus
        {
            public string Value { get; set; }
        }

        public class EcReferenceDelay
        {
            public string Value { get; set; }
        }

        public class Microphone
        {
            public string id { get; set; }
            public ConnectionStatus ConnectionStatus { get; set; }
            public EcReferenceDelay EcReferenceDelay { get; set; }
        }

        public class Connectors
        {
            public List<Microphone> Microphone { get; set; }
        }

        public class Input
        {
            public Connectors Connectors { get; set; }
        }

        public class Mute
        {
            public string Value { get; set; }
        }

        public class Microphones
        {
            public Mute Mute { get; set; }
        }

        public class ConnectionStatus2
        {
            public string Value { get; set; }
        }

        public class DelayMs
        {
            public string Value { get; set; }
        }

        public class Line
        {
            public string id { get; set; }
            public ConnectionStatus2 ConnectionStatus { get; set; }
            public DelayMs DelayMs { get; set; }
        }

        public class Connectors2
        {
            public List<Line> Line { get; set; }
        }

        public class Output
        {
            public Connectors2 Connectors { get; set; }
        }

        public class Volume
        {
            public string Value { get; set; }
        }

        public class VolumeMute
        {
            public string Value { get; set; }
        }

        public class Audio
        {
            public Input Input { get; set; }
            public Microphones Microphones { get; set; }
            public Output Output { get; set; }
            public Volume Volume { get; set; }
            public VolumeMute VolumeMute { get; set; }
        }

        public class Id
        {
            public string Value { get; set; }
        }

        public class Current
        {
            public Id Id { get; set; }
        }

        public class Bookings
        {
            public Current Current { get; set; }
        }

        public class Options
        {
            public string Value { get; set; }
        }

        public class Capabilities
        {
            public Options Options { get; set; }
        }

        public class Connected
        {
            public string Value { get; set; }
        }

        public class Flip
        {
            public string Value { get; set; }
        }

        public class HardwareID
        {
            public string Value { get; set; }
        }

        public class MacAddress
        {
            public string Value { get; set; }
        }

        public class Manufacturer
        {
            public string Value { get; set; }
        }

        public class Model
        {
            public string Value { get; set; }
        }

        public class Pan
        {
            public string Value { get; set; }
        }

        public class Tilt
        {
            public string Value { get; set; }
        }

        public class Zoom
        {
            public string Value { get; set; }
        }

        public class Position
        {
            public Pan Pan { get; set; }
            public Tilt Tilt { get; set; }
            public Zoom Zoom { get; set; }
        }

        public class SerialNumber
        {
            public string Value { get; set; }
        }

        public class SoftwareID
        {
            public string Value { get; set; }
        }

        public class Camera
        {
            public string id { get; set; }
            public Capabilities Capabilities { get; set; }
            public Connected Connected { get; set; }
            public Flip Flip { get; set; }
            public HardwareID HardwareID { get; set; }
            public MacAddress MacAddress { get; set; }
            public Manufacturer Manufacturer { get; set; }
            public Model Model { get; set; }
            public Position Position { get; set; }
            public SerialNumber SerialNumber { get; set; }
            public SoftwareID SoftwareID { get; set; }
        }

        public class Availability
        {
            public string Value { get; set; }
        }

        public class Status2
        {
            public string Value { get; set; }
        }

        public class SpeakerTrack
        {
            public Availability Availability { get; set; }
            public Status2 Status { get; set; }
        }

        public class Cameras
        {
            public List<Camera> Camera { get; set; }
            public SpeakerTrack SpeakerTrack { get; set; }
        }

        public class MaxActiveCalls
        {
            public string Value { get; set; }
        }

        public class MaxAudioCalls
        {
            public string Value { get; set; }
        }

        public class MaxCalls
        {
            public string Value { get; set; }
        }

        public class MaxVideoCalls
        {
            public string Value { get; set; }
        }

        public class Conference
        {
            public MaxActiveCalls MaxActiveCalls { get; set; }
            public MaxAudioCalls MaxAudioCalls { get; set; }
            public MaxCalls MaxCalls { get; set; }
            public MaxVideoCalls MaxVideoCalls { get; set; }
        }

        public class Capabilities2
        {
            public Conference Conference { get; set; }
        }

        public class CallId
        {
            public string Value { get; set; }
        }

        public class ActiveSpeaker
        {
            public CallId CallId { get; set; }
        }

        public class DoNotDisturb
        {
            public string Value { get; set; }
        }

        public class Mode
        {
            public string Value { get; set; }
        }

        public class Line2
        {
            public string id { get; set; }
            public Mode Mode { get; set; }
        }

        public class Mode2
        {
            public string Value { get; set; }
        }

        public class Multipoint
        {
            public Mode2 Mode { get; set; }
        }

        public class CallId2
        {
            public string Value { get; set; }
        }

        public class SendingMode
        {
            public string Value { get; set; }
        }

        public class Source
        {
            public string Value { get; set; }
        }

        public class LocalInstance
        {
            public string id { get; set; }
            public SendingMode SendingMode { get; set; }
            public Source Source { get; set; }
        }

        public class Mode3
        {
            public string Value { get; set; }
        }

        public class Mode4
        {
            public string Value { get; set; }
        }

        public class ReleaseFloorAvailability
        {
            public string Value { get; set; }
        }

        public class RequestFloorAvailability
        {
            public string Value { get; set; }
        }

        public class Whiteboard
        {
            public Mode4 Mode { get; set; }
            public ReleaseFloorAvailability ReleaseFloorAvailability { get; set; }
            public RequestFloorAvailability RequestFloorAvailability { get; set; }
        }

        public class Presentation
        {
            public CallId2 CallId { get; set; }
            public List<LocalInstance> LocalInstance { get; set; }
            public Mode3 Mode { get; set; }
            public Whiteboard Whiteboard { get; set; }
        }

        public class CallId3
        {
            public string Value { get; set; }
        }

        public class Mode5
        {
            public string Value { get; set; }
        }

        public class SpeakerLock
        {
            public CallId3 CallId { get; set; }
            public Mode5 Mode { get; set; }
        }

        public class Conference2
        {
            public ActiveSpeaker ActiveSpeaker { get; set; }
            public DoNotDisturb DoNotDisturb { get; set; }
            public List<Line2> Line { get; set; }
            public Multipoint Multipoint { get; set; }
            public Presentation Presentation { get; set; }
            public SpeakerLock SpeakerLock { get; set; }
        }

        public class Conference3
        {
        }

        public class Experimental
        {
            public Conference3 Conference { get; set; }
        }

        public class Address
        {
            public string Value { get; set; }
        }

        public class Port
        {
            public string Value { get; set; }
        }

        public class Reason
        {
            public string Value { get; set; }
        }

        public class Status3
        {
            public string Value { get; set; }
        }

        public class Gatekeeper
        {
            public Address Address { get; set; }
            public Port Port { get; set; }
            public Reason Reason { get; set; }
            public Status3 Status { get; set; }
        }

        public class Reason2
        {
            public string Value { get; set; }
        }

        public class Status4
        {
            public string Value { get; set; }
        }

        public class Mode6
        {
            public Reason2 Reason { get; set; }
            public Status4 Status { get; set; }
        }

        public class H323
        {
            public Gatekeeper Gatekeeper { get; set; }
            public Mode6 Mode { get; set; }
        }

        public class Expression
        {
            public string id { get; set; }
            public string Value { get; set; }
        }

        public class Format
        {
            public string Value { get; set; }
        }

        public class URL
        {
            public string Value { get; set; }
        }

        public class HttpFeedback
        {
            public string id { get; set; }
            public List<Expression> Expression { get; set; }
            public Format Format { get; set; }
            public URL URL { get; set; }
        }

        public class MediaChannels
        {
        }

        public class Address2
        {
            public string Value { get; set; }
        }

        public class Capabilities3
        {
            public string Value { get; set; }
        }

        public class DeviceId
        {
            public string Value { get; set; }
        }

        public class Duplex
        {
            public string Value { get; set; }
        }

        public class Platform
        {
            public string Value { get; set; }
        }

        public class PortID
        {
            public string Value { get; set; }
        }

        public class PrimaryMgmtAddress
        {
            public string Value { get; set; }
        }

        public class SysName
        {
            public string Value { get; set; }
        }

        public class SysObjectID
        {
            public string Value { get; set; }
        }

        public class VTPMgmtDomain
        {
            public string Value { get; set; }
        }

        public class Version
        {
            public string Value { get; set; }
        }

        public class VoIPApplianceVlanID
        {
            public string Value { get; set; }
        }

        public class CDP
        {
            public Address2 Address { get; set; }
            public Capabilities3 Capabilities { get; set; }
            public DeviceId DeviceId { get; set; }
            public Duplex Duplex { get; set; }
            public Platform Platform { get; set; }
            public PortID PortID { get; set; }
            public PrimaryMgmtAddress PrimaryMgmtAddress { get; set; }
            public SysName SysName { get; set; }
            public SysObjectID SysObjectID { get; set; }
            public VTPMgmtDomain VTPMgmtDomain { get; set; }
            public Version Version { get; set; }
            public VoIPApplianceVlanID VoIPApplianceVlanID { get; set; }
        }

        public class Name
        {
            public string Value { get; set; }
        }

        public class Domain
        {
            public Name Name { get; set; }
        }

        public class Address3
        {
            public string Value { get; set; }
        }

        public class Server
        {
            public string id { get; set; }
            public Address3 Address { get; set; }
        }

        public class DNS
        {
            public Domain Domain { get; set; }
            public List<Server> Server { get; set; }
        }

        public class MacAddress2
        {
            public string Value { get; set; }
        }

        public class Speed
        {
            public string Value { get; set; }
        }

        public class Ethernet
        {
            public MacAddress2 MacAddress { get; set; }
            public Speed Speed { get; set; }
        }

        public class Address4
        {
            public string Value { get; set; }
        }

        public class Gateway
        {
            public string Value { get; set; }
        }

        public class SubnetMask
        {
            public string Value { get; set; }
        }

        public class IPv4
        {
            public Address4 Address { get; set; }
            public Gateway Gateway { get; set; }
            public SubnetMask SubnetMask { get; set; }
        }

        public class Address5
        {
            public string Value { get; set; }
        }

        public class Gateway2
        {
            public string Value { get; set; }
        }

        public class IPv6
        {
            public Address5 Address { get; set; }
            public Gateway2 Gateway { get; set; }
        }

        public class VlanId
        {
            public string Value { get; set; }
        }

        public class Voice
        {
            public VlanId VlanId { get; set; }
        }

        public class VLAN
        {
            public Voice Voice { get; set; }
        }

        public class Network
        {
            public string id { get; set; }
            public CDP CDP { get; set; }
            public DNS DNS { get; set; }
            public Ethernet Ethernet { get; set; }
            public IPv4 IPv4 { get; set; }
            public IPv6 IPv6 { get; set; }
            public VLAN VLAN { get; set; }
        }

        public class CurrentAddress
        {
            public string Value { get; set; }
        }

        public class Address6
        {
            public string Value { get; set; }
        }

        public class Server2
        {
            public string id { get; set; }
            public Address6 Address { get; set; }
        }

        public class Status5
        {
            public string Value { get; set; }
        }

        public class NTP
        {
            public CurrentAddress CurrentAddress { get; set; }
            public List<Server2> Server { get; set; }
            public Status5 Status { get; set; }
        }

        public class NetworkServices
        {
            public NTP NTP { get; set; }
        }

        public class HardwareInfo
        {
            public string Value { get; set; }
        }

        public class ID2
        {
            public string Value { get; set; }
        }

        public class Name2
        {
            public string Value { get; set; }
        }

        public class SoftwareInfo
        {
            public string Value { get; set; }
        }

        public class Status6
        {
            public string Value { get; set; }
        }

        public class Type
        {
            public string Value { get; set; }
        }

        public class UpgradeStatus
        {
            public string Value { get; set; }
        }

        public class ConnectedDevice
        {
            public string id { get; set; }
            public HardwareInfo HardwareInfo { get; set; }
            public ID2 ID { get; set; }
            public Name2 Name { get; set; }
            public SoftwareInfo SoftwareInfo { get; set; }
            public Status6 Status { get; set; }
            public Type Type { get; set; }
            public UpgradeStatus UpgradeStatus { get; set; }
        }

        public class Peripherals
        {
            public List<ConnectedDevice> ConnectedDevice { get; set; }
        }

        public class Enabled
        {
            public string Value { get; set; }
        }

        public class LastLoggedInUserId
        {
            public string Value { get; set; }
        }

        public class LoggedIn
        {
            public string Value { get; set; }
        }

        public class ExtensionMobility
        {
            public Enabled Enabled { get; set; }
            public LastLoggedInUserId LastLoggedInUserId { get; set; }
            public LoggedIn LoggedIn { get; set; }
        }

        public class CUCM
        {
            public ExtensionMobility ExtensionMobility { get; set; }
        }

        public class CompletedAt
        {
            public string Value { get; set; }
        }

        public class URL2
        {
            public string Value { get; set; }
        }

        public class VersionId
        {
            public string Value { get; set; }
        }

        public class Current2
        {
            public CompletedAt CompletedAt { get; set; }
            public URL2 URL { get; set; }
            public VersionId VersionId { get; set; }
        }

        public class LastChange
        {
            public string Value { get; set; }
        }

        public class Message
        {
            public string Value { get; set; }
        }

        public class Phase
        {
            public string Value { get; set; }
        }

        public class SessionId
        {
            public string Value { get; set; }
        }

        public class Status7
        {
            public string Value { get; set; }
        }

        public class URL3
        {
            public string Value { get; set; }
        }

        public class VersionId2
        {
            public string Value { get; set; }
        }

        public class UpgradeStatus2
        {
            public LastChange LastChange { get; set; }
            public Message Message { get; set; }
            public Phase Phase { get; set; }
            public SessionId SessionId { get; set; }
            public Status7 Status { get; set; }
            public URL3 URL { get; set; }
            public VersionId2 VersionId { get; set; }
        }

        public class Software
        {
            public Current2 Current { get; set; }
            public UpgradeStatus2 UpgradeStatus { get; set; }
        }

        public class Status8
        {
            public string Value { get; set; }
        }

        public class Provisioning
        {
            public CUCM CUCM { get; set; }
            public Software Software { get; set; }
            public Status8 Status { get; set; }
        }

        public class Availability2
        {
            public string Value { get; set; }
        }

        public class Services
        {
            public Availability2 Availability { get; set; }
        }

        public class Proximity
        {
            public Services Services { get; set; }
        }

        public class Current3
        {
            public string Value { get; set; }
        }

        public class PeopleCount
        {
            public Current3 Current { get; set; }
        }

        public class PeoplePresence
        {
            public string Value { get; set; }
        }

        public class RoomAnalytics
        {
            public PeopleCount PeopleCount { get; set; }
            public PeoplePresence PeoplePresence { get; set; }
        }

        public class URI
        {
            public string Value { get; set; }
        }

        public class Primary
        {
            public URI URI { get; set; }
        }

        public class AlternateURI
        {
            public Primary Primary { get; set; }
        }

        public class Authentication
        {
            public string Value { get; set; }
        }

        public class DisplayName
        {
            public string Value { get; set; }
        }

        public class Mode7
        {
            public string Value { get; set; }
        }

        public class URI2
        {
            public string Value { get; set; }
        }

        public class CallForward
        {
            public DisplayName DisplayName { get; set; }
            public Mode7 Mode { get; set; }
            public URI2 URI { get; set; }
        }

        public class MessagesWaiting
        {
            public string Value { get; set; }
        }

        public class URI3
        {
            public string Value { get; set; }
        }

        public class Mailbox
        {
            public MessagesWaiting MessagesWaiting { get; set; }
            public URI3 URI { get; set; }
        }

        public class Address7
        {
            public string Value { get; set; }
        }

        public class Status9
        {
            public string Value { get; set; }
        }

        public class Proxy
        {
            public string id { get; set; }
            public Address7 Address { get; set; }
            public Status9 Status { get; set; }
        }

        public class Reason3
        {
            public string Value { get; set; }
        }

        public class Status10
        {
            public string Value { get; set; }
        }

        public class URI4
        {
            public string Value { get; set; }
        }

        public class Registration
        {
            public string id { get; set; }
            public Reason3 Reason { get; set; }
            public Status10 Status { get; set; }
            public URI4 URI { get; set; }
        }

        public class Secure
        {
            public string Value { get; set; }
        }

        public class Verified
        {
            public string Value { get; set; }
        }

        public class SIP
        {
            public AlternateURI AlternateURI { get; set; }
            public Authentication Authentication { get; set; }
            public CallForward CallForward { get; set; }
            public Mailbox Mailbox { get; set; }
            public List<Proxy> Proxy { get; set; }
            public List<Registration> Registration { get; set; }
            public Secure Secure { get; set; }
            public Verified Verified { get; set; }
        }

        public class Mode8
        {
            public string Value { get; set; }
        }

        public class FIPS
        {
            public Mode8 Mode { get; set; }
        }

        public class CallHistory
        {
            public string Value { get; set; }
        }

        public class Configurations
        {
            public string Value { get; set; }
        }

        public class DHCP
        {
            public string Value { get; set; }
        }

        public class InternalLogging
        {
            public string Value { get; set; }
        }

        public class LocalPhonebook
        {
            public string Value { get; set; }
        }

        public class Persistency
        {
            public CallHistory CallHistory { get; set; }
            public Configurations Configurations { get; set; }
            public DHCP DHCP { get; set; }
            public InternalLogging InternalLogging { get; set; }
            public LocalPhonebook LocalPhonebook { get; set; }
        }

        public class Security
        {
            public FIPS FIPS { get; set; }
            public Persistency Persistency { get; set; }
        }

        public class State
        {
            public string Value { get; set; }
        }

        public class Standby
        {
            public State State { get; set; }
        }

        public class CompatibilityLevel
        {
            public string Value { get; set; }
        }

        public class SerialNumber2
        {
            public string Value { get; set; }
        }

        public class Module
        {
            public CompatibilityLevel CompatibilityLevel { get; set; }
            public SerialNumber2 SerialNumber { get; set; }
        }

        public class Hardware
        {
            public Module Module { get; set; }
        }

        public class ProductId
        {
            public string Value { get; set; }
        }

        public class ProductPlatform
        {
            public string Value { get; set; }
        }

        public class ProductType
        {
            public string Value { get; set; }
        }

        public class DisplayName2
        {
            public string Value { get; set; }
        }

        public class Name3
        {
            public string Value { get; set; }
        }

        public class Encryption
        {
            public string Value { get; set; }
        }

        public class MultiSite
        {
            public string Value { get; set; }
        }

        public class RemoteMonitoring
        {
            public string Value { get; set; }
        }

        public class OptionKeys
        {
            public Encryption Encryption { get; set; }
            public MultiSite MultiSite { get; set; }
            public RemoteMonitoring RemoteMonitoring { get; set; }
        }

        public class ReleaseDate
        {
            public string Value { get; set; }
        }

        public class Version2
        {
            public string Value { get; set; }
        }

        public class Software2
        {
            public DisplayName2 DisplayName { get; set; }
            public Name3 Name { get; set; }
            public OptionKeys OptionKeys { get; set; }
            public ReleaseDate ReleaseDate { get; set; }
            public Version2 Version { get; set; }
        }

        public class NumberOfActiveCalls
        {
            public string Value { get; set; }
        }

        public class NumberOfInProgressCalls
        {
            public string Value { get; set; }
        }

        public class NumberOfSuspendedCalls
        {
            public string Value { get; set; }
        }

        public class State2
        {
            public NumberOfActiveCalls NumberOfActiveCalls { get; set; }
            public NumberOfInProgressCalls NumberOfInProgressCalls { get; set; }
            public NumberOfSuspendedCalls NumberOfSuspendedCalls { get; set; }
        }

        public class Uptime
        {
            public string Value { get; set; }
        }

        public class SystemUnit
        {
            public Hardware Hardware { get; set; }
            public ProductId ProductId { get; set; }
            public ProductPlatform ProductPlatform { get; set; }
            public ProductType ProductType { get; set; }
            public Software2 Software { get; set; }
            public State2 State { get; set; }
            public Uptime Uptime { get; set; }
        }

        public class SystemTime
        {
            public DateTime Value { get; set; }
        }

        public class Time
        {
            public SystemTime SystemTime { get; set; }
        }

        public class Number
        {
            public string Value { get; set; }
        }

        public class ContactMethod
        {
            public string id { get; set; }
            public Number Number { get; set; }
        }

        public class Name4
        {
            public string Value { get; set; }
        }

        public class ContactInfo
        {
            public List<ContactMethod> ContactMethod { get; set; }
            public Name4 Name { get; set; }
        }

        public class UserInterface
        {
            public ContactInfo ContactInfo { get; set; }
        }

        public class PIPPosition
        {
            public string Value { get; set; }
        }

        public class ActiveSpeaker2
        {
            public PIPPosition PIPPosition { get; set; }
        }

        public class Connected2
        {
            public string Value { get; set; }
        }

        public class DeviceType
        {
            public string Value { get; set; }
        }

        public class Name5
        {
            public string Value { get; set; }
        }

        public class PowerStatus
        {
            public string Value { get; set; }
        }

        public class VendorId
        {
            public string Value { get; set; }
        }

        public class CEC
        {
            public string id { get; set; }
            public DeviceType DeviceType { get; set; }
            public Name5 Name { get; set; }
            public PowerStatus PowerStatus { get; set; }
            public VendorId VendorId { get; set; }
        }

        public class ConnectedDevice2
        {
            public List<CEC> CEC { get; set; }
        }

        public class SignalState
        {
            public string Value { get; set; }
        }

        public class SourceId
        {
            public string Value { get; set; }
        }

        public class Type2
        {
            public string Value { get; set; }
        }

        public class Connector
        {
            public string id { get; set; }
            public Connected2 Connected { get; set; }
            public ConnectedDevice2 ConnectedDevice { get; set; }
            public SignalState SignalState { get; set; }
            public SourceId SourceId { get; set; }
            public Type2 Type { get; set; }
        }

        public class MainVideoSource
        {
            public string Value { get; set; }
        }

        public class ConnectorId
        {
            public string Value { get; set; }
        }

        public class FormatStatus
        {
            public string Value { get; set; }
        }

        public class FormatType
        {
            public string Value { get; set; }
        }

        public class MediaChannelId
        {
            public string Value { get; set; }
        }

        public class Height
        {
            public string Value { get; set; }
        }

        public class RefreshRate
        {
            public string Value { get; set; }
        }

        public class Width
        {
            public string Value { get; set; }
        }

        public class Resolution
        {
            public Height Height { get; set; }
            public RefreshRate RefreshRate { get; set; }
            public Width Width { get; set; }
        }

        public class Source2
        {
            public string id { get; set; }
            public ConnectorId ConnectorId { get; set; }
            public FormatStatus FormatStatus { get; set; }
            public FormatType FormatType { get; set; }
            public MediaChannelId MediaChannelId { get; set; }
            public Resolution Resolution { get; set; }
        }

        public class Input2
        {
            public List<Connector> Connector { get; set; }
            public MainVideoSource MainVideoSource { get; set; }
            public List<Source2> Source { get; set; }
        }

        public class Local
        {
            public string Value { get; set; }
        }

        public class LayoutFamily
        {
            public Local Local { get; set; }
        }

        public class Layout
        {
            public LayoutFamily LayoutFamily { get; set; }
        }

        public class Monitors
        {
            public string Value { get; set; }
        }

        public class Connected3
        {
            public string Value { get; set; }
        }

        public class DeviceType2
        {
            public string Value { get; set; }
        }

        public class Name6
        {
            public string Value { get; set; }
        }

        public class PowerStatus2
        {
            public string Value { get; set; }
        }

        public class VendorId2
        {
            public string Value { get; set; }
        }

        public class CEC2
        {
            public string id { get; set; }
            public DeviceType2 DeviceType { get; set; }
            public Name6 Name { get; set; }
            public PowerStatus2 PowerStatus { get; set; }
            public VendorId2 VendorId { get; set; }
        }

        public class Name7
        {
            public string Value { get; set; }
        }

        public class PreferredFormat
        {
            public string Value { get; set; }
        }

        public class ConnectedDevice3
        {
            public List<CEC2> CEC { get; set; }
            public Name7 Name { get; set; }
            public PreferredFormat PreferredFormat { get; set; }
        }

        public class MonitorRole
        {
            public string Value { get; set; }
        }

        public class Height2
        {
            public string Value { get; set; }
        }

        public class RefreshRate2
        {
            public string Value { get; set; }
        }

        public class Width2
        {
            public string Value { get; set; }
        }

        public class Resolution2
        {
            public Height2 Height { get; set; }
            public RefreshRate2 RefreshRate { get; set; }
            public Width2 Width { get; set; }
        }

        public class Type3
        {
            public string Value { get; set; }
        }

        public class Connector2
        {
            public string id { get; set; }
            public Connected3 Connected { get; set; }
            public ConnectedDevice3 ConnectedDevice { get; set; }
            public MonitorRole MonitorRole { get; set; }
            public Resolution2 Resolution { get; set; }
            public Type3 Type { get; set; }
        }

        public class Output2
        {
            public List<Connector2> Connector { get; set; }
        }

        public class PIPPosition2
        {
            public string Value { get; set; }
        }

        public class Presentation2
        {
            public PIPPosition2 PIPPosition { get; set; }
        }

        public class FullscreenMode
        {
            public string Value { get; set; }
        }

        public class Mode9
        {
            public string Value { get; set; }
        }

        public class OnMonitorRole
        {
            public string Value { get; set; }
        }

        public class PIPPosition3
        {
            public string Value { get; set; }
        }

        public class Selfview
        {
            public FullscreenMode FullscreenMode { get; set; }
            public Mode9 Mode { get; set; }
            public OnMonitorRole OnMonitorRole { get; set; }
            public PIPPosition3 PIPPosition { get; set; }
        }

        public class Video
        {
            public ActiveSpeaker2 ActiveSpeaker { get; set; }
            public Input2 Input { get; set; }
            public Layout Layout { get; set; }
            public Monitors Monitors { get; set; }
            public Output2 Output { get; set; }
            public Presentation2 Presentation { get; set; }
            public Selfview Selfview { get; set; }
        }

        public class Status
        {
            public Audio Audio { get; set; }
            public Bookings Bookings { get; set; }
            public Cameras Cameras { get; set; }
            public Capabilities2 Capabilities { get; set; }
            public Conference2 Conference { get; set; }
            public Experimental Experimental { get; set; }
            public H323 H323 { get; set; }
            public List<HttpFeedback> HttpFeedback { get; set; }
            public MediaChannels MediaChannels { get; set; }
            public List<Network> Network { get; set; }
            public NetworkServices NetworkServices { get; set; }
            public Peripherals Peripherals { get; set; }
            public Provisioning Provisioning { get; set; }
            public Proximity Proximity { get; set; }
            public RoomAnalytics RoomAnalytics { get; set; }
            public SIP SIP { get; set; }
            public Security Security { get; set; }
            public Standby Standby { get; set; }
            public SystemUnit SystemUnit { get; set; }
            public Time Time { get; set; }
            public UserInterface UserInterface { get; set; }
            public Video Video { get; set; }
        }

        public class RootObject
        {
            public Status Status { get; set; }
        }
    }
}