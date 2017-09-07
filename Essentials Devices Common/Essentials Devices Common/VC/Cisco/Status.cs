using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml.Serialization;

namespace PepperDash.Essentials.Devices.VideoCodec.Cisco
{
    public class CiscoCodecStatus
    {

	    //[XmlRoot(ElementName="Microphone")]
	    public class Microphone {
		    //[XmlElement(ElementName="ConnectionStatus")]
		    public string ConnectionStatus { get; set; }
		    //[XmlAttribute(AttributeName="item")]
		    public string Item { get; set; }
		    //[XmlAttribute(AttributeName="maxOccurrence")]
		    public string MaxOccurrence { get; set; }
	    }

	    //[XmlRoot(ElementName="Connectors")]
	    public class Connectors {
		    //[XmlElement(ElementName="Microphone")]
		    public List<Microphone> Microphone { get; set; }
		    //[XmlElement(ElementName="Line")]
		    public Line Line { get; set; }
	    }

	    //[XmlRoot(ElementName="Input")]
	    public class Input {
		    //[XmlElement(ElementName="Connectors")]
		    public Connectors Connectors { get; set; }
		    //[XmlElement(ElementName="Connector")]
		    public List<Connector> Connector { get; set; }
		    //[XmlElement(ElementName="MainVideoSource")]
		    public string MainVideoSource { get; set; }
		    //[XmlElement(ElementName="Source")]
		    public List<Source> Source { get; set; }
	    }

	    //[XmlRoot(ElementName="Microphones")]
	    public class Microphones {
		    //[XmlElement(ElementName="Mute")]
		    public string Mute { get; set; }
	    }

	    //[XmlRoot(ElementName="Line")]
	    public class Line {
		    //[XmlElement(ElementName="ConnectionStatus")]
		    public string ConnectionStatus { get; set; }
		    //[XmlElement(ElementName="DelayMs")]
		    public string DelayMs { get; set; }
		    //[XmlAttribute(AttributeName="item")]
		    public string Item { get; set; }
		    //[XmlAttribute(AttributeName="maxOccurrence")]
		    public string MaxOccurrence { get; set; }
	    }

	    //[XmlRoot(ElementName="Output")]
	    public class Output {
		    //[XmlElement(ElementName="Connectors")]
		    public Connectors Connectors { get; set; }
		    //[XmlElement(ElementName="Connector")]
		    public List<Connector> Connector { get; set; }
	    }

	    //[XmlRoot(ElementName="Audio")]
	    public class Audio {
		    //[XmlElement(ElementName="Input")]
		    public Input Input { get; set; }
		    //[XmlElement(ElementName="Microphones")]
		    public Microphones Microphones { get; set; }
		    //[XmlElement(ElementName="Output")]
		    public Output Output { get; set; }
		    //[XmlElement(ElementName="Volume")]
		    public string Volume { get; set; }
		    //[XmlElement(ElementName="VolumeMute")]
		    public string VolumeMute { get; set; }
	    }

	    //[XmlRoot(ElementName="Current")]
	    public class Current {
		    //[XmlElement(ElementName="Id")]
		    public string Id { get; set; }
		    //[XmlElement(ElementName="CompletedAt")]
		    public string CompletedAt { get; set; }
		    //[XmlElement(ElementName="URL")]
		    public string URL { get; set; }
		    //[XmlElement(ElementName="VersionId")]
		    public string VersionId { get; set; }
	    }

	    //[XmlRoot(ElementName="Bookings")]
	    public class Bookings {
		    //[XmlElement(ElementName="Current")]
		    public Current Current { get; set; }
	    }

	    //[XmlRoot(ElementName="Capabilities")]
	    public class Capabilities {
		    //[XmlElement(ElementName="Options")]
		    public string Options { get; set; }
		    //[XmlElement(ElementName="Conference")]
		    public Conference Conference { get; set; }
	    }

	    //[XmlRoot(ElementName="Position")]
	    public class Position {
		    //[XmlElement(ElementName="Pan")]
		    public string Pan { get; set; }
		    //[XmlElement(ElementName="Tilt")]
		    public string Tilt { get; set; }
		    //[XmlElement(ElementName="Zoom")]
		    public string Zoom { get; set; }
	    }

