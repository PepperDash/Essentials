using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core.Web.RequestHandlers;
using System.Collections.Generic;
using System.Linq;

namespace PepperDash.Essentials.WebApiHandlers
{
    /// <summary>
    /// Represents a ActionPathsHandler
    /// </summary>
    public class ActionPathsHandler : WebApiBaseRequestHandler
    {
        private readonly MobileControlSystemController mcController;
        public ActionPathsHandler(MobileControlSystemController controller) : base(true)
        {
            mcController = controller;
        }

        protected override void HandleGet(HttpCwsContext context)
        {
            var response = JsonConvert.SerializeObject(new ActionPathsResponse(mcController));

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            context.Response.Headers.Add("Content-Type", "application/json");
            context.Response.Write(response, false);
            context.Response.End();
        }
    }

    /// <summary>
    /// Represents a ActionPathsResponse
    /// </summary>
    public class ActionPathsResponse
    {
        [JsonIgnore]
        private readonly MobileControlSystemController mcController;

        [JsonProperty("actionPaths")]
        public List<ActionPath> ActionPaths => mcController.GetActionDictionaryPaths().Select((path) => new ActionPath { MessengerKey = path.Item1, Path = path.Item2 }).ToList();

        public ActionPathsResponse(MobileControlSystemController mcController)
        {
            this.mcController = mcController;
        }
    }

    /// <summary>
    /// Represents a ActionPath
    /// </summary>
    public class ActionPath
    {
        [JsonProperty("messengerKey")]
        /// <summary>
        /// Gets or sets the MessengerKey
        /// </summary>
        public string MessengerKey { get; set; }

        [JsonProperty("path")]
        /// <summary>
        /// Gets or sets the Path
        /// </summary>
        public string Path { get; set; }
    }
}
