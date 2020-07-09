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
using PepperDash.Essentials.Core.Touchpanels.Keyboards;
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

        public bool ShowVolumeGauge { get; set; }
        public uint PowerOffTimeout { get; set; }
        public string DefaultRoomKey { get; set; }

        public EssentialsRoomBase CurrentRoom
        {
            get { return _currentRoom; }
            set { SetCurrentRoom(value); }
        }

        public SubpageReferenceList MeetingOrContactMethodModalSrl { get; set; }
        public JoinedSigInterlock PopupInterlock { get; private set; }
        public HabaneroKeyboardController Keyboard { get; private set; }

        public EssentialsDualDisplayPanelAvFunctionsDriver(PanelDriverBase parent, CrestronTouchpanelPropertiesConfig config) : base(parent.TriList)
        {
            _config = config;
            _parent = parent;

            _sourceStagingSrl = new SubpageReferenceList(TriList, UISmartObjectJoin.SourceStagingSRL, 3, 3, 3);
            _activityFooterSrl = new SubpageReferenceList(TriList, UISmartObjectJoin.ActivityFooterSRL, 3, 3, 3);
            _callButtonSig = _activityFooterSrl.BoolInputSig(2, 1);
            _shareButtonSig = _activityFooterSrl.BoolInputSig(1, 1);
            _endMeetingButtonSig = _activityFooterSrl.BoolInputSig(3, 1);

            MeetingOrContactMethodModalSrl = new SubpageReferenceList(TriList, UISmartObjectJoin.MeetingListSRL, 3, 3, 3);
        }
    }
}