using System;
using CuyahogaHHS.Middleware;
using Microsoft.AspNetCore.Authentication;

namespace Microsoft.AspNetCore.Builder
{
    public static class CuyahogaHhsApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseApiExceptionHandler(this IApplicationBuilder app, Action<ApiExceptionHandlerOptions> options = null)
        {
            var values = new ApiExceptionHandlerOptions();
            if (options != null)
                options.Invoke(values);
            return app.UseMiddleware<ApiExceptionHandlerMiddleware>(values);
        }

        public static AuthenticationBuilder AddCustomAuthentication(this AuthenticationBuilder builder, Action<CustomAuthenticationHandlerOptions> configureOptions)
        {
            return builder.AddScheme<CustomAuthenticationHandlerOptions, CustomAuthenticationHandlerService>("Custom Scheme", "Custom Auth", configureOptions);
        }
    }
}
