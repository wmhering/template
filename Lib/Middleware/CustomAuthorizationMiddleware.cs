using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace CuyahogaHHS.Middleware
{
    public class CustomAuthorizationMiddleware
    {
        private readonly RequestDelegate _Next;
        private readonly ILogger<CustomAuthorizationMiddleware> _Logger;
        private readonly string _ApplicationName;
        private readonly string _ServiceUrl;

        public CustomAuthorizationMiddleware(RequestDelegate next, ILogger<CustomAuthorizationMiddleware> logger, CustomAuthorizationOptions options)
        {
            _Next = next;
            _Logger = logger;
            _ApplicationName = options.ApplicationName;
            _ServiceUrl = options.ServiceUrl;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (!context.User.Identity.IsAuthenticated)
            {
                await _Next(context);
                return;
            }
            string value;
            if (!context.Request.Cookies.TryGetValue("MyCookie", out value))
                value = "MyRole";
            var claim = new Claim(ClaimTypes.Role, value);
            var identity = new ClaimsIdentity(new Claim[] { claim });
            context.User.AddIdentity(identity);
            var authenticated = context.User.Identity.IsAuthenticated;
            await _Next(context);
            context.Response.Cookies.Append("MyCookie", value, new CookieOptions { MaxAge = TimeSpan.FromMinutes(20) });
        }
    }
}
