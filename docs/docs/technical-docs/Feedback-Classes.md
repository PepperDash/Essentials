# Feedback classes

***
* [YouTube Video - Using Feedbacks in PepperDash Essentials](https://youtu.be/5GQVRKbD9Rk)
***

The various Feedback classes are like "signals". They can enable various events, and are designed to be used where we need small data events to be sent without requiring custom handlers.

## Why Feedbacks?

We have been writing "code" in an environment, Simpl, for years and have taken for granted the power that signals in that environment give us. With the release of the ability to develop in C#, we have been handed a massive set of tools to potentially make our lives better, but because of the age and limited scope of the .NET 3.5 Compact Framework, many of the things that have been very easy to do in the past have become challenging or bulky to write. Crestron classes have things called "Sigs", which are a less-functional version of the signal that we used in Simpl, but we have no ability to use our own Sigs around our own classes. This forces us to break out of the constraints and mindset of Simpl programming, but simultaneously keeps us partially bound to the "old way" of doing things.

Signals as we have known them since Simpl came around are great. They allow a certain type of functional programming to be built, where things operate in solutions, and we are given a whole set of behaviors that we don't really have to think about: Something goes high, the next thing responds, something else happens, etc. With our older C# framework, it is most straightforward (and least-flexible) to take Sig transitions and handle them using very-flat and bulky coding techniques: Switch/case blocks, if/else blocks, slow dictionaries... In the Essentials environment (and in many other frameworks) these methods quickly reveal their flaws.

Enter the Feedback. We want to define simple events that can be attached to various things - TP Sigs, EISC, event handlers - and maintain their own state. This simplifies the interface to various device classes, and allows us to define functional, simple classes with well-defined means of connecting them together.

### Feedbacks are similar to signals

Feedbacks can:

- Fire an event (OutputChange)
- Be linked to one or more matching Crestron Sigs and update those Sigs
- May contain complex computations to define the output value
- Be put into test mode and have their value function overridden

A Feedback is defined on a class using a C# construct called a `Func`. A `Func` is a small operation that returns a single value and is typically written in a lambda. The operation/expression in the `Func` is calculated when FireUpdate() is called on the Feedback. The result is then available for all objects listening to this Feedback.

[Func documentation (MSDN)](<https://msdn.microsoft.com/en-us/library/bb534960(v=vs.110).aspx>)

#### Creating Feedbacks

The following `IntFeedback` returns the value of the `_VolumeLevel` field in this display class:

```cs
public class MyDisplay
{
    public IntFeedback VolumeLevelFeedback { get; private set; }

...

    public MyDisplay(...)
    {
        VolumeLevelFeedback = new IntFeedback(() => { return _VolumeLevel; });

        ...
```

This BoolFeedback, adapted from the DmTx201Controller class, defines the `Func` first, and then creates the BoolFeedback using that `Func`. The value returned is true if the input is the digital-HDMI connection, and the TX hardware's VideoAttributes.HdcpActiveFeedback is true as well.

```cs
public class MyTx
{
    public BoolFeedback HdcpActiveFeedback { get; private set; }

    Func HdcpActiveFeedbackFunc = () =>
        ActualVideoInput == DmTx200Base.eSourceSelection.Digital
        && tx.HdmiInput.VideoAttributes.HdcpActiveFeedback.BoolValue,

...

    public MyTx(...)
    {
        HdcpActiveFeedback = new BoolFeedback(HdcpActiveFeedbackFunc);

        ...
```

#### Triggering Feedback

In your classes, when you need to update the objects listening to a Feedback, you will call MyFeedback.FireUpdate() inside your class. This will trigger the evaluation of the Func value, update any linked Sigs, and fire the OutputChange event.

```cs
int _VolumeLevel;

void ComDataChanged(string data) // volume=77
{
    if(data.StartsWith("volume="))
    {
        _VolumeLevel = MyParseVolumeMethod(data); // get the level, 77
        VolumeLevelFeedback.FireUpdate(); // all listeners updated

```

#### Using Feedbacks

Feedbacks of the various types have BoolValue, IntValue, UShortValue, and StringValue properties that return the current value of the Feedback.

```cs
if (MyTxDevice.HdcpActiveFeedback.BoolValue)
{
    ... do something that needs to happen when HDCP is active ...
```

Feedbacks all share an OutputChange event, that fires an event with an empty EventArgs object. The event handler can go get the appropriate \*Value property when the event fires. The example below is a bit contrived, but explains the idea.

```cs
    ...
    MyDisplayDevice.VolumeLevelFeedback.OutputChange += MyDisplayVolumeHandler;

    ...
}

void MyDisplayVolumeHandler(object o, EventArgs a)
{
    MobileControlServer.VolumeLevel = MyDisplayDevice.VolumeLevelFeedback.IntValue;
```

Feedbacks also have a LinkInputSig(\*InputSig sig) method that can directly trigger one or more Sigs on a Crestron device, without requiring an event handler. This is very useful for attaching states of our devices to Crestron touchpanels or EISCs, for example. The BoolFeedback class also has a LinkComplementInputSig(BoolInputSig sig) method that will invert the BoolFeedback's value to one or more attached Sigs.

As well as updating upon change, the Feedback will set the Sig's value to the Feedback value upon calling the LinkInputSig method. This eliminates the need to walk through an object, property-by-property, and update several Sig values - as well as setting up to watch those values for changes. It is all handled in one step.

```cs
public class MyClass
{
    Tsw760 MyTp;

    MyDisplay Display;

    HookUpSigsMethod()
    {
        ...

        // changes to VolumeLevelFeedback will automatically propagate to UShortInputSig 123
        // changes to HdcpActiveFeedback will propagate to BoolInputSig 456
        // and these two panel Sigs are updated immediately as well.
        Display.VolumeLevelFeedback.LinkInputSig(MyTp.UshortInput[123]);
        MyHdcpDevice.HdcpActiveFeedback.LinkInputSig(MyTp.BoolInput[456]);
```
