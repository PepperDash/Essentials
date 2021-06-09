using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.DeviceSupport;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PepperDash.Core;
using PepperDash.Core.Intersystem.Tokens;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.Bridges;
using PepperDash.Essentials.Core.Config;
using PepperDash.Essentials.Core.DeviceTypeInterfaces;
using PepperDash.Essentials.Core.Routing;
using PepperDash.Essentials.Devices.Common.Cameras;
using PepperDash.Essentials.Devices.Common.Codec;
using PepperDash.Essentials.Devices.Common.VideoCodec.Cisco;
using PepperDash.Essentials.Devices.Common.VideoCodec.Interfaces;

namespace PepperDash.Essentials.Devices.Common.VideoCodec.ZoomRoom
{
	public class ZoomRoom : VideoCodecBase, IHasCodecSelfView, IHasDirectoryHistoryStack, ICommunicationMonitor,
		IRouting,
		IHasScheduleAwareness, IHasCodecCameras, IHasParticipants, IHasCameraOff, IHasCameraMute, IHasCameraAutoMode,
		IHasFarEndContentStatus, IHasSelfviewPosition, IHasPhoneDialing, IHasZoomRoomLayouts, IHasParticipantPinUnpin, IHasParticipantAudioMute, IHasSelfviewSize
	{
		private const long MeetingRefreshTimer = 60000;
		private const uint DefaultMeetingDurationMin = 30;
		private const string Delimiter = "\x0D\x0A";
		private readonly CrestronQueue<string> _receiveQueue;


		private readonly Thread _receiveThread;

		private readonly ZoomRoomSyncState _syncState;
		public bool CommDebuggingIsOn;
		private CodecDirectory _currentDirectoryResult;
		private uint _jsonCurlyBraceCounter;
		private bool _jsonFeedbackMessageIsIncoming;
		private StringBuilder _jsonMessage;
		private int _previousVolumeLevel;
		private CameraBase _selectedCamera;

		private readonly ZoomRoomPropertiesConfig _props;

		public ZoomRoom(DeviceConfig config, IBasicCommunication comm)
			: base(config)
		{
			_props = JsonConvert.DeserializeObject<ZoomRoomPropertiesConfig>(config.Properties.ToString());

			// The queue that will collect the repsonses in the order they are received
			_receiveQueue = new CrestronQueue<string>(1024);

			// The thread responsible for dequeuing and processing the messages
			_receiveThread = new Thread(o => ProcessQueue(), null) { Priority = Thread.eThreadPriority.MediumPriority };

			Communication = comm;

			if (_props.CommunicationMonitorProperties != null)
			{
				CommunicationMonitor = new GenericCommunicationMonitor(this, Communication,
					_props.CommunicationMonitorProperties);
			}
			else
			{
				CommunicationMonitor = new GenericCommunicationMonitor(this, Communication, 30000, 120000, 300000,
					"zStatus SystemUnit\r");
			}

			DeviceManager.AddDevice(CommunicationMonitor);

			Status = new ZoomRoomStatus();

			Configuration = new ZoomRoomConfiguration();

			CodecInfo = new ZoomRoomInfo(Status, Configuration);

			_syncState = new ZoomRoomSyncState(Key + "--Sync", this);

			_syncState.InitialSyncCompleted += SyncState_InitialSyncCompleted;

			PhonebookSyncState = new CodecPhonebookSyncState(Key + "--PhonebookSync");

			PortGather = new CommunicationGather(Communication, "\x0A") { IncludeDelimiter = true };
			PortGather.LineReceived += Port_LineReceived;

			CodecOsdIn = new RoutingInputPort(RoutingPortNames.CodecOsd,
				eRoutingSignalType.Audio | eRoutingSignalType.Video,
				eRoutingPortConnectionType.Hdmi, new Action(StopSharing), this);

			Output1 = new RoutingOutputPort(RoutingPortNames.AnyVideoOut,
				eRoutingSignalType.Audio | eRoutingSignalType.Video,
				eRoutingPortConnectionType.Hdmi, null, this);

			SelfviewIsOnFeedback = new BoolFeedback(SelfViewIsOnFeedbackFunc);

			CameraIsOffFeedback = new BoolFeedback(CameraIsOffFeedbackFunc);

			CameraIsMutedFeedback = CameraIsOffFeedback;

			CameraAutoModeIsOnFeedback = new BoolFeedback(CameraAutoModeIsOnFeedbackFunc);

			CodecSchedule = new CodecScheduleAwareness(MeetingRefreshTimer);

			ReceivingContent = new BoolFeedback(FarEndIsSharingContentFeedbackFunc);

			SelfviewPipPositionFeedback = new StringFeedback(SelfviewPipPositionFeedbackFunc);

			// TODO: #714 [ ] SelfviewPipSizeFeedback
			SelfviewPipSizeFeedback = new StringFeedback(SelfviewPipSizeFeedbackFunc);
			
			SetUpFeedbackActions();

			Cameras = new List<CameraBase>();

			SetUpDirectory();

			Participants = new CodecParticipants();

			SupportsCameraOff = _props.SupportsCameraOff;
			SupportsCameraAutoMode = _props.SupportsCameraAutoMode;

			PhoneOffHookFeedback = new BoolFeedback(PhoneOffHookFeedbackFunc);
			CallerIdNameFeedback = new StringFeedback(CallerIdNameFeedbackFunc);
			CallerIdNumberFeedback = new StringFeedback(CallerIdNumberFeedbackFunc);

			LocalLayoutFeedback = new StringFeedback(LocalLayoutFeedbackFunc);

			LayoutViewIsOnFirstPageFeedback = new BoolFeedback(LayoutViewIsOnFirstPageFeedbackFunc);
			LayoutViewIsOnLastPageFeedback = new BoolFeedback(LayoutViewIsOnLastPageFeedbackFunc);
			CanSwapContentWithThumbnailFeedback = new BoolFeedback(CanSwapContentWithThumbnailFeedbackFunc);
			ContentSwappedWithThumbnailFeedback = new BoolFeedback(ContentSwappedWithThumbnailFeedbackFunc);

			NumberOfScreensFeedback = new IntFeedback(NumberOfScreensFeedbackFunc);

		}

		public CommunicationGather PortGather { get; private set; }

		public ZoomRoomStatus Status { get; private set; }

		public ZoomRoomConfiguration Configuration { get; private set; }

		//CTimer LoginMessageReceivedTimer;
		//CTimer RetryConnectionTimer;

		/// <summary>
		/// Gets and returns the scaled volume of the codec
		/// </summary>
		protected override Func<int> VolumeLevelFeedbackFunc
		{
			get
			{
				return () => CrestronEnvironment.ScaleWithLimits(Configuration.Audio.Output.Volume, 100, 0, 65535, 0);
			}
		}

		protected override Func<bool> PrivacyModeIsOnFeedbackFunc
		{
			get { return () => Configuration.Call.Microphone.Mute; }
		}

		protected override Func<bool> StandbyIsOnFeedbackFunc
		{
			get { return () => false; }
		}

		protected override Func<string> SharingSourceFeedbackFunc
		{
			get { return () => Status.Sharing.dispState; }
		}

		protected override Func<bool> SharingContentIsOnFeedbackFunc
		{
			get { return () => Status.Call.Sharing.IsSharing; }
		}

		protected Func<bool> FarEndIsSharingContentFeedbackFunc
		{
			get { return () => Status.Call.Sharing.State == zEvent.eSharingState.Receiving; }
		}

		protected override Func<bool> MuteFeedbackFunc
		{
			get { return () => Configuration.Audio.Output.Volume == 0; }
		}

		//protected Func<bool> RoomIsOccupiedFeedbackFunc
		//{
		//    get
		//    {
		//        return () => false;
		//    }
		//}

		//protected Func<int> PeopleCountFeedbackFunc
		//{
		//    get
		//    {
		//        return () => 0;
		//    }
		//}

		protected Func<bool> SelfViewIsOnFeedbackFunc
		{
			get { return () => !Configuration.Video.HideConfSelfVideo; }
		}

		protected Func<bool> CameraIsOffFeedbackFunc
		{
			get { return () => Configuration.Call.Camera.Mute; }
		}

		protected Func<bool> CameraAutoModeIsOnFeedbackFunc
		{
			get { return () => false; }
		}

		protected Func<string> SelfviewPipPositionFeedbackFunc
		{
			get
			{
				return
					() =>
						_currentSelfviewPipPosition != null
							? _currentSelfviewPipPosition.Command ?? "Unknown"
							: "Unknown";
			}
		}

		// TODO: #714 [ ] SelfviewPipSizeFeedbackFunc
		protected Func<string> SelfviewPipSizeFeedbackFunc
		{
			get
			{
				return
					() =>
						_currentSelfviewPipSize != null
							? _currentSelfviewPipSize.Command ?? "Unknown"
							: "Unknown";
			}
		}

		protected Func<bool> LocalLayoutIsProminentFeedbackFunc
		{
			get { return () => false; }
		}


		public RoutingInputPort CodecOsdIn { get; private set; }
		public RoutingOutputPort Output1 { get; private set; }

		#region ICommunicationMonitor Members

		public StatusMonitorBase CommunicationMonitor { get; private set; }

		#endregion

		#region IHasCodecCameras Members

		public event EventHandler<CameraSelectedEventArgs> CameraSelected;

		public List<CameraBase> Cameras { get; private set; }

		public CameraBase SelectedCamera
		{
			get { return _selectedCamera; }
			private set
			{
				_selectedCamera = value;
				SelectedCameraFeedback.FireUpdate();
				ControllingFarEndCameraFeedback.FireUpdate();

				var handler = CameraSelected;
				if (handler != null)
				{
					handler(this, new CameraSelectedEventArgs(SelectedCamera));
				}
			}
		}


		public StringFeedback SelectedCameraFeedback { get; private set; }

		public void SelectCamera(string key)
		{
			if (Cameras == null)
			{
				return;
			}

			var camera = Cameras.FirstOrDefault(c => c.Key.IndexOf(key, StringComparison.OrdinalIgnoreCase) > -1);
			if (camera != null)
			{
				Debug.Console(1, this, "Selected Camera with key: '{0}'", camera.Key);
				SelectedCamera = camera;
			}
			else
			{
				Debug.Console(1, this, "Unable to select camera with key: '{0}'", key);
			}
		}

		public CameraBase FarEndCamera { get; private set; }

		public BoolFeedback ControllingFarEndCameraFeedback { get; private set; }

		#endregion

		#region IHasCodecSelfView Members

		public BoolFeedback SelfviewIsOnFeedback { get; private set; }

		public void SelfViewModeOn()
		{
			SendText("zConfiguration Video hide_conf_self_video: off");
		}

		public void SelfViewModeOff()
		{
			SendText("zConfiguration Video hide_conf_self_video: on");
		}

		public void SelfViewModeToggle()
		{
			if (SelfviewIsOnFeedback.BoolValue)
			{
				SelfViewModeOff();
			}
			else
			{
				SelfViewModeOn();
			}
		}

		#endregion

		#region IHasDirectoryHistoryStack Members

		public event EventHandler<DirectoryEventArgs> DirectoryResultReturned;
		public CodecDirectory DirectoryRoot { get; private set; }

		public CodecDirectory CurrentDirectoryResult
		{
			get { return _currentDirectoryResult; }
		}

		public CodecPhonebookSyncState PhonebookSyncState { get; private set; }

		public void SearchDirectory(string searchString)
		{
			var directoryResults = new CodecDirectory();

			directoryResults.AddContactsToDirectory(
				DirectoryRoot.CurrentDirectoryResults.FindAll(
					c => c.Name.IndexOf(searchString, 0, StringComparison.OrdinalIgnoreCase) > -1));

			DirectoryBrowseHistoryStack.Clear();
			_currentDirectoryResult = directoryResults;

			OnDirectoryResultReturned(directoryResults);
		}

		public void GetDirectoryFolderContents(string folderId)
		{
			var directoryResults = new CodecDirectory { ResultsFolderId = folderId };

			directoryResults.AddContactsToDirectory(
				DirectoryRoot.CurrentDirectoryResults.FindAll(c => c.ParentFolderId.Equals(folderId)));

			DirectoryBrowseHistoryStack.Push(_currentDirectoryResult);

			_currentDirectoryResult = directoryResults;

			OnDirectoryResultReturned(directoryResults);
		}

		public void SetCurrentDirectoryToRoot()
		{
			DirectoryBrowseHistoryStack.Clear();

			_currentDirectoryResult = DirectoryRoot;

			OnDirectoryResultReturned(DirectoryRoot);
		}

		public void GetDirectoryParentFolderContents()
		{
			if (DirectoryBrowseHistoryStack.Count == 0)
			{
				return;
			}

			var currentDirectory = DirectoryBrowseHistoryStack.Pop();

			_currentDirectoryResult = currentDirectory;

			OnDirectoryResultReturned(currentDirectory);
		}

		public BoolFeedback CurrentDirectoryResultIsNotDirectoryRoot { get; private set; }

		public List<CodecDirectory> DirectoryBrowseHistory { get; private set; }

		public Stack<CodecDirectory> DirectoryBrowseHistoryStack { get; private set; }

		#endregion

		#region IHasScheduleAwareness Members

		public CodecScheduleAwareness CodecSchedule { get; private set; }

		public void GetSchedule()
		{
			GetBookings();
		}

		#endregion

		#region IRouting Members

		public void ExecuteSwitch(object inputSelector, object outputSelector, eRoutingSignalType signalType)
		{
			ExecuteSwitch(inputSelector);
		}

		#endregion

		private void SyncState_InitialSyncCompleted(object sender, EventArgs e)
		{
			SetUpRouting();

			SetIsReady();
		}

		private void SetUpCallFeedbackActions()
		{
			Status.Call.Sharing.PropertyChanged += (o, a) =>
			{
				if (a.PropertyName == "State")
				{
					SharingContentIsOnFeedback.FireUpdate();
					ReceivingContent.FireUpdate();
				}
			};

			Status.Call.PropertyChanged += (o, a) =>
			{
				if (a.PropertyName == "Info")
				{
					Debug.Console(1, this, "Updating Call Status");
					UpdateCallStatus();
				}
			};
		}

		/// <summary>
		/// Subscribes to the PropertyChanged events on the state objects and fires the corresponding feedbacks.
		/// </summary>
		private void SetUpFeedbackActions()
		{
			Configuration.Audio.Output.PropertyChanged += (o, a) =>
			{
				if (a.PropertyName == "Volume")
				{
					VolumeLevelFeedback.FireUpdate();
					MuteFeedback.FireUpdate();
				}
			};

			Configuration.Call.Microphone.PropertyChanged += (o, a) =>
			{
				if (a.PropertyName == "Mute")
				{
					PrivacyModeIsOnFeedback.FireUpdate();
				}
			};

			Configuration.Video.PropertyChanged += (o, a) =>
			{
				if (a.PropertyName == "HideConfSelfVideo")
				{
					SelfviewIsOnFeedback.FireUpdate();
				}
			};
			Configuration.Video.Camera.PropertyChanged += (o, a) =>
			{
				if (a.PropertyName == "SelectedId")
				{
					SelectCamera(Configuration.Video.Camera.SelectedId);
					// this will in turn fire the affected feedbacks
				}
			};

			Configuration.Call.Camera.PropertyChanged += (o, a) =>
			{
				Debug.Console(1, this, "Configuration.Call.Camera.PropertyChanged: {0}", a.PropertyName);

				if (a.PropertyName != "Mute") return;

				CameraIsOffFeedback.FireUpdate();
				CameraAutoModeIsOnFeedback.FireUpdate();
			};

			Configuration.Call.Layout.PropertyChanged += (o, a) =>
			{
				switch (a.PropertyName)
				{
					case "Position":
						{
							ComputeSelfviewPipPositionStatus();

							SelfviewPipPositionFeedback.FireUpdate();

							break;
						}
					case "ShareThumb":
						{
							ContentSwappedWithThumbnailFeedback.FireUpdate();
							break;
						}
					case "Style":
						{
							LocalLayoutFeedback.FireUpdate();
							break;
						}
					case "Size":
					{
						// TODO: #714 [ ] SetupFeedbackActions >> Size
						ComputeSelfviewPipSizeStatus();

						SelfviewPipSizeFeedback.FireUpdate();

						break;
					}

				}
			};

            // This is to deal with incorrect object structure coming back from the Zoom Room on v 5.6.3
            Configuration.Client.Call.Layout.PropertyChanged += (o,a) =>
            {
                switch (a.PropertyName)
                {
                    case "Position":
                        {
                            ComputeSelfviewPipPositionStatus();

                            SelfviewPipPositionFeedback.FireUpdate();

                            break;
                        }
                    case "ShareThumb":
                        {
                            ContentSwappedWithThumbnailFeedback.FireUpdate();
                            break;
                        }
                    case "Style":
                        {
                            LocalLayoutFeedback.FireUpdate();
                            break;
                        }

                }
            };

			Status.Call.Sharing.PropertyChanged += (o, a) =>
			{
				if (a.PropertyName == "State")
				{
					SharingContentIsOnFeedback.FireUpdate();
					ReceivingContent.FireUpdate();
				}
			};

			Status.Call.PropertyChanged += (o, a) =>
			{
				if (a.PropertyName == "Info")
				{
					Debug.Console(1, this, "Updating Call Status");
					UpdateCallStatus();
				}
			};

			Status.Sharing.PropertyChanged += (o, a) =>
			{
				switch (a.PropertyName)
				{
					case "dispState":
						SharingSourceFeedback.FireUpdate();
						break;
					case "password":
						break;
				}
			};

			Status.PhoneCall.PropertyChanged += (o, a) =>
			{
				switch (a.PropertyName)
				{
					case "IsIncomingCall":
						Debug.Console(1, this, "Incoming Phone Call: {0}", Status.PhoneCall.IsIncomingCall);
						break;
					case "PeerDisplayName":
						Debug.Console(1, this, "Peer Display Name: {0}", Status.PhoneCall.PeerDisplayName);
						CallerIdNameFeedback.FireUpdate();
						break;
					case "PeerNumber":
						Debug.Console(1, this, "Peer Number: {0}", Status.PhoneCall.PeerNumber);
						CallerIdNumberFeedback.FireUpdate();
						break;
					case "OffHook":
						Debug.Console(1, this, "Phone is OffHook: {0}", Status.PhoneCall.OffHook);
						PhoneOffHookFeedback.FireUpdate();
						break;
				}
			};

			Status.Layout.PropertyChanged += (o, a) =>
			{
				Debug.Console(1, this, "Status.Layout.PropertyChanged a.PropertyName: {0}", a.PropertyName);
				switch (a.PropertyName.ToLower())
				{
					case "can_switch_speaker_view":
					case "can_switch_wall_view":
					case "can_switch_share_on_all_screens":
						{
							ComputeAvailableLayouts();
							break;
						}
					case "is_in_first_page":
						{
							LayoutViewIsOnFirstPageFeedback.FireUpdate();
							break;
						}
					case "is_in_last_page":
						{
							LayoutViewIsOnLastPageFeedback.FireUpdate();
							break;
						}
					//case "video_type":
					//    {
					//        It appears as though the actual value we want to watch is Configuration.Call.Layout.Style
					//        LocalLayoutFeedback.FireUpdate();
					//        break;
					//    }
				}
			};

			Status.NumberOfScreens.PropertyChanged += (o, a) =>
				{
					switch (a.PropertyName)
					{
						case "NumberOfScreens":
							{
								NumberOfScreensFeedback.FireUpdate();
								break;
							}
					}
				};
		}

		private void SetUpDirectory()
		{
			DirectoryRoot = new CodecDirectory();

			DirectoryBrowseHistory = new List<CodecDirectory>();
			DirectoryBrowseHistoryStack = new Stack<CodecDirectory>();

			CurrentDirectoryResultIsNotDirectoryRoot = new BoolFeedback(() => _currentDirectoryResult != DirectoryRoot);

			CurrentDirectoryResultIsNotDirectoryRoot.FireUpdate();
		}

		private void SetUpRouting()
		{
			// Set up input ports
			CreateOsdSource();
			InputPorts.Add(CodecOsdIn);

			// Set up output ports
			OutputPorts.Add(Output1);
		}

		/// <summary>
		/// Creates the fake OSD source, and connects it's AudioVideo output to the CodecOsdIn input
		/// to enable routing 
		/// </summary>
		private void CreateOsdSource()
		{
			OsdSource = new DummyRoutingInputsDevice(Key + "[osd]");
			DeviceManager.AddDevice(OsdSource);
			var tl = new TieLine(OsdSource.AudioVideoOutputPort, CodecOsdIn);
			TieLineCollection.Default.Add(tl);

			//foreach(var input in Status.Video.
		}

		/// <summary>
		/// Starts the HTTP feedback server and syncronizes state of codec
		/// </summary>
		/// <returns></returns>
		public override bool CustomActivate()
		{
			CrestronConsole.AddNewConsoleCommand(SetCommDebug, "SetCodecCommDebug", "0 for Off, 1 for on",
				ConsoleAccessLevelEnum.AccessOperator);
			if (!_props.DisablePhonebookAutoDownload)
			{
				CrestronConsole.AddNewConsoleCommand(s => SendText("zCommand Phonebook List Offset: 0 Limit: 512"),
					"GetZoomRoomContacts", "Triggers a refresh of the codec phonebook",
					ConsoleAccessLevelEnum.AccessOperator);
			}

			CrestronConsole.AddNewConsoleCommand(s => GetBookings(), "GetZoomRoomBookings",
				"Triggers a refresh of the booking data for today", ConsoleAccessLevelEnum.AccessOperator);

			var socket = Communication as ISocketStatus;
			if (socket != null)
			{
				socket.ConnectionChange += socket_ConnectionChange;
			}

			CommDebuggingIsOn = false;

			Communication.Connect();

			CommunicationMonitor.Start();

			return base.CustomActivate();
		}

		public void SetCommDebug(string s)
		{
			if (s == "1")
			{
				CommDebuggingIsOn = true;
				Debug.Console(0, this, "Comm Debug Enabled.");
			}
			else
			{
				CommDebuggingIsOn = false;
				Debug.Console(0, this, "Comm Debug Disabled.");
			}
		}

		private void socket_ConnectionChange(object sender, GenericSocketStatusChageEventArgs e)
		{
			Debug.Console(1, this, "Socket status change {0}", e.Client.ClientStatus);
			if (e.Client.IsConnected)
			{
			}
			else
			{
				_syncState.CodecDisconnected();
				PhonebookSyncState.CodecDisconnected();
			}
		}

		public void SendText(string command)
		{
			if (CommDebuggingIsOn)
			{
				Debug.Console(1, this, "Sending: '{0}'", command);
			}

			Communication.SendText(command + Delimiter);
		}

		/// <summary>
		/// Gathers responses and enqueues them.
		/// </summary>
		/// <param name="dev"></param>
		/// <param name="args"></param>
		private void Port_LineReceived(object dev, GenericCommMethodReceiveTextArgs args)
		{
			//if (CommDebuggingIsOn)
			//    Debug.Console(1, this, "Gathered: '{0}'", args.Text);

			_receiveQueue.Enqueue(args.Text);

			// If the receive thread has for some reason stopped, this will restart it
			if (_receiveThread.ThreadState != Thread.eThreadStates.ThreadRunning)
			{
				_receiveThread.Start();
			}
		}


		/// <summary>
		/// Runs in it's own thread to dequeue messages in the order they were received to be processed
		/// </summary>
		/// <returns></returns>
		private object ProcessQueue()
		{
			try
			{
				while (true)
				{
					var message = _receiveQueue.Dequeue();

					ProcessMessage(message);
				}
			}
			catch (Exception e)
			{
				Debug.Console(1, this, "Error Processing Queue: {0}", e);
			}

			return null;
		}


		/// <summary>
		/// Queues the initial queries to be sent upon connection
		/// </summary>
		private void SetUpSyncQueries()
		{
			// zStatus
			_syncState.AddQueryToQueue("zStatus Call Status");
			_syncState.AddQueryToQueue("zStatus Audio Input Line");
			_syncState.AddQueryToQueue("zStatus Audio Output Line");
			_syncState.AddQueryToQueue("zStatus Video Camera Line");
			_syncState.AddQueryToQueue("zStatus Video Optimizable");
			_syncState.AddQueryToQueue("zStatus Capabilities");
			_syncState.AddQueryToQueue("zStatus Sharing");
			_syncState.AddQueryToQueue("zStatus CameraShare");
			_syncState.AddQueryToQueue("zStatus Call Layout");
			_syncState.AddQueryToQueue("zStatus Call ClosedCaption Available");
			_syncState.AddQueryToQueue("zStatus NumberOfScreens");

			// zConfiguration

			_syncState.AddQueryToQueue("zConfiguration Call Sharing optimize_video_sharing");
			_syncState.AddQueryToQueue("zConfiguration Call Microphone Mute");
			_syncState.AddQueryToQueue("zConfiguration Call Camera Mute");
			_syncState.AddQueryToQueue("zConfiguration Audio Input SelectedId");
			_syncState.AddQueryToQueue("zConfiguration Audio Input is_sap_disabled");
			_syncState.AddQueryToQueue("zConfiguration Audio Input reduce_reverb");
			_syncState.AddQueryToQueue("zConfiguration Audio Input volume");
			_syncState.AddQueryToQueue("zConfiguration Audio Output selectedId");
			_syncState.AddQueryToQueue("zConfiguration Audio Output volume");
			_syncState.AddQueryToQueue("zConfiguration Video hide_conf_self_video");
			_syncState.AddQueryToQueue("zConfiguration Video Camera selectedId");
			_syncState.AddQueryToQueue("zConfiguration Video Camera Mirror");
			_syncState.AddQueryToQueue("zConfiguration Client appVersion");
			_syncState.AddQueryToQueue("zConfiguration Client deviceSystem");
			_syncState.AddQueryToQueue("zConfiguration Call Layout ShareThumb");
			_syncState.AddQueryToQueue("zConfiguration Call Layout Style");
			_syncState.AddQueryToQueue("zConfiguration Call Layout Size");
			_syncState.AddQueryToQueue("zConfiguration Call Layout Position");
			_syncState.AddQueryToQueue("zConfiguration Call Lock Enable");
			_syncState.AddQueryToQueue("zConfiguration Call MuteUserOnEntry Enable");
			_syncState.AddQueryToQueue("zConfiguration Call ClosedCaption FontSize ");
			_syncState.AddQueryToQueue("zConfiguration Call ClosedCaption Visible");

			// zCommand

			if (!_props.DisablePhonebookAutoDownload)
			{
				_syncState.AddQueryToQueue("zCommand Phonebook List Offset: 0 Limit: 512");
			}

			_syncState.AddQueryToQueue("zCommand Bookings List");
			_syncState.AddQueryToQueue("zCommand Call ListParticipants");
			_syncState.AddQueryToQueue("zCommand Call Info");


			_syncState.StartSync();
		}

		/// <summary>
		/// Processes messages as they are dequeued
		/// </summary>
		/// <param name="message"></param>
		private void ProcessMessage(string message)
		{
			// Counts the curly braces
			if (message.Contains("client_loop: send disconnect: Broken pipe"))
			{
				Debug.Console(0, this, Debug.ErrorLogLevel.Error,
					"Zoom Room Controller or App connected. Essentials will NOT control the Zoom Room until it is disconnected.");

				return;
			}

			if (message.Contains('{'))
			{
				_jsonCurlyBraceCounter++;
			}

			if (message.Contains('}'))
			{
				_jsonCurlyBraceCounter--;
			}

			//Debug.Console(2, this, "JSON Curly Brace Count: {0}", _jsonCurlyBraceCounter);

			if (!_jsonFeedbackMessageIsIncoming && message.Trim('\x20') == "{" + Delimiter)
			// Check for the beginning of a new JSON message
			{
				_jsonFeedbackMessageIsIncoming = true;
				_jsonCurlyBraceCounter = 1; // reset the counter for each new message

				_jsonMessage = new StringBuilder();

				_jsonMessage.Append(message);

				if (CommDebuggingIsOn)
				{
					Debug.Console(2, this, "Incoming JSON message...");
				}

				return;
			}
			if (_jsonFeedbackMessageIsIncoming && message.Trim('\x20') == "}" + Delimiter)
			// Check for the end of a JSON message
			{
				_jsonMessage.Append(message);

				if (_jsonCurlyBraceCounter == 0)
				{
					_jsonFeedbackMessageIsIncoming = false;

					if (CommDebuggingIsOn)
					{
						Debug.Console(2, this, "Complete JSON Received:\n{0}", _jsonMessage.ToString());
					}

					// Forward the complete message to be deserialized
					DeserializeResponse(_jsonMessage.ToString());
				}

				//JsonMessage = new StringBuilder();
				return;
			}

			// NOTE: This must happen after the above conditions have been checked
			// Append subsequent partial JSON fragments to the string builder
			if (_jsonFeedbackMessageIsIncoming)
			{
				_jsonMessage.Append(message);

				//Debug.Console(1, this, "Building JSON:\n{0}", JsonMessage.ToString());
				return;
			}

			if (CommDebuggingIsOn)
			{
				Debug.Console(1, this, "Non-JSON response: '{0}'", message);
			}

			_jsonCurlyBraceCounter = 0; // reset on non-JSON response

			if (!_syncState.InitialSyncComplete)
			{
				switch (message.Trim().ToLower()) // remove the whitespace
				{
					case "*r login successful":
						{
							_syncState.LoginMessageReceived();

							// Fire up a thread to send the intial commands.
							CrestronInvoke.BeginInvoke(o =>
							{
								Thread.Sleep(100);
								// disable echo of commands
								SendText("echo off");
								Thread.Sleep(100);
								// set feedback exclusions
								SendText("zFeedback Register Op: ex Path: /Event/InfoResult/info/callin_country_list");
								Thread.Sleep(100);
								SendText("zFeedback Register Op: ex Path: /Event/InfoResult/info/callout_country_list");
								Thread.Sleep(100);

								if (!_props.DisablePhonebookAutoDownload)
								{
									SendText("zFeedback Register Op: ex Path: /Event/Phonebook/AddedContact");
								}
								// switch to json format
								SendText("format json");
							});

							break;
						}
				}
			}
		}

		/// <summary>
		/// Deserializes a JSON formatted response
		/// </summary>
		/// <param name="response"></param>
		private void DeserializeResponse(string response)
		{
			try
			{
				var trimmedResponse = response.Trim();

				if (trimmedResponse.Length <= 0)
				{
					return;
				}

				var message = JObject.Parse(trimmedResponse);

				var eType =
					(eZoomRoomResponseType)
						Enum.Parse(typeof(eZoomRoomResponseType), message["type"].Value<string>(), true);

				var topKey = message["topKey"].Value<string>();

				var responseObj = message[topKey];

				Debug.Console(1, "{0} Response Received. topKey: '{1}'\n{2}", eType, topKey, responseObj.ToString());

				switch (eType)
				{
					case eZoomRoomResponseType.zConfiguration:
						{
							switch (topKey.ToLower())
							{
								case "call":
									{
										JsonConvert.PopulateObject(responseObj.ToString(), Configuration.Call);

										break;
									}
								case "audio":
									{
										JsonConvert.PopulateObject(responseObj.ToString(), Configuration.Audio);

										break;
									}
								case "video":
									{
										JsonConvert.PopulateObject(responseObj.ToString(), Configuration.Video);

										break;
									}
								case "client":
									{
										JsonConvert.PopulateObject(responseObj.ToString(), Configuration.Client);

										break;
									}
								default:
									{
										break;
									}
							}
							break;
						}
					case eZoomRoomResponseType.zCommand:
						{
							switch (topKey.ToLower())
							{
								case "inforesult":
									{
										JsonConvert.PopulateObject(responseObj.ToString(), Status.Call.Info);
										break;
									}
								case "phonebooklistresult":
									{
										JsonConvert.PopulateObject(responseObj.ToString(), Status.Phonebook);

										if (!PhonebookSyncState.InitialSyncComplete)
										{
											PhonebookSyncState.InitialPhonebookFoldersReceived();
											PhonebookSyncState.PhonebookRootEntriesReceived();
											PhonebookSyncState.SetPhonebookHasFolders(false);
											PhonebookSyncState.SetNumberOfContacts(Status.Phonebook.Contacts.Count);
										}

										var directoryResults =
											zStatus.Phonebook.ConvertZoomContactsToGeneric(Status.Phonebook.Contacts);

										DirectoryRoot = directoryResults;

										_currentDirectoryResult = DirectoryRoot;

										OnDirectoryResultReturned(directoryResults);

										break;
									}
								case "listparticipantsresult":
									{
										Debug.Console(1, this, "JTokenType: {0}", responseObj.Type);

										switch (responseObj.Type)
										{
											case JTokenType.Array:
												Status.Call.Participants =
													JsonConvert.DeserializeObject<List<zCommand.ListParticipant>>(
														responseObj.ToString());
												break;
											case JTokenType.Object:
												{
													// this is a single participant event notification

													var participant =
														JsonConvert.DeserializeObject<zCommand.ListParticipant>(
															responseObj.ToString());

													if (participant != null)
													{
														switch (participant.Event)
														{
															case "ZRCUserChangedEventUserInfoUpdated":
															case "ZRCUserChangedEventLeftMeeting":
																{
																	var existingParticipant =
																		Status.Call.Participants.FirstOrDefault(
																			p => p.UserId.Equals(participant.UserId));

																	if (existingParticipant != null)
																	{
																		switch (participant.Event)
																		{
																			case "ZRCUserChangedEventLeftMeeting":
																				Status.Call.Participants.Remove(existingParticipant);
																				break;
																			case "ZRCUserChangedEventUserInfoUpdated":
																				JsonConvert.PopulateObject(responseObj.ToString(),
																					existingParticipant);
																				break;
																		}
																	}
																}
																break;
															case "ZRCUserChangedEventJoinedMeeting":
																Status.Call.Participants.Add(participant);
																break;
														}
													}
												}
												break;
										}

										var participants =
											zCommand.ListParticipant.GetGenericParticipantListFromParticipantsResult(
												Status.Call.Participants);

										Participants.CurrentParticipants = participants;

										PrintCurrentCallParticipants();

										break;
									}
								default:
									{
										break;
									}
							}
							break;
						}
					case eZoomRoomResponseType.zEvent:
						{
							switch (topKey.ToLower())
							{
								case "phonebook":
									{
										if (responseObj["Updated Contact"] != null)
										{
											var updatedContact =
												JsonConvert.DeserializeObject<zStatus.Contact>(
													responseObj["Updated Contact"].ToString());

											var existingContact =
												Status.Phonebook.Contacts.FirstOrDefault(c => c.Jid.Equals(updatedContact.Jid));

											if (existingContact != null)
											{
												// Update existing contact
												JsonConvert.PopulateObject(responseObj["Updated Contact"].ToString(),
													existingContact);
											}
										}
										else if (responseObj["Added Contact"] != null)
										{
											var jToken = responseObj["Updated Contact"];
											if (jToken != null)
											{
												var newContact =
													JsonConvert.DeserializeObject<zStatus.Contact>(
														jToken.ToString());

												// Add a new contact
												Status.Phonebook.Contacts.Add(newContact);
											}
										}

										break;
									}
								case "bookingslistresult":
									{
										if (!_syncState.InitialSyncComplete)
										{
											_syncState.LastQueryResponseReceived();
										}

										var codecBookings = JsonConvert.DeserializeObject<List<zCommand.BookingsListResult>>(
											responseObj.ToString());

										if (codecBookings != null && codecBookings.Count > 0)
										{
											CodecSchedule.Meetings = zCommand.GetGenericMeetingsFromBookingResult(
												codecBookings, CodecSchedule.MeetingWarningMinutes);
										}

										break;
									}
								case "bookings updated":
									{
										GetBookings();

										break;
									}
								case "sharingstate":
									{
										JsonConvert.PopulateObject(responseObj.ToString(), Status.Call.Sharing);

										SetLayout();

										break;
									}
								case "incomingcallindication":
									{
										var incomingCall =
											JsonConvert.DeserializeObject<zEvent.IncomingCallIndication>(responseObj.ToString());

										if (incomingCall != null)
										{
											var newCall = new CodecActiveCallItem
											{
												Direction = eCodecCallDirection.Incoming,
												Status = eCodecCallStatus.Ringing,
												Type = eCodecCallType.Unknown,
												Name = incomingCall.callerName,
												Id = incomingCall.callerJID
											};

											ActiveCalls.Add(newCall);

											OnCallStatusChange(newCall);
										}

										break;
									}
								case "treatedincomingcallindication":
									{
										var incomingCall =
											JsonConvert.DeserializeObject<zEvent.IncomingCallIndication>(responseObj.ToString());

										if (incomingCall != null)
										{
											var existingCall =
												ActiveCalls.FirstOrDefault(c => c.Id.Equals(incomingCall.callerJID));

											if (existingCall != null)
											{
												existingCall.Status = !incomingCall.accepted
													? eCodecCallStatus.Disconnected
													: eCodecCallStatus.Connecting;

												OnCallStatusChange(existingCall);
											}

											UpdateCallStatus();
										}

										break;
									}
								case "calldisconnect":
									{
										var disconnectEvent =
											JsonConvert.DeserializeObject<zEvent.CallDisconnect>(responseObj.ToString());

										if (disconnectEvent.Successful)
										{
											if (ActiveCalls.Count > 0)
											{
												var activeCall = ActiveCalls.FirstOrDefault(c => c.IsActiveCall);

												if (activeCall != null)
												{
													activeCall.Status = eCodecCallStatus.Disconnected;

													OnCallStatusChange(activeCall);
												}
											}
											var emptyList = new List<Participant>();
											Participants.CurrentParticipants = emptyList;
										}

										UpdateCallStatus();
										break;
									}
								case "callconnecterror":
									{
										UpdateCallStatus();
										break;
									}
								case "videounmuterequest":
									{
										// TODO: notify room of a request to unmute video
										break;
									}
								case "meetingneedspassword":
									{
										// TODO: notify user to enter a password
										break;
									}
								case "needwaitforhost":
									{
										var needWait =
											JsonConvert.DeserializeObject<zEvent.NeedWaitForHost>(responseObj.ToString());

										if (needWait.Wait)
										{
											// TODO: notify user to wait for host
										}

										break;
									}
								case "openvideofailforhoststop":
									{
										// TODO: notify user that host has disabled unmuting video
										break;
									}
								case "updatedcallrecordinfo":
									{
										JsonConvert.PopulateObject(responseObj.ToString(), Status.Call.CallRecordInfo);

										break;
									}
								case "phonecallstatus":
									{
										JsonConvert.PopulateObject(responseObj.ToString(), Status.PhoneCall);
										break;
									}
								case "pinstatusofscreennotification":
									{
										var status = responseObj.ToObject<zEvent.PinStatusOfScreenNotification>();

                                        Debug.Console(1, this, "Pin Status notification for UserId: {0}, ScreenIndex: {1}", status.PinnedUserId, status.ScreenIndex);

                                        Participant alreadyPinnedParticipant = null;

                                        // Check for a participant already pinned to the same screen index.
                                        if (status.PinnedUserId > 0)
                                        {
                                            alreadyPinnedParticipant = Participants.CurrentParticipants.FirstOrDefault(p => p.ScreenIndexIsPinnedToFb.Equals(status.ScreenIndex));

                                            // Make sure that the already pinned participant isn't the same ID as for this message.  If true, clear the pinned fb.
                                            if (alreadyPinnedParticipant != null && alreadyPinnedParticipant.UserId != status.PinnedUserId)
                                            {
                                                Debug.Console(1, this, "Participant: {0} with id: {1} already pinned to screenIndex {2}.  Clearing pinned fb.",
                                                    alreadyPinnedParticipant.Name, alreadyPinnedParticipant.UserId, alreadyPinnedParticipant.ScreenIndexIsPinnedToFb);
                                                alreadyPinnedParticipant.IsPinnedFb = false;
                                                alreadyPinnedParticipant.ScreenIndexIsPinnedToFb = -1;
                                            }
                                        }

										var participant = Participants.CurrentParticipants.FirstOrDefault(p => p.UserId.Equals(status.PinnedUserId));

										if (participant != null)
										{
											participant.IsPinnedFb = true;
											participant.ScreenIndexIsPinnedToFb = status.ScreenIndex;
										}
										else
										{
											participant = Participants.CurrentParticipants.FirstOrDefault(p => p.ScreenIndexIsPinnedToFb.Equals(status.ScreenIndex));

											if (participant == null && alreadyPinnedParticipant == null)
											{
												Debug.Console(1, this, "no matching participant found by pinned_user_id: {0} or screen_index: {1}", status.PinnedUserId, status.ScreenIndex);
												return;
											}
											else if (participant != null)
											{
                                                Debug.Console(2, this, "Unpinning {0} with id: {1} from screen index: {2}", participant.Name, participant.UserId, status.ScreenIndex);
												participant.IsPinnedFb = false;
												participant.ScreenIndexIsPinnedToFb = -1;
											}
										}

										// fire the event as we've modified the participants list
										Participants.OnParticipantsChanged();

										break;
									}
								default:
									{
										break;
									}
							}
							break;
						}
					case eZoomRoomResponseType.zStatus:
						{
							switch (topKey.ToLower())
							{
								case "login":
									{
										_syncState.LoginMessageReceived();

										if (!_syncState.InitialQueryMessagesWereSent)
										{
											SetUpSyncQueries();
										}

										JsonConvert.PopulateObject(responseObj.ToString(), Status.Login);

										break;
									}
								case "systemunit":
									{
										JsonConvert.PopulateObject(responseObj.ToString(), Status.SystemUnit);

										break;
									}
								case "call":
									{
										JsonConvert.PopulateObject(responseObj.ToString(), Status.Call);

										UpdateCallStatus();

										break;
									}
								case "capabilities":
									{
										JsonConvert.PopulateObject(responseObj.ToString(), Status.Capabilities);
										break;
									}
								case "sharing":
									{
										JsonConvert.PopulateObject(responseObj.ToString(), Status.Sharing);

										break;
									}
								case "numberofscreens":
									{
										JsonConvert.PopulateObject(responseObj.ToString(), Status.NumberOfScreens);
										break;
									}
								case "video":
									{
										JsonConvert.PopulateObject(responseObj.ToString(), Status.Video);
										break;
									}
								case "camerashare":
									{
										JsonConvert.PopulateObject(responseObj.ToString(), Status.CameraShare);
										break;
									}
								case "layout":
									{
										JsonConvert.PopulateObject(responseObj.ToString(), Status.Layout);
										break;
									}
								case "audio input line":
									{
										JsonConvert.PopulateObject(responseObj.ToString(), Status.AudioInputs);
										break;
									}
								case "audio output line":
									{
										JsonConvert.PopulateObject(responseObj.ToString(), Status.AudioOuputs);
										break;
									}
								case "video camera line":
									{
										JsonConvert.PopulateObject(responseObj.ToString(), Status.Cameras);

										if (!_syncState.CamerasHaveBeenSetUp)
										{
											SetUpCameras();
										}

										break;
									}
								default:
									{
										break;
									}
							}

							break;
						}
					default:
						{
							Debug.Console(1, "Unknown Response Type:");
							break;
						}
				}
			}
			catch (Exception ex)
			{
				Debug.Console(1, this, "Error Deserializing feedback: {0}", ex);
			}
		}

		private void SetLayout()
		{
			if (!_props.AutoDefaultLayouts) return;

			if (
				(Status.Call.Sharing.State == zEvent.eSharingState.Receiving ||
				 Status.Call.Sharing.State == zEvent.eSharingState.Sending))
			{
				SendText(String.Format("zconfiguration call layout style: {0}",
					_props.DefaultSharingLayout));
			}
			else
			{
				SendText(String.Format("zconfiguration call layout style: {0}",
					_props.DefaultCallLayout));
			}
		}

		public void PrintCurrentCallParticipants()
		{
			if (Debug.Level <= 0)
			{
				return;
			}

			Debug.Console(1, this, "*************************** Call Participants **************************");
			foreach (var participant in Participants.CurrentParticipants)
			{
				Debug.Console(1, this, "Name: {0} Audio: {1} IsHost: {2}", participant.Name,
					participant.AudioMuteFb, participant.IsHost);
			}
			Debug.Console(1, this, "************************************************************************");
		}

		/// <summary>
		/// Retrieves bookings list
		/// </summary>
		private void GetBookings()
		{
			SendText("zCommand Bookings List");
		}


		/// <summary>
		/// Updates the current call status
		/// </summary>
		private void UpdateCallStatus()
		{
			Debug.Console(1, this, "[UpdateCallStatus] Current Call Status: {0}",
				Status.Call != null ? Status.Call.Sharing.State.ToString() : "no call");

			if (Status.Call != null)
			{
				var callStatus = Status.Call.Status;

				// If not currently in a meeting, intialize the call object
				if (callStatus != zStatus.eCallStatus.IN_MEETING && callStatus != zStatus.eCallStatus.CONNECTING_MEETING)
				{
					Debug.Console(1, this, "Creating new Status.Call object");
					Status.Call = new zStatus.Call { Status = callStatus };

					SetUpCallFeedbackActions();
				}

				if (ActiveCalls.Count == 0)
				{
					if (callStatus == zStatus.eCallStatus.CONNECTING_MEETING ||
						callStatus == zStatus.eCallStatus.IN_MEETING)
					{
						var newStatus = eCodecCallStatus.Unknown;

						switch (callStatus)
						{
							case zStatus.eCallStatus.CONNECTING_MEETING:
								newStatus = eCodecCallStatus.Connecting;
								break;
							case zStatus.eCallStatus.IN_MEETING:
								newStatus = eCodecCallStatus.Connected;
								break;
						}

						var newCall = new CodecActiveCallItem { Status = newStatus };

						ActiveCalls.Add(newCall);

						Debug.Console(1, this, "[UpdateCallStatus] Current Call Status: {0}",
							Status.Call != null ? Status.Call.Sharing.State.ToString() : "no call");

						OnCallStatusChange(newCall);
					}
				}
				else
				{
					var existingCall = ActiveCalls.FirstOrDefault(c => !c.Status.Equals(eCodecCallStatus.Ringing));

					switch (callStatus)
					{
						case zStatus.eCallStatus.IN_MEETING:
							existingCall.Status = eCodecCallStatus.Connected;
							break;
						case zStatus.eCallStatus.NOT_IN_MEETING:
							existingCall.Status = eCodecCallStatus.Disconnected;
							break;
					}

					Debug.Console(1, this, "[UpdateCallStatus] Current Call Status: {0}",
						Status.Call != null ? Status.Call.Sharing.State.ToString() : "no call");

					OnCallStatusChange(existingCall);
				}
			}

			Debug.Console(1, this, "*************************** Active Calls ********************************");

			// Clean up any disconnected calls left in the list
			for (int i = 0; i < ActiveCalls.Count; i++)
			{
				var call = ActiveCalls[i];

				Debug.Console(1, this,
					@"Name: {0}
                    ID: {1}
                    IsActive: {2}
                    Status: {3}
                    Direction: {4}", call.Name, call.Id, call.IsActiveCall, call.Status, call.Direction);

				if (!call.IsActiveCall)
				{
					Debug.Console(1, this, "***** Removing Inactive Call: {0} *****", call.Name);
					ActiveCalls.Remove(call);
				}
			}
			Debug.Console(1, this, "**************************************************************************");

			//clear participants list after call cleanup
			if (ActiveCalls.Count == 0)
			{
				Participants.CurrentParticipants = new List<Participant>();
			}
		}

		protected override void OnCallStatusChange(CodecActiveCallItem item)
		{
			base.OnCallStatusChange(item);

			Debug.Console(1, this, "[OnCallStatusChange] Current Call Status: {0}",
				Status.Call != null ? Status.Call.Sharing.State.ToString() : "no call");

			if (_props.AutoDefaultLayouts)
			{
				SetLayout();
			}
		}

		public override void StartSharing()
		{
			SendText("zCommand Call Sharing HDMI Start");
		}

		/// <summary>
		/// Stops sharing the current presentation
		/// </summary>
		public override void StopSharing()
		{
			SendText("zCommand Call Sharing Disconnect");
		}

		public override void PrivacyModeOn()
		{
			SendText("zConfiguration Call Microphone Mute: on");
		}

		public override void PrivacyModeOff()
		{
			SendText("zConfiguration Call Microphone Mute: off");
		}

		public override void PrivacyModeToggle()
		{
			if (PrivacyModeIsOnFeedback.BoolValue)
			{
				PrivacyModeOff();
			}
			else
			{
				PrivacyModeOn();
			}
		}

		public override void MuteOff()
		{
			SetVolume((ushort)_previousVolumeLevel);
		}

		public override void MuteOn()
		{
			_previousVolumeLevel = Configuration.Audio.Output.Volume; // Store the previous level for recall

			SetVolume(0);
		}

		public override void MuteToggle()
		{
			if (MuteFeedback.BoolValue)
			{
				MuteOff();
			}
			else
			{
				MuteOn();
			}
		}


		/// <summary>
		/// Increments the voluem
		/// </summary>
		/// <param name="pressRelease"></param>
		public override void VolumeUp(bool pressRelease)
		{
			// TODO: Implment volume decrement that calls SetVolume()
		}

		/// <summary>
		/// Decrements the volume
		/// </summary>
		/// <param name="pressRelease"></param>
		public override void VolumeDown(bool pressRelease)
		{
			// TODO: Implment volume decrement that calls SetVolume()
		}

		/// <summary>
		/// Scales the level and sets the codec to the specified level within its range
		/// </summary>
		/// <param name="level">level from slider (0-65535 range)</param>
		public override void SetVolume(ushort level)
		{
			var scaledLevel = CrestronEnvironment.ScaleWithLimits(level, 65535, 0, 100, 0);
			SendText(string.Format("zConfiguration Audio Output volume: {0}", scaledLevel));
		}

		/// <summary>
		/// Recalls the default volume on the codec
		/// </summary>
		public void VolumeSetToDefault()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public override void StandbyActivate()
		{
			// No corresponding function on device
		}

		/// <summary>
		/// 
		/// </summary>
		public override void StandbyDeactivate()
		{
			// No corresponding function on device
		}

		public override void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
		{
			var joinMap = new ZoomRoomJoinMap(joinStart);

			var customJoins = JoinMapHelper.TryGetJoinMapAdvancedForDevice(joinMapKey);

			if (customJoins != null)
			{
				joinMap.SetCustomJoinData(customJoins);
			}

			if (bridge != null)
			{
				bridge.AddJoinMap(Key, joinMap);
			}

			LinkVideoCodecToApi(this, trilist, joinMap);

			LinkZoomRoomToApi(trilist, joinMap);			
		}

		/// <summary>
		/// Links all the specific Zoom functionality to the API bridge
		/// </summary>
		/// <param name="trilist"></param>
		/// <param name="joinMap"></param>
		public void LinkZoomRoomToApi(BasicTriList trilist, ZoomRoomJoinMap joinMap)
		{
			var layoutsCodec = this as IHasZoomRoomLayouts;
			if (layoutsCodec != null)
			{
				layoutsCodec.AvailableLayoutsChanged += (o, a) =>
				{
					trilist.SetBool(joinMap.LayoutGalleryIsAvailable.JoinNumber, zConfiguration.eLayoutStyle.Gallery
						== (a.AvailableLayouts & zConfiguration.eLayoutStyle.Gallery));
					trilist.SetBool(joinMap.LayoutSpeakerIsAvailable.JoinNumber, zConfiguration.eLayoutStyle.Speaker
						== (a.AvailableLayouts & zConfiguration.eLayoutStyle.Speaker));
					trilist.SetBool(joinMap.LayoutStripIsAvailable.JoinNumber, zConfiguration.eLayoutStyle.Strip
						== (a.AvailableLayouts & zConfiguration.eLayoutStyle.Strip));
					trilist.SetBool(joinMap.LayoutShareAllIsAvailable.JoinNumber, zConfiguration.eLayoutStyle.ShareAll
						== (a.AvailableLayouts & zConfiguration.eLayoutStyle.ShareAll));

					// pass the names used to set the layout through the bridge
					trilist.SetString(joinMap.LayoutGalleryIsAvailable.JoinNumber, zConfiguration.eLayoutStyle.Gallery.ToString());
					trilist.SetString(joinMap.LayoutSpeakerIsAvailable.JoinNumber, zConfiguration.eLayoutStyle.Speaker.ToString());
					trilist.SetString(joinMap.LayoutStripIsAvailable.JoinNumber, zConfiguration.eLayoutStyle.Strip.ToString());
					trilist.SetString(joinMap.LayoutShareAllIsAvailable.JoinNumber, zConfiguration.eLayoutStyle.ShareAll.ToString());
				};

				layoutsCodec.CanSwapContentWithThumbnailFeedback.LinkInputSig(trilist.BooleanInput[joinMap.CanSwapContentWithThumbnail.JoinNumber]);
				trilist.SetSigFalseAction(joinMap.SwapContentWithThumbnail.JoinNumber, () => layoutsCodec.SwapContentWithThumbnail());
				layoutsCodec.ContentSwappedWithThumbnailFeedback.LinkInputSig(trilist.BooleanInput[joinMap.SwapContentWithThumbnail.JoinNumber]);

				layoutsCodec.LayoutViewIsOnFirstPageFeedback.LinkInputSig(trilist.BooleanInput[joinMap.LayoutIsOnFirstPage.JoinNumber]);
				layoutsCodec.LayoutViewIsOnLastPageFeedback.LinkInputSig(trilist.BooleanInput[joinMap.LayoutIsOnLastPage.JoinNumber]);
				trilist.SetSigFalseAction(joinMap.LayoutTurnToNextPage.JoinNumber, () => layoutsCodec.LayoutTurnNextPage());
				trilist.SetSigFalseAction(joinMap.LayoutTurnToPreviousPage.JoinNumber, () => layoutsCodec.LayoutTurnPreviousPage());
				trilist.SetSigFalseAction(joinMap.GetAvailableLayouts.JoinNumber, () => layoutsCodec.GetAvailableLayouts());

				trilist.SetStringSigAction(joinMap.GetSetCurrentLayout.JoinNumber, (s) =>
					{
						try
						{
							var style = (zConfiguration.eLayoutStyle)Enum.Parse(typeof(zConfiguration.eLayoutStyle), s, true);
							SetLayout(style);
						}
						catch (Exception e)
						{
							Debug.Console(1, this, "Unable to parse '{0}' to zConfiguration.eLayoutStyle: {1}", s, e);
						}
					});

				layoutsCodec.LocalLayoutFeedback.LinkInputSig(trilist.StringInput[joinMap.GetSetCurrentLayout.JoinNumber]);								
			}

			var pinCodec = this as IHasParticipantPinUnpin;
			if (pinCodec != null)
			{
				pinCodec.NumberOfScreensFeedback.LinkInputSig(trilist.UShortInput[joinMap.NumberOfScreens.JoinNumber]);

				// Set the value of the local property to be used when pinning a participant
				trilist.SetUShortSigAction(joinMap.ScreenIndexToPinUserTo.JoinNumber, (u) => ScreenIndexToPinUserTo = u);
			}

			// TODO: #714 [ ] LinkZoomRoomToApi >> layoutSizeCoodec
			var layoutSizeCodec = this as IHasSelfviewSize;
			if (layoutSizeCodec != null)
			{
				trilist.SetSigFalseAction(joinMap.GetSetSelfviewPipSize.JoinNumber, layoutSizeCodec.SelfviewPipSizeToggle);
				trilist.SetStringSigAction(joinMap.GetSetSelfviewPipSize.JoinNumber, (s) =>
				{
					try
					{
						var size = (zConfiguration.eLayoutSize)Enum.Parse(typeof(zConfiguration.eLayoutSize), s, true);					
						var cmd = SelfviewPipSizes.FirstOrDefault(c => c.Command.Equals(size.ToString()));
						SelfviewPipSizeSet(cmd);
					}
					catch (Exception e)
					{
						Debug.Console(1, this, "Unable to parse '{0}' to zConfiguration.eLayoutSize: {1}", s, e);
					}
				});

				layoutSizeCodec.SelfviewPipSizeFeedback.LinkInputSig(trilist.StringInput[joinMap.GetSetSelfviewPipSize.JoinNumber]);
			}

			trilist.OnlineStatusChange += (device, args) =>
			{
				if (!args.DeviceOnLine) return;

				layoutsCodec.LocalLayoutFeedback.FireUpdate();
				pinCodec.NumberOfScreensFeedback.FireUpdate();
				layoutSizeCodec.SelfviewPipSizeFeedback.FireUpdate();
			};
		}		

		public override void ExecuteSwitch(object selector)
		{
			var action = selector as Action;
			if (action == null)
			{
				return;
			}

			action();
		}

		public void AcceptCall()
		{
			var incomingCall =
				ActiveCalls.FirstOrDefault(
					c => c.Status.Equals(eCodecCallStatus.Ringing) && c.Direction.Equals(eCodecCallDirection.Incoming));

			AcceptCall(incomingCall);
		}

		public override void AcceptCall(CodecActiveCallItem call)
		{
			SendText(string.Format("zCommand Call Accept callerJID: {0}", call.Id));

			call.Status = eCodecCallStatus.Connected;

			OnCallStatusChange(call);

			UpdateCallStatus();
		}

		public void RejectCall()
		{
			var incomingCall =
				ActiveCalls.FirstOrDefault(
					c => c.Status.Equals(eCodecCallStatus.Ringing) && c.Direction.Equals(eCodecCallDirection.Incoming));

			RejectCall(incomingCall);
		}

		public override void RejectCall(CodecActiveCallItem call)
		{
			SendText(string.Format("zCommand Call Reject callerJID: {0}", call.Id));

			call.Status = eCodecCallStatus.Disconnected;

			OnCallStatusChange(call);

			UpdateCallStatus();
		}

		public override void Dial(Meeting meeting)
		{
			Debug.Console(1, this, "Dialing meeting.Id: {0} Title: {1}", meeting.Id, meeting.Title);
			SendText(string.Format("zCommand Dial Start meetingNumber: {0}", meeting.Id));
		}

		public override void Dial(string number)
		{
			SendText(string.Format("zCommand Dial Join meetingNumber: {0}", number));
		}

		/// <summary>
		/// Invites a contact to either a new meeting (if not already in a meeting) or the current meeting.
		/// Currently only invites a single user
		/// </summary>
		/// <param name="contact"></param>
		public override void Dial(IInvitableContact contact)
		{
			var ic = contact as zStatus.ZoomDirectoryContact;

			if (ic != null)
			{
				Debug.Console(1, this, "Attempting to Dial (Invite): {0}", ic.Name);

				if (!IsInCall)
				{
					SendText(string.Format("zCommand Invite Duration: {0} user: {1}", DefaultMeetingDurationMin,
						ic.ContactId));
				}
				else
				{
					SendText(string.Format("zCommand Call invite user: {0}", ic.ContactId));
				}
			}
		}

		public override void EndCall(CodecActiveCallItem call)
		{
			SendText("zCommand Call Disconnect");
		}

		public override void EndAllCalls()
		{
			SendText("zCommand Call Disconnect");
		}

		public override void SendDtmf(string s)
		{
			SendDtmfToPhone(s);
		}

		/// <summary>
		/// Call when directory results are updated
		/// </summary>
		/// <param name="result"></param>
		private void OnDirectoryResultReturned(CodecDirectory result)
		{
			CurrentDirectoryResultIsNotDirectoryRoot.FireUpdate();

			// This will return the latest results to all UIs.  Multiple indendent UI Directory browsing will require a different methodology
			var handler = DirectoryResultReturned;
			if (handler != null)
			{
				handler(this, new DirectoryEventArgs
				{
					Directory = result,
					DirectoryIsOnRoot = !CurrentDirectoryResultIsNotDirectoryRoot.BoolValue
				});
			}

			//PrintDirectory(result);
		}

		/// <summary>
		/// Builds the cameras List by using the Zoom Room zStatus.Cameras data.  Could later be modified to build from config data
		/// </summary>
		private void SetUpCameras()
		{
			SelectedCameraFeedback = new StringFeedback(() => Configuration.Video.Camera.SelectedId);

			ControllingFarEndCameraFeedback = new BoolFeedback(() => SelectedCamera is IAmFarEndCamera);

			foreach (var cam in Status.Cameras)
			{
				var camera = new ZoomRoomCamera(cam.id, cam.Name, this);

				Cameras.Add(camera);

				if (cam.Selected)
				{
					SelectedCamera = camera;
				}
			}

			if (IsInCall)
			{
				UpdateFarEndCameras();
			}

			_syncState.CamerasSetUp();
		}

		/// <summary>
		/// Dynamically creates far end cameras for call participants who have far end control enabled.
		/// </summary>
		private void UpdateFarEndCameras()
		{
			// TODO: set up far end cameras for the current call
		}

		#region Implementation of IHasParticipants

		public CodecParticipants Participants { get; private set; }

		#endregion

		#region IHasParticipantAudioMute Members

		public void MuteAudioForParticipant(int userId)
		{
			SendText(string.Format("zCommand Call MuteParticipant Mute: on Id: {0}", userId));
		}

		public void UnmuteAudioForParticipant(int userId)
		{
			SendText(string.Format("zCommand Call MuteParticipant Mute: off Id: {0}", userId));
		}

		public void ToggleAudioForParticipant(int userId)
		{
			var user = Participants.CurrentParticipants.FirstOrDefault(p => p.UserId.Equals(userId));

			if (user == null)
			{
				Debug.Console(2, this, "Unable to find user with id: {0}", userId);
				return;
			}

			if (user.AudioMuteFb)
			{
				UnmuteAudioForParticipant(userId);
			}
			else
			{
				MuteAudioForParticipant(userId);
			}
		}

		#endregion

		#region IHasParticipantVideoMute Members

		public void MuteVideoForParticipant(int userId)
		{
			SendText(string.Format("zCommand Call MuteParticipantVideo Mute: on Id: {0}", userId));
		}

		public void UnmuteVideoForParticipant(int userId)
		{
			SendText(string.Format("zCommand Call MuteParticipantVideo Mute: off Id: {0}", userId));
		}

		public void ToggleVideoForParticipant(int userId)
		{
			var user = Participants.CurrentParticipants.FirstOrDefault(p => p.UserId.Equals(userId));

			if (user == null)
			{
				Debug.Console(2, this, "Unable to find user with id: {0}", userId);
				return;
			}

			if (user.VideoMuteFb)
			{
				UnmuteVideoForParticipant(userId);
			}
			else
			{
				MuteVideoForParticipant(userId);
			}
		}

		#endregion

		#region IHasParticipantPinUnpin Members

		private Func<int> NumberOfScreensFeedbackFunc { get { return () => Status.NumberOfScreens.NumOfScreens; } }

		public IntFeedback NumberOfScreensFeedback { get; private set; }

		public int ScreenIndexToPinUserTo { get; private set; }

		public void PinParticipant(int userId, int screenIndex)
		{
			SendText(string.Format("zCommand Call Pin Id: {0} Enable: on Screen: {1}", userId, screenIndex));
		}

		public void UnPinParticipant(int userId)
		{
			SendText(string.Format("zCommand Call Pin Id: {0} Enable: off", userId));
		}

		public void ToggleParticipantPinState(int userId, int screenIndex)
		{
			var user = Participants.CurrentParticipants.FirstOrDefault(p => p.UserId.Equals(userId));

			if (user == null)
			{
				Debug.Console(2, this, "Unable to find user with id: {0}", userId);
				return;
			}

			if (user.IsPinnedFb)
			{
				UnPinParticipant(userId);
			}
			else
			{
				PinParticipant(userId, screenIndex);
			}
		}

		#endregion

		#region Implementation of IHasCameraOff

		public BoolFeedback CameraIsOffFeedback { get; private set; }

		public void CameraOff()
		{
			CameraMuteOn();
		}

		#endregion

		public BoolFeedback CameraIsMutedFeedback { get; private set; }

		public void CameraMuteOn()
		{
			SendText("zConfiguration Call Camera Mute: On");
		}

		public void CameraMuteOff()
		{
			SendText("zConfiguration Call Camera Mute: Off");
		}

		public void CameraMuteToggle()
		{
			if (CameraIsMutedFeedback.BoolValue)
				CameraMuteOff();
			else
				CameraMuteOn();
		}

		#region Implementation of IHasCameraAutoMode

		//Zoom doesn't support camera auto modes. Setting this to just unmute video
		public void CameraAutoModeOn()
		{
			CameraMuteOff();
			throw new NotImplementedException("Zoom Room Doesn't support camera auto mode");
		}

		//Zoom doesn't support camera auto modes. Setting this to just unmute video
		public void CameraAutoModeOff()
		{
			SendText("zConfiguration Call Camera Mute: Off");
		}

		public void CameraAutoModeToggle()
		{
			throw new NotImplementedException("Zoom Room doesn't support camera auto mode");
		}

		public BoolFeedback CameraAutoModeIsOnFeedback { get; private set; }

		#endregion

		#region Implementation of IHasFarEndContentStatus

		public BoolFeedback ReceivingContent { get; private set; }

		#endregion

		#region Implementation of IHasSelfviewPosition

		private CodecCommandWithLabel _currentSelfviewPipPosition;

		public StringFeedback SelfviewPipPositionFeedback { get; private set; }

		public void SelfviewPipPositionSet(CodecCommandWithLabel position)
		{
			SendText(String.Format("zConfiguration Call Layout Position: {0}", position.Command));
		}

		public void SelfviewPipPositionToggle()
		{
			if (_currentSelfviewPipPosition != null)
			{
				var nextPipPositionIndex = SelfviewPipPositions.IndexOf(_currentSelfviewPipPosition) + 1;

				if (nextPipPositionIndex >= SelfviewPipPositions.Count)
					// Check if we need to loop back to the first item in the list
					nextPipPositionIndex = 0;

				SelfviewPipPositionSet(SelfviewPipPositions[nextPipPositionIndex]);
			}
		}

		public List<CodecCommandWithLabel> SelfviewPipPositions = new List<CodecCommandWithLabel>()
        {
            new CodecCommandWithLabel("UpLeft", "Center Left"),
            new CodecCommandWithLabel("UpRight", "Center Right"),
            new CodecCommandWithLabel("DownRight", "Lower Right"),
            new CodecCommandWithLabel("DownLeft", "Lower Left")
        };

		private void ComputeSelfviewPipPositionStatus()
		{
			_currentSelfviewPipPosition =
				SelfviewPipPositions.FirstOrDefault(
					p => p.Command.ToLower().Equals(Configuration.Call.Layout.Position.ToString().ToLower()));
		}

		#endregion

		// TODO: #714 [ ] Implementation of IHasSelfviewPipSize
		#region Implementation of IHasSelfviewPipSize

		private CodecCommandWithLabel _currentSelfviewPipSize;

		public StringFeedback SelfviewPipSizeFeedback { get; private set; }

		public void SelfviewPipSizeSet(CodecCommandWithLabel size)
		{
			SendText(String.Format("zConfiguration Call Layout Size: {0}", size.Command));
		}

		public void SelfviewPipSizeToggle()
		{
			if (_currentSelfviewPipSize != null)
			{
				var nextPipSizeIndex = SelfviewPipSizes.IndexOf(_currentSelfviewPipSize) + 1;

				if (nextPipSizeIndex >= SelfviewPipSizes.Count)
					// Check if we need to loop back to the first item in the list
					nextPipSizeIndex = 0;

				SelfviewPipSizeSet(SelfviewPipSizes[nextPipSizeIndex]);
			}
		}

		public List<CodecCommandWithLabel> SelfviewPipSizes = new List<CodecCommandWithLabel>()
        {
            new CodecCommandWithLabel("Off", "Off"),
            new CodecCommandWithLabel("Size1", "Size 1"),
            new CodecCommandWithLabel("Size2", "Size 2"),
            new CodecCommandWithLabel("Size3", "Size 3"),
			new CodecCommandWithLabel("Strip", "Strip")
        };

		private void ComputeSelfviewPipSizeStatus()
		{
			_currentSelfviewPipSize =
				SelfviewPipSizes.FirstOrDefault(
					p => p.Command.ToLower().Equals(Configuration.Call.Layout.Size.ToString().ToLower()));
		}


		#endregion

		#region Implementation of IHasPhoneDialing

		private Func<bool> PhoneOffHookFeedbackFunc { get { return () => Status.PhoneCall.OffHook; } }
		private Func<string> CallerIdNameFeedbackFunc { get { return () => Status.PhoneCall.PeerDisplayName; } }
		private Func<string> CallerIdNumberFeedbackFunc { get { return () => Status.PhoneCall.PeerNumber; } }

		public BoolFeedback PhoneOffHookFeedback { get; private set; }
		public StringFeedback CallerIdNameFeedback { get; private set; }
		public StringFeedback CallerIdNumberFeedback { get; private set; }

		public void DialPhoneCall(string number)
		{
			SendText(String.Format("zCommand Dial PhoneCallOut Number: {0}", number));
		}

		public void EndPhoneCall()
		{
			SendText(String.Format("zCommand Dial PhoneHangUp CallId: {0}", Status.PhoneCall.CallId));
		}

		public void SendDtmfToPhone(string digit)
		{
			SendText(String.Format("zCommand SendSipDTMF CallId: {0} Key: {1}", Status.PhoneCall.CallId, digit));
		}

		#endregion

		#region IHasZoomRoomLayouts Members

		public event EventHandler<LayoutInfoChangedEventArgs> AvailableLayoutsChanged;

		private Func<bool> LayoutViewIsOnFirstPageFeedbackFunc { get { return () => Status.Layout.is_In_First_Page; } }
		private Func<bool> LayoutViewIsOnLastPageFeedbackFunc { get { return () => Status.Layout.is_In_Last_Page; } }
		private Func<bool> CanSwapContentWithThumbnailFeedbackFunc { get { return () => Status.Layout.can_Switch_Floating_Share_Content; } }
		private Func<bool> ContentSwappedWithThumbnailFeedbackFunc { get { return () => Configuration.Call.Layout.ShareThumb; } }

		public BoolFeedback LayoutViewIsOnFirstPageFeedback { get; private set; }

		public BoolFeedback LayoutViewIsOnLastPageFeedback { get; private set; }

		public BoolFeedback CanSwapContentWithThumbnailFeedback { get; private set; }

		public BoolFeedback ContentSwappedWithThumbnailFeedback { get; private set; }


		public zConfiguration.eLayoutStyle LastSelectedLayout { get; private set; }

		public zConfiguration.eLayoutStyle AvailableLayouts { get; private set; }

		/// <summary>
		/// Reads individual properties to determine if which layouts are avalailable
		/// </summary>
		private void ComputeAvailableLayouts()
		{
			Debug.Console(1, this, "Computing available layouts...");
			zConfiguration.eLayoutStyle availableLayouts = zConfiguration.eLayoutStyle.None;
			if (Status.Layout.can_Switch_Wall_View)
			{
				availableLayouts |= zConfiguration.eLayoutStyle.Gallery;
			}

			if (Status.Layout.can_Switch_Speaker_View)
			{
				availableLayouts |= zConfiguration.eLayoutStyle.Speaker;
			}

			if (Status.Layout.can_Switch_Share_On_All_Screens)
			{
				availableLayouts |= zConfiguration.eLayoutStyle.ShareAll;
			}

			// There is no property that directly reports if strip mode is valid, but API stipulates
			// that strip mode is available if the number of screens is 1
			if (Status.NumberOfScreens.NumOfScreens == 1)
			{
				availableLayouts |= zConfiguration.eLayoutStyle.Strip;
			}

			Debug.Console(1, this, "Available layouts: {0}", availableLayouts);

			var handler = AvailableLayoutsChanged;
			if (handler != null)
			{
				handler(this, new LayoutInfoChangedEventArgs() { AvailableLayouts = availableLayouts });
			}

			AvailableLayouts = availableLayouts;
		}

		public void GetAvailableLayouts()
		{
			SendText("zStatus Call Layout");
		}

		public void SetLayout(zConfiguration.eLayoutStyle layoutStyle)
		{
			LastSelectedLayout = layoutStyle;
			SendText(String.Format("zConfiguration Call Layout Style: {0}", layoutStyle.ToString()));
		}

		public void SwapContentWithThumbnail()
		{
			if (CanSwapContentWithThumbnailFeedback.BoolValue)
			{
				var oppositeValue = ContentSwappedWithThumbnailFeedback.BoolValue ? "on" : "off"; // Get the value based on the opposite of the current state
				// TODO: #697 [*] Need to verify the ternary above and make sure that the correct on/off value is being send based on the true/false value of the feedback
				// to toggle the state
				SendText(String.Format("zConfiguration Call Layout ShareThumb: {0}", oppositeValue));
			}
		}

		public void LayoutTurnNextPage()
		{
			SendText("zCommand Call Layout TurnPage Forward: On");
		}

		public void LayoutTurnPreviousPage()
		{
			SendText("zCommand Call Layout TurnPage Forward: Off");
		}

		#endregion

		#region IHasCodecLayouts Members

        private Func<string> LocalLayoutFeedbackFunc
        {
            get
            {
                return () =>
                    {
                        if (Configuration.Call.Layout.Style != zConfiguration.eLayoutStyle.None)
                            return Configuration.Call.Layout.Style.ToString();
                        else
                            return Configuration.Client.Call.Layout.Style.ToString();
                    };
            }
        }

		public StringFeedback LocalLayoutFeedback { get; private set; }

		public void LocalLayoutToggle()
		{
			throw new NotImplementedException();
		}

		public void LocalLayoutToggleSingleProminent()
		{
			throw new NotImplementedException();
		}

		public void MinMaxLayoutToggle()
		{
			throw new NotImplementedException();
		}		

		#endregion

	}

	/// <summary>
	/// Zoom Room specific info object
	/// </summary>
	public class ZoomRoomInfo : VideoCodecInfo
	{
		public ZoomRoomInfo(ZoomRoomStatus status, ZoomRoomConfiguration configuration)
		{
			Status = status;
			Configuration = configuration;
		}

		public ZoomRoomStatus Status { get; private set; }
		public ZoomRoomConfiguration Configuration { get; private set; }

		public override bool AutoAnswerEnabled
		{
			get { return Status.SystemUnit.RoomInfo.AutoAnswerIsEnabled; }
		}

		public override string E164Alias
		{
			get
			{
				if (!string.IsNullOrEmpty(Status.SystemUnit.MeetingNumber))
				{
					return Status.SystemUnit.MeetingNumber;
				}
				return string.Empty;
			}
		}

		public override string H323Id
		{
			get
			{
				if (!string.IsNullOrEmpty(Status.Call.Info.meeting_list_item.third_party.h323_address))
				{
					return Status.Call.Info.meeting_list_item.third_party.h323_address;
				}
				return string.Empty;
			}
		}

		public override string IpAddress
		{
			get
			{
				if (!string.IsNullOrEmpty(Status.SystemUnit.RoomInfo.AccountEmail))
				{
					return Status.SystemUnit.RoomInfo.AccountEmail;
				}
				return string.Empty;
			}
		}

		public override bool MultiSiteOptionIsEnabled
		{
			get { return true; }
		}

		public override string SipPhoneNumber
		{
			get
			{
				if (!string.IsNullOrEmpty(Status.Call.Info.dialIn))
				{
					return Status.Call.Info.dialIn;
				}
				return string.Empty;
			}
		}

		public override string SipUri
		{
			get
			{
				if (!string.IsNullOrEmpty(Status.Call.Info.meeting_list_item.third_party.sip_address))
				{
					return Status.Call.Info.meeting_list_item.third_party.sip_address;
				}
				return string.Empty;
			}
		}
	}

	/// <summary>
	/// Tracks the initial sycnronization state when establishing a new connection
	/// </summary>
	public class ZoomRoomSyncState : IKeyed
	{
		private readonly ZoomRoom _parent;
		private readonly CrestronQueue<string> _syncQueries;
		private bool _initialSyncComplete;

		public ZoomRoomSyncState(string key, ZoomRoom parent)
		{
			_parent = parent;
			Key = key;
			_syncQueries = new CrestronQueue<string>(50);
			CodecDisconnected();
		}

		public bool InitialSyncComplete
		{
			get { return _initialSyncComplete; }
			private set
			{
				if (value)
				{
					var handler = InitialSyncCompleted;
					if (handler != null)
					{
						handler(this, new EventArgs());
					}
				}
				_initialSyncComplete = value;
			}
		}

		public bool LoginMessageWasReceived { get; private set; }

		public bool InitialQueryMessagesWereSent { get; private set; }

		public bool LastQueryResponseWasReceived { get; private set; }

		public bool CamerasHaveBeenSetUp { get; private set; }

		#region IKeyed Members

		public string Key { get; private set; }

		#endregion

		public event EventHandler<EventArgs> InitialSyncCompleted;

		public void StartSync()
		{
			DequeueQueries();
		}

		private void DequeueQueries()
		{
			while (!_syncQueries.IsEmpty)
			{
				var query = _syncQueries.Dequeue();

				_parent.SendText(query);
			}

			InitialQueryMessagesSent();
		}

		public void AddQueryToQueue(string query)
		{
			_syncQueries.Enqueue(query);
		}

		public void LoginMessageReceived()
		{
			LoginMessageWasReceived = true;
			Debug.Console(1, this, "Login Message Received.");
			CheckSyncStatus();
		}

		public void InitialQueryMessagesSent()
		{
			InitialQueryMessagesWereSent = true;
			Debug.Console(1, this, "Query Messages Sent.");
			CheckSyncStatus();
		}

		public void LastQueryResponseReceived()
		{
			LastQueryResponseWasReceived = true;
			Debug.Console(1, this, "Last Query Response Received.");
			CheckSyncStatus();
		}

		public void CamerasSetUp()
		{
			CamerasHaveBeenSetUp = true;
			Debug.Console(1, this, "Cameras Set Up.");
			CheckSyncStatus();
		}

		public void CodecDisconnected()
		{
			_syncQueries.Clear();
			LoginMessageWasReceived = false;
			InitialQueryMessagesWereSent = false;
			LastQueryResponseWasReceived = false;
			CamerasHaveBeenSetUp = false;
			InitialSyncComplete = false;
		}

		private void CheckSyncStatus()
		{
			if (LoginMessageWasReceived && InitialQueryMessagesWereSent && LastQueryResponseWasReceived &&
				CamerasHaveBeenSetUp)
			{
				InitialSyncComplete = true;
				Debug.Console(1, this, "Initial Codec Sync Complete!");
			}
			else
			{
				InitialSyncComplete = false;
			}
		}
	}

	public class ZoomRoomFactory : EssentialsDeviceFactory<ZoomRoom>
	{
		public ZoomRoomFactory()
		{
			TypeNames = new List<string> { "zoomroom" };
		}

		public override EssentialsDevice BuildDevice(DeviceConfig dc)
		{
			Debug.Console(1, "Factory Attempting to create new ZoomRoom Device");
			var comm = CommFactory.CreateCommForDevice(dc);
			return new ZoomRoom(dc, comm);
		}
	}
}