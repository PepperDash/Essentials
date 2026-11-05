
# Essentials Framework Devices by Type

The device types built into Essentials v3 are listed below with their config `type` names. Other devices are supported through plugins; a plugin must be built for Essentials v3 to load. To list the types a running program can build, including types from loaded plugins, use the console command `gettypes`.

## Disc Player

* Any IR disc player that implements standard RAD commands (`genericIrController`)

## Displays

* Any IR display that implements standard RAD commands (`basicirdisplay`)
* Screen lift controller (`screenliftcontroller`)
* Panasonic Projectors [(via plugin)](https://github.com/PepperDash/epi-panasonic-projector)
* LG Commercial series [(via plugin)](https://github.com/PepperDash/epi-lg-display)
* Generic CEC control via HDMI [(via plugin)](https://github.com/PepperDash/epi-generic-cec-display)
* Crestron Certified Driver Display [(via plugin)](https://github.com/batourin/epi-display-ccd)

## Lighting/Shading

* Relay-controlled shades (`relaycontrolledshade`, `shadecontroller`)

## Set Top Boxes

* Any IR set top box that implements standard RAD commands (`settopbox`)

## Sources

* In-room PC (`inroompc`)
* Laptop (`laptop`)
* Generic source (`genericsource`)

## Streaming Players

* AppleTV (IR) (`appletv`)
* Roku (IR) (`roku`)

## Video Codecs

* Generic soft codec (`genericsoftcodec`)
* BlueJeans PC (`bluejeanspc`)

## DSPs / Audio Codecs

* Generic audio output with volume (`genericaudiooutwithvolume`)
* BiAmp Tesira [(via plugin)](https://github.com/PepperDash/epi-biamp-tesira)

## Crestron Devices

* Relay outputs, digital inputs and Versiports (`relayoutput`, `digitalinput`, `versiportinput`, `versiportanaloginput`, `versiportoutput`)
* COM ports and generic TCP/IP communication (`genericComm`)
* IR ports (`genericIrController`)
* TSW/TS touchpanels, XPanel, DGE-1000 and the Crestron App, through Mobile Control (`mctsw550`, `mctsw750`, `mctsw1050`, `mctsw560`, `mctsw760`, `mctsw1060`, `mctsw570`, `mctsw770`, `mcts770`, `mctsw1070`, `mcts1070`, `mctsw1080`, `mcts1080`, `mcxpanel`, `mcdge1000`, `mccrestronapp`), and a Crestron UC-ENGINE-FLEX-TEAMS (Microsoft Teams Rooms) as the panel (`mcucengineflexteams`)
* Internal card cage (`internalcardcage`) [(via plugin)](https://github.com/PepperDash/epi-crestron-io)
* Fusion Room and Assets (`fusionRoom`)
* Mobile Control, which serves the web UI and connects UI clients to the system (`mobilecontrol`)
* EISC bridges to SIMPL Windows programs (`eiscapiadvanced` and related types)

## Mock Devices

* Mock display, video codec, audio codec, audio device, lighting device and routing midpoint (`mockdisplay`, `mockvc`, `mockac`, `mockaudiodevice`, `mockLightingDevice`, `mockroutingmidpoint`)
