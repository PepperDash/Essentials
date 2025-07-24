using System;
using System.ComponentModel;

namespace PepperDash.Essentials.Core.Web.Attributes
{
    /// <summary>
    /// Base class for HTTP method attributes
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public abstract class HttpMethodAttribute : Attribute
    {
        public string Method { get; }

        protected HttpMethodAttribute(string method)
        {
            Method = method;
        }
    }

    /// <summary>
    /// Indicates that a request handler supports HTTP GET operations
    /// </summary>
    public class HttpGetAttribute : HttpMethodAttribute
    {
        public HttpGetAttribute() : base("GET") { }
    }

    /// <summary>
    /// Indicates that a request handler supports HTTP POST operations
    /// </summary>
    public class HttpPostAttribute : HttpMethodAttribute
    {
        public HttpPostAttribute() : base("POST") { }
    }

    /// <summary>
    /// Provides OpenAPI operation metadata for a request handler
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class OpenApiOperationAttribute : Attribute
    {
        /// <summary>
        /// A brief summary of what the operation does
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// A verbose explanation of the operation behavior
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Unique string used to identify the operation
        /// </summary>
        public string OperationId { get; set; }

        /// <summary>
        /// A list of tags for API documentation control
        /// </summary>
        public string[] Tags { get; set; }

        public OpenApiOperationAttribute()
        {
        }
    }

    /// <summary>
    /// Describes a response from an API operation
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class OpenApiResponseAttribute : Attribute
    {
        /// <summary>
        /// The HTTP status code
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// A short description of the response
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The content type of the response
        /// </summary>
        public string ContentType { get; set; } = "application/json";

        /// <summary>
        /// The type that represents the response schema
        /// </summary>
        public Type Type { get; set; }

        public OpenApiResponseAttribute(int statusCode)
        {
            StatusCode = statusCode;
        }
    }

    /// <summary>
    /// Indicates that an operation requires a request body
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class OpenApiRequestBodyAttribute : Attribute
    {
        /// <summary>
        /// Determines if the request body is required
        /// </summary>
        public bool Required { get; set; } = true;

        /// <summary>
        /// The content type of the request body
        /// </summary>
        public string ContentType { get; set; } = "application/json";

        /// <summary>
        /// The type that represents the request body schema
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Description of the request body
        /// </summary>
        public string Description { get; set; }

        public OpenApiRequestBodyAttribute()
        {
        }
    }

    /// <summary>
    /// Describes a parameter for the operation
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class OpenApiParameterAttribute : Attribute
    {
        /// <summary>
        /// The name of the parameter
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The location of the parameter
        /// </summary>
        public ParameterLocation In { get; set; } = ParameterLocation.Path;

        /// <summary>
        /// Determines whether this parameter is mandatory
        /// </summary>
        public bool Required { get; set; } = true;

        /// <summary>
        /// A brief description of the parameter
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The type of the parameter
        /// </summary>
        public Type Type { get; set; } = typeof(string);

        public OpenApiParameterAttribute(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// The location of the parameter
    /// </summary>
    public enum ParameterLocation
    {
        Query,
        Header,
        Path,
        Cookie
    }
}