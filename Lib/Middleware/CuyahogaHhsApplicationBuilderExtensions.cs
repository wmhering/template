using System;
using CuyahogaHHS.Middleware;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.AspNetCore.Builder
{
    public static class CuyahogaHhsApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseApiExceptionHandler(this IApplicationBuilder app, Action<ApiExceptionHandlerOptions> configure = null)
        {
            var values = new ApiExceptionHandlerOptions();
            configure?.Invoke(values);
            return app.UseMiddleware<ApiExceptionHandlerMiddleware>(values);
        }

        public static IApplicationBuilder UseCustomAuthorization(this IApplicationBuilder app, Action<CustomAuthorizationOptions> configure)
        {
            var values = new CustomAuthorizationOptions();
            if (configure == null)
                throw new ArgumentNullException(nameof(configure),
                    $"A configuration function is required, ex: app.{nameof(UseCustomAuthorization)}(options => {{ options.ApplicationName = \"myApp\"}})");
            configure.Invoke(values);
            return app.UseMiddleware<CustomAuthorizationMiddleware>(values);
        }

        public static AuthenticationBuilder AddCustomAuthentication(this AuthenticationBuilder builder, Action<CustomAuthenticationHandlerOptions> configureOptions)
        {
            return builder.AddScheme<CustomAuthenticationHandlerOptions, CustomAuthenticationHandlerService>("Custom Scheme", "Custom Auth", configureOptions);
        }
    }
}
