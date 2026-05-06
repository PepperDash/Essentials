using System;
using System.Collections.Generic;
using PepperDash.Essentials.AppServer.Messengers;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.CrestronIO;
using PepperDash.Essentials.Core.DeviceInfo;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Lighting;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Core.Shades;
using PepperDash.Essentials.Devices.Common.AudioCodec;
using PepperDash.Essentials.Devices.Common.Cameras;
using PepperDash.Essentials.Devices.Common.Displays;
using PepperDash.Essentials.Devices.Common.SoftCodec;
using PepperDash.Essentials.Devices.Common.VideoCodec;
using PepperDash.Essentials.Room.MobileControl;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Describes a single entry in <see cref="MessengerFactoryRegistry"/>.
    /// </summary>
    internal sealed class MessengerFactoryEntry
    {
        /// <summary>
        /// The primary type used for both <c>IsAssignableFrom</c> matching and for
        /// caller-supplied type filtering via <see cref="AddDefaultMessengersForDevice"/>.
        /// </summary>
        public Type InterfaceType { get; }

        /// <summary>
        /// Optional additional condition. When <c>null</c> the entry matches any device
        /// whose runtime type satisfies <see cref="InterfaceType.IsAssignableFrom"/>.
        /// </summary>
        public Func<EssentialsDevice, bool> Predicate { get; }

        /// <summary>
        /// Factory that creates the messenger. Parameters are
        /// <c>(device, messagePath, controllerKey)</c>.
        /// </summary>
        public Func<EssentialsDevice, string, string, IMobileControlMessenger> Factory { get; }

        public MessengerFactoryEntry(
            Type interfaceType,
            Func<EssentialsDevice, string, string, IMobileControlMessenger> factory,
            Func<EssentialsDevice, bool> predicate = null)
        {
            InterfaceType = interfaceType;
            Factory = factory;
            Predicate = predicate;
        }

        /// <summary>
        /// Returns <c>true</c> when this entry should produce a messenger for
        /// <paramref name="device"/>.
        /// </summary>
        public bool Matches(EssentialsDevice device)
        {
            return Predicate != null
                ? Predicate(device)
                : InterfaceType.IsAssignableFrom(device.GetType());
        }
    }

    /// <summary>
    /// Maps device interface / class types to messenger factories, replacing the
    /// previous cascade of <c>if (device is X)</c> checks in
    /// <c>SetupDefaultDeviceMessengers</c>.
    /// </summary>
    internal static class MessengerFactoryRegistry
    {
        public static IReadOnlyList<MessengerFactoryEntry> Entries { get; } =
            new List<MessengerFactoryEntry>
            {
                // ── Communication ────────────────────────────────────────────────────────
                new MessengerFactoryEntry(
                    typeof(ICommunicationMonitor),
                    (d, mp, ck) => new ICommunicationMonitorMessenger(
                        $"{d.Key}-commMonitor-{ck}", mp, (ICommunicationMonitor)d)
                ),

                // ── Cameras ──────────────────────────────────────────────────────────────
                // CameraBase only when the device does NOT also implement IHasCameraControls
                new MessengerFactoryEntry(
                    typeof(CameraBase),
                    (d, mp, ck) => new CameraBaseMessenger<CameraBase>(
                        $"{d.Key}-cameraBase-{ck}", (CameraBase)d, mp),
                    predicate: d => d is CameraBase && !(d is IHasCameraControls)
                ),
                
                new MessengerFactoryEntry(
                    typeof(IHasCameraControls),
                    (d, mp, ck) => new CameraBaseMessenger<IHasCameraControls>(
                        $"{d.Key}-hasCamerasWithControls-{ck}", (IHasCameraControls)d, mp)
                ),
                new MessengerFactoryEntry(
                    typeof(IHasCamerasWithControls),
                    (d, mp, ck) => new IHasCamerasWithControlMessenger(
                        $"{d.Key}-cameras-{ck}", mp, (IHasCamerasWithControls)d)
                ),

                // ── Routing ──────────────────────────────────────────────────────────────
                // BlueJeansPc implements IRunRouteAction
                new MessengerFactoryEntry(
                    typeof(BlueJeansPc),
                    (d, mp, ck) => new RunRouteActionMessenger(
                        $"{d.Key}-runRouteAction-{ck}", (IRunRouteAction)d, mp)
                ),

                // ── Presets ──────────────────────────────────────────────────────────────
                new MessengerFactoryEntry(
                    typeof(ITvPresetsProvider),
                    (d, mp, ck) => new DevicePresetsModelMessenger(
                        $"{d.Key}-presets-{ck}", mp, (ITvPresetsProvider)d)
                ),

                // ── Displays ─────────────────────────────────────────────────────────────
                new MessengerFactoryEntry(
                    typeof(DisplayBase),
                    (d, mp, ck) => new DisplayBaseMessenger(
                        $"{d.Key}-displayBase-{ck}", mp, (DisplayBase)d)
                ),
                new MessengerFactoryEntry(
                    typeof(TwoWayDisplayBase),
                    (d, mp, ck) => new TwoWayDisplayBaseMessenger(
                        $"{d.Key}-twoWayDisplay-{ck}", mp, (TwoWayDisplayBase)d)
                ),

                // ── Audio / Video ─────────────────────────────────────────────────────────
                new MessengerFactoryEntry(
                    typeof(IBasicVolumeControls),
                    (d, mp, ck) => new DeviceVolumeMessenger(
                        $"{d.Key}-volume-{ck}", mp, (IBasicVolumeControls)d)
                ),
                new MessengerFactoryEntry(
                    typeof(IBasicVideoMuteWithFeedback),
                    (d, mp, ck) => new IBasicVideoMuteWithFeedbackMessenger(
                        $"{d.Key}-videoMute-{ck}", mp, (IBasicVideoMuteWithFeedback)d)
                ),

                // ── Lighting ─────────────────────────────────────────────────────────────
                // ILightingScenes covers LightingBase too (LightingBase : ILightingScenes)
                new MessengerFactoryEntry(
                    typeof(ILightingScenes),
                    (d, mp, ck) => new ILightingScenesMessenger(
                        $"{d.Key}-lighting-{ck}", (ILightingScenes)d, mp)
                ),

                // ── Shades ───────────────────────────────────────────────────────────────
                new MessengerFactoryEntry(
                    typeof(IShadesOpenCloseStop),
                    (d, mp, ck) => new IShadesOpenCloseStopMessenger(
                        $"{d.Key}-shades-{ck}", (IShadesOpenCloseStop)d, mp)
                ),

                // ── Codecs ───────────────────────────────────────────────────────────────
                new MessengerFactoryEntry(
                    typeof(VideoCodecBase),
                    (d, mp, ck) => new VideoCodecBaseMessenger(
                        $"{d.Key}-videoCodec-{ck}", (VideoCodecBase)d, mp)
                ),
                new MessengerFactoryEntry(
                    typeof(AudioCodecBase),
                    (d, mp, ck) => new AudioCodecBaseMessenger(
                        $"{d.Key}-audioCodec-{ck}", (AudioCodecBase)d, mp)
                ),

                // ── Set-top box controls ──────────────────────────────────────────────────
                new MessengerFactoryEntry(
                    typeof(ISetTopBoxControls),
                    (d, mp, ck) => new ISetTopBoxControlsMessenger(
                        $"{d.Key}-stb-{ck}", mp, (ISetTopBoxControls)d)
                ),
                new MessengerFactoryEntry(
                    typeof(IChannel),
                    (d, mp, ck) => new IChannelMessenger(
                        $"{d.Key}-channel-{ck}", mp, (IChannel)d)
                ),
                new MessengerFactoryEntry(
                    typeof(IColor),
                    (d, mp, ck) => new IColorMessenger(
                        $"{d.Key}-color-{ck}", mp, (IColor)d)
                ),
                new MessengerFactoryEntry(
                    typeof(IDPad),
                    (d, mp, ck) => new IDPadMessenger(
                        $"{d.Key}-dPad-{ck}", mp, (IDPad)d)
                ),
                new MessengerFactoryEntry(
                    typeof(INumericKeypad),
                    (d, mp, ck) => new INumericKeypadMessenger(
                        $"{d.Key}-numericKeypad-{ck}", mp, (INumericKeypad)d)
                ),

                // ── Power ─────────────────────────────────────────────────────────────────
                new MessengerFactoryEntry(
                    typeof(IHasPowerControl),
                    (d, mp, ck) => new IHasPowerMessenger(
                        $"{d.Key}-powerControl-{ck}", mp, (IHasPowerControl)d)
                ),
                new MessengerFactoryEntry(
                    typeof(IHasPowerControlWithFeedback),
                    (d, mp, ck) => new IHasPowerControlWithFeedbackMessenger(
                        $"{d.Key}-powerFeedback-{ck}", mp, (IHasPowerControlWithFeedback)d)
                ),

                // ── Transport / sources ───────────────────────────────────────────────────
                new MessengerFactoryEntry(
                    typeof(ITransport),
                    (d, mp, ck) => new ITransportMessenger(
                        $"{d.Key}-transport-{ck}", mp, (ITransport)d)
                ),
                new MessengerFactoryEntry(
                    typeof(ICurrentSources),
                    (d, mp, ck) => new ICurrentSourcesMessenger(
                        $"{d.Key}-currentSources-{ck}", mp, (ICurrentSources)d)
                ),
                new MessengerFactoryEntry(
                    typeof(ISwitchedOutput),
                    (d, mp, ck) => new ISwitchedOutputMessenger(
                        $"{d.Key}-switchedOutput-{ck}", (ISwitchedOutput)d, mp)
                ),

                // ── Device info / levels / inputs ─────────────────────────────────────────
                new MessengerFactoryEntry(
                    typeof(IDeviceInfoProvider),
                    (d, mp, ck) => new IDeviceInfoProviderMessenger(
                        $"{d.Key}-deviceInfo-{ck}", mp, (IDeviceInfoProvider)d)
                ),
                new MessengerFactoryEntry(
                    typeof(ILevelControls),
                    (d, mp, ck) => new ILevelControlsMessenger(
                        $"{d.Key}-levelControls-{ck}", mp, (ILevelControls)d)
                ),
                new MessengerFactoryEntry(
                    typeof(IHasInputs<string>),
                    (d, mp, ck) => new IHasInputsMessenger<string>(
                        $"{d.Key}-inputs-{ck}", mp, (IHasInputs<string>)d)
                ),
                new MessengerFactoryEntry(
                    typeof(IHasInputs<byte>),
                    (d, mp, ck) => new IHasInputsMessenger<byte>(
                        $"{d.Key}-inputs-{ck}", mp, (IHasInputs<byte>)d)
                ),
                new MessengerFactoryEntry(
                    typeof(IHasInputs<int>),
                    (d, mp, ck) => new IHasInputsMessenger<int>(
                        $"{d.Key}-inputs-{ck}", mp, (IHasInputs<int>)d)
                ),

                // ── Matrix routing ────────────────────────────────────────────────────────
                // Preserving original key format (no controller key suffix)
                new MessengerFactoryEntry(
                    typeof(IMatrixRouting),
                    (d, mp, _) => new IMatrixRoutingMessenger(
                        $"{d.Key}-matrixRouting", mp, (IMatrixRouting)d)
                ),

                // ── Environmental sensors ─────────────────────────────────────────────────
                // Preserving original key format (no controller key suffix)
                new MessengerFactoryEntry(
                    typeof(ITemperatureSensor),
                    (d, mp, _) => new ITemperatureSensorMessenger(
                        $"{d.Key}-tempSensor", (ITemperatureSensor)d, mp)
                ),
                new MessengerFactoryEntry(
                    typeof(IHumiditySensor),
                    (d, mp, _) => new IHumiditySensorMessenger(
                        $"{d.Key}-humiditySensor", (IHumiditySensor)d, mp)
                ),

                // ── Room combining ────────────────────────────────────────────────────────
                new MessengerFactoryEntry(
                    typeof(IEssentialsRoomCombiner),
                    (d, mp, ck) => new IEssentialsRoomCombinerMessenger(
                        $"{d.Key}-roomCombiner-{ck}", mp, (IEssentialsRoomCombiner)d)
                ),

                // ── Projector screen ──────────────────────────────────────────────────────
                new MessengerFactoryEntry(
                    typeof(IProjectorScreenLiftControl),
                    (d, mp, ck) => new IProjectorScreenLiftControlMessenger(
                        $"{d.Key}-screenLiftControl-{ck}", mp, (IProjectorScreenLiftControl)d)
                ),

                // ── DSP ───────────────────────────────────────────────────────────────────
                new MessengerFactoryEntry(
                    typeof(IDspPresets),
                    (d, mp, ck) => new IDspPresetsMessenger(
                        $"{d.Key}-dspPresets-{ck}", mp, (IDspPresets)d)
                ),

                // Room Entries
                // ── Event schedule ────────────────────────────────────────────────────────
                new MessengerFactoryEntry(
                    typeof(IRoomEventSchedule),
                    (d, mp, ck) => new RoomEventScheduleMessenger(
                        $"{d.Key}-schedule-{ck}", mp, (IRoomEventSchedule)d)
                ),

                // ── Tech password ─────────────────────────────────────────────────────────
                new MessengerFactoryEntry(
                    typeof(ITechPassword),
                    (d, mp, ck) => new ITechPasswordMessenger(
                        $"{d.Key}-techPassword-{ck}", mp, (ITechPassword)d)
                ),

                // ── Shutdown prompt timer ─────────────────────────────────────────────────
                new MessengerFactoryEntry(
                    typeof(IShutdownPromptTimer),
                    (d, mp, ck) => new IShutdownPromptTimerMessenger(
                        $"{d.Key}-shutdownPromptTimer-{ck}", mp, (IShutdownPromptTimer)d)
                ),

                // ── Level controls ────────────────────────────────────────────────────────
                new MessengerFactoryEntry(
                    typeof(ILevelControls),
                    (d, mp, ck) => new ILevelControlsMessenger(
                        $"{d.Key}-levelControls-{ck}", mp, (ILevelControls)d)
                ),
            };
    }
}
