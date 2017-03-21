//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharpPro;

//namespace PepperDash.Essentials.Core
//{
//    public class SigId
//    {
//        public uint Number { get; private set; }
//        public eSigType Type { get; private set; }

//        public SigId(eSigType type, uint number)
//        {
//            Type = type;
//            Number = number;
//        }

//        public override bool Equals(object id)
//        {
//            if (id is SigId)
//            {
//                var sigId = id as SigId;
//                return this.Number == sigId.Number && this.Type == sigId.Type;
//            }
//            else
//                return base.Equals(id);
//        }

//        public override int GetHashCode()
//        {
//            return base.GetHashCode();
//        }
//    }

//}