	    //[XmlRoot(ElementName="Camera")]
	    public class Camera {
		    //[XmlElement(ElementName="Capabilities")]
		    public Capabilities Capabilities { get; set; }
		    //[XmlElement(ElementName="Connected")]
		    public string Connected { get; set; }
		    //[XmlElement(ElementName="Framerate")]
		    public string Framerate { get; set; }
		    //[XmlElement(ElementName="Manufacturer")]
		    public string Manufacturer { get; set; }
		    //[XmlElement(ElementName="Model")]
		    public string Model { get; set; }
		    //[XmlElement(ElementName="Position")]
		    public Position Position { get; set; }
		    //[XmlAttribute(AttributeName="item")]
		    public string Item { get; set; }
		    //[XmlAttribute(AttributeName="maxOccurrence")]
		    public string MaxOccurrence { get; set; }
	    }

	    //[XmlRoot(ElementName="SpeakerTrack")]
	    public class SpeakerTrack {
		    //[XmlElement(ElementName="Availability")]
		    public string Availability { get; set; }
		    //[XmlElement(ElementName="Status")]
		    public string Status { get; set; }
	    }

	    //[XmlRoot(ElementName="Cameras")]
	    public class Cameras {
		    //[XmlElement(ElementName="Camera")]
		    public Camera Camera { get; set; }
		    //[XmlElement(ElementName="SpeakerTrack")]
		    public SpeakerTrack SpeakerTrack { get; set; }
	    }

	    //[XmlRoot(ElementName="Conference")]
	    public class Conference {
		    //[XmlElement(ElementName="MaxActiveCalls")]
		    public string MaxActiveCalls { get; set; }
		    //[XmlElement(ElementName="MaxAudioCalls")]
		    public string MaxAudioCalls { get; set; }
		    //[XmlElement(ElementName="MaxCalls")]
		    public string MaxCalls { get; set; }
		    //[XmlElement(ElementName="MaxVideoCalls")]
		    public string MaxVideoCalls { get; set; }
		    //[XmlElement(ElementName="ActiveSpeaker")]
		    public ActiveSpeaker ActiveSpeaker { get; set; }
		    //[XmlElement(ElementName="DoNotDisturb")]
		    public string DoNotDisturb { get; set; }
		    //[XmlElement(ElementName="Multipoint")]
		    public Multipoint Multipoint { get; set; }
		    //[XmlElement(ElementName="Presentation")]
		    public Presentation Presentation { get; set; }
		    //[XmlElement(ElementName="SpeakerLock")]
		    public SpeakerLock SpeakerLock { get; set; }
	    }

	    //[XmlRoot(ElementName="ActiveSpeaker")]
	    public class ActiveSpeaker {
		    //[XmlElement(ElementName="CallId")]
		    public string CallId { get; set; }
		    //[XmlElement(ElementName="PIPPosition")]
		    public string PIPPosition { get; set; }
	    }

	    //[XmlRoot(ElementName="Multipoint")]
	    public class Multipoint {
		    //[XmlElement(ElementName="Mode")]
		    public string Mode { get; set; }
	    }

	    //[XmlRoot(ElementName="Whiteboard")]
	    public class Whiteboard {
		    //[XmlElement(ElementName="Mode")]
		    public string Mode { get; set; }
		    //[XmlElement(ElementName="ReleaseFloorAvailability")]
		    public string ReleaseFloorAvailability { get; set; }
		    //[XmlElement(ElementName="RequestFloorAvailability")]
		    public string RequestFloorAvailability { get; set; }
	    }

	    //[XmlRoot(ElementName="Presentation")]
	    public class Presentation {
		    //[XmlElement(ElementName="CallId")]
		    public string CallId { get; set; }
		    //[XmlElement(ElementName="Mode")]
		    public string Mode { get; set; }
		    //[XmlElement(ElementName="Whiteboard")]
		    public Whiteboard Whiteboard { get; set; }
		    //[XmlElement(ElementName="PIPPosition")]
		    public string PIPPosition { get; set; }
	    }

	    //[XmlRoot(ElementName="SpeakerLock")]
	    public class SpeakerLock {
		    //[XmlElement(ElementName="CallId")]
		    public string CallId { get; set; }
		    //[XmlElement(ElementName="Mode")]
		    public string Mode { get; set; }
	    }

