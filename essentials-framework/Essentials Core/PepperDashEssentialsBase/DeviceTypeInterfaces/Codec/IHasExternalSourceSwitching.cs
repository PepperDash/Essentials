using System;

namespace PepperDash.Essentials.Devices.Common.Codec
{
	public interface IHasExternalSourceSwitching
	{
		bool ExternalSourceListEnabled { get; }
        string ExternalSourceInputPort { get; }
		void AddExternalSource(string connectorId, string key, string name, eExternalSourceType type);
		void SetExternalSourceState(string key, eExternalSourceMode mode);
		void ClearExternalSources();
        void SetSelectedSource(string key);
		Action<string, string> RunRouteAction { set;}
	}

    public enum eExternalSourceType { camera, desktop, document_camera, mediaplayer, PC, whiteboard, other }
    public enum eExternalSourceMode { Ready, NotReady, Hidden, Error } 
}