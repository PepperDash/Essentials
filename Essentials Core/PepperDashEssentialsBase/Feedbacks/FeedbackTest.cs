using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

using PepperDash.Core;

namespace PepperDash.Essentials.Core
{
    public class FeedbackTest
    {
        public void doit()
        {
            var sf = new StringFeedback(() => "Ho");

            sf.OutputChange += new EventHandler<FeedbackEventArgs>(sf_OutputChange);
        }

        void sf_OutputChange(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

    }

    public class Hmmm
    {
        IBasicCommunication comm;

        public Hmmm()
        {
            comm.TextReceived += new EventHandler<GenericCommMethodReceiveTextArgs>(comm_TextReceived);

            var d = new Dictionary<string, EventHandler<GenericCommMethodReceiveTextArgs>>
            {
                { "textReceived1", comm_TextReceived} 
            };
        }

        void comm_TextReceived(object sender, GenericCommMethodReceiveTextArgs e)
        {
            throw new NotImplementedException();
        }
    }
}