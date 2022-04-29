using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core.Feedbacks
{
	public class BoolWithFeedback 
	{
		private bool _Value;
		public BoolFeedback Feedback;
		public bool Value
		{
			get
			{
				return _Value;
			}
			set
			{
				_Value = value;
				Feedback.FireUpdate();
			}
		}
		public BoolWithFeedback()
			
		{
			Feedback = new BoolFeedback(() => { return Value; });
		}
	}

}