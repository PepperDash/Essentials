using System;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core.Web.RequestHandlers;

namespace PepperDash.Essentials.Core.Web.RequestHandlers
{
    public class SwaggerHandler : WebApiBaseRequestHandler
    {
        private HttpCwsRouteCollection routeCollection;
        private string basePath;

        public SwaggerHandler(HttpCwsRouteCollection routeCollection, string basePath) 
        {
            this.routeCollection = routeCollection;
            this.basePath = basePath;
        }

        protected override void HandleGet(HttpCwsContext context)
        {
            var currentIp = CrestronEthernetHelper.GetEthernetParameter(
                CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_CURRENT_IP_ADDRESS, 0);

            var hostname = CrestronEthernetHelper.GetEthernetParameter(
                    CrestronEthernetHelper.ETHERNET_PARAMETER_TO_GET.GET_HOSTNAME, 0);

            var serverUrl = CrestronEnvironment.DevicePlatform == eDevicePlatform.Server
                ? $"https://{hostname}/VirtualControl/Rooms/{InitialParametersClass.RoomId}/cws{basePath}"
                : $"https://{currentIp}/cws{basePath}";

            var openApiDoc = GenerateOpenApiDocument(serverUrl);

            var response = JsonConvert.SerializeObject(openApiDoc, Formatting.Indented);

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            context.Response.Headers.Add("Content-Type", "application/json");
            context.Response.Write(response, false);
            context.Response.End();
        }

        private object GenerateOpenApiDocument(string serverUrl)
        {
            var paths = new Dictionary<string, object>();

            // Add paths based on existing routes
            foreach (var route in routeCollection)
            {
                var pathKey = "/" + route.Url;
                var pathItem = GeneratePathItem(route);
                if (pathItem != null)
                {
                    paths[pathKey] = pathItem;
                }
            }

            return new
            {
                openapi = "3.0.3",
                info = new
                {
                    title = "PepperDash Essentials API",
                    description = "RESTful API for PepperDash Essentials control system",
                    version = "1.0.0",
                    contact = new
                    {
                        name = "PepperDash Technology",
                        url = "https://www.pepperdash.com"
                    }
                },
                servers = new[]
                {
                    new { url = serverUrl, description = "Essentials API Server" }
                },
                paths = paths,
                components = new
                {
                    schemas = GetSchemas()
                }
            };
        }

        private object GeneratePathItem(HttpCwsRoute route)
        {
            // Determine HTTP methods and create appropriate operation objects
            var operations = new Dictionary<string, object>();

            // Based on the route name and common patterns, determine likely HTTP methods
            var routeName = route.Name?.ToLower() ?? "";
            var routeUrl = route.Url?.ToLower() ?? "";

            if (routeName.Contains("get") || routeUrl.Contains("devices") || routeUrl.Contains("config") || 
                routeUrl.Contains("versions") || routeUrl.Contains("types") || routeUrl.Contains("tielines") ||
                routeUrl.Contains("apipaths") || routeUrl.Contains("feedbacks") || routeUrl.Contains("properties") ||
                routeUrl.Contains("methods") || routeUrl.Contains("joinmap") || routeUrl.Contains("routingports"))
            {
                operations["get"] = GenerateOperation(route, "GET");
            }

            if (routeName.Contains("command") || routeName.Contains("restart") || routeName.Contains("load") ||
                routeName.Contains("debug") || routeName.Contains("disable"))
            {
                operations["post"] = GenerateOperation(route, "POST");
            }

            return operations.Count > 0 ? operations : null;
        }

        private object GenerateOperation(HttpCwsRoute route, string method)
        {
            var operation = new Dictionary<string, object>
            {
                ["summary"] = route.Name ?? "API Operation",
                ["operationId"] = route.Name?.Replace(" ", "") ?? "operation",
                ["responses"] = new Dictionary<string, object>
                {
                    ["200"] = new
                    {
                        description = "Successful response",
                        content = new Dictionary<string, object>
                        {
                            ["application/json"] = new { schema = new { type = "object" } }
                        }
                    },
                    ["400"] = new { description = "Bad Request" },
                    ["404"] = new { description = "Not Found" },
                    ["500"] = new { description = "Internal Server Error" }
                }
            };

            // Add parameters for path variables
            if (route.Url.Contains("{"))
            {
                var parameters = new List<object>();
                var url = route.Url;
                while (url.Contains("{"))
                {
                    var start = url.IndexOf("{");
                    var end = url.IndexOf("}", start);
                    if (end > start)
                    {
                        var paramName = url.Substring(start + 1, end - start - 1);
                        parameters.Add(new
                        {
                            name = paramName,
                            @in = "path",
                            required = true,
                            schema = new { type = "string" },
                            description = $"The {paramName} parameter"
                        });
                        url = url.Substring(end + 1);
                    }
                    else break;
                }
                if (parameters.Count > 0)
                {
                    operation["parameters"] = parameters;
                }
            }

            // Add request body for POST operations
            if (method == "POST" && route.Name != null && route.Name.Contains("Command"))
            {
                operation["requestBody"] = new
                {
                    required = true,
                    content = new Dictionary<string, object>
                    {
                        ["application/json"] = new 
                        { 
                            schema = new Dictionary<string, object> { ["$ref"] = "#/components/schemas/DeviceCommand" }
                        }
                    }
                };
            }

