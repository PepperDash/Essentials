using System.Linq;
using Crestron.SimplSharpPro;
using PepperDash.Core;
using PepperDash.Essentials;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.Rooms.Config;

namespace PepperDashEssentials.UIDrivers.EssentialsDualDisplay
{
    public class EssentialsDualDisplayPanelAvFunctionsDriver : EssentialsHuddleVtc1PanelAvFunctionsDriver
    {
        private EssentialsDualDisplayRoom _currentRoom;

        private readonly BoolInputSig _routeToggleVisibility;
        private readonly BoolInputSig _dualDisplayControlVisibility;

        private readonly BoolOutputSig _sharingModeSig;

        private readonly BoolFeedback _sharingModeFeedback;
        private readonly BoolFeedback _dualDisplayVisiblityFeedback;
        private readonly BoolFeedback _routeToggleVisiblityFeedback;

        public EssentialsDualDisplayPanelAvFunctionsDriver(PanelDriverBase parent, CrestronTouchpanelPropertiesConfig config) : base(parent, config)
        {
            _routeToggleVisibility = parent.TriList.BooleanInput[UIBoolJoin.ToggleSharingModeVisible];
            _dualDisplayControlVisibility = parent.TriList.BooleanInput[UIBoolJoin.DualDisplayPageVisible];
            _sharingModeSig = parent.TriList.BooleanOutput[UIBoolJoin.ToggleSharingModePress];

            _sharingModeSig.SetBoolSigAction(ToggleVideoBehavior);

            _sharingModeFeedback = new BoolFeedback(() => _currentRoom.VideoRoutingBehavior == EVideoBehavior.Advanced);
            _sharingModeFeedback.LinkInputSig(parent.TriList.BooleanInput[UIBoolJoin.ToggleSharingModePress]);

            _dualDisplayVisiblityFeedback =
                new BoolFeedback(
                    () =>
                        _currentRoom.VideoRoutingBehavior == EVideoBehavior.Advanced &&
                        CurrentMode == UiDisplayMode.Presentation);
            _dualDisplayVisiblityFeedback.LinkInputSig(_dualDisplayControlVisibility);

            _routeToggleVisiblityFeedback =
                new BoolFeedback(
                    () => _currentRoom.RoomConfig.EnableVideoBehaviorToggle && CurrentMode == UiDisplayMode.Presentation);
            _routeToggleVisiblityFeedback.LinkInputSig(_routeToggleVisibility);
        }

        #region Overrides of EssentialsHuddleVtc1PanelAvFunctionsDriver

        protected override void ShowCurrentSource()
        {
            if (_currentRoom.VideoRoutingBehavior == EVideoBehavior.Advanced) return;

            base.ShowCurrentSource();
        }

        #endregion

        private void ToggleVideoBehavior(bool value)
        {
            if (!value) return;

            _currentRoom.VideoRoutingBehavior = _currentRoom.VideoRoutingBehavior == EVideoBehavior.Basic
                        ? EVideoBehavior.Advanced
                        : EVideoBehavior.Basic;

            _sharingModeFeedback.FireUpdate();
            _dualDisplayVisiblityFeedback.FireUpdate();

            TriList.SetBool(UIBoolJoin.SelectASourceVisible, _currentRoom.VideoRoutingBehavior == EVideoBehavior.Basic);

            if (_currentRoom.VideoRoutingBehavior == EVideoBehavior.Advanced)
            {
                CurrentSourcePageManager.Hide();
            }
            else
            {
                if (_currentRoom.CurrentSourceInfo != null)
                {
                    UiSelectSource(_currentRoom.CurrentSourceInfoKey);
                    TriList.SetBool(UIBoolJoin.SelectASourceVisible, false);
                }
                CurrentSourcePageManager.Show();
            }
        }

        #region Overrides of EssentialsHuddleVtc1PanelAvFunctionsDriver

        #region Overrides of EssentialsHuddleVtc1PanelAvFunctionsDriver

        protected override void SetCurrentRoom(EssentialsHuddleVtc1Room room)
        {
            _currentRoom = room as EssentialsDualDisplayRoom;

            base.SetCurrentRoom(room);
        }

