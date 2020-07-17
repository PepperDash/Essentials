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

        private BoolInputSig _routeToggleVisibility;

        private readonly BoolFeedback _sharingMode;

        public EssentialsDualDisplayPanelAvFunctionsDriver(PanelDriverBase parent, CrestronTouchpanelPropertiesConfig config) : base(parent, config)
        {
            _routeToggleVisibility = parent.TriList.BooleanInput[UIBoolJoin.ToggleSharingModeVisible];

            _sharingMode = new BoolFeedback(() => _currentRoom.VideoRoutingBehavior == EVideoBehavior.Advanced);

            _sharingMode.LinkInputSig(parent.TriList.BooleanInput[UIBoolJoin.ToggleSharingModePress]);
        }

        #region Overrides of EssentialsHuddleVtc1PanelAvFunctionsDriver

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
            foreach (var src in sourceList.Where(
                src =>
                    src.Value.IncludeInSourceList && (inCall && !src.Value.DisableCodecSharing) &&
                    (CurrentMode != UiDisplayMode.Call && !src.Value.DisableCodecSharing)))
            {
                var source = src.Value;
                var sourceKey = src.Key;
                Debug.Console(1, "**** {0}, {1}, {2}, {3}, {4}", source.PreferredName, source.IncludeInSourceList,
                    source.DisableCodecSharing, inCall, CurrentMode);

                var srlItem = new SubpageReferenceListSourceItem(i++, SourceStagingSrl, src.Value,
                    b => { if (!b) UiSelectSource(sourceKey); });

                SourceStagingSrl.AddItem(srlItem);
                srlItem.RegisterForSourceChange(_currentRoom);
            }
        }

        private void RefreshRoom(EssentialsDualDisplayRoom room)
        {
            RefreshCurrentRoom(room);

            _currentRoom = room;

            _routeToggleVisibility.BoolValue = _currentRoom.RoomConfig.EnableVideoBehaviorToggle;
            _sharingMode.FireUpdate();
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