	    //[XmlRoot(ElementName="Message")]
	    public class Message {
		    //[XmlElement(ElementName="Description")]
		    public string Description { get; set; }
		    //[XmlElement(ElementName="Level")]
		    public string Level { get; set; }
		    //[XmlElement(ElementName="References")]
		    public string References { get; set; }
		    //[XmlElement(ElementName="Type")]
		    public string Type { get; set; }
		    //[XmlAttribute(AttributeName="item")]
		    public string Item { get; set; }
		    //[XmlAttribute(AttributeName="maxOccurrence")]
		    public string MaxOccurrence { get; set; }
	    }

	    //[XmlRoot(ElementName="Diagnostics")]
	    public class Diagnostics {
		    //[XmlElement(ElementName="Message")]
		    public List<Message> Message { get; set; }
	    }

	    //[XmlRoot(ElementName="Experimental")]
	    public class Experimental {
		    //[XmlElement(ElementName="Conference")]
		    public string Conference { get; set; }
	    }

	    //[XmlRoot(ElementName="Gatekeeper")]
	    public class Gatekeeper {
		    //[XmlElement(ElementName="Address")]
		    public string Address { get; set; }
		    //[XmlElement(ElementName="Port")]
		    public string Port { get; set; }
		    //[XmlElement(ElementName="Reason")]
		    public string Reason { get; set; }
		    //[XmlElement(ElementName="Status")]
		    public string Status { get; set; }
	    }

	    //[XmlRoot(ElementName="Mode")]
	    public class Mode {
		    //[XmlElement(ElementName="Reason")]
		    public string Reason { get; set; }
		    //[XmlElement(ElementName="Status")]
		    public string Status { get; set; }
	    }

	    //[XmlRoot(ElementName="H323")]
	    public class H323 {
		    //[XmlElement(ElementName="Gatekeeper")]
		    public Gatekeeper Gatekeeper { get; set; }
		    //[XmlElement(ElementName="Mode")]
		    public Mode Mode { get; set; }
	    }

	    //[XmlRoot(ElementName="Expression")]
	    public class Expression {
		    //[XmlAttribute(AttributeName="item")]
		    public string Item { get; set; }
		    //[XmlAttribute(AttributeName="maxOccurrence")]
		    public string MaxOccurrence { get; set; }
		    //[XmlText]
		    public string Text { get; set; }
	    }

	    //[XmlRoot(ElementName="HttpFeedback")]
	    public class HttpFeedback {
		    //[XmlElement(ElementName="Expression")]
		    public List<Expression> Expression { get; set; }
		    //[XmlElement(ElementName="Format")]
		    public string Format { get; set; }
		    //[XmlElement(ElementName="URL")]
		    public string URL { get; set; }
		    //[XmlAttribute(AttributeName="item")]
		    public string Item { get; set; }
		    //[XmlAttribute(AttributeName="maxOccurrence")]
		    public string MaxOccurrence { get; set; }
	    }

	    //[XmlRoot(ElementName="CDP")]
	    public class CDP {
		    //[XmlElement(ElementName="Address")]
		    public string Address { get; set; }
		    //[XmlElement(ElementName="Capabilities")]
		    public string Capabilities { get; set; }
		    //[XmlElement(ElementName="DeviceId")]
		    public string DeviceId { get; set; }
		    //[XmlElement(ElementName="Duplex")]
		    public string Duplex { get; set; }
		    //[XmlElement(ElementName="Platform")]
		    public string Platform { get; set; }
		    //[XmlElement(ElementName="PortID")]
		    public string PortID { get; set; }
		    //[XmlElement(ElementName="PrimaryMgmtAddress")]
		    public string PrimaryMgmtAddress { get; set; }
		    //[XmlElement(ElementName="SysName")]
		    public string SysName { get; set; }
		    //[XmlElement(ElementName="SysObjectID")]
		    public string SysObjectID { get; set; }
		    //[XmlElement(ElementName="VTPMgmtDomain")]
		    public string VTPMgmtDomain { get; set; }
		    //[XmlElement(ElementName="Version")]
		    public string Version { get; set; }
		    //[XmlElement(ElementName="VoIPApplianceVlanID")]
		    public string VoIPApplianceVlanID { get; set; }
	    }

	    //[XmlRoot(ElementName="Domain")]
	    public class Domain {
		    //[XmlElement(ElementName="Name")]
		    public string Name { get; set; }
	    }

