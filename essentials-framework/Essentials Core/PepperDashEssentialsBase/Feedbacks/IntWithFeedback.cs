using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace PepperDash.Essentials.Core.Feedbacks
{
	public class IntWithFeedback : IntFeedback
	{
		private int _Value;
		public int Value
		{
			get
			{
				return _Value;
			}
			set
			{
				_Value = value;
				this.FireUpdate();
			}
		}
		public IntWithFeedback()
			: base(() => Value)
		{

		}
	}
}