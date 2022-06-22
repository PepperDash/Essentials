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
    public class ScreenSaverController : PanelDriverBase, IDisposable
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

            var cmdName = String.Format("shwscrsvr-{0:X2}", parent.TriList.ID);

            CrestronConsole.AddNewConsoleCommand((o) => Show(), cmdName, "Shows Panel Screensaver", ConsoleAccessLevelEnum.AccessOperator);

            TriList.SetSigFalseAction(UIBoolJoin.MCScreenSaverClosePress, Hide);
        }

        public override void Show()
        {
            //Debug.Console(2, "Showing ScreenSaverController: {0:X2}", TriList.ID);

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
            //Debug.Console(2, "Hiding ScreenSaverController: {0:X2}", TriList.ID);

            if (PositionTimer != null)
            {
                //Debug.Console(2, "Stopping PositionTimer: {0:X2}", TriList.ID);
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
            //Debug.Console(2,  "Starting Position Timer: {0:X2}", TriList.ID);

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

            //Debug.Console(2, "ScreenSaver Position Timer Expired: Setting new position: {0} ID: {1:X2}", CurrentPositionIndex, TriList.ID);
        }

        //
        void ShowCurrentPosition()
        {
            // Set based on current index
            PositionInterlock.ShowInterlocked(PositionJoins[CurrentPositionIndex]);
        }

        void ClearAllPositions()
        {
            //Debug.Console(2, "Hiding all screensaver positions: {0:X2}", TriList.ID);

            PositionInterlock.HideAndClear();
        }

        #region IDisposable Members

        public void Dispose()
        {
            Hide();
        }

        #endregion
    }
 
}