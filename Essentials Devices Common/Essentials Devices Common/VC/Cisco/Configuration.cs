using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Devices.VideoCodec.Cisco
{
    public class CiscoCodecConfiguration
    {
        //[XmlRoot(ElementName = "DefaultVolume")]
        public class DefaultVolume
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Mode")]
        public class Mode
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Microphone")]
        public class Microphone
        {
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
            //[XmlAttribute(AttributeName = "item")]
            public string Item { get; set; }
            //[XmlAttribute(AttributeName = "maxOccurrence")]
            public string MaxOccurrence { get; set; }
            //[XmlElement(ElementName = "EchoControl")]
            public EchoControl EchoControl { get; set; }
            //[XmlElement(ElementName = "Level")]
            public Level Level { get; set; }
        }

        //[XmlRoot(ElementName = "Dereverberation")]
        public class Dereverberation
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "NoiseReduction")]
        public class NoiseReduction
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "EchoControl")]
        public class EchoControl
        {
            //[XmlElement(ElementName = "Dereverberation")]
            public Dereverberation Dereverberation { get; set; }
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
            //[XmlElement(ElementName = "NoiseReduction")]
            public NoiseReduction NoiseReduction { get; set; }
        }

        //[XmlRoot(ElementName = "Level")]
        public class Level
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Input")]
        public class Input
        {
            //[XmlElement(ElementName = "Microphone")]
            public List<Microphone> Microphone { get; set; }
            //[XmlElement(ElementName = "Connector")]
            public List<Connector> Connector { get; set; }
        }

        //[XmlRoot(ElementName = "Enabled")]
        public class Enabled
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Mute")]
        public class Mute
        {
            //[XmlElement(ElementName = "Enabled")]
            public Enabled Enabled { get; set; }
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Microphones")]
        public class Microphones
        {
            //[XmlElement(ElementName = "Mute")]
            public Mute Mute { get; set; }
        }

        //[XmlRoot(ElementName = "InternalSpeaker")]
        public class InternalSpeaker
        {
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
        }

        //[XmlRoot(ElementName = "OutputType")]
        public class OutputType
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Line")]
        public class Line
        {
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
            //[XmlElement(ElementName = "OutputType")]
            public OutputType OutputType { get; set; }
            //[XmlAttribute(AttributeName = "item")]
            public string Item { get; set; }
            //[XmlAttribute(AttributeName = "maxOccurrence")]
            public string MaxOccurrence { get; set; }
        }

        //[XmlRoot(ElementName = "Output")]
        public class Output
        {
            //[XmlElement(ElementName = "InternalSpeaker")]
            public InternalSpeaker InternalSpeaker { get; set; }
            //[XmlElement(ElementName = "Line")]
            public Line Line { get; set; }
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
            //[XmlElement(ElementName = "Connector")]
            public List<Connector> Connector { get; set; }
        }

        //[XmlRoot(ElementName = "RingTone")]
        public class RingTone
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "RingVolume")]
        public class RingVolume
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "SoundsAndAlerts")]
        public class SoundsAndAlerts
        {
            //[XmlElement(ElementName = "RingTone")]
            public RingTone RingTone { get; set; }
            //[XmlElement(ElementName = "RingVolume")]
            public RingVolume RingVolume { get; set; }
        }

        //[XmlRoot(ElementName = "Audio")]
        public class Audio
        {
            //[XmlElement(ElementName = "DefaultVolume")]
            public DefaultVolume DefaultVolume { get; set; }
            //[XmlElement(ElementName = "Input")]
            public Input Input { get; set; }
            //[XmlElement(ElementName = "Microphones")]
            public Microphones Microphones { get; set; }
            //[XmlElement(ElementName = "Output")]
            public Output Output { get; set; }
            //[XmlElement(ElementName = "SoundsAndAlerts")]
            public SoundsAndAlerts SoundsAndAlerts { get; set; }
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Framerate")]
        public class Framerate
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Camera")]
        public class Camera
        {
            //[XmlElement(ElementName = "Framerate")]
            public Framerate Framerate { get; set; }
        }

        //[XmlRoot(ElementName = "Closeup")]
        public class Closeup
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "SpeakerTrack")]
        public class SpeakerTrack
        {
            //[XmlElement(ElementName = "Closeup")]
            public Closeup Closeup { get; set; }
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
        }

        //[XmlRoot(ElementName = "Cameras")]
        public class Cameras
        {
            //[XmlElement(ElementName = "Camera")]
            public Camera Camera { get; set; }
            //[XmlElement(ElementName = "SpeakerTrack")]
            public SpeakerTrack SpeakerTrack { get; set; }
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Delay")]
        public class Delay
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "AutoAnswer")]
        public class AutoAnswer
        {
            //[XmlElement(ElementName = "Delay")]
            public Delay Delay { get; set; }
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
            //[XmlElement(ElementName = "Mute")]
            public Mute Mute { get; set; }
        }

        //[XmlRoot(ElementName = "Protocol")]
        public class Protocol
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Rate")]
        public class Rate
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "DefaultCall")]
        public class DefaultCall
        {
            //[XmlElement(ElementName = "Protocol")]
            public Protocol Protocol { get; set; }
            //[XmlElement(ElementName = "Rate")]
            public Rate Rate { get; set; }
        }

        //[XmlRoot(ElementName = "DefaultTimeout")]
        public class DefaultTimeout
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "DoNotDisturb")]
        public class DoNotDisturb
        {
            //[XmlElement(ElementName = "DefaultTimeout")]
            public DefaultTimeout DefaultTimeout { get; set; }
        }

        //[XmlRoot(ElementName = "Encryption")]
        public class Encryption
        {
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
            //[XmlElement(ElementName = "KeySize")]
            public KeySize KeySize { get; set; }
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "FarEndControl")]
        public class FarEndControl
        {
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
        }

        //[XmlRoot(ElementName = "MaxReceiveCallRate")]
        public class MaxReceiveCallRate
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "MaxTotalReceiveCallRate")]
        public class MaxTotalReceiveCallRate
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "MaxTotalTransmitCallRate")]
        public class MaxTotalTransmitCallRate
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "MaxTransmitCallRate")]
        public class MaxTransmitCallRate
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "MultiStream")]
        public class MultiStream
        {
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
        }

        //[XmlRoot(ElementName = "Conference")]
        public class Conference
        {
            //[XmlElement(ElementName = "AutoAnswer")]
            public AutoAnswer AutoAnswer { get; set; }
            //[XmlElement(ElementName = "DefaultCall")]
            public DefaultCall DefaultCall { get; set; }
            //[XmlElement(ElementName = "DoNotDisturb")]
            public DoNotDisturb DoNotDisturb { get; set; }
            //[XmlElement(ElementName = "Encryption")]
            public Encryption Encryption { get; set; }
            //[XmlElement(ElementName = "FarEndControl")]
            public FarEndControl FarEndControl { get; set; }
            //[XmlElement(ElementName = "MaxReceiveCallRate")]
            public MaxReceiveCallRate MaxReceiveCallRate { get; set; }
            //[XmlElement(ElementName = "MaxTotalReceiveCallRate")]
            public MaxTotalReceiveCallRate MaxTotalReceiveCallRate { get; set; }
            //[XmlElement(ElementName = "MaxTotalTransmitCallRate")]
            public MaxTotalTransmitCallRate MaxTotalTransmitCallRate { get; set; }
            //[XmlElement(ElementName = "MaxTransmitCallRate")]
            public MaxTransmitCallRate MaxTransmitCallRate { get; set; }
            //[XmlElement(ElementName = "MultiStream")]
            public MultiStream MultiStream { get; set; }
        }

        //[XmlRoot(ElementName = "LoginName")]
        public class LoginName
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
        }

        //[XmlRoot(ElementName = "Password")]
        public class Password
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Authentication")]
        public class Authentication
        {
            //[XmlElement(ElementName = "LoginName")]
            public LoginName LoginName { get; set; }
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
            //[XmlElement(ElementName = "Password")]
            public Password Password { get; set; }
            //[XmlElement(ElementName = "UserName")]
            public UserName UserName { get; set; }
        }

        //[XmlRoot(ElementName = "CallSetup")]
        public class CallSetup
        {
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
        }

        //[XmlRoot(ElementName = "KeySize")]
        public class KeySize
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Address")]
        public class Address
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Gatekeeper")]
        public class Gatekeeper
        {
            //[XmlElement(ElementName = "Address")]
            public Address Address { get; set; }
        }

        //[XmlRoot(ElementName = "E164")]
        public class E164
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "ID")]
        public class ID
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "H323Alias")]
        public class H323Alias
        {
            //[XmlElement(ElementName = "E164")]
            public E164 E164 { get; set; }
            //[XmlElement(ElementName = "ID")]
            public ID ID { get; set; }
        }

        //[XmlRoot(ElementName = "NAT")]
        public class NAT
        {
            //[XmlElement(ElementName = "Address")]
            public Address Address { get; set; }
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
        }

        //[XmlRoot(ElementName = "H323")]
        public class H323
        {
            //[XmlElement(ElementName = "Authentication")]
            public Authentication Authentication { get; set; }
            //[XmlElement(ElementName = "CallSetup")]
            public CallSetup CallSetup { get; set; }
            //[XmlElement(ElementName = "Encryption")]
            public Encryption Encryption { get; set; }
            //[XmlElement(ElementName = "Gatekeeper")]
            public Gatekeeper Gatekeeper { get; set; }
            //[XmlElement(ElementName = "H323Alias")]
            public H323Alias H323Alias { get; set; }
            //[XmlElement(ElementName = "NAT")]
            public NAT NAT { get; set; }
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
        }

        //[XmlRoot(ElementName = "Name")]
        public class Name
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Domain")]
        public class Domain
        {
            //[XmlElement(ElementName = "Name")]
            public Name Name { get; set; }
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
        }

        //[XmlRoot(ElementName = "Server")]
        public class Server
        {
            //[XmlElement(ElementName = "Address")]
            public Address Address { get; set; }
            //[XmlAttribute(AttributeName = "item")]
            public string Item { get; set; }
            //[XmlAttribute(AttributeName = "maxOccurrence")]
            public string MaxOccurrence { get; set; }
            //[XmlElement(ElementName = "MinimumTLSVersion")]
            public MinimumTLSVersion MinimumTLSVersion { get; set; }
            //[XmlElement(ElementName = "ID")]
            public ID ID { get; set; }
            //[XmlElement(ElementName = "Type")]
            public Type Type { get; set; }
            //[XmlElement(ElementName = "URL")]
            public URL URL { get; set; }
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlElement(ElementName = "Port")]
            public Port Port { get; set; }
        }

        //[XmlRoot(ElementName = "DNS")]
        public class DNS
        {
            //[XmlElement(ElementName = "Domain")]
            public Domain Domain { get; set; }
            //[XmlElement(ElementName = "Server")]
            public List<Server> Server { get; set; }
        }

        //[XmlRoot(ElementName = "AnonymousIdentity")]
        public class AnonymousIdentity
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
        }

        //[XmlRoot(ElementName = "Md5")]
        public class Md5
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Peap")]
        public class Peap
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Tls")]
        public class Tls
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Ttls")]
        public class Ttls
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Eap")]
        public class Eap
        {
            //[XmlElement(ElementName = "Md5")]
            public Md5 Md5 { get; set; }
            //[XmlElement(ElementName = "Peap")]
            public Peap Peap { get; set; }
            //[XmlElement(ElementName = "Tls")]
            public Tls Tls { get; set; }
            //[XmlElement(ElementName = "Ttls")]
            public Ttls Ttls { get; set; }
        }

        //[XmlRoot(ElementName = "Identity")]
        public class Identity
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
        }

        //[XmlRoot(ElementName = "TlsVerify")]
        public class TlsVerify
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "UseClientCertificate")]
        public class UseClientCertificate
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "IEEE8021X")]
        public class IEEE8021X
        {
            //[XmlElement(ElementName = "AnonymousIdentity")]
            public AnonymousIdentity AnonymousIdentity { get; set; }
            //[XmlElement(ElementName = "Eap")]
            public Eap Eap { get; set; }
            //[XmlElement(ElementName = "Identity")]
            public Identity Identity { get; set; }
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
            //[XmlElement(ElementName = "Password")]
            public Password Password { get; set; }
            //[XmlElement(ElementName = "TlsVerify")]
            public TlsVerify TlsVerify { get; set; }
            //[XmlElement(ElementName = "UseClientCertificate")]
            public UseClientCertificate UseClientCertificate { get; set; }
        }

        //[XmlRoot(ElementName = "IPStack")]
        public class IPStack
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Assignment")]
        public class Assignment
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Gateway")]
        public class Gateway
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "SubnetMask")]
        public class SubnetMask
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "IPv4")]
        public class IPv4
        {
            //[XmlElement(ElementName = "Address")]
            public Address Address { get; set; }
            //[XmlElement(ElementName = "Assignment")]
            public Assignment Assignment { get; set; }
            //[XmlElement(ElementName = "Gateway")]
            public Gateway Gateway { get; set; }
            //[XmlElement(ElementName = "SubnetMask")]
            public SubnetMask SubnetMask { get; set; }
        }

        //[XmlRoot(ElementName = "DHCPOptions")]
        public class DHCPOptions
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "IPv6")]
        public class IPv6
        {
            //[XmlElement(ElementName = "Address")]
            public Address Address { get; set; }
            //[XmlElement(ElementName = "Assignment")]
            public Assignment Assignment { get; set; }
            //[XmlElement(ElementName = "DHCPOptions")]
            public DHCPOptions DHCPOptions { get; set; }
            //[XmlElement(ElementName = "Gateway")]
            public Gateway Gateway { get; set; }
        }

        //[XmlRoot(ElementName = "MTU")]
        public class MTU
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Data")]
        public class Data
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "ICMPv6")]
        public class ICMPv6
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "NTP")]
        public class NTP
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
            //[XmlElement(ElementName = "Server")]
            public List<Server> Server { get; set; }
        }

        //[XmlRoot(ElementName = "Signalling")]
        public class Signalling
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Video")]
        public class Video
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
            //[XmlElement(ElementName = "DefaultMainSource")]
            public DefaultMainSource DefaultMainSource { get; set; }
            //[XmlElement(ElementName = "Input")]
            public Input Input { get; set; }
            //[XmlElement(ElementName = "Monitors")]
            public Monitors Monitors { get; set; }
            //[XmlElement(ElementName = "Output")]
            public Output Output { get; set; }
            //[XmlElement(ElementName = "Presentation")]
            public Presentation Presentation { get; set; }
            //[XmlElement(ElementName = "Selfview")]
            public Selfview Selfview { get; set; }
        }

        //[XmlRoot(ElementName = "Diffserv")]
        public class Diffserv
        {
            //[XmlElement(ElementName = "Audio")]
            public Audio Audio { get; set; }
            //[XmlElement(ElementName = "Data")]
            public Data Data { get; set; }
            //[XmlElement(ElementName = "ICMPv6")]
            public ICMPv6 ICMPv6 { get; set; }
            //[XmlElement(ElementName = "NTP")]
            public NTP NTP { get; set; }
            //[XmlElement(ElementName = "Signalling")]
            public Signalling Signalling { get; set; }
            //[XmlElement(ElementName = "Video")]
            public Video Video { get; set; }
        }

        //[XmlRoot(ElementName = "QoS")]
        public class QoS
        {
            //[XmlElement(ElementName = "Diffserv")]
            public Diffserv Diffserv { get; set; }
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
        }

        //[XmlRoot(ElementName = "Allow")]
        public class Allow
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
        }

        //[XmlRoot(ElementName = "RemoteAccess")]
        public class RemoteAccess
        {
            //[XmlElement(ElementName = "Allow")]
            public Allow Allow { get; set; }
        }

        //[XmlRoot(ElementName = "Speed")]
        public class Speed
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "VlanId")]
        public class VlanId
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Voice")]
        public class Voice
        {
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
            //[XmlElement(ElementName = "VlanId")]
            public VlanId VlanId { get; set; }
        }

        //[XmlRoot(ElementName = "VLAN")]
        public class VLAN
        {
            //[XmlElement(ElementName = "Voice")]
            public Voice Voice { get; set; }
        }

        //[XmlRoot(ElementName = "Network")]
        public class Network
        {
            //[XmlElement(ElementName = "DNS")]
            public DNS DNS { get; set; }
            //[XmlElement(ElementName = "IEEE8021X")]
            public IEEE8021X IEEE8021X { get; set; }
            //[XmlElement(ElementName = "IPStack")]
            public IPStack IPStack { get; set; }
            //[XmlElement(ElementName = "IPv4")]
            public IPv4 IPv4 { get; set; }
            //[XmlElement(ElementName = "IPv6")]
            public IPv6 IPv6 { get; set; }
            //[XmlElement(ElementName = "MTU")]
            public MTU MTU { get; set; }
            //[XmlElement(ElementName = "QoS")]
            public QoS QoS { get; set; }
            //[XmlElement(ElementName = "RemoteAccess")]
            public RemoteAccess RemoteAccess { get; set; }
            //[XmlElement(ElementName = "Speed")]
            public Speed Speed { get; set; }
            //[XmlElement(ElementName = "VLAN")]
            public VLAN VLAN { get; set; }
            //[XmlAttribute(AttributeName = "item")]
            public string Item { get; set; }
            //[XmlAttribute(AttributeName = "maxOccurrence")]
            public string MaxOccurrence { get; set; }
        }

        //[XmlRoot(ElementName = "CDP")]
        public class CDP
        {
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
        }

        //[XmlRoot(ElementName = "HTTP")]
        public class HTTP
        {
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
        }

        //[XmlRoot(ElementName = "MinimumTLSVersion")]
        public class MinimumTLSVersion
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "StrictTransportSecurity")]
        public class StrictTransportSecurity
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "VerifyClientCertificate")]
        public class VerifyClientCertificate
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "VerifyServerCertificate")]
        public class VerifyServerCertificate
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "HTTPS")]
        public class HTTPS
        {
            //[XmlElement(ElementName = "Server")]
            public Server Server { get; set; }
            //[XmlElement(ElementName = "StrictTransportSecurity")]
            public StrictTransportSecurity StrictTransportSecurity { get; set; }
            //[XmlElement(ElementName = "VerifyClientCertificate")]
            public VerifyClientCertificate VerifyClientCertificate { get; set; }
            //[XmlElement(ElementName = "VerifyServerCertificate")]
            public VerifyServerCertificate VerifyServerCertificate { get; set; }
        }

        //[XmlRoot(ElementName = "SIP")]
        public class SIP
        {
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
            //[XmlElement(ElementName = "Authentication")]
            public Authentication Authentication { get; set; }
            //[XmlElement(ElementName = "DefaultTransport")]
            public DefaultTransport DefaultTransport { get; set; }
            //[XmlElement(ElementName = "DisplayName")]
            public DisplayName DisplayName { get; set; }
            //[XmlElement(ElementName = "Ice")]
            public Ice Ice { get; set; }
            //[XmlElement(ElementName = "ListenPort")]
            public ListenPort ListenPort { get; set; }
            //[XmlElement(ElementName = "Proxy")]
            public List<Proxy> Proxy { get; set; }
            //[XmlElement(ElementName = "Turn")]
            public Turn Turn { get; set; }
            //[XmlElement(ElementName = "URI")]
            public URI URI { get; set; }
        }

        //[XmlRoot(ElementName = "CommunityName")]
        public class CommunityName
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Host")]
        public class Host
        {
            //[XmlElement(ElementName = "Address")]
            public Address Address { get; set; }
            //[XmlAttribute(AttributeName = "item")]
            public string Item { get; set; }
            //[XmlAttribute(AttributeName = "maxOccurrence")]
            public string MaxOccurrence { get; set; }
        }

        //[XmlRoot(ElementName = "SystemContact")]
        public class SystemContact
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
        }

        //[XmlRoot(ElementName = "SystemLocation")]
        public class SystemLocation
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
        }

        //[XmlRoot(ElementName = "SNMP")]
        public class SNMP
        {
            //[XmlElement(ElementName = "CommunityName")]
            public CommunityName CommunityName { get; set; }
            //[XmlElement(ElementName = "Host")]
            public List<Host> Host { get; set; }
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
            //[XmlElement(ElementName = "SystemContact")]
            public SystemContact SystemContact { get; set; }
            //[XmlElement(ElementName = "SystemLocation")]
            public SystemLocation SystemLocation { get; set; }
        }

        //[XmlRoot(ElementName = "SSH")]
        public class SSH
        {
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
        }

        //[XmlRoot(ElementName = "UPnP")]
        public class UPnP
        {
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
        }

        //[XmlRoot(ElementName = "WelcomeText")]
        public class WelcomeText
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "NetworkServices")]
        public class NetworkServices
        {
            //[XmlElement(ElementName = "CDP")]
            public CDP CDP { get; set; }
            //[XmlElement(ElementName = "H323")]
            public H323 H323 { get; set; }
            //[XmlElement(ElementName = "HTTP")]
            public HTTP HTTP { get; set; }
            //[XmlElement(ElementName = "HTTPS")]
            public HTTPS HTTPS { get; set; }
            //[XmlElement(ElementName = "NTP")]
            public NTP NTP { get; set; }
            //[XmlElement(ElementName = "SIP")]
            public SIP SIP { get; set; }
            //[XmlElement(ElementName = "SNMP")]
            public SNMP SNMP { get; set; }
            //[XmlElement(ElementName = "SSH")]
            public SSH SSH { get; set; }
            //[XmlElement(ElementName = "UPnP")]
            public UPnP UPnP { get; set; }
            //[XmlElement(ElementName = "WelcomeText")]
            public WelcomeText WelcomeText { get; set; }
        }

        //[XmlRoot(ElementName = "ControlSystems")]
        public class ControlSystems
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "TouchPanels")]
        public class TouchPanels
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Profile")]
        public class Profile
        {
            //[XmlElement(ElementName = "Cameras")]
            public Cameras Cameras { get; set; }
            //[XmlElement(ElementName = "ControlSystems")]
            public ControlSystems ControlSystems { get; set; }
            //[XmlElement(ElementName = "TouchPanels")]
            public TouchPanels TouchPanels { get; set; }
        }

        //[XmlRoot(ElementName = "Peripherals")]
        public class Peripherals
        {
            //[XmlElement(ElementName = "Profile")]
            public Profile Profile { get; set; }
        }

        //[XmlRoot(ElementName = "Type")]
        public class Type
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "URL")]
        public class URL
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Phonebook")]
        public class Phonebook
        {
            //[XmlElement(ElementName = "Server")]
            public Server Server { get; set; }
        }

        //[XmlRoot(ElementName = "Connectivity")]
        public class Connectivity
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "AlternateAddress")]
        public class AlternateAddress
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
        }

        //[XmlRoot(ElementName = "Path")]
        public class Path
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "ExternalManager")]
        public class ExternalManager
        {
            //[XmlElement(ElementName = "Address")]
            public Address Address { get; set; }
            //[XmlElement(ElementName = "AlternateAddress")]
            public AlternateAddress AlternateAddress { get; set; }
            //[XmlElement(ElementName = "Domain")]
            public Domain Domain { get; set; }
            //[XmlElement(ElementName = "Path")]
            public Path Path { get; set; }
            //[XmlElement(ElementName = "Protocol")]
            public Protocol Protocol { get; set; }
        }

        //[XmlRoot(ElementName = "HttpMethod")]
        public class HttpMethod
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Provisioning")]
        public class Provisioning
        {
            //[XmlElement(ElementName = "Connectivity")]
            public Connectivity Connectivity { get; set; }
            //[XmlElement(ElementName = "ExternalManager")]
            public ExternalManager ExternalManager { get; set; }
            //[XmlElement(ElementName = "HttpMethod")]
            public HttpMethod HttpMethod { get; set; }
            //[XmlElement(ElementName = "LoginName")]
            public LoginName LoginName { get; set; }
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
            //[XmlElement(ElementName = "Password")]
            public Password Password { get; set; }
        }

        //[XmlRoot(ElementName = "CallControl")]
        public class CallControl
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "FromClients")]
        public class FromClients
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "ToClients")]
        public class ToClients
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "ContentShare")]
        public class ContentShare
        {
            //[XmlElement(ElementName = "FromClients")]
            public FromClients FromClients { get; set; }
            //[XmlElement(ElementName = "ToClients")]
            public ToClients ToClients { get; set; }
        }

        //[XmlRoot(ElementName = "Services")]
        public class Services
        {
            //[XmlElement(ElementName = "CallControl")]
            public CallControl CallControl { get; set; }
            //[XmlElement(ElementName = "ContentShare")]
            public ContentShare ContentShare { get; set; }
        }

        //[XmlRoot(ElementName = "Proximity")]
        public class Proximity
        {
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
            //[XmlElement(ElementName = "Services")]
            public Services Services { get; set; }
        }

        //[XmlRoot(ElementName = "PeopleCountOutOfCall")]
        public class PeopleCountOutOfCall
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "PeoplePresenceDetector")]
        public class PeoplePresenceDetector
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "RoomAnalytics")]
        public class RoomAnalytics
        {
            //[XmlElement(ElementName = "PeopleCountOutOfCall")]
            public PeopleCountOutOfCall PeopleCountOutOfCall { get; set; }
            //[XmlElement(ElementName = "PeoplePresenceDetector")]
            public PeoplePresenceDetector PeoplePresenceDetector { get; set; }
        }

        //[XmlRoot(ElementName = "UserName")]
        public class UserName
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
        }

        //[XmlRoot(ElementName = "DefaultTransport")]
        public class DefaultTransport
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "DisplayName")]
        public class DisplayName
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
        }

        //[XmlRoot(ElementName = "DefaultCandidate")]
        public class DefaultCandidate
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Ice")]
        public class Ice
        {
            //[XmlElement(ElementName = "DefaultCandidate")]
            public DefaultCandidate DefaultCandidate { get; set; }
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
        }

        //[XmlRoot(ElementName = "ListenPort")]
        public class ListenPort
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Proxy")]
        public class Proxy
        {
            //[XmlElement(ElementName = "Address")]
            public Address Address { get; set; }
            //[XmlAttribute(AttributeName = "item")]
            public string Item { get; set; }
            //[XmlAttribute(AttributeName = "maxOccurrence")]
            public string MaxOccurrence { get; set; }
        }

        //[XmlRoot(ElementName = "Turn")]
        public class Turn
        {
            //[XmlElement(ElementName = "Password")]
            public Password Password { get; set; }
            //[XmlElement(ElementName = "Server")]
            public Server Server { get; set; }
            //[XmlElement(ElementName = "UserName")]
            public UserName UserName { get; set; }
        }

        //[XmlRoot(ElementName = "URI")]
        public class URI
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
        }

        //[XmlRoot(ElementName = "BaudRate")]
        public class BaudRate
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "LoginRequired")]
        public class LoginRequired
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "SerialPort")]
        public class SerialPort
        {
            //[XmlElement(ElementName = "BaudRate")]
            public BaudRate BaudRate { get; set; }
            //[XmlElement(ElementName = "LoginRequired")]
            public LoginRequired LoginRequired { get; set; }
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
        }

        //[XmlRoot(ElementName = "BootAction")]
        public class BootAction
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Control")]
        public class Control
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "StandbyAction")]
        public class StandbyAction
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "WakeupAction")]
        public class WakeupAction
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Standby")]
        public class Standby
        {
            //[XmlElement(ElementName = "BootAction")]
            public BootAction BootAction { get; set; }
            //[XmlElement(ElementName = "Control")]
            public Control Control { get; set; }
            //[XmlElement(ElementName = "Delay")]
            public Delay Delay { get; set; }
            //[XmlElement(ElementName = "StandbyAction")]
            public StandbyAction StandbyAction { get; set; }
            //[XmlElement(ElementName = "WakeupAction")]
            public WakeupAction WakeupAction { get; set; }
        }

        //[XmlRoot(ElementName = "SystemUnit")]
        public class SystemUnit
        {
            //[XmlElement(ElementName = "Name")]
            public Name Name { get; set; }
        }

        //[XmlRoot(ElementName = "DateFormat")]
        public class DateFormat
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "TimeFormat")]
        public class TimeFormat
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Zone")]
        public class Zone
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Time")]
        public class Time
        {
            //[XmlElement(ElementName = "DateFormat")]
            public DateFormat DateFormat { get; set; }
            //[XmlElement(ElementName = "TimeFormat")]
            public TimeFormat TimeFormat { get; set; }
            //[XmlElement(ElementName = "Zone")]
            public Zone Zone { get; set; }
        }

        //[XmlRoot(ElementName = "ContactInfo")]
        public class ContactInfo
        {
            //[XmlElement(ElementName = "Type")]
            public Type Type { get; set; }
        }

        //[XmlRoot(ElementName = "KeyTones")]
        public class KeyTones
        {
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
        }

        //[XmlRoot(ElementName = "Language")]
        public class Language
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "OSD")]
        public class OSD
        {
            //[XmlElement(ElementName = "Output")]
            public Output Output { get; set; }
        }

        //[XmlRoot(ElementName = "UserInterface")]
        public class UserInterface
        {
            //[XmlElement(ElementName = "ContactInfo")]
            public ContactInfo ContactInfo { get; set; }
            //[XmlElement(ElementName = "KeyTones")]
            public KeyTones KeyTones { get; set; }
            //[XmlElement(ElementName = "Language")]
            public Language Language { get; set; }
            //[XmlElement(ElementName = "OSD")]
            public OSD OSD { get; set; }
        }

        //[XmlRoot(ElementName = "Filter")]
        public class Filter
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
        }

        //[XmlRoot(ElementName = "Group")]
        public class Group
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
        }

        //[XmlRoot(ElementName = "Admin")]
        public class Admin
        {
            //[XmlElement(ElementName = "Filter")]
            public Filter Filter { get; set; }
            //[XmlElement(ElementName = "Group")]
            public Group Group { get; set; }
        }

        //[XmlRoot(ElementName = "Attribute")]
        public class Attribute
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
        }

        //[XmlRoot(ElementName = "BaseDN")]
        public class BaseDN
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
        }

        //[XmlRoot(ElementName = "Port")]
        public class Port
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "LDAP")]
        public class LDAP
        {
            //[XmlElement(ElementName = "Admin")]
            public Admin Admin { get; set; }
            //[XmlElement(ElementName = "Attribute")]
            public Attribute Attribute { get; set; }
            //[XmlElement(ElementName = "BaseDN")]
            public BaseDN BaseDN { get; set; }
            //[XmlElement(ElementName = "Encryption")]
            public Encryption Encryption { get; set; }
            //[XmlElement(ElementName = "MinimumTLSVersion")]
            public MinimumTLSVersion MinimumTLSVersion { get; set; }
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
            //[XmlElement(ElementName = "Server")]
            public Server Server { get; set; }
            //[XmlElement(ElementName = "VerifyServerCertificate")]
            public VerifyServerCertificate VerifyServerCertificate { get; set; }
        }

        //[XmlRoot(ElementName = "UserManagement")]
        public class UserManagement
        {
            //[XmlElement(ElementName = "LDAP")]
            public LDAP LDAP { get; set; }
        }

        //[XmlRoot(ElementName = "DefaultMainSource")]
        public class DefaultMainSource
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "CameraId")]
        public class CameraId
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "CameraControl")]
        public class CameraControl
        {
            //[XmlElement(ElementName = "CameraId")]
            public CameraId CameraId { get; set; }
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
        }

        //[XmlRoot(ElementName = "InputSourceType")]
        public class InputSourceType
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Visibility")]
        public class Visibility
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Connector")]
        public class Connector
        {
            //[XmlElement(ElementName = "CameraControl")]
            public CameraControl CameraControl { get; set; }
            //[XmlElement(ElementName = "InputSourceType")]
            public InputSourceType InputSourceType { get; set; }
            //[XmlElement(ElementName = "Name")]
            public Name Name { get; set; }
            //[XmlElement(ElementName = "Visibility")]
            public Visibility Visibility { get; set; }
            //[XmlAttribute(AttributeName = "item")]
            public string Item { get; set; }
            //[XmlAttribute(AttributeName = "maxOccurrence")]
            public string MaxOccurrence { get; set; }
            //[XmlElement(ElementName = "PreferredResolution")]
            public PreferredResolution PreferredResolution { get; set; }
            //[XmlElement(ElementName = "PresentationSelection")]
            public PresentationSelection PresentationSelection { get; set; }
            //[XmlElement(ElementName = "Quality")]
            public Quality Quality { get; set; }
            //[XmlElement(ElementName = "CEC")]
            public CEC CEC { get; set; }
            //[XmlElement(ElementName = "MonitorRole")]
            public MonitorRole MonitorRole { get; set; }
            //[XmlElement(ElementName = "Resolution")]
            public Resolution Resolution { get; set; }
        }

        //[XmlRoot(ElementName = "PreferredResolution")]
        public class PreferredResolution
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "PresentationSelection")]
        public class PresentationSelection
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Quality")]
        public class Quality
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Monitors")]
        public class Monitors
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "CEC")]
        public class CEC
        {
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
        }

        //[XmlRoot(ElementName = "MonitorRole")]
        public class MonitorRole
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Resolution")]
        public class Resolution
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "DefaultSource")]
        public class DefaultSource
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Presentation")]
        public class Presentation
        {
            //[XmlElement(ElementName = "DefaultSource")]
            public DefaultSource DefaultSource { get; set; }
        }

        //[XmlRoot(ElementName = "FullscreenMode")]
        public class FullscreenMode
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "OnMonitorRole")]
        public class OnMonitorRole
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "PIPPosition")]
        public class PIPPosition
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "Default")]
        public class Default
        {
            //[XmlElement(ElementName = "FullscreenMode")]
            public FullscreenMode FullscreenMode { get; set; }
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
            //[XmlElement(ElementName = "OnMonitorRole")]
            public OnMonitorRole OnMonitorRole { get; set; }
            //[XmlElement(ElementName = "PIPPosition")]
            public PIPPosition PIPPosition { get; set; }
        }

        //[XmlRoot(ElementName = "Duration")]
        public class Duration
        {
            //[XmlAttribute(AttributeName = "valueSpaceRef")]
            public string ValueSpaceRef { get; set; }
            //[XmlText]
            public string Text { get; set; }
        }

        //[XmlRoot(ElementName = "OnCall")]
        public class OnCall
        {
            //[XmlElement(ElementName = "Duration")]
            public Duration Duration { get; set; }
            //[XmlElement(ElementName = "Mode")]
            public Mode Mode { get; set; }
        }

        //[XmlRoot(ElementName = "Selfview")]
        public class Selfview
        {
            //[XmlElement(ElementName = "Default")]
            public Default Default { get; set; }
            //[XmlElement(ElementName = "OnCall")]
            public OnCall OnCall { get; set; }
        }

        //[XmlRoot(ElementName = "Configuration")]
        public class Configuration
        {
            //[XmlElement(ElementName = "Audio")]
            public Audio Audio { get; set; }
            //[XmlElement(ElementName = "Cameras")]
            public Cameras Cameras { get; set; }
            //[XmlElement(ElementName = "Conference")]
            public Conference Conference { get; set; }
            //[XmlElement(ElementName = "H323")]
            public H323 H323 { get; set; }
            //[XmlElement(ElementName = "Network")]
            public Network Network { get; set; }
            //[XmlElement(ElementName = "NetworkServices")]
            public NetworkServices NetworkServices { get; set; }
            //[XmlElement(ElementName = "Peripherals")]
            public Peripherals Peripherals { get; set; }
            //[XmlElement(ElementName = "Phonebook")]
            public Phonebook Phonebook { get; set; }
            //[XmlElement(ElementName = "Provisioning")]
            public Provisioning Provisioning { get; set; }
            //[XmlElement(ElementName = "Proximity")]
            public Proximity Proximity { get; set; }
            //[XmlElement(ElementName = "RoomAnalytics")]
            public RoomAnalytics RoomAnalytics { get; set; }
            //[XmlElement(ElementName = "SIP")]
            public SIP SIP { get; set; }
            //[XmlElement(ElementName = "SerialPort")]
            public SerialPort SerialPort { get; set; }
            //[XmlElement(ElementName = "Standby")]
            public Standby Standby { get; set; }
            //[XmlElement(ElementName = "SystemUnit")]
            public SystemUnit SystemUnit { get; set; }
            //[XmlElement(ElementName = "Time")]
            public Time Time { get; set; }
            //[XmlElement(ElementName = "UserInterface")]
            public UserInterface UserInterface { get; set; }
            //[XmlElement(ElementName = "UserManagement")]
            public UserManagement UserManagement { get; set; }
            //[XmlElement(ElementName = "Video")]
            public Video Video { get; set; }
            //[XmlAttribute(AttributeName = "product")]
            public string Product { get; set; }
            //[XmlAttribute(AttributeName = "version")]
            public string Version { get; set; }
            //[XmlAttribute(AttributeName = "apiVersion")]
            public string ApiVersion { get; set; }
        }
    }
}