	    //[XmlRoot(ElementName="Server")]
	    public class Server {
		    //[XmlElement(ElementName="Address")]
		    public string Address { get; set; }
		    //[XmlAttribute(AttributeName="item")]
		    public string Item { get; set; }
		    //[XmlAttribute(AttributeName="maxOccurrence")]
		    public string MaxOccurrence { get; set; }
	    }

	    //[XmlRoot(ElementName="DNS")]
	    public class DNS {
		    //[XmlElement(ElementName="Domain")]
		    public Domain Domain { get; set; }
		    //[XmlElement(ElementName="Server")]
		    public List<Server> Server { get; set; }
	    }

	    //[XmlRoot(ElementName="Ethernet")]
	    public class Ethernet {
		    //[XmlElement(ElementName="MacAddress")]
		    public string MacAddress { get; set; }
		    //[XmlElement(ElementName="Speed")]
		    public string Speed { get; set; }
	    }

	    //[XmlRoot(ElementName="IPv4")]
	    public class IPv4 {
		    //[XmlElement(ElementName="Address")]
		    public string Address { get; set; }
		    //[XmlElement(ElementName="Gateway")]
		    public string Gateway { get; set; }
		    //[XmlElement(ElementName="SubnetMask")]
		    public string SubnetMask { get; set; }
	    }

	    //[XmlRoot(ElementName="IPv6")]
	    public class IPv6 {
		    //[XmlElement(ElementName="Address")]
		    public string Address { get; set; }
		    //[XmlElement(ElementName="Gateway")]
		    public string Gateway { get; set; }
	    }

	    //[XmlRoot(ElementName="Voice")]
	    public class Voice {
		    //[XmlElement(ElementName="VlanId")]
		    public string VlanId { get; set; }
	    }

	    //[XmlRoot(ElementName="VLAN")]
	    public class VLAN {
		    //[XmlElement(ElementName="Voice")]
		    public Voice Voice { get; set; }
	    }

	    //[XmlRoot(ElementName="Network")]
	    public class Network {
		    //[XmlElement(ElementName="CDP")]
		    public CDP CDP { get; set; }
		    //[XmlElement(ElementName="DNS")]
		    public DNS DNS { get; set; }
		    //[XmlElement(ElementName="Ethernet")]
		    public Ethernet Ethernet { get; set; }
		    //[XmlElement(ElementName="IPv4")]
		    public IPv4 IPv4 { get; set; }
		    //[XmlElement(ElementName="IPv6")]
		    public IPv6 IPv6 { get; set; }
		    //[XmlElement(ElementName="VLAN")]
		    public VLAN VLAN { get; set; }
		    //[XmlAttribute(AttributeName="item")]
		    public string Item { get; set; }
		    //[XmlAttribute(AttributeName="maxOccurrence")]
		    public string MaxOccurrence { get; set; }
	    }

	    //[XmlRoot(ElementName="NTP")]
	    public class NTP {
		    //[XmlElement(ElementName="CurrentAddress")]
		    public string CurrentAddress { get; set; }
		    //[XmlElement(ElementName="Server")]
		    public List<Server> Server { get; set; }
		    //[XmlElement(ElementName="Status")]
		    public string Status { get; set; }
	    }

	    //[XmlRoot(ElementName="NetworkServices")]
	    public class NetworkServices {
		    //[XmlElement(ElementName="NTP")]
		    public NTP NTP { get; set; }
	    }

	    //[XmlRoot(ElementName="ConnectedDevice")]
	    public class ConnectedDevice {
		    //[XmlElement(ElementName="HardwareInfo")]
		    public string HardwareInfo { get; set; }
		    //[XmlElement(ElementName="ID")]
		    public string ID { get; set; }
		    //[XmlElement(ElementName="Name")]
		    public string Name { get; set; }
		    //[XmlElement(ElementName="SoftwareInfo")]
		    public string SoftwareInfo { get; set; }
		    //[XmlElement(ElementName="Status")]
		    public string Status { get; set; }
		    //[XmlElement(ElementName="Type")]
		    public string Type { get; set; }
		    //[XmlElement(ElementName="UpgradeStatus")]
		    public string UpgradeStatus { get; set; }
		    //[XmlAttribute(AttributeName="item")]
		    public string Item { get; set; }
		    //[XmlAttribute(AttributeName="maxOccurrence")]
		    public string MaxOccurrence { get; set; }
		    //[XmlElement(ElementName="PreferredFormat")]
		    public string PreferredFormat { get; set; }
	    }

