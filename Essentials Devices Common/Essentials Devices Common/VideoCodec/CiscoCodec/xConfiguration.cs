using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.Cisco
{
    /// <summary>
    /// This class exists to capture serialized data sent back by a Cisco codec in JSON output mode
    /// </summary>
    public class CiscoCodecConfiguration
    {
        public class DefaultVolume
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Mode
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Dereverberation
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Mode2
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class NoiseReduction
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class EchoControl
        {
            public Dereverberation Dereverberation { get; set; }
            public Mode2 Mode { get; set; }
            public NoiseReduction NoiseReduction { get; set; }
        }

        public class Level
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Microphone
        {
            public string id { get; set; }
            public Mode Mode { get; set; }
            public EchoControl EchoControl { get; set; }
            public Level Level { get; set; }
        }

        public class Input
        {
            public List<Microphone> Microphone { get; set; }
        }

        public class Enabled
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Mute
        {
            public Enabled Enabled { get; set; }
        }

        public class Microphones
        {
            public Mute Mute { get; set; }
        }

        public class Mode3
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class InternalSpeaker
        {
            public Mode3 Mode { get; set; }
        }

        public class Mode4
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class OutputType
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Line
        {
            public string id { get; set; }
            public Mode4 Mode { get; set; }
            public OutputType OutputType { get; set; }
        }

        public class Output
        {
            public InternalSpeaker InternalSpeaker { get; set; }
            public List<Line> Line { get; set; }
        }

        public class RingTone
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class RingVolume
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class SoundsAndAlerts
        {
            public RingTone RingTone { get; set; }
            public RingVolume RingVolume { get; set; }
        }

        public class Audio
        {
            public DefaultVolume DefaultVolume { get; set; }
            public Input Input { get; set; }
            public Microphones Microphones { get; set; }
            public Output Output { get; set; }
            public SoundsAndAlerts SoundsAndAlerts { get; set; }
        }

        public class Framerate
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Camera
        {
            public Framerate Framerate { get; set; }
        }

        public class Closeup
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Mode5
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class SpeakerTrack
        {
            public Closeup Closeup { get; set; }
            public Mode5 Mode { get; set; }
        }

        public class Cameras
        {
            public Camera Camera { get; set; }
            public SpeakerTrack SpeakerTrack { get; set; }
        }

        public class Delay
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Mode6
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Mute2
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class AutoAnswer
        {
            public Delay Delay { get; set; }
            public Mode6 Mode { get; set; }
            public Mute2 Mute { get; set; }
        }

        public class Protocol
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Rate
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class DefaultCall
        {
            public Protocol Protocol { get; set; }
            public Rate Rate { get; set; }
        }

        public class DefaultTimeout
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class DoNotDisturb
        {
            public DefaultTimeout DefaultTimeout { get; set; }
        }

        public class Mode7
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Encryption
        {
            public Mode7 Mode { get; set; }
        }

        public class Mode8
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class FarEndControl
        {
            public Mode8 Mode { get; set; }
        }

        public class MaxReceiveCallRate
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class MaxTotalReceiveCallRate
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class MaxTotalTransmitCallRate
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class MaxTransmitCallRate
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Mode9
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class MultiStream
        {
            public Mode9 Mode { get; set; }
        }

        public class Conference
        {
            public AutoAnswer AutoAnswer { get; set; }
            public DefaultCall DefaultCall { get; set; }
            public DoNotDisturb DoNotDisturb { get; set; }
            public Encryption Encryption { get; set; }
            public FarEndControl FarEndControl { get; set; }
            public MaxReceiveCallRate MaxReceiveCallRate { get; set; }
            public MaxTotalReceiveCallRate MaxTotalReceiveCallRate { get; set; }
            public MaxTotalTransmitCallRate MaxTotalTransmitCallRate { get; set; }
            public MaxTransmitCallRate MaxTransmitCallRate { get; set; }
            public MultiStream MultiStream { get; set; }
        }

        public class LoginName
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Mode10
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Password
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Authentication
        {
            public LoginName LoginName { get; set; }
            public Mode10 Mode { get; set; }
            public Password Password { get; set; }
        }

        public class Mode11
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class CallSetup
        {
            public Mode11 Mode { get; set; }
        }

        public class KeySize
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Encryption2
        {
            public KeySize KeySize { get; set; }
        }

        public class Address
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Gatekeeper
        {
            public Address Address { get; set; }
        }

        public class E164
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class ID
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class H323Alias
        {
            public E164 E164 { get; set; }
            public ID ID { get; set; }
        }

        public class Address2
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Mode12
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class NAT
        {
            public Address2 Address { get; set; }
            public Mode12 Mode { get; set; }
        }

        public class H323
        {
            public Authentication Authentication { get; set; }
            public CallSetup CallSetup { get; set; }
            public Encryption2 Encryption { get; set; }
            public Gatekeeper Gatekeeper { get; set; }
            public H323Alias H323Alias { get; set; }
            public NAT NAT { get; set; }
        }

        public class Name
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Domain
        {
            public Name Name { get; set; }
        }

        public class Address3
        {
            public string valueSpaceRef { get; set; }
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

        public class AnonymousIdentity
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Md5
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Peap
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Tls
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Ttls
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Eap
        {
            public Md5 Md5 { get; set; }
            public Peap Peap { get; set; }
            public Tls Tls { get; set; }
            public Ttls Ttls { get; set; }
        }

        public class Identity
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Mode13
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Password2
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class TlsVerify
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class UseClientCertificate
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class IEEE8021X
        {
            public AnonymousIdentity AnonymousIdentity { get; set; }
            public Eap Eap { get; set; }
            public Identity Identity { get; set; }
            public Mode13 Mode { get; set; }
            public Password2 Password { get; set; }
            public TlsVerify TlsVerify { get; set; }
            public UseClientCertificate UseClientCertificate { get; set; }
        }

        public class IPStack
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Address4
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Assignment
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Gateway
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class SubnetMask
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class IPv4
        {
            public Address4 Address { get; set; }
            public Assignment Assignment { get; set; }
            public Gateway Gateway { get; set; }
            public SubnetMask SubnetMask { get; set; }
        }

        public class Address5
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Assignment2
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class DHCPOptions
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Gateway2
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class IPv6
        {
            public Address5 Address { get; set; }
            public Assignment2 Assignment { get; set; }
            public DHCPOptions DHCPOptions { get; set; }
            public Gateway2 Gateway { get; set; }
        }

        public class MTU
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Audio2
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Data
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class ICMPv6
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class NTP
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Signalling
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Video
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Diffserv
        {
            public Audio2 Audio { get; set; }
            public Data Data { get; set; }
            public ICMPv6 ICMPv6 { get; set; }
            public NTP NTP { get; set; }
            public Signalling Signalling { get; set; }
            public Video Video { get; set; }
        }

        public class Mode14
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class QoS
        {
            public Diffserv Diffserv { get; set; }
            public Mode14 Mode { get; set; }
        }

        public class Allow
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class RemoteAccess
        {
            public Allow Allow { get; set; }
        }

        public class Speed
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Mode15
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class VlanId
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Voice
        {
            public Mode15 Mode { get; set; }
            public VlanId VlanId { get; set; }
        }

        public class VLAN
        {
            public Voice Voice { get; set; }
        }

        public class Network
        {
            public string id { get; set; }
            public DNS DNS { get; set; }
            public IEEE8021X IEEE8021X { get; set; }
            public IPStack IPStack { get; set; }
            public IPv4 IPv4 { get; set; }
            public IPv6 IPv6 { get; set; }
            public MTU MTU { get; set; }
            public QoS QoS { get; set; }
            public RemoteAccess RemoteAccess { get; set; }
            public Speed Speed { get; set; }
            public VLAN VLAN { get; set; }
        }

        public class Mode16
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class CDP
        {
            public Mode16 Mode { get; set; }
        }

        public class Mode17
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class H3232
        {
            public Mode17 Mode { get; set; }
        }

        public class Mode18
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class HTTP
        {
            public Mode18 Mode { get; set; }
        }

        public class MinimumTLSVersion
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Server2
        {
            public MinimumTLSVersion MinimumTLSVersion { get; set; }
        }

        public class StrictTransportSecurity
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class VerifyClientCertificate
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class VerifyServerCertificate
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class HTTPS
        {
            public Server2 Server { get; set; }
            public StrictTransportSecurity StrictTransportSecurity { get; set; }
            public VerifyClientCertificate VerifyClientCertificate { get; set; }
            public VerifyServerCertificate VerifyServerCertificate { get; set; }
        }

        public class Mode19
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Address6
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Server3
        {
            public string id { get; set; }
            public Address6 Address { get; set; }
        }

        public class NTP2
        {
            public Mode19 Mode { get; set; }
            public List<Server3> Server { get; set; }
        }

        public class Mode20
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class SIP
        {
            public Mode20 Mode { get; set; }
        }

        public class CommunityName
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Address7
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Host
        {
            public string id { get; set; }
            public Address7 Address { get; set; }
        }

        public class Mode21
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class SystemContact
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class SystemLocation
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class SNMP
        {
            public CommunityName CommunityName { get; set; }
            public List<Host> Host { get; set; }
            public Mode21 Mode { get; set; }
            public SystemContact SystemContact { get; set; }
            public SystemLocation SystemLocation { get; set; }
        }

        public class Mode22
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class SSH
        {
            public Mode22 Mode { get; set; }
        }

        public class Mode23
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class UPnP
        {
            public Mode23 Mode { get; set; }
        }

        public class WelcomeText
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class NetworkServices
        {
            public CDP CDP { get; set; }
            public H3232 H323 { get; set; }
            public HTTP HTTP { get; set; }
            public HTTPS HTTPS { get; set; }
            public NTP2 NTP { get; set; }
            public SIP SIP { get; set; }
            public SNMP SNMP { get; set; }
            public SSH SSH { get; set; }
            public UPnP UPnP { get; set; }
            public WelcomeText WelcomeText { get; set; }
        }

        public class Cameras2
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class ControlSystems
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class TouchPanels
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Profile
        {
            public Cameras2 Cameras { get; set; }
            public ControlSystems ControlSystems { get; set; }
            public TouchPanels TouchPanels { get; set; }
        }

        public class Peripherals
        {
            public Profile Profile { get; set; }
        }

        public class ID2
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Type
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class URL
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Server4
        {
            public string id { get; set; }
            public ID2 ID { get; set; }
            public Type Type { get; set; }
            public URL URL { get; set; }
        }

        public class Phonebook
        {
            public List<Server4> Server { get; set; }
        }

        public class Connectivity
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Address8
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class AlternateAddress
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Domain2
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Path
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Protocol2
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class ExternalManager
        {
            public Address8 Address { get; set; }
            public AlternateAddress AlternateAddress { get; set; }
            public Domain2 Domain { get; set; }
            public Path Path { get; set; }
            public Protocol2 Protocol { get; set; }
        }

        public class HttpMethod
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class LoginName2
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Mode24
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Password3
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Provisioning
        {
            public Connectivity Connectivity { get; set; }
            public ExternalManager ExternalManager { get; set; }
            public HttpMethod HttpMethod { get; set; }
            public LoginName2 LoginName { get; set; }
            public Mode24 Mode { get; set; }
            public Password3 Password { get; set; }
        }

        public class Mode25
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class CallControl
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class FromClients
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class ToClients
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class ContentShare
        {
            public FromClients FromClients { get; set; }
            public ToClients ToClients { get; set; }
        }

        public class Services
        {
            public CallControl CallControl { get; set; }
            public ContentShare ContentShare { get; set; }
        }

        public class Proximity
        {
            public Mode25 Mode { get; set; }
            public Services Services { get; set; }
        }

        public class PeopleCountOutOfCall
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class PeoplePresenceDetector
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class RoomAnalytics
        {
            public PeopleCountOutOfCall PeopleCountOutOfCall { get; set; }
            public PeoplePresenceDetector PeoplePresenceDetector { get; set; }
        }

        public class Password4
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class UserName
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Authentication2
        {
            public Password4 Password { get; set; }
            public UserName UserName { get; set; }
        }

        public class DefaultTransport
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class DisplayName
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class DefaultCandidate
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Mode26
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Ice
        {
            public DefaultCandidate DefaultCandidate { get; set; }
            public Mode26 Mode { get; set; }
        }

        public class ListenPort
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Address9
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Proxy
        {
            public string id { get; set; }
            public Address9 Address { get; set; }
        }

        public class Password5
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Server5
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class UserName2
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Turn
        {
            public Password5 Password { get; set; }
            public Server5 Server { get; set; }
            public UserName2 UserName { get; set; }
        }

        public class URI
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class SIP2
        {
            public Authentication2 Authentication { get; set; }
            public DefaultTransport DefaultTransport { get; set; }
            public DisplayName DisplayName { get; set; }
            public Ice Ice { get; set; }
            public ListenPort ListenPort { get; set; }
            public List<Proxy> Proxy { get; set; }
            public Turn Turn { get; set; }
            public URI URI { get; set; }
        }

        public class BaudRate
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class LoginRequired
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Mode27
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class SerialPort
        {
            public BaudRate BaudRate { get; set; }
            public LoginRequired LoginRequired { get; set; }
            public Mode27 Mode { get; set; }
        }

        public class BootAction
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Control
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Delay2
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class StandbyAction
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class WakeupAction
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Standby
        {
            public BootAction BootAction { get; set; }
            public Control Control { get; set; }
            public Delay2 Delay { get; set; }
            public StandbyAction StandbyAction { get; set; }
            public WakeupAction WakeupAction { get; set; }
        }

        public class Name2
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class SystemUnit
        {
            public Name2 Name { get; set; }
        }

        public class DateFormat
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class TimeFormat
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Zone
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Time
        {
            public DateFormat DateFormat { get; set; }
            public TimeFormat TimeFormat { get; set; }
            public Zone Zone { get; set; }
        }

        public class Type2
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class ContactInfo
        {
            public Type2 Type { get; set; }
        }

        public class Mode28
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class KeyTones
        {
            public Mode28 Mode { get; set; }
        }

        public class Language
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Output2
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class OSD
        {
            public Output2 Output { get; set; }
        }

        public class UserInterface
        {
            public ContactInfo ContactInfo { get; set; }
            public KeyTones KeyTones { get; set; }
            public Language Language { get; set; }
            public OSD OSD { get; set; }
        }

        public class Filter
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Group
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Admin
        {
            public Filter Filter { get; set; }
            public Group Group { get; set; }
        }

        public class Attribute
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class BaseDN
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Encryption3
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class MinimumTLSVersion2
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Mode29
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Address10
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Port
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Server6
        {
            public Address10 Address { get; set; }
            public Port Port { get; set; }
        }

        public class VerifyServerCertificate2
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class LDAP
        {
            public Admin Admin { get; set; }
            public Attribute Attribute { get; set; }
            public BaseDN BaseDN { get; set; }
            public Encryption3 Encryption { get; set; }
            public MinimumTLSVersion2 MinimumTLSVersion { get; set; }
            public Mode29 Mode { get; set; }
            public Server6 Server { get; set; }
            public VerifyServerCertificate2 VerifyServerCertificate { get; set; }
        }

        public class UserManagement
        {
            public LDAP LDAP { get; set; }
        }

        public class DefaultMainSource
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class CameraId
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Mode30
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class CameraControl
        {
            public CameraId CameraId { get; set; }
            public Mode30 Mode { get; set; }
        }

        public class InputSourceType
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Name3
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Visibility
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class PreferredResolution
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class PresentationSelection
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Quality
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Connector
        {
            public string id { get; set; }
            public CameraControl CameraControl { get; set; }
            public InputSourceType InputSourceType { get; set; }
            public Name3 Name { get; set; }
            public Visibility Visibility { get; set; }
            public PreferredResolution PreferredResolution { get; set; }
            public PresentationSelection PresentationSelection { get; set; }
            public Quality Quality { get; set; }
        }

        public class Input2
        {
            public List<Connector> Connector { get; set; }
        }

        public class Monitors
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Mode31
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class CEC
        {
            public Mode31 Mode { get; set; }
        }

        public class MonitorRole
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Resolution
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Connector2
        {
            public string id { get; set; }
            public CEC CEC { get; set; }
            public MonitorRole MonitorRole { get; set; }
            public Resolution Resolution { get; set; }
        }

        public class Output3
        {
            public List<Connector2> Connector { get; set; }
        }

        public class DefaultSource
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Presentation
        {
            public DefaultSource DefaultSource { get; set; }
        }

        public class FullscreenMode
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Mode32
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class OnMonitorRole
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class PIPPosition
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Default
        {
            public FullscreenMode FullscreenMode { get; set; }
            public Mode32 Mode { get; set; }
            public OnMonitorRole OnMonitorRole { get; set; }
            public PIPPosition PIPPosition { get; set; }
        }

        public class Duration
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class Mode33
        {
            public string valueSpaceRef { get; set; }
            public string Value { get; set; }
        }

        public class OnCall
        {
            public Duration Duration { get; set; }
            public Mode33 Mode { get; set; }
        }

        public class Selfview
        {
            public Default Default { get; set; }
            public OnCall OnCall { get; set; }
        }

        public class Video2
        {
            public DefaultMainSource DefaultMainSource { get; set; }
            public Input2 Input { get; set; }
            public Monitors Monitors { get; set; }
            public Output3 Output { get; set; }
            public Presentation Presentation { get; set; }
            public Selfview Selfview { get; set; }
        }

        public class Configuration
        {
            public Audio Audio { get; set; }
            public Cameras Cameras { get; set; }
            public Conference Conference { get; set; }
            public H323 H323 { get; set; }
            public List<Network> Network { get; set; }
            public NetworkServices NetworkServices { get; set; }
            public Peripherals Peripherals { get; set; }
            public Phonebook Phonebook { get; set; }
            public Provisioning Provisioning { get; set; }
            public Proximity Proximity { get; set; }
            public RoomAnalytics RoomAnalytics { get; set; }
            public SIP2 SIP { get; set; }
            public SerialPort SerialPort { get; set; }
            public Standby Standby { get; set; }
            public SystemUnit SystemUnit { get; set; }
            public Time Time { get; set; }
            public UserInterface UserInterface { get; set; }
            public UserManagement UserManagement { get; set; }
            public Video2 Video { get; set; }
        }

        public class RootObject
        {
            public Configuration Configuration { get; set; }
        }
    }
}