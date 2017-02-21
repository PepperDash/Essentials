using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;
using PepperDash.Essentials.Core.PageManagers;

namespace PepperDash.Essentials
{
    public class DualDisplaySimpleOrAdvancedRouting : PanelDriverBase
    {
        EssentialsPresentationPanelAvFunctionsDriver Parent;

        /// <summary>
        /// Smart Object 3200
        /// </summary>
        SubpageReferenceList SourcesSrl;

        /// <summary>
        /// For tracking feedback on last selected
        /// </summary>
        BoolInputSig LastSelectedSourceSig;
        
        /// <summary>
        ///  The source that has been selected and is awaiting assignment to a display
        /// </summary>
        SourceListItem PendingSource;

        bool IsSharingModeAdvanced;

        public DualDisplaySimpleOrAdvancedRouting(EssentialsPresentationPanelAvFunctionsDriver parent) : base(parent.TriList)
        {
            Parent = parent;
            SourcesSrl = new SubpageReferenceList(TriList, 3200, 3, 3, 3);

            TriList.SetSigFalseAction(UIBoolJoin.ToggleSharingModePress, ToggleSharingModePressed);

            TriList.SetSigFalseAction(UIBoolJoin.Display1AudioButtonPressAndFb, Display1AudioPress);
            TriList.SetSigFalseAction(UIBoolJoin.Display1ControlButtonPress, Display1ControlPress);
            TriList.SetSigTrueAction(UIBoolJoin.Display1SelectPress, Display1Press);

            TriList.SetSigFalseAction(UIBoolJoin.Display2AudioButtonPressAndFb, Display2AudioPress);
            TriList.SetSigFalseAction(UIBoolJoin.Display2ControlButtonPress, Display2ControlPress);
            TriList.SetSigTrueAction(UIBoolJoin.Display2SelectPress, Display2Press);
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Show()
        {
            TriList.BooleanInput[UIBoolJoin.ToggleSharingModeVisible].BoolValue = true;
            TriList.BooleanInput[UIBoolJoin.StagingPageVisible].BoolValue = true;
            if(IsSharingModeAdvanced)
                TriList.BooleanInput[UIBoolJoin.DualDisplayPageVisible].BoolValue = true;
            else
                TriList.BooleanInput[UIBoolJoin.SelectASourceVisible].BoolValue = true;
            base.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        //public override void Hide()
        //{
        //    TriList.BooleanInput[UIBoolJoin.ToggleSharingModeVisible].BoolValue = false;
        //    TriList.BooleanInput[UIBoolJoin.StagingPageVisible].BoolValue = false;
        //    if(IsSharingModeAdvanced)
        //        TriList.BooleanInput[UIBoolJoin.DualDisplayPageVisible].BoolValue = false;
        //    else
        //        TriList.BooleanInput[UIBoolJoin.SelectASourceVisible].BoolValue = false;
        //    base.Hide();
        //}

        public void SetCurrentRoomFromParent()
        {
            if (IsSharingModeAdvanced)
                return; // add stuff here
            else
                SetupSourceListForSimpleRouting();
        }

        /// <summary>
        /// 
        /// </summary>
        void SetupSourceListForSimpleRouting()
        {
            // get the source list config and set up the source list
            var config = ConfigReader.ConfigObject.SourceLists;
            if (config.ContainsKey(Parent.CurrentRoom.SourceListKey))
            {
                var srcList = config[Parent.CurrentRoom.SourceListKey]
                    .Values.ToList().OrderBy(s => s.Order);
                // Setup sources list			
                uint i = 1; // counter for UI list
                foreach (var srcConfig in srcList)
                {
                    if (!srcConfig.IncludeInSourceList) // Skip sources marked this way
                        continue;

                    var sourceKey = srcConfig.SourceKey;
                    var actualSource = DeviceManager.GetDeviceForKey(sourceKey) as Device;
                    if (actualSource == null)
                    {
                        Debug.Console(0, "Cannot assign missing source '{0}' to source UI list",
                            srcConfig.SourceKey);
                        continue;
                    }
                    var localSrcItem = srcConfig; // lambda scope below
                    var localIndex = i;
                    SourcesSrl.GetBoolFeedbackSig(i, 1).UserObject = new Action<bool>(b =>
                         {
                             if (IsSharingModeAdvanced)
                             {
                                 if (LastSelectedSourceSig != null)
                                     LastSelectedSourceSig.BoolValue = false;
                                 SourceListButtonPress(localSrcItem);
                                 LastSelectedSourceSig = SourcesSrl.BoolInputSig(localIndex, 1);
                                 LastSelectedSourceSig.BoolValue = true;
                             }
                             else
                                 Parent.CurrentRoom.DoSourceToAllDestinationsRoute(localSrcItem);
                         });
                    SourcesSrl.StringInputSig(i, 1).StringValue = srcConfig.PreferredName;
                    i++;

                    //var item = new SubpageReferenceListSourceItem(i++, SourcesSrl, srcConfig,
                    //    b => { if (!b) UiSelectSource(localSrcConfig); });
                    //SourcesSrl.AddItem(item); // add to the SRL
                    //item.RegisterForSourceChange(Parent.CurrentRoom);
                }
                SourcesSrl.Count = (ushort)(i - 1);
                Parent.CurrentRoom.CurrentSingleSourceChange += CurrentRoom_CurrentSourceInfoChange;
                Parent.CurrentRoom.CurrentDisplay1SourceChange += CurrentRoom_CurrentDisplay1SourceChange;
                Parent.CurrentRoom.CurrentDisplay2SourceChange += CurrentRoom_CurrentDisplay2SourceChange;
            }
        }

        void SetupSourceListForAdvancedRouting()
        {

        }

        void CurrentRoom_CurrentSourceInfoChange(EssentialsRoomBase room, SourceListItem info, ChangeType type)
        {
            
        }

        void CurrentRoom_CurrentDisplay1SourceChange(EssentialsRoomBase room, SourceListItem info, ChangeType type)
        {
            TriList.StringInput[UIStringJoin.Display1SourceLabel].StringValue = PendingSource.PreferredName;

        }

        void CurrentRoom_CurrentDisplay2SourceChange(EssentialsRoomBase room, SourceListItem info, ChangeType type)
        {
            TriList.StringInput[UIStringJoin.Display2SourceLabel].StringValue = PendingSource.PreferredName;
        }

        /// <summary>
        /// 
        /// </summary>
        void ToggleSharingModePressed()
        {
            Hide();
            IsSharingModeAdvanced = !IsSharingModeAdvanced;
            TriList.BooleanInput[UIBoolJoin.ToggleSharingModePress].BoolValue = IsSharingModeAdvanced;
            Show();
        }

        public void SourceListButtonPress(SourceListItem item)
        {
            // start the timer
            // show FB on potential source
            TriList.BooleanInput[UIBoolJoin.Display1AudioButtonEnable].BoolValue = false;
            TriList.BooleanInput[UIBoolJoin.Display1ControlButtonEnable].BoolValue = false;
            TriList.BooleanInput[UIBoolJoin.Display2AudioButtonEnable].BoolValue = false;
            TriList.BooleanInput[UIBoolJoin.Display2ControlButtonEnable].BoolValue = false;
            PendingSource = item;
        }

        void EnableAppropriateDisplayButtons()
        {
            TriList.BooleanInput[UIBoolJoin.Display1AudioButtonEnable].BoolValue = true;
            TriList.BooleanInput[UIBoolJoin.Display1ControlButtonEnable].BoolValue = true;
            TriList.BooleanInput[UIBoolJoin.Display2AudioButtonEnable].BoolValue = true;
            TriList.BooleanInput[UIBoolJoin.Display2ControlButtonEnable].BoolValue = true;
            if (LastSelectedSourceSig != null)
                LastSelectedSourceSig.BoolValue = false;
        }

        public void Display1Press()
        {
            EnableAppropriateDisplayButtons();
            Parent.CurrentRoom.SourceToDisplay1(PendingSource);
            // Enable end meeting
        }

        public void Display1AudioPress()
        {

        }


        public void Display1ControlPress()
        {

        }

        public void Display2Press()
        {
            EnableAppropriateDisplayButtons();
            Parent.CurrentRoom.SourceToDisplay2(PendingSource);
        }

        public void Display2AudioPress()
        {

        }

        public void Display2ControlPress()
        {

        }
    }
}