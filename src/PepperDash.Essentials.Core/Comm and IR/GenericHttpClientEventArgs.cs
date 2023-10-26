using System;
using Crestron.SimplSharp.Net.Http;

namespace PepperDash.Essentials.Core
{
    public class GenericHttpClientEventArgs : EventArgs
    {
        public string ResponseText { get; private set; }
        public string RequestPath { get; private set; }
        public HTTP_CALLBACK_ERROR Error { get; set; }
        public GenericHttpClientEventArgs(string response, string request, HTTP_CALLBACK_ERROR error)
        {
            ResponseText = response;
            RequestPath  = request;
            Error        = error;
        }
    }
}