	    //[XmlRoot(ElementName="Peripherals")]
	    public class Peripherals {
		    //[XmlElement(ElementName="ConnectedDevice")]
		    public ConnectedDevice ConnectedDevice { get; set; }
	    }

	    //[XmlRoot(ElementName="ExtensionMobility")]
	    public class ExtensionMobility {
		    //[XmlElement(ElementName="Enabled")]
		    public string Enabled { get; set; }
		    //[XmlElement(ElementName="LastLoggedInUserId")]
		    public string LastLoggedInUserId { get; set; }
		    //[XmlElement(ElementName="LoggedIn")]
		    public string LoggedIn { get; set; }
	    }

	    //[XmlRoot(ElementName="CUCM")]
	    public class CUCM {
		    //[XmlElement(ElementName="ExtensionMobility")]
		    public ExtensionMobility ExtensionMobility { get; set; }
	    }

	    //[XmlRoot(ElementName="UpgradeStatus")]
	    public class UpgradeStatus {
		    //[XmlElement(ElementName="LastChange")]
		    public string LastChange { get; set; }
		    //[XmlElement(ElementName="Message")]
		    public string Message { get; set; }
		    //[XmlElement(ElementName="Phase")]
		    public string Phase { get; set; }
		    //[XmlElement(ElementName="SessionId")]
		    public string SessionId { get; set; }
		    //[XmlElement(ElementName="Status")]
		    public string Status { get; set; }
		    //[XmlElement(ElementName="URL")]
		    public string URL { get; set; }
		    //[XmlElement(ElementName="VersionId")]
		    public string VersionId { get; set; }
	    }

	    //[XmlRoot(ElementName="Software")]
	    public class Software {
		    //[XmlElement(ElementName="Current")]
		    public Current Current { get; set; }
		    //[XmlElement(ElementName="UpgradeStatus")]
		    public UpgradeStatus UpgradeStatus { get; set; }
		    //[XmlElement(ElementName="DisplayName")]
		    public string DisplayName { get; set; }
		    //[XmlElement(ElementName="Name")]
		    public string Name { get; set; }
		    //[XmlElement(ElementName="OptionKeys")]
		    public OptionKeys OptionKeys { get; set; }
		    //[XmlElement(ElementName="ReleaseDate")]
		    public string ReleaseDate { get; set; }
		    //[XmlElement(ElementName="Version")]
		    public string Version { get; set; }
	    }

	    //[XmlRoot(ElementName="Provisioning")]
	    public class Provisioning {
		    //[XmlElement(ElementName="CUCM")]
		    public CUCM CUCM { get; set; }
		    //[XmlElement(ElementName="Software")]
		    public Software Software { get; set; }
		    //[XmlElement(ElementName="Status")]
		    public string Status { get; set; }
	    }

	    //[XmlRoot(ElementName="Services")]
	    public class Services {
		    //[XmlElement(ElementName="Availability")]
		    public string Availability { get; set; }
	    }

	    //[XmlRoot(ElementName="Proximity")]
	    public class Proximity {
		    //[XmlElement(ElementName="Services")]
		    public Services Services { get; set; }
	    }

	    //[XmlRoot(ElementName="PeopleCount")]
	    public class PeopleCount {
		    //[XmlElement(ElementName="Current")]
		    public string Current { get; set; }
	    }

	    //[XmlRoot(ElementName="RoomAnalytics")]
	    public class RoomAnalytics {
		    //[XmlElement(ElementName="PeopleCount")]
		    public PeopleCount PeopleCount { get; set; }
		    //[XmlElement(ElementName="PeoplePresence")]
		    public string PeoplePresence { get; set; }
	    }

	    //[XmlRoot(ElementName="CallForward")]
	    public class CallForward {
		    //[XmlElement(ElementName="DisplayName")]
		    public string DisplayName { get; set; }
		    //[XmlElement(ElementName="Mode")]
		    public string Mode { get; set; }
		    //[XmlElement(ElementName="URI")]
		    public string URI { get; set; }
	    }

