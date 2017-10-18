using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronDataStore;
using Crestron.SimplSharpPro;

//using PepperDash.Essentials.Core.Http;
using PepperDash.Essentials.License;



namespace PepperDash.Essentials.Core
{
	public static class Global
	{
		public static CrestronControlSystem ControlSystem { get; set; }

		public static LicenseManager LicenseManager { get; set; }

		//public static EssentialsHttpServer HttpConfigServer
		//{
		//    get
		//    {
		//        if (_HttpConfigServer == null)
		//            _HttpConfigServer = new EssentialsHttpServer();
		//        return _HttpConfigServer;
		//    }
		//}
		//static EssentialsHttpServer _HttpConfigServer;


		static Global()
		{
			// Fire up CrestronDataStoreStatic
			var err = CrestronDataStoreStatic.InitCrestronDataStore();
			if (err != CrestronDataStore.CDS_ERROR.CDS_SUCCESS)
			{
				CrestronConsole.PrintLine("Error starting CrestronDataStoreStatic: {0}", err);
				return;
			}
		}

	}
}