using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
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
            // If there is a roles cookie and the user is au
            await _Next(context);
        }
    }
}