	    //[XmlRoot(ElementName="Mailbox")]
	    public class Mailbox {
		    //[XmlElement(ElementName="MessagesWaiting")]
		    public string MessagesWaiting { get; set; }
		    //[XmlElement(ElementName="URI")]
		    public string URI { get; set; }
	    }

	    //[XmlRoot(ElementName="Proxy")]
	    public class Proxy {
		    //[XmlElement(ElementName="Address")]
		    public string Address { get; set; }
		    //[XmlElement(ElementName="Status")]
		    public string Status { get; set; }
		    //[XmlAttribute(AttributeName="item")]
		    public string Item { get; set; }
		    //[XmlAttribute(AttributeName="maxOccurrence")]
		    public string MaxOccurrence { get; set; }
	    }

	    //[XmlRoot(ElementName="Registration")]
	    public class Registration {
		    //[XmlElement(ElementName="Reason")]
		    public string Reason { get; set; }
		    //[XmlElement(ElementName="Status")]
		    public string Status { get; set; }
		    //[XmlElement(ElementName="URI")]
		    public string URI { get; set; }
		    //[XmlAttribute(AttributeName="item")]
		    public string Item { get; set; }
		    //[XmlAttribute(AttributeName="maxOccurrence")]
		    public string MaxOccurrence { get; set; }
	    }

	    //[XmlRoot(ElementName="SIP")]
	    public class SIP {
		    //[XmlElement(ElementName="Authentication")]
		    public string Authentication { get; set; }
		    //[XmlElement(ElementName="CallForward")]
		    public CallForward CallForward { get; set; }
		    //[XmlElement(ElementName="Mailbox")]
		    public Mailbox Mailbox { get; set; }
		    //[XmlElement(ElementName="Proxy")]
		    public Proxy Proxy { get; set; }
		    //[XmlElement(ElementName="Registration")]
		    public Registration Registration { get; set; }
		    //[XmlElement(ElementName="Secure")]
		    public string Secure { get; set; }
		    //[XmlElement(ElementName="Verified")]
		    public string Verified { get; set; }
	    }

	    //[XmlRoot(ElementName="FIPS")]
	    public class FIPS {
		    //[XmlElement(ElementName="Mode")]
		    public string Mode { get; set; }
	    }

	    //[XmlRoot(ElementName="Persistency")]
	    public class Persistency {
		    //[XmlElement(ElementName="CallHistory")]
		    public string CallHistory { get; set; }
		    //[XmlElement(ElementName="Configurations")]
		    public string Configurations { get; set; }
		    //[XmlElement(ElementName="DHCP")]
		    public string DHCP { get; set; }
		    //[XmlElement(ElementName="InternalLogging")]
		    public string InternalLogging { get; set; }
		    //[XmlElement(ElementName="LocalPhonebook")]
		    public string LocalPhonebook { get; set; }
	    }

	    //[XmlRoot(ElementName="Security")]
	    public class Security {
		    //[XmlElement(ElementName="FIPS")]
		    public FIPS FIPS { get; set; }
		    //[XmlElement(ElementName="Persistency")]
		    public Persistency Persistency { get; set; }
	    }

	    //[XmlRoot(ElementName="Standby")]
	    public class Standby {
		    //[XmlElement(ElementName="State")]
		    public string State { get; set; }
	    }

	    //[XmlRoot(ElementName="Module")]
	    public class Module {
		    //[XmlElement(ElementName="CompatibilityLevel")]
		    public string CompatibilityLevel { get; set; }
		    //[XmlElement(ElementName="SerialNumber")]
		    public string SerialNumber { get; set; }
	    }

	    //[XmlRoot(ElementName="Hardware")]
	    public class Hardware {
		    //[XmlElement(ElementName="Module")]
		    public Module Module { get; set; }
	    }

	    //[XmlRoot(ElementName="OptionKeys")]
	    public class OptionKeys {
		    //[XmlElement(ElementName="Encryption")]
		    public string Encryption { get; set; }
		    //[XmlElement(ElementName="MultiSite")]
		    public string MultiSite { get; set; }
		    //[XmlElement(ElementName="RemoteMonitoring")]
		    public string RemoteMonitoring { get; set; }
	    }

	    //[XmlRoot(ElementName="State")]
	    public class State {
		    //[XmlElement(ElementName="NumberOfActiveCalls")]
		    public string NumberOfActiveCalls { get; set; }
		    //[XmlElement(ElementName="NumberOfInProgressCalls")]
		    public string NumberOfInProgressCalls { get; set; }
		    //[XmlElement(ElementName="NumberOfSuspendedCalls")]
		    public string NumberOfSuspendedCalls { get; set; }
	    }

