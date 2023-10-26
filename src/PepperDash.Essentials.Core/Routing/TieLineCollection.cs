using System.Collections.Generic;

namespace PepperDash.Essentials.Core
{
    public class TieLineCollection : List<TieLine>
    {
        public static TieLineCollection Default
        {
            get
            {
                if (_Default == null)
                    _Default = new TieLineCollection();
                return _Default;
            }
        }
        static TieLineCollection _Default;
    }
}