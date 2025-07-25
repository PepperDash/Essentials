using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Crestron.SimplSharp;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core.Web.RequestHandlers;
using PepperDash.Essentials.Core.Web.Attributes;

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
            if (route.RouteHandler == null) return null;

            var handlerType = route.RouteHandler.GetType();
            var operations = new Dictionary<string, object>();

            // Get HTTP method attributes from the handler class
            var httpMethodAttributes = handlerType.GetCustomAttributes(typeof(HttpMethodAttribute), false)
                .Cast<HttpMethodAttribute>()
                .ToList();

            // If no HTTP method attributes found, fall back to the original logic
            if (!httpMethodAttributes.Any())
            {
                httpMethodAttributes = DetermineHttpMethodsFromRoute(route);
            }

            foreach (var methodAttr in httpMethodAttributes)
            {
                var operation = GenerateOperation(route, methodAttr.Method, handlerType);
                if (operation != null)
                {
                    operations[methodAttr.Method.ToLower()] = operation;
                }
            }

            return operations.Count > 0 ? operations : null;
        }

        private List<HttpMethodAttribute> DetermineHttpMethodsFromRoute(HttpCwsRoute route)
        {
            var methods = new List<HttpMethodAttribute>();
            var routeName = route.Name?.ToLower() ?? "";
            var routeUrl = route.Url?.ToLower() ?? "";

            // Fallback logic for routes without attributes
            if (routeName.Contains("get") || routeUrl.Contains("devices") || routeUrl.Contains("config") || 
                routeUrl.Contains("versions") || routeUrl.Contains("types") || routeUrl.Contains("tielines") ||
                routeUrl.Contains("apipaths") || routeUrl.Contains("feedbacks") || routeUrl.Contains("properties") ||
                routeUrl.Contains("methods") || routeUrl.Contains("joinmap") || routeUrl.Contains("routingports"))
            {
                methods.Add(new HttpGetAttribute());
            }

            if (routeName.Contains("command") || routeName.Contains("restart") || routeName.Contains("load") ||
                routeName.Contains("debug") || routeName.Contains("disable"))
            {
                methods.Add(new HttpPostAttribute());
            }

            return methods;
        }

        private object GenerateOperation(HttpCwsRoute route, string method, Type handlerType)
        {
            var operation = new Dictionary<string, object>();

            // Get OpenApiOperation attribute
            var operationAttr = handlerType.GetCustomAttribute<OpenApiOperationAttribute>();
            if (operationAttr != null)
            {
                operation["summary"] = operationAttr.Summary ?? route.Name ?? "API Operation";
                operation["operationId"] = operationAttr.OperationId ?? route.Name?.Replace(" ", "") ?? "operation";
                
                if (!string.IsNullOrEmpty(operationAttr.Description))
                {
                    operation["description"] = operationAttr.Description;
                }

                if (operationAttr.Tags != null && operationAttr.Tags.Length > 0)
                {
                    operation["tags"] = operationAttr.Tags;
                }
            }
            else
            {
                // Fallback to route name
                operation["summary"] = route.Name ?? "API Operation";
                operation["operationId"] = route.Name?.Replace(" ", "") ?? "operation";
                
                // Add fallback description
                var fallbackDescription = GetFallbackDescription(route);
                if (!string.IsNullOrEmpty(fallbackDescription))
                {
                    operation["description"] = fallbackDescription;
                }
            }

            // Get response attributes
            var responses = new Dictionary<string, object>();
            var responseAttrs = handlerType.GetCustomAttributes<OpenApiResponseAttribute>().ToList();
            
            if (responseAttrs.Any())
            {
                foreach (var responseAttr in responseAttrs)
                {
                    var responseObj = new Dictionary<string, object>
                    {
                        ["description"] = responseAttr.Description ?? "Response"
                    };

                    if (!string.IsNullOrEmpty(responseAttr.ContentType))
                    {
                        responseObj["content"] = new Dictionary<string, object>
                        {
                            [responseAttr.ContentType] = new { schema = new { type = "object" } }
                        };
                    }

                    responses[responseAttr.StatusCode.ToString()] = responseObj;
                }
            }
            else
            {
                // Default responses
                responses["200"] = new
                {
                    description = "Successful response",
                    content = new Dictionary<string, object>
                    {
                        ["application/json"] = new { schema = new { type = "object" } }
                    }
                };
                responses["400"] = new { description = "Bad Request" };
                responses["404"] = new { description = "Not Found" };
                responses["500"] = new { description = "Internal Server Error" };
            }

            operation["responses"] = responses;

            // Get parameter attributes
            var parameterAttrs = handlerType.GetCustomAttributes<OpenApiParameterAttribute>().ToList();
            var parameters = new List<object>();

            // Add parameters from attributes
            foreach (var paramAttr in parameterAttrs)
            {
                parameters.Add(new
                {
                    name = paramAttr.Name,
                    @in = paramAttr.In.ToString().ToLower(),
                    required = paramAttr.Required,
                    schema = new { type = GetSchemaType(paramAttr.Type) },
                    description = paramAttr.Description ?? $"The {paramAttr.Name} parameter"
                });
            }

            // Add parameters from URL path variables (fallback)
            if (route.Url.Contains("{"))
            {
                var url = route.Url;
                while (url.Contains("{"))
                {
                    var start = url.IndexOf("{");
                    var end = url.IndexOf("}", start);
                    if (end > start)
                    {
                        var paramName = url.Substring(start + 1, end - start - 1);
                        
                        // Only add if not already added from attributes
                        if (!parameters.Any(p => ((dynamic)p).name == paramName))
                        {
                            parameters.Add(new
                            {
                                name = paramName,
                                @in = "path",
                                required = true,
                                schema = new { type = "string" },
                                description = $"The {paramName} parameter"
                            });
                        }
                        url = url.Substring(end + 1);
                    }
                    else break;
                }
            }

            if (parameters.Count > 0)
            {
                operation["parameters"] = parameters;
            }

            // Get request body attribute for POST operations
            if (method == "POST")
            {
                var requestBodyAttr = handlerType.GetCustomAttribute<OpenApiRequestBodyAttribute>();
                if (requestBodyAttr != null)
                {
                    operation["requestBody"] = new
                    {
                        required = requestBodyAttr.Required,
                        description = requestBodyAttr.Description,
                        content = new Dictionary<string, object>
                        {
                            [requestBodyAttr.ContentType] = new Dictionary<string, object>
                            { 
                                ["schema"] = requestBodyAttr.Type != null 
                                    ? (object)new Dictionary<string, object> { ["$ref"] = $"#/components/schemas/{requestBodyAttr.Type.Name}" }
                                    : new Dictionary<string, object> { ["type"] = "object" }
                            }
                        }
                    };
                }
                else if (route.Name != null && route.Name.Contains("Command"))
                {
                    // Fallback for command routes
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
            }

            return operation;
        }

        private string GetSchemaType(Type type)
        {
            if (type == typeof(string)) return "string";
            if (type == typeof(int) || type == typeof(long)) return "integer";
            if (type == typeof(bool)) return "boolean";
            if (type == typeof(double) || type == typeof(float)) return "number";
            return "string"; // default
        }

        private string GetFallbackDescription(HttpCwsRoute route)
        {
            var routeName = route.Name?.ToLower() ?? "";
            var routeUrl = route.Url?.ToLower() ?? "";

            if (routeUrl.Contains("devices") && !routeUrl.Contains("{"))
            {
                return "Retrieve a list of all devices in the system";
            }
            else if (routeUrl.Contains("versions"))
            {
                return "Get version information for loaded assemblies";
            }
            else if (routeUrl.Contains("config"))
            {
                return "Retrieve the current system configuration";
            }
            else if (routeUrl.Contains("devicecommands"))
            {
                return "Send a command to a specific device";
            }
            else if (routeUrl.Contains("devicefeedbacks"))
            {
                return "Get feedback values from a specific device";
            }
            else if (routeUrl.Contains("deviceproperties"))
            {
                return "Get properties of a specific device";
            }
            else if (routeUrl.Contains("devicemethods"))
            {
                return "Get available methods for a specific device";
            }
            else if (routeUrl.Contains("types"))
            {
                return routeUrl.Contains("{") ? "Get types filtered by the specified filter" : "Get all available types";
            }
            else if (routeUrl.Contains("tielines"))
            {
                return "Get information about tielines in the system";
            }
            else if (routeUrl.Contains("joinmap"))
            {
                return "Get join map information for bridge or device";
            }
            else if (routeUrl.Contains("routingports"))
            {
                return "Get routing ports for a specific device";
            }
            else if (routeUrl.Contains("apipaths"))
            {
                return "Get available API paths and routes";
            }
            else if (routeName.Contains("restart"))
            {
                return "Restart the program";
            }
            else if (routeName.Contains("debug"))
            {
                return "Debug operation";
            }

            return null;
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