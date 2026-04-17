
using System;
using Crestron.SimplSharp.CrestronAuthentication;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using PepperDash.Core.Web.RequestHandlers;

namespace PepperDash.Essentials.Core.Web.RequestHandlers;

/// <summary>
/// Represents a LoginRequestHandler
/// </summary>
public class LoginRequestHandler : WebApiBaseRequestHandler
{
    /// <summary>
    /// Constructor
    /// </summary>
    /// <remarks>
    /// base(true) enables CORS support by default
    /// </remarks>
    public LoginRequestHandler()
        : base(true)
    {
    }

    /// <summary>
    /// Handles POST method requests for user login and token generation
    /// </summary>
    /// <param name="context">The HTTP context for the request.</param>
    protected override void HandlePost(HttpCwsContext context)
    {
        try
        {
            if (context.Request.ContentLength < 0)
            {
                context.Response.StatusCode = 400;
                context.Response.StatusDescription = "Bad Request";
                context.Response.End();

                return;
            }

            var data = context.Request.GetRequestBody();
            if (string.IsNullOrEmpty(data))
            {
                context.Response.StatusCode = 400;
                context.Response.StatusDescription = "Bad Request";
                context.Response.End();

                return;
            }

            var loginRequest = JsonConvert.DeserializeObject<LoginRequest>(data);

            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Username) || string.IsNullOrEmpty(loginRequest.Password))
            {
                context.Response.StatusCode = 400;
                context.Response.StatusDescription = "Bad Request";
                context.Response.End();

                return;
            }

            Authentication.UserToken token;

            try
            {
                token = Authentication.GetAuthenticationToken(loginRequest.Username, loginRequest.Password);
            }
            catch (ArgumentException)
            {
                context.Response.StatusCode = 401;
                context.Response.StatusDescription = "Bad Request";
                context.Response.End();

                return;
            }

            if (!token.Valid)
            {
                context.Response.StatusCode = 401;
                context.Response.StatusDescription = "Unauthorized";
                context.Response.End();

                return;
            }

            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.Write(JsonConvert.SerializeObject(new { Token = token }, Formatting.Indented), false);
            context.Response.End();
        }
        catch (System.Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.StatusDescription = "Internal Server Error";
            context.Response.ContentType = "application/json";
            context.Response.ContentEncoding = System.Text.Encoding.UTF8;
            context.Response.Write(JsonConvert.SerializeObject(new { Error = ex.Message }, Formatting.Indented), false);
            context.Response.End();
        }
    }
}

/// <summary>
/// Represents a LoginRequest
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Gets or sets the username.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Gets or sets the password.
    /// </summary>
    public string Password { get; set; }
}
