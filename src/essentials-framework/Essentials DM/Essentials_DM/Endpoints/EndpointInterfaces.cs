using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints;
using PepperDash.Essentials.Core;

namespace PepperDash_Essentials_DM
{
    public interface IHasDmInHdcpSet
    {
        void SetDmInHdcpState(eHdcpCapabilityType hdcpState);
    }

    public interface IHasDmInHdcpGet
    {
        IntFeedback DmInHdcpStateFeedback { get; }
    }

    public interface IHasDmInHdcp : IHasDmInHdcpGet, IHasDmInHdcpSet
    {
        eHdcpCapabilityType DmInHdcpCapability { get; }
    }


    public interface IHasHdmiInHdcpSet
    {
        void SetHdmiInHdcpState(eHdcpCapabilityType hdcpState);
    }

    public interface IHasHdmiInHdcpGet
    {
        IntFeedback HdmiInHdcpStateFeedback { get; }
    }

    public interface IHasHdmiInHdcp : IHasHdmiInHdcpGet, IHasHdmiInHdcpSet
    {
        eHdcpCapabilityType HdmiInHdcpCapability { get; }
    }


    public interface IHasHdmiIn1HdcpSet
    {
        void SetHdmiIn1HdcpState(eHdcpCapabilityType hdcpState);
    }

    public interface IHasHdmiIn1HdcpGet
    {
        IntFeedback HdmiIn1HdcpStateFeedback { get; }
    }

    public interface IHasHdmiIn1Hdcp : IHasHdmiIn1HdcpGet, IHasHdmiIn1HdcpSet
    {
        eHdcpCapabilityType HdmiIn1HdcpCapability { get; }
    }


    public interface IHasHdmiIn2HdcpSet
    {
        void SetHdmiIn2HdcpState(eHdcpCapabilityType hdcpState);
    }

    public interface IHasHdmiIn2HdcpGet
    {
        IntFeedback HdmiInIn2HdcpStateFeedback { get; }
    }

    public interface IHasHdmi2InHdcp : IHasHdmiIn2HdcpGet, IHasHdmiIn2HdcpSet
    {
        eHdcpCapabilityType Hdmi2InHdcpCapability { get; }
    }



    public interface IHasDisplayPortInHdcpGet
    {
        IntFeedback DisplayPortInHdcpStateFeedback { get; }
    }

    public interface IHasDisplayPortInHdcpSet
    {
        void SetDisplayPortInHdcpState(eHdcpCapabilityType hdcpState);
    }

    public interface IHasDisplayPortInHdcp : IHasDisplayPortInHdcpGet, IHasDisplayPortInHdcpSet
    {
        eHdcpCapabilityType DisplayPortInHdcpCapability { get; }
    }
}