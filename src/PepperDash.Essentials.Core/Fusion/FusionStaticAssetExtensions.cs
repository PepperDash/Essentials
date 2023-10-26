using Crestron.SimplSharpPro.Fusion;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.Fusion
{
    /// <summary>
    /// Extensions to enhance Fusion room, asset and signal creation.
    /// </summary>
    public static class FusionStaticAssetExtensions
    {
        /// <summary>
        /// Tries to set a Fusion asset with the make and model of a device.
        /// If the provided Device is IMakeModel, will set the corresponding parameters on the fusion static asset.
        /// Otherwise, does nothing.
        /// </summary>
        public static void TrySetMakeModel(this FusionStaticAsset asset, Device device)
        {
            var mm = device as IMakeModel;
            if (mm != null)
            {
                asset.ParamMake.Value  = mm.DeviceMake;
                asset.ParamModel.Value = mm.DeviceModel;
            }
        }

        /// <summary>
        /// Tries to attach the AssetError input on a Fusion asset to a Device's
        /// CommunicationMonitor.StatusChange event. Does nothing if the device is not 
        /// IStatusMonitor
        /// </summary>
        /// <param name="asset"></param>
        /// <param name="device"></param>
        public static void TryLinkAssetErrorToCommunication(this FusionStaticAsset asset, Device device)
        {
            if (device is ICommunicationMonitor)
            {
                var monitor = (device as ICommunicationMonitor).CommunicationMonitor;
                monitor.StatusChange += (o, a) =>
                {
                    // Link connected and error inputs on asset
                    asset.Connected.InputSig.BoolValue    = a.Status == MonitorStatus.IsOk;
                    asset.AssetError.InputSig.StringValue = a.Status.ToString();
                };
                // set current value
                asset.Connected.InputSig.BoolValue    = monitor.Status == MonitorStatus.IsOk;
                asset.AssetError.InputSig.StringValue = monitor.Status.ToString();
            }
        }
    }
}