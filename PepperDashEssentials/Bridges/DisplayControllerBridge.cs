//using system;
//using system.collections.generic;
//using system.linq;
//using system.text;
//using crestron.simplsharp;
//using crestron.simplsharppro.devicesupport;
//using pepperdash.core;
//using pepperdash.essentials.core;
//using pepperdash.essentials.devices.common;

//namespace pepperdash.essentials.bridges
//{
//    public static class displaycontrollerapiextensions
//    {

//        public static int inputnumber;
//        public static intfeedback inputnumberfeedback;
//        public static list<string> inputkeys = new list<string>();
//        public static void linktoapi(this pepperdash.essentials.core.displaybase displaydevice, basictrilist trilist, uint joinstart, string joinmapkey)
//        {

//                var joinmap = joinmaphelper.getjoinmapfordevice(joinmapkey) as displaycontrollerjoinmap;

//                if (joinmap == null)
//                {
//                    joinmap = new displaycontrollerjoinmap();
//                }

//                joinmap.offsetjoinnumbers(joinstart);

//                debug.console(1, "linking to trilist '{0}'",trilist.id.tostring("x"));
//                debug.console(0, "linking to display: {0}", displaydevice.name);

//                trilist.stringinput[joinmap.name].stringvalue = displaydevice.name;			

//                var commmonitor = displaydevice as icommunicationmonitor;
//                if (commmonitor != null)
//                {
//                    commmonitor.communicationmonitor.isonlinefeedback.linkinputsig(trilist.booleaninput[joinmap.isonline]);
//                }

//                inputnumberfeedback.linkinputsig(trilist.ushortinput[joinmap.inputselect]);
//                // two way feedbacks
//                var twowaydisplay = displaydevice as pepperdash.essentials.core.twowaydisplaybase;
//                if (twowaydisplay != null)
//                {
//                    trilist.setbool(joinmap.istwowaydisplay, true);

//                    twowaydisplay.currentinputfeedback.outputchange += new eventhandler<feedbackeventargs>(currentinputfeedback_outputchange);


                   
//                }

//                // power off
//                trilist.setsigtrueaction(joinmap.poweroff, () =>
//                    {
//                        inputnumber = 102;
//                        inputnumberfeedback.fireupdate();
//                        displaydevice.poweroff();
//                    });

//                displaydevice.powerisonfeedback.outputchange += new eventhandler<feedbackeventargs>(powerisonfeedback_outputchange);
//                displaydevice.powerisonfeedback.linkcomplementinputsig(trilist.booleaninput[joinmap.poweroff]);

//                // poweron
//                trilist.setsigtrueaction(joinmap.poweron, () =>
//                    {
//                        inputnumber = 0;
//                        inputnumberfeedback.fireupdate();
//                        displaydevice.poweron();
//                    });

				
//                displaydevice.powerisonfeedback.linkinputsig(trilist.booleaninput[joinmap.poweron]);

//                int count = 1;
//                foreach (var input in displaydevice.inputports)
//                {
//                    inputkeys.add(input.key.tostring());
//                    var tempkey = inputkeys.elementat(count - 1);
//                    trilist.setsigtrueaction((ushort)(joinmap.inputselectoffset + count), () => { displaydevice.executeswitch(displaydevice.inputports[tempkey].selector); });
//                    debug.console(2, displaydevice, "setting input select action on digital join {0} to input: {1}", joinmap.inputselectoffset + count, displaydevice.inputports[tempkey].key.tostring());
//                    trilist.stringinput[(ushort)(joinmap.inputnamesoffset + count)].stringvalue = input.key.tostring();
//                    count++;
//                }

//                debug.console(2, displaydevice, "setting input select action on analog join {0}", joinmap.inputselect);
//                trilist.setushortsigaction(joinmap.inputselect, (a) =>
				
