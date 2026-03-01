# Essentials architecture: DeviceManager activation

## What is all this?

The Essentials system architecture is a loose collection of "things" - generally real or logical Devices - that all need to relate to each other. In the interest of keeping Essentials extensible and flexible, we use an non-ordered collection of objects that should only have references to each other in the least-binding way possible. Meaning: Devices should be designed to be able to function without related objects present, and when they are present they should only retain loose reference to those other objects for memory management and later deconstruction as Essentials grows into a real-time configurable environment.

In order to facilitate this loose coupling, Essentials devices go through five phases during the startup process: Construction; addition to `DeviceManager`; pre-activation; activation; post-activation. We will describe what is optimal behavior for each of the steps below:

### Classes Referenced

* `PepperDash.Core.Device`
* `PepperDash.Essentials.Core.EssentialsDevice`
* `PepperDash.Essentials.Core.DeviceManager`
* `PepperDash.Essentials.Core.Privacy.MicrophonePrivacyController`

## 1. Construction and addition to the DeviceManager

In general, a device's constructor should only be used to get the "framework" of the device in place. All devices are constructed in this stage.  Rooms and fusion bridges included. Simple devices like IR driver devices, and devices with no controls can be completely spun up in this phase. All devices are added to the `DeviceManager` after they are constructed, but may not be fully functional.

## 2. Pre-activation

This stage is rarely used. It exists to allow an opportunity for any necessary logic to take place before the main activation phase.

## 3. Activation

This stage is the main phase of startup, and where most devices will get up and running, if they need additional startup behavior defined.  The developer will code an optional overridden `CustomActivate()` method on the device class. This is where hardware ports may be set up; signals and feedbacks linked; UI drivers fired up; rooms linked to their displays and codec... With the exception of early-designed devices, most new Essentials classes do all of their startup here, rather than in their constructors.

Remember that in your `CustomActivate()` method, you cannot assume that a device you depend on is alive and running yet.  It may be activating later.  You _can_ depend on that device's existence, and link yourself to it, but it may not be functional yet. In general, there should be no conditions in any Essentials code that depend on device startup sequence and ordering. All rooms, devices, classes should be able to function without linked devices being alive, and respond appropriately when they do come to life. Any post-activation steps can be done in step four below - and should be avoided in general.

If the `CustomActivate()` method is long, consider breaking it up into many smaller methods. This will enhance exception handling and debugging when things go wrong, with more-detailed stack traces, and makes for easier-to-read code.

Note: It is best-practice in Essentials to not write arbitrarily-timed startup sequences to ensure that a "system" or room is functional. Rather, we encourage the developer to use various properties and conditions on devices to aggregate together "room is ready" statuses that can trigger further action. This ensures that all devices can be up and alive, allowing them to be debugged within a program that may otherwise be misbehaving - as well as not making users and expensive developers wait for code to start up!

```cs
public override bool CustomActivate()
{
    Debug.Console(0, this, "Final activation. Setting up actions and feedbacks");
    SetupFunctions();
    SetupFeedbacks();

    EISC.SigChange += EISC_SigChange;
    ...
}
```

## 4. Post-activation

This phase is used primarily to handle any logic in a device that might be dependent on another device, and we need to ensure that we have waited for the dependent device to be activated first.  For example, if we look at the `MicrophonePrivacyController` class, this is a "virtual" device whose purpose is to control the mute state of microphones from one or more contact closure inputs as well as provide feedback via different colored LEDs as to the current mute state.  This virtual-device doesn't actually represent any sort of physical hardware device, but rather relies on associating itself with other devices that represent digital inputs and relays as well as whatever device is responsible for preforming the actual muting of the microphones.

We can see in the example below that during the `CustomActivate()` phase, we define a post-activation action via a lambda in `AddPostActivationAction()` that will execute during the post-activation phase.  The purpose here is to check the state of the microphone mute and set the state of the relays that control the LEDs accordingly.  We need to do this as a post-activation action because we need to make sure that the devices PrivacyDevice, RedLedRelay and GreenLedRelay are fully activated before we can attempt to interact with them.

### **Example**

```cs
public override bool CustomActivate()
{
    foreach (var i in Config.Inputs)
    {
        var input = DeviceManager.GetDeviceForKey(i.DeviceKey) as IDigitalInput;
        if(input != null)
            AddInput(input);
    }

    var greenLed = DeviceManager.GetDeviceForKey(Config.GreenLedRelay.DeviceKey) as GenericRelayDevice;
    if (greenLed != null)
        GreenLedRelay = greenLed;
    else
        Debug.Console(0, this, "Unable to add Green LED device");

    var redLed = DeviceManager.GetDeviceForKey(Config.RedLedRelay.DeviceKey) as GenericRelayDevice;
    if (redLed != null)
        RedLedRelay = redLed;
    else
        Debug.Console(0, this, "Unable to add Red LED device");

    AddPostActivationAction(() => {
        CheckPrivacyMode();
        PrivacyDevice.PrivacyModeIsOnFeedback.OutputChange -= PrivacyModeIsOnFeedback_OutputChange;
        PrivacyDevice.PrivacyModeIsOnFeedback.OutputChange += PrivacyModeIsOnFeedback_OutputChange;
    });

    initialized = true;

    return base.CustomActivate();
}

void CheckPrivacyMode()
{
    if (PrivacyDevice != null)
    {
        var privacyState = PrivacyDevice.PrivacyModeIsOnFeedback.BoolValue;
        if (privacyState)
            TurnOnRedLeds();
        else
            TurnOnGreenLeds();
    }
}
```