        #endregion

        #region Overrides of EssentialsHuddleVtc1PanelAvFunctionsDriver

        protected override void ActivityShareButtonPressed()
        {
            base.ActivityShareButtonPressed();

            _dualDisplayVisiblityFeedback.FireUpdate();
            _routeToggleVisiblityFeedback.FireUpdate();
        }

        #region Overrides of EssentialsHuddleVtc1PanelAvFunctionsDriver

        public override void ActivityCallButtonPressed()
        {
            base.ActivityCallButtonPressed();

            _dualDisplayVisiblityFeedback.FireUpdate();
            _routeToggleVisiblityFeedback.FireUpdate();
        }

        #endregion

        #endregion

        protected override void SetupSourceList()
        {
            var inCall = _currentRoom.InCallFeedback.BoolValue;
            var sourceLists = ConfigReader.ConfigObject.SourceLists;
            if (!sourceLists.ContainsKey(_currentRoom.SourceListKey))
            {
                return;
            }

            var sourceList = sourceLists[_currentRoom.SourceListKey].OrderBy(kv => kv.Value.Order);

            SourceStagingSrl.Clear();
            uint i = 1;
            foreach (var src in sourceList)
            {
                var source = src.Value;
                var sourceKey = src.Key;
                Debug.Console(1, "**** {0}, {1}, {2}, {3}, {4}", source.PreferredName, source.IncludeInSourceList,
                    source.DisableCodecSharing, inCall, CurrentMode);

                if (!source.IncludeInSourceList || (inCall && source.DisableCodecSharing)
                    || CurrentMode == UiDisplayMode.Call && source.DisableCodecSharing)
                {
                    Debug.Console(1, "Skipping {0}", source.PreferredName);
                    continue;
                }
                var srlItem = new SubpageReferenceListSourceItem(i++, SourceStagingSrl, source,
                    b => { if (!b) UiSelectSource(sourceKey); });

                SourceStagingSrl.AddItem(srlItem);
                srlItem.RegisterForSourceChange(_currentRoom);
            }
            SourceStagingSrl.Count = (ushort) (i - 1);
            Debug.Console(2, "Dual Display Source Count: {0}", SourceStagingSrl.Count);
        }

        #region Overrides of EssentialsHuddleVtc1PanelAvFunctionsDriver

        protected override void SetSourceFeedback()
        {
            if (!CurrentRoom.OnFeedback.BoolValue)
            {
                // If there's no default, show UI elements
                if (!CurrentRoom.RunDefaultPresentRoute() && _currentRoom.VideoRoutingBehavior == EVideoBehavior.Basic)
                {
                    TriList.SetBool(UIBoolJoin.SelectASourceVisible, true);
                }
            }
            else // room is on show what's active or select a source if nothing is yet active
            {
                if (CurrentRoom.CurrentSourceInfo == null ||
                    CurrentRoom.CurrentSourceInfoKey == EssentialsHuddleVtc1Room.DefaultCodecRouteString)
                {
                    TriList.SetBool(UIBoolJoin.SelectASourceVisible, true);
                    return;
                }

                if (CurrentSourcePageManager == null) return;

                if (_currentRoom.VideoRoutingBehavior == EVideoBehavior.Advanced)
                {
                    CurrentSourcePageManager.Hide();
                    return;
                }

                CurrentSourcePageManager.Show();
                
            }
        }

        #endregion

        private void RefreshRoom(EssentialsDualDisplayRoom room)
        {
            RefreshCurrentRoom(room);

            _currentRoom = room;

            _routeToggleVisibility.BoolValue = _currentRoom.RoomConfig.EnableVideoBehaviorToggle;
            _sharingModeFeedback.FireUpdate();
        }

        private void UiSelectSource(string sourceKey)
        {
            if (_currentRoom.VideoRoutingBehavior == EVideoBehavior.Advanced)
            {
                _currentRoom.SelectSource(sourceKey, _currentRoom.SourceListKey);
                return;
            }

            _currentRoom.RunRouteAction(sourceKey);

        }

        #endregion
    }
}