//                {
//                        if (a == 0)
//                        {
//                            displaydevice.poweroff();
//                            inputnumber = 0;
//                            inputnumberfeedback.fireupdate();
//                        }
//                        else if (a > 0 && a < displaydevice.inputports.count )
//                        {
//                            displaydevice.executeswitch(displaydevice.inputports.elementat(a - 1).selector);
//                            inputnumber = a;
//                            inputnumberfeedback.fireupdate();
//                        }
//                        else if (a == 102)
//                        {
//                            displaydevice.powertoggle();

//                        }
	                    
							

//                });


//                var volumedisplay = displaydevice as ibasicvolumecontrols;
//                if (volumedisplay != null)
//                {
//                    trilist.setboolsigaction(joinmap.volumeup, (b) => volumedisplay.volumeup(b));
//                    trilist.setboolsigaction(joinmap.volumedown, (b) => volumedisplay.volumedown(b));
//                    trilist.setsigtrueaction(joinmap.volumemute, () => volumedisplay.mutetoggle());

//                    var volumedisplaywithfeedback = volumedisplay as ibasicvolumewithfeedback;
//                    if(volumedisplaywithfeedback != null)
//                    {
//                        volumedisplaywithfeedback.volumelevelfeedback.linkinputsig(trilist.ushortinput[joinmap.volumelevelfb]);
//                        volumedisplaywithfeedback.mutefeedback.linkinputsig(trilist.booleaninput[joinmap.volumemute]);
//                    }
//                }
//            }

//        static void currentinputfeedback_outputchange(object sender, feedbackeventargs e)
//        {

//            debug.console(0, "currentinputfeedback_outputchange {0}", e.stringvalue);

//        }

//        static void powerisonfeedback_outputchange(object sender, feedbackeventargs e)
//        {

//            // debug.console(0, "powerisonfeedback_outputchange {0}",  e.boolvalue);
//            if (!e.boolvalue)
//            {
//                inputnumber = 102;
//                inputnumberfeedback.fireupdate();

//            }
//            else
//            {
//                inputnumber = 0;
//                inputnumberfeedback.fireupdate();
//            }
//        }




//    }
//    public class displaycontrollerjoinmap : joinmapbase
//    {
//        // digital
//        public uint poweroff { get; set; }
//        public uint poweron { get; set; }
//        public uint istwowaydisplay { get; set; }
//        public uint volumeup { get; set; }
//        public uint volumedown { get; set; }
//        public uint volumemute { get; set; }
//        public uint inputselectoffset { get; set; }
//        public uint buttonvisibilityoffset { get; set; }
//        public uint isonline { get; set; }

//        // analog
//        public uint inputselect { get; set; }
//        public uint volumelevelfb { get; set; }

//        // serial
//        public uint name { get; set; }
//        public uint inputnamesoffset { get; set; }


//        public displaycontrollerjoinmap()
//        {
//            // digital
//            isonline = 50;
//            poweroff = 1;
//            poweron = 2;
//            istwowaydisplay = 3;
//            volumeup = 5;
//            volumedown = 6;
//            volumemute = 7;

//            buttonvisibilityoffset = 40;
//            inputselectoffset = 10;

//            // analog
//            inputselect = 11;
//            volumelevelfb = 5;

//            // serial
//            name = 1;
//            inputnamesoffset = 10;
//        }

//        public override void offsetjoinnumbers(uint joinstart)
//        {
//            var joinoffset = joinstart - 1;

//            isonline = isonline + joinoffset;
//            poweroff = poweroff + joinoffset;
//            poweron = poweron + joinoffset;
//            istwowaydisplay = istwowaydisplay + joinoffset;
//            buttonvisibilityoffset = buttonvisibilityoffset + joinoffset;
//            name = name + joinoffset;
//            inputnamesoffset = inputnamesoffset + joinoffset;
//            inputselectoffset = inputselectoffset + joinoffset;

//            inputselect = inputselect + joinoffset;

//            volumeup = volumeup + joinoffset;
//            volumedown = volumedown + joinoffset;
//            volumemute = volumemute + joinoffset;
//            volumelevelfb = volumelevelfb + joinoffset;
//        }
//    }
//}