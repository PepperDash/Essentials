using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core.Feedbacks
{
	public class IntWithFeedback 
	{
		private int _Value;
		public IntFeedback Feedback;
		public int Value
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
		public IntWithFeedback()
			
		{
			Feedback = new IntFeedback((() => Value));
		}
	}
}