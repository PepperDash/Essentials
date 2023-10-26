using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace PepperDash.Essentials.Core
{


	public abstract class BoolFeedbackLogic
	{
		/// <summary>
		/// Output representing the "and" value of all connected inputs
		/// </summary>
		public BoolFeedback Output { get; private set; }

		/// <summary>
		/// List of all connected outputs
		/// </summary>
		protected List<BoolFeedback> OutputsIn = new List<BoolFeedback>();

		protected bool ComputedValue;

	    protected BoolFeedbackLogic()
		{
			Output = new BoolFeedback(() => ComputedValue);
		}	

		public void AddOutputIn(BoolFeedback output)
		{
			// Don't double up outputs
			if(OutputsIn.Contains(output)) return;

			OutputsIn.Add(output);
			output.OutputChange += AnyInput_OutputChange;
			Evaluate();
		}

		public void AddOutputsIn(List<BoolFeedback> outputs)
		{
		    foreach (var o in outputs.Where(o => !OutputsIn.Contains(o)))
		    {
		        OutputsIn.Add(o);
		        o.OutputChange += AnyInput_OutputChange;
		    }
		    Evaluate();
		}

	    public void RemoveOutputIn(BoolFeedback output)
		{
			// Don't double up outputs
			if (!OutputsIn.Contains(output)) return;

			OutputsIn.Remove(output);
			output.OutputChange -= AnyInput_OutputChange;
			Evaluate();
		}

		public void RemoveOutputsIn(List<BoolFeedback> outputs)
		{
			foreach (var o in outputs)
			{
				OutputsIn.Remove(o);
				o.OutputChange -= AnyInput_OutputChange;
			}
			Evaluate();
		}

	    public void ClearOutputs()
	    {
	        OutputsIn.Clear();
            Evaluate();
	    }

		void AnyInput_OutputChange(object sender, EventArgs e)
		{
			Evaluate();
		}

		protected abstract void Evaluate();
	}
}