	    //[XmlRoot(ElementName="SystemUnit")]
	    public class SystemUnit {
		    //[XmlElement(ElementName="Hardware")]
		    public Hardware Hardware { get; set; }
		    //[XmlElement(ElementName="ProductId")]
		    public string ProductId { get; set; }
		    //[XmlElement(ElementName="ProductPlatform")]
		    public string ProductPlatform { get; set; }
		    //[XmlElement(ElementName="ProductType")]
		    public string ProductType { get; set; }
		    //[XmlElement(ElementName="Software")]
		    public Software Software { get; set; }
		    //[XmlElement(ElementName="State")]
		    public State State { get; set; }
		    //[XmlElement(ElementName="Uptime")]
		    public string Uptime { get; set; }
	    }

	    //[XmlRoot(ElementName="Time")]
	    public class Time {
		    //[XmlElement(ElementName="SystemTime")]
		    public string SystemTime { get; set; }
	    }

	    //[XmlRoot(ElementName="ContactMethod")]
	    public class ContactMethod {
		    //[XmlElement(ElementName="Number")]
		    public string Number { get; set; }
		    //[XmlAttribute(AttributeName="item")]
		    public string Item { get; set; }
		    //[XmlAttribute(AttributeName="maxOccurrence")]
		    public string MaxOccurrence { get; set; }
	    }

	    //[XmlRoot(ElementName="ContactInfo")]
	    public class ContactInfo {
		    //[XmlElement(ElementName="ContactMethod")]
		    public List<ContactMethod> ContactMethod { get; set; }
		    //[XmlElement(ElementName="Name")]
		    public string Name { get; set; }
	    }

	    //[XmlRoot(ElementName="UserInterface")]
	    public class UserInterface {
		    //[XmlElement(ElementName="ContactInfo")]
		    public ContactInfo ContactInfo { get; set; }
	    }

	    //[XmlRoot(ElementName="Connector")]
	    public class Connector {
		    //[XmlElement(ElementName="Connected")]
		    public string Connected { get; set; }
		    //[XmlElement(ElementName="SignalState")]
		    public string SignalState { get; set; }
		    //[XmlElement(ElementName="SourceId")]
		    public string SourceId { get; set; }
		    //[XmlElement(ElementName="Type")]
		    public string Type { get; set; }
		    //[XmlAttribute(AttributeName="item")]
		    public string Item { get; set; }
		    //[XmlAttribute(AttributeName="maxOccurrence")]
		    public string MaxOccurrence { get; set; }
		    //[XmlElement(ElementName="ConnectedDevice")]
		    public ConnectedDevice ConnectedDevice { get; set; }
		    //[XmlElement(ElementName="MonitorRole")]
		    public string MonitorRole { get; set; }
		    //[XmlElement(ElementName="Resolution")]
		    public Resolution Resolution { get; set; }
	    }

	    //[XmlRoot(ElementName="Resolution")]
	    public class Resolution {
		    //[XmlElement(ElementName="Height")]
		    public string Height { get; set; }
		    //[XmlElement(ElementName="RefreshRate")]
		    public string RefreshRate { get; set; }
		    //[XmlElement(ElementName="Width")]
		    public string Width { get; set; }
	    }

	    //[XmlRoot(ElementName="Source")]
	    public class Source {
		    //[XmlElement(ElementName="ConnectorId")]
		    public string ConnectorId { get; set; }
		    //[XmlElement(ElementName="FormatStatus")]
		    public string FormatStatus { get; set; }
		    //[XmlElement(ElementName="FormatType")]
		    public string FormatType { get; set; }
		    //[XmlElement(ElementName="MediaChannelId")]
		    public string MediaChannelId { get; set; }
		    //[XmlElement(ElementName="Resolution")]
		    public Resolution Resolution { get; set; }
		    //[XmlAttribute(AttributeName="item")]
		    public string Item { get; set; }
		    //[XmlAttribute(AttributeName="maxOccurrence")]
		    public string MaxOccurrence { get; set; }
	    }

	    //[XmlRoot(ElementName="LayoutFamily")]
	    public class LayoutFamily {
		    //[XmlElement(ElementName="Local")]
		    public string Local { get; set; }
	    }

