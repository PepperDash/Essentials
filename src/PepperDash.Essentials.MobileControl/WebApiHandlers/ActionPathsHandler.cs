using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core.Web.RequestHandlers;

namespace PepperDash.Essentials.WebApiHandlers
{
    /// <summary>
    /// Represents a ActionPathsHandler
    /// </summary>
    public class ActionPathsHandler : WebApiBaseRequestHandler
    {
        private readonly MobileControlSystemController mcController;

        /// <summary>
        /// Create an instance of the <see cref="ActionPathsHandler"/> class.
        /// </summary>
        /// <param name="controller"></param>
        public ActionPathsHandler(MobileControlSystemController controller) : base(true)
        {
            mcController = controller;
        }

        /// <summary>
        /// Handle a request to get the action paths
        /// </summary>
        /// <param name="context">Request Context</param>
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

        /// <summary>
        /// Registered action paths for this system
        /// </summary>
        [JsonProperty("actionPaths")]
        public List<ActionPath> ActionPaths => mcController.GetActionDictionaryPaths().Select((path) => new ActionPath { MessengerKey = path.Item1, Path = path.Item2 }).ToList();

        /// <summary>
        /// Create an instance of the <see cref="ActionPathsResponse"/> class.
        /// </summary>
        /// <param name="mcController"></param>
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

        /// <summary>
        /// Gets or sets the MessengerKey
        /// </summary>
        [JsonProperty("messengerKey")]
        public string MessengerKey { get; set; }


        /// <summary>
        /// Gets or sets the Path
        /// </summary>
        [JsonProperty("path")]
        public string Path { get; set; }
    }
}
