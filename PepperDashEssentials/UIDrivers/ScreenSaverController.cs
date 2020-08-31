using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;
using PepperDash.Essentials.Core;

namespace PepperDash.Essentials
{
    /// <summary>
    /// Driver responsible for controlling the screenshaver showing the client logo, MC connection information and QR Code.  Moves the elements around to prevent screen burn in
    /// </summary>
    public class ScreenSaverController : PanelDriverBase
    {

        /// <summary>
        /// The parent driver for this
        /// </summary>
        private readonly EssentialsPanelMainInterfaceDriver _parent;


        private JoinedSigInterlock PositionInterlock;

        CTimer PositionTimer;

        uint PositionTimeoutMs;

        List<uint> PositionJoins;

        int CurrentPositionIndex = 0;

        public ScreenSaverController(EssentialsPanelMainInterfaceDriver parent, CrestronTouchpanelPropertiesConfig config)
            : base(parent.TriList)
        {
            _parent = parent;

            PositionTimeoutMs = config.ScreenSaverMovePositionIntervalMs;

            PositionJoins = new List<uint>() { UIBoolJoin.MCScreenSaverPosition1Visible, UIBoolJoin.MCScreenSaverPosition2Visible, UIBoolJoin.MCScreenSaverPosition3Visible, UIBoolJoin.MCScreenSaverPosition4Visible };

            PositionInterlock = new JoinedSigInterlock(parent.TriList);

            var cmdName = String.Format("shwscrsvr-{0}", parent.TriList.ID);

            CrestronConsole.AddNewConsoleCommand((o) => Show(), cmdName, "Shows Panel Screensaver", ConsoleAccessLevelEnum.AccessOperator);

            TriList.SetSigFalseAction(UIBoolJoin.MCScreenSaverClosePress, Hide);
        }

        public override void Show()
        {
            if (_parent.AvDriver != null)
            {
                _parent.AvDriver.PopupInterlock.ShowInterlocked(UIBoolJoin.MCScreenSaverVisible);
            }

            CurrentPositionIndex = 0;
            ShowCurrentPosition();
            StartPositionTimer();

            base.Show();
        }

        public override void Hide()
        {
            Debug.Console(1, "Hiding ScreenSaverController");

            if (PositionTimer != null)
            {
                PositionTimer.Stop();
                PositionTimer.Dispose();
                PositionTimer = null;
            }

            ClearAllPositions();

            if (_parent.AvDriver != null)
            {
                _parent.AvDriver.PopupInterlock.HideAndClear();
            }

            base.Hide();
        }

        void StartPositionTimer()
        {
            if (PositionTimer == null)
            {
                PositionTimer = new CTimer((o) => PositionTimerExpired(), PositionTimeoutMs);
            }
            else
            {
                PositionTimer.Reset(PositionTimeoutMs);
            }

        }

        void PositionTimerExpired()
        {
            IncrementPositionIndex();

            ShowCurrentPosition();

            StartPositionTimer();
        }

        void IncrementPositionIndex()
        {
            if (CurrentPositionIndex < PositionJoins.Count - 1)
            {
                CurrentPositionIndex++;
            }
            else
            {
                CurrentPositionIndex = 0;
            }

            Debug.Console(1, "ScreenSaver Position Timer Expired: Setting new position: {0}", CurrentPositionIndex);
        }

        //
        void ShowCurrentPosition()
        {
            // Set based on current index
            PositionInterlock.ShowInterlocked(PositionJoins[CurrentPositionIndex]);
        }

        void ClearAllPositions()
        {
            Debug.Console(1, "Hiding all screensaver positions");
            PositionInterlock.HideAndClear();
        }
    }
 
}