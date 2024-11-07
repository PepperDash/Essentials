using System;

namespace PepperDash.Essentials.Core.Ramps_and_Increments
{
    public class NumericalHelpers 
    {  
        /// <summary>
        /// Scales a value
        /// </summary>
        /// <param name="input"></param>
        /// <param name="inMin"></param>
        /// <param name="inMax"></param>
        /// <param name="outMin"></param>
        /// <param name="outMax"></param>
        /// <returns></returns>
        public static double Scale(double input, double inMin, double inMax, double outMin, double outMax)
        {
            //Debug.LogMessage(LogEventLevel.Verbose, this, "Scaling (double) input '{0}' with min '{1}'/max '{2}' to output range min '{3}'/max '{4}'", input, inMin, inMax, outMin, outMax);

            double inputRange = inMax - inMin;

            if (inputRange <= 0)
            {
                throw new ArithmeticException(string.Format("Invalid Input Range '{0}' for Scaling.  Min '{1}' Max '{2}'.", inputRange, inMin, inMax));
            }

            double outputRange = outMax - outMin;

            var output = (((input - inMin) * outputRange) / inputRange) + outMin;

           // Debug.LogMessage(LogEventLevel.Verbose, this, "Scaled output '{0}'", output);

            return output;
        }
    }
}