using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using Crestron.SimplSharp.Net.Http;
using PepperDash.Core;
using PepperDash.Essentials.Core;
using PepperDash.Essentials.Core.PageManagers;

namespace PepperDash.Essentials
{
    public class CotijaInterfaceController : Device
    {
        public CotijaInterfaceController(string key) : base(key)
        {
            CrestronConsole.AddNewConsoleCommand(InitializeClient, "InitializeHttpClient", "Initializes a new HTTP client connection to a specified URL", ConsoleAccessLevelEnum.AccessOperator);
        }

        public void InitializeClient(string url)
        {
            HttpClient webClient = new HttpClient();
            webClient.Verbose = true;
            HttpClientRequest request = new HttpClientRequest();
            request.Url.Parse(url);
            request.Header.AddHeader(new HttpHeader("accept", "text/event-stream"));
            request.Header.SetHeaderValue("Expect", "");
            webClient.DispatchAsync(request, (resp, err) =>
            {
                CrestronConsole.PrintLine(resp.ContentString);
            });
        }
    }
}