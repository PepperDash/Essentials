using System;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Fusion;
using PepperDash.Core;

namespace PepperDash.Essentials.Core.Fusion
{
    public static class FusionRoomExtensions
    {
        /// <summary>
        /// Creates and returns a fusion attribute. The join number will match the established Simpl
        /// standard of 50+, and will generate a 50+ join in the RVI. It calls
        /// FusionRoom.AddSig with join number - 49
        /// </summary>
        /// <returns>The new attribute</returns>
        public static BooleanSigData CreateOffsetBoolSig(this FusionRoom fr, uint number, string name, eSigIoMask mask)
        {
            if (number < 50)
            {
                throw new ArgumentOutOfRangeException("number", "Cannot be less than 50");
            }
            number -= 49;
            fr.AddSig(eSigType.Bool, number, name, mask);
            return fr.UserDefinedBooleanSigDetails[number];
        }

        /// <summary>
        /// Creates and returns a fusion attribute. The join number will match the established Simpl
        /// standard of 50+, and will generate a 50+ join in the RVI. It calls
        /// FusionRoom.AddSig with join number - 49
        /// </summary>
        /// <returns>The new attribute</returns>
        public static UShortSigData CreateOffsetUshortSig(this FusionRoom fr, uint number, string name, eSigIoMask mask)
        {
            if (number < 50)
            {
                throw new ArgumentOutOfRangeException("number", "Cannot be less than 50");
            }
            number -= 49;
            fr.AddSig(eSigType.UShort, number, name, mask);
            return fr.UserDefinedUShortSigDetails[number];
        }

        /// <summary>
        /// Creates and returns a fusion attribute. The join number will match the established Simpl
        /// standard of 50+, and will generate a 50+ join in the RVI. It calls
        /// FusionRoom.AddSig with join number - 49
        /// </summary>
        /// <returns>The new attribute</returns>
        public static StringSigData CreateOffsetStringSig(this FusionRoom fr, uint number, string name, eSigIoMask mask)
        {
            if (number < 50)
            {
                throw new ArgumentOutOfRangeException("number", "Cannot be less than 50");
            }
            number -= 49;
            fr.AddSig(eSigType.String, number, name, mask);
            return fr.UserDefinedStringSigDetails[number];
        }

        /// <summary>
        /// Creates and returns a static asset
        /// </summary>
        /// <returns>the new asset</returns>
        public static FusionStaticAsset CreateStaticAsset(this FusionRoom fr, uint number, string name, string type,
            string instanceId)
        {
            try
            {
                Debug.Console(0, "Adding Fusion Static Asset '{0}' to slot {1} with GUID: '{2}'", name, number, instanceId);

                fr.AddAsset(eAssetType.StaticAsset, number, name, type, instanceId);
                return fr.UserConfigurableAssetDetails[number].Asset as FusionStaticAsset;
            }
            catch (InvalidOperationException ex)
            {
                Debug.Console(0, Debug.ErrorLogLevel.Notice, "Error creating Static Asset for device: '{0}'.  Check that multiple devices don't have missing or duplicate uid properties in configuration. /r/nError: {1}", name, ex);
                return null;
            }
            catch (Exception e)
            {
                Debug.Console(2, Debug.ErrorLogLevel.Error, "Error creating Static Asset: {0}", e);
                return null;
            }
        }

        public static FusionOccupancySensor CreateOccupancySensorAsset(this FusionRoom fr, uint number, string name,
            string type, string instanceId)
        {
            try
            {
                Debug.Console(0, "Adding Fusion Occupancy Sensor Asset '{0}' to slot {1} with GUID: '{2}'", name, number,
                    instanceId);

                fr.AddAsset(eAssetType.OccupancySensor, number, name, type, instanceId);
                return fr.UserConfigurableAssetDetails[number].Asset as FusionOccupancySensor;
            }
            catch (InvalidOperationException ex)
            {
                Debug.Console(0, Debug.ErrorLogLevel.Notice, "Error creating Static Asset for device: '{0}'.  Check that multiple devices don't have missing or duplicate uid properties in configuration.  Error: {1}", name, ex);
                return null;
            }
            catch (Exception e)
            {
                Debug.Console(2, Debug.ErrorLogLevel.Error, "Error creating Static Asset: {0}", e);
                return null;
            }
        }
    }
}