	    //[XmlRoot(ElementName="Layout")]
	    public class Layout {
		    //[XmlElement(ElementName="LayoutFamily")]
		    public LayoutFamily LayoutFamily { get; set; }
	    }

	    //[XmlRoot(ElementName="Selfview")]
	    public class Selfview {
		    //[XmlElement(ElementName="FullscreenMode")]
		    public string FullscreenMode { get; set; }
		    //[XmlElement(ElementName="Mode")]
		    public string Mode { get; set; }
		    //[XmlElement(ElementName="OnMonitorRole")]
		    public string OnMonitorRole { get; set; }
		    //[XmlElement(ElementName="PIPPosition")]
		    public string PIPPosition { get; set; }
	    }

	    //[XmlRoot(ElementName="Video")]
	    public class Video {
		    //[XmlElement(ElementName="ActiveSpeaker")]
		    public ActiveSpeaker ActiveSpeaker { get; set; }
		    //[XmlElement(ElementName="Input")]
		    public Input Input { get; set; }
		    //[XmlElement(ElementName="Layout")]
		    public Layout Layout { get; set; }
		    //[XmlElement(ElementName="Monitors")]
		    public string Monitors { get; set; }
		    //[XmlElement(ElementName="Output")]
		    public Output Output { get; set; }
		    //[XmlElement(ElementName="Presentation")]
		    public Presentation Presentation { get; set; }
		    //[XmlElement(ElementName="Selfview")]
		    public Selfview Selfview { get; set; }
	    }

	    //[XmlRoot(ElementName="Status")]
	    public class Status {
		    //[XmlElement(ElementName="Audio")]
		    public Audio Audio { get; set; }
		    //[XmlElement(ElementName="Bookings")]
		    public Bookings Bookings { get; set; }
		    //[XmlElement(ElementName="Cameras")]
		    public Cameras Cameras { get; set; }
		    //[XmlElement(ElementName="Capabilities")]
		    public Capabilities Capabilities { get; set; }
		    //[XmlElement(ElementName="Conference")]
		    public Conference Conference { get; set; }
		    //[XmlElement(ElementName="Diagnostics")]
		    public Diagnostics Diagnostics { get; set; }
		    //[XmlElement(ElementName="Experimental")]
		    public Experimental Experimental { get; set; }
		    //[XmlElement(ElementName="H323")]
		    public H323 H323 { get; set; }
		    //[XmlElement(ElementName="HttpFeedback")]
		    public HttpFeedback HttpFeedback { get; set; }
		    //[XmlElement(ElementName="MediaChannels")]
		    public string MediaChannels { get; set; }
		    //[XmlElement(ElementName="Network")]
		    public Network Network { get; set; }
		    //[XmlElement(ElementName="NetworkServices")]
		    public NetworkServices NetworkServices { get; set; }
		    //[XmlElement(ElementName="Peripherals")]
		    public Peripherals Peripherals { get; set; }
		    //[XmlElement(ElementName="Provisioning")]
		    public Provisioning Provisioning { get; set; }
		    //[XmlElement(ElementName="Proximity")]
		    public Proximity Proximity { get; set; }
		    //[XmlElement(ElementName="RoomAnalytics")]
		    public RoomAnalytics RoomAnalytics { get; set; }
		    //[XmlElement(ElementName="SIP")]
		    public SIP SIP { get; set; }
		    //[XmlElement(ElementName="Security")]
		    public Security Security { get; set; }
		    //[XmlElement(ElementName="Standby")]
		    public Standby Standby { get; set; }
		    //[XmlElement(ElementName="SystemUnit")]
		    public SystemUnit SystemUnit { get; set; }
		    //[XmlElement(ElementName="Time")]
		    public Time Time { get; set; }
		    //[XmlElement(ElementName="UserInterface")]
		    public UserInterface UserInterface { get; set; }
		    //[XmlElement(ElementName="Video")]
		    public Video Video { get; set; }
		    //[XmlAttribute(AttributeName="product")]
		    public string Product { get; set; }
		    //[XmlAttribute(AttributeName="version")]
		    public string Version { get; set; }
		    //[XmlAttribute(AttributeName="apiVersion")]
		    public string ApiVersion { get; set; }
	    }

    }
}
