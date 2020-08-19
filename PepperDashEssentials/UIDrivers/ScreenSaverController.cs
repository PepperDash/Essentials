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
        EssentialsPanelMainInterfaceDriver Parent;


        CTimer PositionTimer;

        uint PositionTimeoutMs;

        List<uint> PositionJoins;

        int CurrentPositionIndex = 0;

        public ScreenSaverController(EssentialsPanelMainInterfaceDriver parent, CrestronTouchpanelPropertiesConfig config)
            : base(parent.TriList)
        {
            Parent = parent;

            PositionTimeoutMs = config.ScreenSaverMovePositionIntervalMs;

            TriList.SetSigFalseAction(UIBoolJoin.MCScreenSaverClosePress, () => this.Hide());

        }

        public override void Show()
        {
            TriList.SetBool(UIBoolJoin.MCScreenSaverVisible, true);
            Parent.AvDriver.PopupInterlock.ShowInterlockedWithToggle(UIBoolJoin.MCScreenSaverVisible);
            //TriList.SetBool(UIBoolJoin.MCScreenSaverVisible, true);

            CurrentPositionIndex = 0;
            SetCurrentPosition();

            ClearAllPositions();

            TriList.SetBool(UIBoolJoin.MCScreenSaverVisible, false);
            Parent.AvDriver.PopupInterlock.HideAndClear();
            //TriList.SetBool(UIBoolJoin.MCScreenSaverVisible, false);

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

            SetCurrentPosition();

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
        void SetCurrentPosition()
        {
            ClearAllPositions();

            // Set based on current index
            TriList.SetBool(PositionJoins[CurrentPositionIndex], true);
        }

        void ClearAllPositions()
        {
            foreach (var join in PositionJoins)
            {
                TriList.SetBool(join, false);
            }
        }
    }
 
}