## Activation exceptions

Each of the three activation phases operates in a try/catch block for each device.  This way if one device has a fatal failure during activation, the failure will be logged and the system can continue to activate. This allows the developer to chase down multiple issues per load while testing, or to fix configuration omissions/errors as a group rather than one-at-a-time. A program can theoretically be fully-initialized and have many or all devices fail. We generally do not want to depend on exception handling to log device failures. Construction and activation code should have plenty of null checks, parameter validity checks, and debugging output to prevent exceptions from occurring. `String.IsEmptyOrNull(myString)` and `if(myObject == null)` are your friends. Invite them often.

## Interdependence

In any real-world system, devices and business logic need to talk to each other, otherwise, what's the point of all this coding? When creating your classes and configuration, it is best practice to _try_ not to "plug" one device into another during construction or activation. For example your touchpanel controller class has a `Display1` property that holds the display-1 object. Rather, it may be better to refer to the device as it is stored in the `DeviceManager` when it's needed using the static `DeviceManager.GetDeviceForKey(key)` method to get a reference to the device, which can be cast using various interfaces/class types, and then interacted with.  This prevents objects from being referenced in places where the developer may later forget to dereference them, causing memory leak.  This will become more important as Essentials becomes more able to be reconfigured at runtime.

As an example, [connection-based routing](~/docs/technical-docs/Connection-based-routing.md#essentials-connection-based-routing) uses these methods.  When a route is requested, the collection of tielines and devices is searched for the devices and paths necessary to complete a route, but there are no devices or tie lines that are object-referenced in running code.  It can all be torn down and reconfigured without any memory-management dereferencing, setting things to null.

## Device Initialization

Once the `DeviceManager` has completed the activation phase cycle for all devices, the devices themselves can be initialized.  The `EssentialsDevice` class subscribes to the `DeviceManager.AllDevicesActivated` event and invokes the virtual `Initialize()` method on `Device` in a separate thread.  This allows all devices to concurrently initialize in parallel threads. 

The main task that should be undertaken in the `Initialize()` method for any 3rd party device class, it to begin communication with the device via its API.  Ideally, no class that communicates with a 3rd party device outside the program should attempt to start communicating before this point.

### Example (from `PepperDash.Essentials.Devices.Common.VideoCodec.Cisco.CiscoSparkCodec`)
```cs
        public override void Initialize()
        {
            var socket = Communication as ISocketStatus;
            if (socket != null)
            {
                socket.ConnectionChange += new EventHandler<GenericSocketStatusChageEventArgs>(socket_ConnectionChange);
            }

            Communication.Connect();

            CommunicationMonitor.Start();

            const string prefix = "xFeedback register ";

            CliFeedbackRegistrationExpression =
                prefix + "/Configuration" + Delimiter +
                prefix + "/Status/Audio" + Delimiter +
                prefix + "/Status/Call" + Delimiter +
                prefix + "/Status/Conference/Presentation" + Delimiter +
                prefix + "/Status/Cameras/SpeakerTrack" + Delimiter +
                prefix + "/Status/RoomAnalytics" + Delimiter +
                prefix + "/Status/RoomPreset" + Delimiter +
                prefix + "/Status/Standby" + Delimiter +
                prefix + "/Status/Video/Selfview" + Delimiter +
                prefix + "/Status/Video/Layout" + Delimiter +
                prefix + "/Status/Video/Input/MainVideoMute" + Delimiter +
                prefix + "/Bookings" + Delimiter +
                prefix + "/Event/CallDisconnect" + Delimiter +
                prefix + "/Event/Bookings" + Delimiter +
                prefix + "/Event/CameraPresetListUpdated" + Delimiter +
                prefix + "/Event/UserInterface/Presentation/ExternalSource/Selected/SourceIdentifier" + Delimiter;
        }
```

## The goal

Robust C#-based system code should not depend on "order" or "time" to get running.  We do not need to manage the order of our startup in this environment.  Our Room class may come alive before our DSP and or Codec, and the Room is responsible for handling things when those devices become available. The UI layer is responsible for blocking the UI or providing status when the Room's requirements are coming alive, or if something has gone away. We use events or `Feedbacks` to notify dependents that other devices/classes are ready or not, but we do not prevent continued construction/activation of the system when many of these events don't happen, or don't happen in a timely fashion.  This removes the need for startup management, which is often prolonged and consumes _tons_ of developer/installer time.  A fully-loaded Essentials system may go through activation in several seconds, with all devices concurrently getting themselves going, where legacy code may take 10 minutes.

When designing new Device-based classes, be it rooms, devices, port controllers, bridges, make them as independent as possible.  They could exist alone in a program with no required partner objects, and just quietly exist without failing. We want the system to be fast and flexible, and keeping the interdependence between objects at a minimum improves this flexibility into the future.

Next: [More architecture](~/docs/technical-docs/Arch-topics.md)
