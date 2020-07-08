using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Essentials;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.PageManagers;
using PepperDash.Essentials.UIDrivers;
using PepperDash.Essentials.UIDrivers.VC;

namespace PepperDashEssentials.UIDrivers.EssentialsDualDisplay
{
    public class EssentialsDualDisplayPanelAvFunctionsDriver:PanelDriverBase, IAVWithVCDriver
    {
        public enum UiDisplayMode
        {
            Presentation,
            AudioSetup,
            Call,
            Start
        }

        private readonly SubpageReferenceList _activityFooterSrl;
        private readonly BoolInputSig _callButtonSig;

        private readonly List<BoolInputSig> _currentDisplayModeSigsInUse = new List<BoolInputSig>();
        private readonly BoolInputSig _endMeetingButtonSig;
        private readonly Dictionary<object, PageManager> _pageManagers = new Dictionary<object, PageManager>();
        private readonly PanelDriverBase _parent;
        private readonly BoolInputSig _shareButtonSig;

        private readonly SubpageReferenceList _sourceStagingSrl;

        private readonly CrestronTouchpanelPropertiesConfig _config;
        private BoolFeedback _callSharingInfoVisibleFeedback;
        private List<PanelDriverBase> _childDrivers = new List<PanelDriverBase>();
        private UiDisplayMode _currentMode = UiDisplayMode.Start;
        private PageManager _currentSourcePageManager;
        private string _lastMeetingDismissedId;
        private CTimer _nextMeetingTimer;
        private CTimer _powerOffTimer;
        private CTimer _ribbonTimer;
        private ModalDialog _powerDownModal;
        private EssentialsVideoCodecUiDriver _vcDriver;
        private EssentialsHuddleTechPageDriver _techDriver;
 

        public EssentialsDualDisplayPanelAvFunctionsDriver(PanelDriverBase parent, CrestronTouchpanelPropertiesConfig config) : base(parent.TriList)
        {

        }
    }
}