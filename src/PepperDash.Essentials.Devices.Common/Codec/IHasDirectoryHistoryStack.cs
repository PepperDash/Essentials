using System.Collections.Generic;

namespace PepperDash.Essentials.Devices.Common.Codec
{
    public interface IHasDirectoryHistoryStack : IHasDirectory
    {
        Stack<CodecDirectory> DirectoryBrowseHistoryStack { get; } 
    }
}