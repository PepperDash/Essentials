using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

using PepperDash.Core;
using PepperDash.Essentials;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.SmartObjects;
using PepperDash.Essentials.Core.Touchpanels.Keyboards;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec;

namespace PepperDash.Essentials.UIDrivers.VC
{
    /// <summary>
    /// This fella will likely need to interact with the room's source, although that is routed via the spark...
    /// Probably needs event or FB to feed AV driver - to show two-mute volume when appropriate.
    /// 
    /// </summary>
    public class EssentialsVideoCodecUiDriver : PanelDriverBase
    {
        IAVWithVCDriver Parent;

        /// <summary>
        /// 
        /// </summary>
        VideoCodecBase Codec;

        /// <summary>
        /// To drive UI elements outside of this driver that may be dependent on this.
        /// </summary>
		//BoolFeedback InCall;
        BoolFeedback LocalPrivacyIsMuted;

        /// <summary>
        /// For the subpages above the bar
        /// </summary>
        JoinedSigInterlock VCControlsInterlock;

        /// <summary>
        /// For the different staging bars: Active, inactive
        /// </summary>
        JoinedSigInterlock StagingBarsInterlock;

        /// <summary>
        /// For the staging button feedbacks
        /// </summary>
        JoinedSigInterlock StagingButtonsFeedbackInterlock;

        SmartObjectNumeric DialKeypad;

        SubpageReferenceList ActiveCallsSRL;

        SmartObjectDynamicList RecentCallsList;

		SmartObjectDynamicList DirectoryList;
        
        BoolFeedback DirectoryBackButtonVisibleFeedback;

        // These are likely temp until we get a keyboard built
        StringFeedback DialStringFeedback;
        StringBuilder DialStringBuilder = new StringBuilder();
        BoolFeedback DialStringBackspaceVisibleFeedback;

        StringFeedback SearchStringFeedback;
        StringBuilder SearchStringBuilder = new StringBuilder();
        BoolFeedback SearchStringBackspaceVisibleFeedback;

        ModalDialog IncomingCallModal;

        eKeypadMode KeypadMode;

		bool CodecHasFavorites;

		CTimer BackspaceTimer;