            // Add specific descriptions based on route patterns
            AddRouteSpecificDescription(operation, route);

            return operation;
        }

        private void AddRouteSpecificDescription(Dictionary<string, object> operation, HttpCwsRoute route)
        {
            var routeName = route.Name?.ToLower() ?? "";
            var routeUrl = route.Url?.ToLower() ?? "";

            if (routeUrl.Contains("devices") && !routeUrl.Contains("{"))
            {
                operation["description"] = "Retrieve a list of all devices in the system";
            }
            else if (routeUrl.Contains("versions"))
            {
                operation["description"] = "Get version information for loaded assemblies";
            }
            else if (routeUrl.Contains("config"))
            {
                operation["description"] = "Retrieve the current system configuration";
            }
            else if (routeUrl.Contains("devicecommands"))
            {
                operation["description"] = "Send a command to a specific device";
            }
            else if (routeUrl.Contains("devicefeedbacks"))
            {
                operation["description"] = "Get feedback values from a specific device";
            }
            else if (routeUrl.Contains("deviceproperties"))
            {
                operation["description"] = "Get properties of a specific device";
            }
            else if (routeUrl.Contains("devicemethods"))
            {
                operation["description"] = "Get available methods for a specific device";
            }
            else if (routeUrl.Contains("types"))
            {
                operation["description"] = routeUrl.Contains("{") ? "Get types filtered by the specified filter" : "Get all available types";
            }
            else if (routeUrl.Contains("tielines"))
            {
                operation["description"] = "Get information about tielines in the system";
            }
            else if (routeUrl.Contains("joinmap"))
            {
                operation["description"] = "Get join map information for bridge or device";
            }
            else if (routeUrl.Contains("routingports"))
            {
                operation["description"] = "Get routing ports for a specific device";
            }
            else if (routeUrl.Contains("apipaths"))
            {
                operation["description"] = "Get available API paths and routes";
            }
            else if (routeName.Contains("restart"))
            {
                operation["description"] = "Restart the program";
            }
            else if (routeName.Contains("debug"))
            {
                operation["description"] = "Debug operation";
            }
        }

        private Dictionary<string, object> GetSchemas()
        {
            return new Dictionary<string, object>
            {
                ["DeviceCommand"] = new
                {
                    type = "object",
                    properties = new Dictionary<string, object>
                    {
                        ["deviceKey"] = new { type = "string", description = "The key of the device" },
                        ["methodName"] = new { type = "string", description = "The method to call on the device" },
                        ["params"] = new { type = "array", items = new { type = "object" }, description = "Parameters for the method call" }
                    },
                    required = new[] { "deviceKey", "methodName" }
                },
                ["Device"] = new
                {
                    type = "object",
                    properties = new Dictionary<string, object>
                    {
                        ["key"] = new { type = "string", description = "Device key" },
                        ["name"] = new { type = "string", description = "Device name" },
                        ["type"] = new { type = "string", description = "Device type" },
                        ["isOnline"] = new { type = "boolean", description = "Device online status" }
                    }
                },
                ["Feedback"] = new
                {
                    type = "object",
                    properties = new Dictionary<string, object>
                    {
                        ["BoolValues"] = new { type = "array", items = new Dictionary<string, object> { ["$ref"] = "#/components/schemas/BoolFeedback" } },
                        ["IntValues"] = new { type = "array", items = new Dictionary<string, object> { ["$ref"] = "#/components/schemas/IntFeedback" } },
                        ["SerialValues"] = new { type = "array", items = new Dictionary<string, object> { ["$ref"] = "#/components/schemas/StringFeedback" } }
                    }
                },
                ["BoolFeedback"] = new
                {
                    type = "object",
                    properties = new Dictionary<string, object>
                    {
                        ["FeedbackKey"] = new { type = "string" },
                        ["Value"] = new { type = "boolean" }
                    }
                },
                ["IntFeedback"] = new
                {
                    type = "object",
                    properties = new Dictionary<string, object>
                    {
                        ["FeedbackKey"] = new { type = "string" },
                        ["Value"] = new { type = "integer" }
                    }
                },
                ["StringFeedback"] = new
                {
                    type = "object",
                    properties = new Dictionary<string, object>
                    {
                        ["FeedbackKey"] = new { type = "string" },
                        ["Value"] = new { type = "string" }
                    }
                },
                ["ApiRoutes"] = new
                {
                    type = "object",
                    properties = new Dictionary<string, object>
                    {
                        ["url"] = new { type = "string", description = "Base URL for the API" },
                        ["routes"] = new { type = "array", items = new Dictionary<string, object> { ["$ref"] = "#/components/schemas/Route" } }
                    }
                },
                ["Route"] = new
                {
                    type = "object",
                    properties = new Dictionary<string, object>
                    {
                        ["name"] = new { type = "string", description = "Route name" },
                        ["url"] = new { type = "string", description = "Route URL pattern" }
                    }
                }
            };
        }
    }
}