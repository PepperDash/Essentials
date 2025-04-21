//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using Crestron.SimplSharp;

//namespace PepperDash.Essentials.Core
//{
//    public interface IPresetsFileChanged : IKeyed
//    {
//        public event EventHandler<PresetsFileChangeEventArgs> PresetsFileChanged;
//    }

//    public class PresetsFileChangeEventArgs : EventArgs
//    {
//        public string FilePath { get; private set; }
//        public PresetsFileChangeEventArgs(string filePath)
//        {
//            FilePath = filePath;
//        }
//    }
//}