        /// <summary>
        /// The panel header driver
        /// </summary>
        EssentialsHeaderDriver HeaderDriver;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="triList"></param>
        /// <param name="codec"></param>
        public EssentialsVideoCodecUiDriver(BasicTriListWithSmartObject triList, IAVWithVCDriver parent, VideoCodecBase codec, EssentialsHeaderDriver headerDriver)
            : base(triList)
        {
            try
            {
                if (codec == null)
                    throw new ArgumentNullException("Codec cannot be null");
                Codec = codec;
                Parent = parent;
                HeaderDriver = headerDriver;
                SetupCallStagingPopover();
                SetupDialKeypad();
                ActiveCallsSRL = new SubpageReferenceList(triList, UISmartObjectJoin.CodecActiveCallsHeaderList, 5,5,5);
                SetupRecentCallsList();
                SetupFavorites();
                SetupLayoutControls();

                codec.CallStatusChange += new EventHandler<CodecCallStatusItemChangeEventArgs>(Codec_CallStatusChange);

                // If the codec is ready, then get the values we want, otherwise wait
                if (Codec.IsReady)
                    Codec_IsReady();
                else
                    codec.IsReadyChange += (o, a) => Codec_IsReady();

                //InCall = new BoolFeedback(() => false);
                LocalPrivacyIsMuted = new BoolFeedback(() => false);

                VCControlsInterlock = new JoinedSigInterlock(triList);
                if (CodecHasFavorites)
                    VCControlsInterlock.SetButDontShow(UIBoolJoin.VCKeypadWithFavoritesVisible);
                else
                    VCControlsInterlock.SetButDontShow(UIBoolJoin.VCKeypadVisible);

                StagingBarsInterlock = new JoinedSigInterlock(triList);
                StagingBarsInterlock.SetButDontShow(UIBoolJoin.VCStagingInactivePopoverVisible);

                StagingButtonsFeedbackInterlock = new JoinedSigInterlock(triList);
                StagingButtonsFeedbackInterlock.ShowInterlocked(UIBoolJoin.VCStagingKeypadPress);

                // Return formatted when dialing, straight digits when in call
                DialStringFeedback = new StringFeedback(() =>
                {
                    if (KeypadMode == eKeypadMode.Dial)
                        return GetFormattedDialString(DialStringBuilder.ToString());
                    else
                        return DialStringBuilder.ToString();

                });
                DialStringFeedback.LinkInputSig(triList.StringInput[UIStringJoin.CodecAddressEntryText]);

                DialStringBackspaceVisibleFeedback = new BoolFeedback(() => DialStringBuilder.Length > 0);
                DialStringBackspaceVisibleFeedback
                    .LinkInputSig(triList.BooleanInput[UIBoolJoin.VCKeypadBackspaceVisible]);

                SearchStringFeedback = new StringFeedback(() =>
                {
                    if (SearchStringBuilder.Length > 0)
                    {
                        Parent.Keyboard.EnableGoButton();
                        return SearchStringBuilder.ToString();
                    }
                    else
                    {
                        Parent.Keyboard.DisableGoButton();
                        return "Tap for keyboard";
                    }
                });
                SearchStringFeedback.LinkInputSig(triList.StringInput[UIStringJoin.CodecDirectorySearchEntryText]);

                SetupDirectoryList();

                SearchStringBackspaceVisibleFeedback = new BoolFeedback(() => SearchStringBuilder.Length > 0);
                SearchStringBackspaceVisibleFeedback.LinkInputSig(triList.BooleanInput[UIBoolJoin.VCDirectoryBackspaceVisible]);

                triList.SetSigFalseAction(UIBoolJoin.VCDirectoryBackPress, GetDirectoryParentFolderContents);

                DirectoryBackButtonVisibleFeedback = (codec as IHasDirectory).CurrentDirectoryResultIsNotDirectoryRoot;
                DirectoryBackButtonVisibleFeedback
                    .LinkInputSig(triList.BooleanInput[UIBoolJoin.VCDirectoryBackVisible]);

                triList.SetSigFalseAction(UIBoolJoin.VCKeypadTextPress, RevealKeyboard);

                triList.SetSigFalseAction(UIBoolJoin.VCDirectorySearchTextPress, RevealKeyboard);

                triList.SetSigHeldAction(UIBoolJoin.VCDirectoryBackspacePress, 500,
                    StartSearchBackspaceRepeat, StopSearchBackspaceRepeat, SearchKeypadBackspacePress);

            }
            catch (Exception e)
            {
                Debug.Console(1, "Exception in VideoCodecUiDriver Constructor: {0}", e);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Codec_IsReady()
        {
            string roomNumberSipUri = "";

            if (!string.IsNullOrEmpty(Codec.CodecInfo.SipUri)) // If both values are present, format the string with a pipe divider
                roomNumberSipUri = string.Format("{0} | {1}", GetFormattedPhoneNumber(Codec.CodecInfo.SipPhoneNumber), Codec.CodecInfo.SipUri);
            else                                               // If only one value present, just show the phone number
                roomNumberSipUri = Codec.CodecInfo.SipPhoneNumber;

            if(string.IsNullOrEmpty(roomNumberSipUri))
                roomNumberSipUri = string.Format("{0} | {1}", Codec.CodecInfo.E164Alias, Codec.CodecInfo.H323Id);

            TriList.SetString(UIStringJoin.RoomPhoneText, roomNumberSipUri);

            if(HeaderDriver.HeaderButtonsAreSetUp)
                HeaderDriver.ComputeHeaderCallStatus(Codec);
        }

        /// <summary>
        /// Handles status changes for calls
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void Codec_CallStatusChange(object sender, CodecCallStatusItemChangeEventArgs e)
        {
            var call = e.CallItem;

            switch (e.CallItem.Status)
            {
                case eCodecCallStatus.Connected:
                    // fire at SRL item
                    KeypadMode = eKeypadMode.DTMF;
                    DialStringBuilder.Remove(0, DialStringBuilder.Length);
                    DialStringFeedback.FireUpdate();
					DialStringTextCheckEnables();
                    Parent.ShowNotificationRibbon("Connected", 2000);
                    StagingButtonsFeedbackInterlock.ShowInterlocked(UIBoolJoin.VCStagingKeypadPress);
					ShowKeypad();
                    ((Parent.CurrentRoom as IHasCurrentVolumeControls).CurrentVolumeControls as IBasicVolumeWithFeedback).MuteOff();
					//VCControlsInterlock.ShowInterlocked(UIBoolJoin.VCKeypadVisible);
                    break;
                case eCodecCallStatus.Connecting:
                    // fire at SRL item
                    Parent.ShowNotificationRibbon("Connecting", 0);
                    break;
                case eCodecCallStatus.Dialing:
                    Parent.ShowNotificationRibbon("Connecting", 0);
                    break;
                case eCodecCallStatus.Disconnected:
					if (IncomingCallModal != null)
						IncomingCallModal.HideDialog();
                    if (!Codec.IsInCall)
                    {
                        KeypadMode = eKeypadMode.Dial;
						// show keypad if we're in call UI mode
						ShowKeypad();
                        DialStringBuilder.Remove(0, DialStringBuilder.Length);
                        DialStringFeedback.FireUpdate();
                        Parent.ShowNotificationRibbon("Disconnected", 2000);
                    }
                    break;
                case eCodecCallStatus.Disconnecting:
                    break;
                case eCodecCallStatus.EarlyMedia:
                    break;
                case eCodecCallStatus.Idle:
                    break;
                case eCodecCallStatus.OnHold:
                    break;
                case eCodecCallStatus.Preserved:
                    break;
                case eCodecCallStatus.RemotePreserved:
                    break;
                case eCodecCallStatus.Ringing:
                    {
                        // fire up a modal
                        if( !Codec.CodecInfo.AutoAnswerEnabled && call.Direction == eCodecCallDirection.Incoming)
                            ShowIncomingModal(call);
                        break;
                    }
                default:
                    break;
            }
            TriList.UShortInput[UIUshortJoin.VCStagingConnectButtonMode].UShortValue = (ushort)(Codec.IsInCall ? 1 : 0);
			
			uint stageJoin;
			if (Codec.IsInCall)
				stageJoin = UIBoolJoin.VCStagingActivePopoverVisible;
			else
				stageJoin = UIBoolJoin.VCStagingInactivePopoverVisible;
			if (IsVisible)
				StagingBarsInterlock.ShowInterlocked(stageJoin);
			else
				StagingBarsInterlock.SetButDontShow(stageJoin);

            HeaderDriver.ComputeHeaderCallStatus(Codec);

            // Update active call list
            UpdateHeaderActiveCallList();
        }

        /// <summary>
        /// Redraws the calls list on the header
        /// </summary>
        void UpdateHeaderActiveCallList()
        {
            var activeList = Codec.ActiveCalls.Where(c => c.IsActiveCall).ToList();
            ActiveCallsSRL.Clear();
            ushort i = 1;
            foreach (var c in activeList)
            {
				//var item = new SubpageReferenceListItem(1, ActiveCallsSRL);
                ActiveCallsSRL.StringInputSig(i, 1).StringValue = c.Name;
                ActiveCallsSRL.StringInputSig(i, 2).StringValue = c.Number;
                ActiveCallsSRL.StringInputSig(i, 3).StringValue = c.Status.ToString();
                ActiveCallsSRL.StringInputSig(i, 4).StringValue = string.Format("Participant {0}", i);
                ActiveCallsSRL.UShortInputSig(i, 1).UShortValue = (ushort)(c.Type == eCodecCallType.Video ? 2 : 1);
                var cc = c; // for scope in lambda
                ActiveCallsSRL.GetBoolFeedbackSig(i, 1).SetSigFalseAction(() => Codec.EndCall(cc));
                i++;
            }
                ActiveCallsSRL.Count = (ushort)activeList.Count;

            // If Active Calls list is visible and codec is not in a call, hide the list    
            if (!Codec.IsInCall && Parent.PopupInterlock.CurrentJoin == UIBoolJoin.HeaderActiveCallsListVisible)
                Parent.PopupInterlock.ShowInterlockedWithToggle(UIBoolJoin.HeaderActiveCallsListVisible);
        }
        
        /// <summary>
        /// 
        /// </summary>
        void ShowIncomingModal(CodecActiveCallItem call)
        {
			(Parent as IAVWithVCDriver).PrepareForCodecIncomingCall();
            IncomingCallModal = new ModalDialog(TriList);
            string msg;
            string icon;
            if (call.Type == eCodecCallType.Audio)
            {
                icon = "Phone";
                msg = string.Format("Incoming phone call from: {0}", call.Name);
            }
            else
            {
                icon = "Camera";
                msg = string.Format("Incoming video call from: {0}", call.Name);
            }
            IncomingCallModal.PresentModalDialog(2, "Incoming Call", icon, msg,
                "Ignore", "Accept", false, false, b =>
                    {
						if (b == 1)
							Codec.RejectCall(call);
						else //2
							AcceptIncomingCall(call);
                        IncomingCallModal = null;
                    });
        }

		/// <summary>
		/// 
		/// </summary>
		void AcceptIncomingCall(CodecActiveCallItem call)
		{
			(Parent as IAVWithVCDriver).PrepareForCodecIncomingCall();
            (Parent as IAVWithVCDriver).ActivityCallButtonPressed();
			Codec.AcceptCall(call);
		}

        /// <summary>
        /// 
        /// </summary>
        public override void Show()
        {
            VCControlsInterlock.Show();
            StagingBarsInterlock.Show();
            DialStringFeedback.FireUpdate();
            base.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Hide()
        {
            VCControlsInterlock.Hide();
            StagingBarsInterlock.Hide();
            base.Hide();
        }

        /// <summary>
        /// Builds the call stage
        /// </summary>
        void SetupCallStagingPopover()
        {
            TriList.SetSigFalseAction(UIBoolJoin.VCStagingDirectoryPress, ShowDirectory);
            TriList.SetSigFalseAction(UIBoolJoin.VCStagingKeypadPress, ShowKeypad);
            TriList.SetSigFalseAction(UIBoolJoin.VCStagingRecentsPress, ShowRecents);
            TriList.SetSigFalseAction(UIBoolJoin.VCStagingConnectPress, ConnectPress);
            TriList.SetSigFalseAction(UIBoolJoin.CallEndPress, () =>
                {
                    if (Codec.ActiveCalls.Count > 1)
                    {
                        Parent.PopupInterlock.ShowInterlocked(UIBoolJoin.HeaderActiveCallsListVisible);
                    }
                    else
                        Codec.EndAllCalls();
                });
			TriList.SetSigFalseAction(UIBoolJoin.CallEndAllConfirmPress, () =>
				{
					Parent.PopupInterlock.HideAndClear();
					Codec.EndAllCalls();
				});
        }

        /// <summary>
        /// 
        /// </summary>
        void SetupDialKeypad()
        {
            if(TriList.SmartObjects.Contains(UISmartObjectJoin.VCDialKeypad))
            {
                DialKeypad = new SmartObjectNumeric(TriList.SmartObjects[UISmartObjectJoin.VCDialKeypad], true);
                DialKeypad.Digit0.SetSigFalseAction(() => DialKeypadPress("0"));
                DialKeypad.Digit1.SetSigFalseAction(() => DialKeypadPress("1"));
                DialKeypad.Digit2.SetSigFalseAction(() => DialKeypadPress("2"));
                DialKeypad.Digit3.SetSigFalseAction(() => DialKeypadPress("3"));
                DialKeypad.Digit4.SetSigFalseAction(() => DialKeypadPress("4"));
                DialKeypad.Digit5.SetSigFalseAction(() => DialKeypadPress("5"));
                DialKeypad.Digit6.SetSigFalseAction(() => DialKeypadPress("6"));
                DialKeypad.Digit7.SetSigFalseAction(() => DialKeypadPress("7"));
                DialKeypad.Digit8.SetSigFalseAction(() => DialKeypadPress("8"));
                DialKeypad.Digit9.SetSigFalseAction(() => DialKeypadPress("9"));
                DialKeypad.Misc1SigName = "*";
                DialKeypad.Misc1.SetSigFalseAction(() => DialKeypadPress("*"));
                DialKeypad.Misc2SigName = "#";
                DialKeypad.Misc2.SetSigFalseAction(() => DialKeypadPress("#"));
				//TriList.SetSigFalseAction(UIBoolJoin.VCKeypadBackspacePress, DialKeypadBackspacePress);
				TriList.SetSigHeldAction(UIBoolJoin.VCKeypadBackspacePress, 500,
					StartBackspaceRepeat, StopBackspaceRepeat, DialKeypadBackspacePress);
            }
            else
                Debug.Console(0, "Trilist {0:x2}, VC dial keypad object {1} not found. Check SGD file or VTP",
                    TriList.ID, UISmartObjectJoin.VCDialKeypad);
        }

        /// <summary>
        /// 
        /// </summary>
        void SetupRecentCallsList()
        {
            var codec = Codec as IHasCallHistory;
            if (codec != null)
            {
				codec.CallHistory.RecentCallsListHasChanged += (o, a) => RefreshRecentCallsList();
                // EVENT??????????????? Pointed at refresh
				RecentCallsList = new SmartObjectDynamicList(TriList.SmartObjects[UISmartObjectJoin.VCRecentsList], true, 1200);
				RefreshRecentCallsList();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        void RefreshRecentCallsList()
        {
            var codec = Codec as IHasCallHistory;
			uint textOffset = 1200;
			uint timeTextOffset = 1230;
            if (codec != null)
            {
                ushort i = 0;
                foreach (var c in codec.CallHistory.RecentCalls)
                {
                    i++;
					TriList.SetString(textOffset + i, c.Name);
					// if it's today, show a simpler string
					string timeText = null;
					if (c.StartTime.Date == DateTime.Now.Date)
						timeText = c.StartTime.ToShortTimeString();
					else if (c.StartTime == DateTime.MinValue)
						timeText = "";
					else
						timeText = c.StartTime.ToString();
					TriList.SetString(timeTextOffset + i, timeText);

					string iconName = null;
					if (c.OccurrenceType == eCodecOccurrenceType.Received)
                        iconName = "Misc-18_Light";
                    else if (c.OccurrenceType == eCodecOccurrenceType.Placed)
                        iconName = "Misc-17_Light";
					else
						iconName = "Delete";
					RecentCallsList.SetItemIcon(i, iconName);

                    var call = c; // for lambda scope
                    RecentCallsList.SetItemButtonAction(i, b => { if(!b) Codec.Dial(call.Number); });
                }
                RecentCallsList.Count = i;
            }
        }

		/// <summary>
		/// 
		/// </summary>
		void SetupFavorites()
		{
			var c = Codec as IHasCallFavorites;
			if (c != null && c.CallFavorites != null)
			{
				CodecHasFavorites = true;
				var favs = c.CallFavorites.Favorites;
				for (uint i = 0; i <= 3; i++)
				{
					if (i < favs.Count)
					{
						var fav = favs[(int)i];
                        TriList.SetString(UIStringJoin.VCFavoritesStart + i, fav.Name);
						TriList.SetBool(UIBoolJoin.VCFavoriteVisibleStart + i, true);
						TriList.SetSigFalseAction(UIBoolJoin.VCFavoritePressStart + i, () =>
							{
								Codec.Dial(fav.Number);
							});
					}
					else
                        TriList.SetBool(UIBoolJoin.VCFavoriteVisibleStart + i, false);
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		void SetupDirectoryList()
		{
			var codec = Codec as IHasDirectory;
			if (codec != null)
			{
				DirectoryList = new SmartObjectDynamicList(TriList.SmartObjects[UISmartObjectJoin.VCDirectoryList],
					true, 1300);
				codec.DirectoryResultReturned += new EventHandler<DirectoryEventArgs>(dir_DirectoryResultReturned);

				if (codec.PhonebookSyncState.InitialSyncComplete)
					SetCurrentDirectoryToRoot();
				else
				{
					codec.PhonebookSyncState.InitialSyncCompleted += new EventHandler<EventArgs>(PhonebookSyncState_InitialSyncCompleted);
				}

			    RefreshDirectory();
				
			}
		}

        /// <summary>
        /// Sets the current directory results to the DirectoryRoot and updates Back Button visibiltiy
        /// </summary>
        void SetCurrentDirectoryToRoot()
        {
            (Codec as IHasDirectory).SetCurrentDirectoryToRoot();

            SearchKeypadClear();

            RefreshDirectory();
        }

        /// <summary>
        /// Setup the Directory list when notified that the initial phonebook sync is completed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PhonebookSyncState_InitialSyncCompleted(object sender, EventArgs e)
        {
            var codec = Codec as IHasDirectory;

            SetCurrentDirectoryToRoot();

            RefreshDirectory();
            
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void dir_DirectoryResultReturned(object sender, DirectoryEventArgs e)
		{

			RefreshDirectory();
		}

        /// <summary>
        /// Helper method to retrieve directory folder contents and store last requested folder id
        /// </summary>
        /// <param name="folderId"></param>
        void GetDirectoryFolderContents(DirectoryFolder folder)
        {
            (Codec as IHasDirectory).GetDirectoryFolderContents(folder.FolderId);

        }

        /// <summary>
        /// Request the parent folder contents or sets back to the root if no parent folder
        /// </summary>
        void GetDirectoryParentFolderContents()
        {
            var codec = Codec as IHasDirectory;

            if (codec != null)
            {
                codec.GetDirectoryParentFolderContents();

                //RefreshDirectory();
            }
 
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dir"></param>
		void RefreshDirectory()
		{
            if ((Codec as IHasDirectory).CurrentDirectoryResult.CurrentDirectoryResults.Count > 0)
            {
                ushort i = 0;
                foreach (var r in (Codec as IHasDirectory).CurrentDirectoryResult.CurrentDirectoryResults)
                {
                    if (i == DirectoryList.MaxCount)
                    {
                        break;
                    }

                    i++;

                    if (r is DirectoryContact)
                    {
                        DirectoryList.SetItemMainText(i, r.Name);

                        var dc = r as DirectoryContact;

                        if (dc.ContactMethods.Count > 1)
                        {
                            // If more than one contact method, show contact method modal dialog
                            DirectoryList.SetItemButtonAction(i, b =>
                            {
                                if (!b)
                                {
                                    // Refresh the contact methods list
                                    RefreshContactMethodsModalList(dc);
                                    Parent.PopupInterlock.ShowInterlockedWithToggle(UIBoolJoin.MeetingsOrContacMethodsListVisible);
                                }
                            });

                        }
                        else
                        {
                            // If only one contact method, just dial that method
							DirectoryList.SetItemButtonAction(i, b => { if (!b) Codec.Dial(dc.ContactMethods[0].Number); });
                        }
                    }
                    else    // is DirectoryFolder
                    {
                        DirectoryList.SetItemMainText(i, string.Format("[+] {0}", r.Name));

                        var df = r as DirectoryFolder;

                        DirectoryList.SetItemButtonAction(i, b =>
                        {
                            if (!b)
                            {
                                GetDirectoryFolderContents(df);
                                // will later call event handler after folder contents retrieved
                            }
                        });
                    }
                }
                DirectoryList.Count = i;
            }
            else        // No results in directory, display message to user
            {
                DirectoryList.Count = 1;

                DirectoryList.SetItemMainText(1, "No Results Found");
            }

		}

        void RefreshContactMethodsModalList(DirectoryContact contact)
        {
            TriList.SetString(UIStringJoin.MeetingsOrContactMethodListIcon, "Users");
            TriList.SetString(UIStringJoin.MeetingsOrContactMethodListTitleText, "Contact Methods");

            ushort i = 0;
            foreach (var c in contact.ContactMethods)
            {
                i++;
                Parent.MeetingOrContactMethodModalSrl.StringInputSig(i, 1).StringValue = c.Device.ToString();
                Parent.MeetingOrContactMethodModalSrl.StringInputSig(i, 2).StringValue = c.CallType.ToString();
                Parent.MeetingOrContactMethodModalSrl.StringInputSig(i, 3).StringValue = c.Number;
                Parent.MeetingOrContactMethodModalSrl.StringInputSig(i, 4).StringValue = "";
                Parent.MeetingOrContactMethodModalSrl.StringInputSig(i, 5).StringValue = "Connect";
                Parent.MeetingOrContactMethodModalSrl.BoolInputSig(i, 2).BoolValue = true;
                var cc = c; // to maintian lambda scope
                Parent.MeetingOrContactMethodModalSrl.GetBoolFeedbackSig(i, 1).SetSigFalseAction(() =>
                {
                    Parent.PopupInterlock.Hide();
                    var codec = Codec as VideoCodecBase;
                    if (codec != null)
                        codec.Dial(cc.Number);
                });
            }
            Parent.MeetingOrContactMethodModalSrl.Count = i;

        }

		/// <summary>
		/// 
		/// </summary>
		void SetupLayoutControls()
		{
			TriList.SetSigFalseAction(UIBoolJoin.VCStagingSelfViewLayoutPress, this.ShowSelfViewLayout);
			var svc = Codec as IHasCodecSelfView;
			if (svc != null)
			{
				TriList.SetSigFalseAction(UIBoolJoin.VCSelfViewTogglePress, svc.SelfViewModeToggle);
				svc.SelfviewIsOnFeedback.LinkInputSig(TriList.BooleanInput[UIBoolJoin.VCSelfViewTogglePress]);
			}
			var lc = Codec as IHasCodecLayouts;
			if (lc != null)
			{
                TriList.SetSigFalseAction(UIBoolJoin.VCLayoutTogglePress, lc.LocalLayoutToggleSingleProminent);
				lc.LocalLayoutFeedback.LinkInputSig(TriList.StringInput[UIStringJoin.VCLayoutModeText]);
				lc.LocalLayoutFeedback.OutputChange += (o,a) => 
				{
					TriList.BooleanInput[UIBoolJoin.VCLayoutTogglePress].BoolValue =
						lc.LocalLayoutFeedback.StringValue == "Prominent";
				};


				// attach to cisco special things to enable buttons
				var cisco = Codec as PepperDash.Essentials.Devices.Common.VideoCodec.Cisco.CiscoSparkCodec;
				if (cisco != null)
				{
					// Cisco has min/max buttons that need special sauce
					cisco.SharingContentIsOnFeedback.OutputChange += CiscoSharingAndPresentation_OutputChanges;
					//cisco.PresentationViewMaximizedFeedback.OutputChange += CiscoSharingAndPresentation_OutputChanges;

					TriList.SetSigFalseAction(UIBoolJoin.VCMinMaxPress, cisco.MinMaxLayoutToggle);
				}
				 
			}
		}

		/// <summary>
		/// This should only be linked by cisco classes (spark initially)
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		void CiscoSharingAndPresentation_OutputChanges(object sender, EventArgs e)
		{
			var cisco = Codec as PepperDash.Essentials.Devices.Common.VideoCodec.Cisco.CiscoSparkCodec;
			if (cisco != null)
			{
                var sharingNear = cisco.SharingContentIsOnFeedback.BoolValue;

				var sharingFar = cisco.FarEndIsSharingContentFeedback.BoolValue;
				//set feedback and enables
                TriList.BooleanInput[UIBoolJoin.VCMinMaxEnable].BoolValue = sharingNear;
                TriList.BooleanInput[UIBoolJoin.VCLayoutToggleEnable].BoolValue = sharingNear || sharingFar;
                TriList.BooleanInput[UIBoolJoin.VCMinMaxPress].BoolValue = sharingNear;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        void RevealKeyboard()
        {
            if (VCControlsInterlock.CurrentJoin == UIBoolJoin.VCKeypadWithFavoritesVisible && KeypadMode == eKeypadMode.Dial)
            {
                var kb = Parent.Keyboard;
				kb.KeyPress -= Keyboard_DialKeyPress;
                kb.KeyPress += Keyboard_DialKeyPress;
                kb.HideAction = this.DetachDialKeyboard;
                kb.GoButtonText = "Connect";
                kb.GoButtonVisible = true;
                DialStringTextCheckEnables();
                kb.Show();
            }
            else if(VCControlsInterlock.CurrentJoin == UIBoolJoin.VCDirectoryVisible)
            {
                var kb = Parent.Keyboard;
				kb.KeyPress -= Keyboard_SearchKeyPress;
				kb.KeyPress += Keyboard_SearchKeyPress;
                kb.HideAction = this.DetachSearchKeyboard;
                kb.GoButtonText = "Search";
                kb.GoButtonVisible = true;
                SearchStringKeypadCheckEnables();
                kb.Show();
            }
        }

        /// <summary>
        /// Event handler for keyboard dialing
        /// </summary>
        void Keyboard_DialKeyPress(object sender, PepperDash.Essentials.Core.Touchpanels.Keyboards.KeyboardControllerPressEventArgs e)
        {
            if (VCControlsInterlock.CurrentJoin == UIBoolJoin.VCKeypadWithFavoritesVisible && KeypadMode == eKeypadMode.Dial)
            {
				if (e.Text != null)
					DialStringBuilder.Append(e.Text);
				else
				{
					if (e.SpecialKey == KeyboardSpecialKey.Backspace)
						DialKeypadBackspacePress();
					else if (e.SpecialKey == KeyboardSpecialKey.Clear)
						DialKeypadClear();
					else if (e.SpecialKey == KeyboardSpecialKey.GoButton)
					{
						ConnectPress();
					}
				}
                DialStringFeedback.FireUpdate();
                DialStringTextCheckEnables();
            } 
        }

		/// <summary>
		/// Event handler for keyboard directory searches
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
        void Keyboard_SearchKeyPress(object sender, KeyboardControllerPressEventArgs e)
        {
            if (VCControlsInterlock.CurrentJoin == UIBoolJoin.VCDirectoryVisible)
            {
                if (e.Text != null)
                    SearchStringBuilder.Append(e.Text);
                else
                {
                    if (e.SpecialKey == KeyboardSpecialKey.Backspace)
                        SearchKeypadBackspacePress();
                    else if (e.SpecialKey == KeyboardSpecialKey.Clear)
                        SearchKeypadClear();
                    else if (e.SpecialKey == KeyboardSpecialKey.GoButton)
                    {
                        SearchPress();
                        Parent.Keyboard.Hide();
                    }
                }
                SearchStringFeedback.FireUpdate();
                SearchStringKeypadCheckEnables();
            }
        }

		/// <summary>
		/// Call
		/// </summary>
        void DetachDialKeyboard()
        {
            Parent.Keyboard.KeyPress -= Keyboard_DialKeyPress;
        }

        void DetachSearchKeyboard()
        {
            Parent.Keyboard.KeyPress -= Keyboard_SearchKeyPress;
        }

        /// <summary>
        /// Shows the camera controls subpage
        /// </summary>
        void ShowCameraControls()
        {
            VCControlsInterlock.ShowInterlocked(UIBoolJoin.VCCameraVisible);
            StagingButtonsFeedbackInterlock.ShowInterlocked(UIBoolJoin.VCStagingCameraPress);
        }

		/// <summary>
		/// shows the directory subpage
		/// </summary>
        void ShowDirectory()
        {
            // populate directory
            VCControlsInterlock.ShowInterlocked(UIBoolJoin.VCDirectoryVisible);
            StagingButtonsFeedbackInterlock.ShowInterlocked(UIBoolJoin.VCStagingDirectoryPress);

        }

		/// <summary>
		/// shows the appropriate keypad depending on mode and whether visible
		/// </summary>
        void ShowKeypad()
        {
			uint join = Codec.IsInCall ? UIBoolJoin.VCKeypadVisible : UIBoolJoin.VCKeypadWithFavoritesVisible;
			if (IsVisible)
				VCControlsInterlock.ShowInterlocked(join);
			else
				VCControlsInterlock.SetButDontShow(join);
            StagingButtonsFeedbackInterlock.ShowInterlocked(UIBoolJoin.VCStagingKeypadPress);
        }

		/// <summary>
		/// Shows the self-view layout controls subpage
		/// </summary>
		void ShowSelfViewLayout()
		{
			VCControlsInterlock.ShowInterlocked(UIBoolJoin.VCSelfViewLayoutVisible);
			StagingButtonsFeedbackInterlock.ShowInterlocked(UIBoolJoin.VCStagingSelfViewLayoutPress);
		}

		/// <summary>
		/// Shows the recents subpage
		/// </summary>
        void ShowRecents()
        {
            //populate recents
            VCControlsInterlock.ShowInterlocked(UIBoolJoin.VCRecentsVisible);
            StagingButtonsFeedbackInterlock.ShowInterlocked(UIBoolJoin.VCStagingRecentsPress);
        }

        /// <summary>
        /// Connect call button
        /// </summary>
        void ConnectPress()
        {
			if (Parent.Keyboard != null)
				Parent.Keyboard.Hide();
            Codec.Dial(DialStringBuilder.ToString());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="i"></param>
        void DialKeypadPress(string i)
        {
            if (KeypadMode == eKeypadMode.Dial)
            {
                DialStringBuilder.Append(i);
                DialStringFeedback.FireUpdate();
                DialStringTextCheckEnables();
            }
            else
            {
                Codec.SendDtmf(i);
                DialStringBuilder.Append(i);
                DialStringFeedback.FireUpdate();
                // no delete key in this mode!
            }
			DialStringTextCheckEnables();
		}
	
		/// <summary>
		/// Does what it says
		/// </summary>
		void StartBackspaceRepeat()
		{
			if (BackspaceTimer == null)
			{
				BackspaceTimer = new CTimer(o => DialKeypadBackspacePress(), null, 0, 175);
			}
		}

		/// <summary>
		/// Does what it says
		/// </summary>
		void StopBackspaceRepeat()
		{
			if (BackspaceTimer != null)
			{
				BackspaceTimer.Stop();
				BackspaceTimer = null;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        void DialKeypadBackspacePress()
        {
			if (KeypadMode == eKeypadMode.Dial)
			{
				DialStringBuilder.Remove(DialStringBuilder.Length - 1, 1);
				DialStringFeedback.FireUpdate();
				DialStringTextCheckEnables();
			}
			else
				DialKeypadClear();
        }

        /// <summary>
        /// Clears the dial keypad
        /// </summary>
        void DialKeypadClear()
        {
            DialStringBuilder.Remove(0, DialStringBuilder.Length);
            DialStringFeedback.FireUpdate();
            DialStringTextCheckEnables();
        }

        /// <summary>
        /// Checks the enabled states of various elements around the keypad
        /// </summary>
        void DialStringTextCheckEnables()
        {
            var textIsEntered = DialStringBuilder.Length > 0;
            TriList.SetBool(UIBoolJoin.VCKeypadBackspaceVisible, textIsEntered);
            TriList.SetBool(UIBoolJoin.VCStagingConnectEnable, textIsEntered);
            if (textIsEntered)
                Parent.Keyboard.EnableGoButton();
            else
                Parent.Keyboard.DisableGoButton();
        }

		/// <summary>
		/// 
		/// </summary>
        void SearchPress()
        {
            (Codec as IHasDirectory).SearchDirectory(SearchStringBuilder.ToString());
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="i"></param>
        void SearchKeyboardPress(string i)
        {
            SearchStringBuilder.Append(i);
            SearchStringFeedback.FireUpdate();
            SearchStringKeypadCheckEnables();
        }

		/// <summary>
		/// Does what it says
		/// </summary>
		void StartSearchBackspaceRepeat()
		{
			if (BackspaceTimer == null)
			{
				BackspaceTimer = new CTimer(o => SearchKeypadBackspacePress(), null, 0, 175);
			}
		}

		/// <summary>
		/// Does what it says
		/// </summary>
		void StopSearchBackspaceRepeat()
		{
			if (BackspaceTimer != null)
			{
				BackspaceTimer.Stop();
				BackspaceTimer = null;
			}
		}

        /// <summary>
        /// 
        /// </summary>
        void SearchKeypadBackspacePress()
        {
            SearchStringBuilder.Remove(SearchStringBuilder.Length - 1, 1);

            if (SearchStringBuilder.Length == 0)
                SetCurrentDirectoryToRoot();

            SearchStringFeedback.FireUpdate();
            SearchStringKeypadCheckEnables();
        }

        /// <summary>
        /// Clears the Search keypad
        /// </summary>
        void SearchKeypadClear()
        {
            SearchStringBuilder.Remove(0, SearchStringBuilder.Length);
            SearchStringFeedback.FireUpdate();
            SearchStringKeypadCheckEnables();

            if ((Codec as IHasDirectory).CurrentDirectoryResultIsNotDirectoryRoot.BoolValue)
                SetCurrentDirectoryToRoot();
        }

        /// <summary>
        /// Checks the enabled states of various elements around the keypad
        /// </summary>
        void SearchStringKeypadCheckEnables()
        {
            var textIsEntered = SearchStringBuilder.Length > 0;
            TriList.SetBool(UIBoolJoin.VCDirectoryBackspaceVisible, textIsEntered);
            if (textIsEntered)
                Parent.Keyboard.EnableGoButton();
            else
                Parent.Keyboard.DisableGoButton();
        }


        /// <summary>
        /// Returns the text value for the keypad dial entry field
        /// </summary>
        /// <returns></returns>
        string GetFormattedDialString(string ds)
        {
            if (DialStringBuilder.Length == 0 && !Codec.IsInCall)
            {
                return "Tap for keyboard";
            }

            return GetFormattedPhoneNumber(ds);
            
        }

        /// <summary>
        /// Formats a string of numbers as a North American phone number
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        string GetFormattedPhoneNumber(string s)
        {
            if (Regex.Match(s, @"^\d{4,7}$").Success) // 456-7890
                return string.Format("{0}-{1}", s.Substring(0, 3), s.Substring(3));
            if (Regex.Match(s, @"^9\d{4,7}$").Success) // 456-7890
                return string.Format("9 {0}-{1}", s.Substring(1, 3), s.Substring(4));
            if (Regex.Match(s, @"^\d{8,10}$").Success) // 123-456-78
                return string.Format("({0}) {1}-{2}", s.Substring(0, 3), s.Substring(3, 3), s.Substring(6));
            if (Regex.Match(s, @"^\d{10}$").Success) // 123-456-7890 full
                return string.Format("({0}) {1}-{2}", s.Substring(0, 3), s.Substring(3, 3), s.Substring(6));
            if (Regex.Match(s, @"^1\d{10}$").Success)
                return string.Format("+1 ({0}) {1}-{2}", s.Substring(1, 3), s.Substring(4, 3), s.Substring(7));
            if (Regex.Match(s, @"^9\d{10}$").Success)
                return string.Format("9 ({0}) {1}-{2}", s.Substring(1, 3), s.Substring(4, 3), s.Substring(7));
            if (Regex.Match(s, @"^91\d{10}$").Success)
                return string.Format("9 +1 ({0}) {1}-{2}", s.Substring(2, 3), s.Substring(5, 3), s.Substring(8));
            return s;
        }

        enum eKeypadMode
        {
            Dial = 0,
            DTMF
        }
    }
}