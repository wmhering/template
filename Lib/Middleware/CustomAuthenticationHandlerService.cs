using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace CuyahogaHHS.Middleware
{
    public class CustomAuthenticationHandlerService : AuthenticationHandler<CustomAuthenticationHandlerOptions>
    {
        public CustomAuthenticationHandlerService(IOptionsMonitor<CustomAuthenticationHandlerOptions> options, ILoggerFactory loggerFactory, UrlEncoder encoder, ISystemClock clock) : base(options, loggerFactory, encoder, clock)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            await Task.Run(() => { });
            var result = AuthenticateResult.NoResult();
            return result;
        }
    }
}
