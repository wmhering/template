using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CuyahogaHHS.Middleware
{
    public class ApiExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _Next;
        private readonly ILogger<ApiExceptionHandlerMiddleware> _Logger;
        private readonly LogLevel _LogLevel;
        private string[] _Message;

        public ApiExceptionHandlerMiddleware(RequestDelegate next, ILogger<ApiExceptionHandlerMiddleware> logger, ApiExceptionHandlerOptions options)
        {
            _Next = next;
            _Logger = logger;
            _LogLevel = options.LogLevel;
            _Message = options.Message;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _Next(context);
            }
            catch (Exception ex)
            {

                _Logger.Log(_LogLevel, ex, "An unexpected exception has been thrown.");
                // Once response has been started the only thing left to do is default processing.
                if (context.Response.HasStarted)
                {
                    _Logger.Log(_LogLevel, "Unable to handle exception in middleware.");
                    throw;
                }
                context.Response.StatusCode = 500;
                context.Response.Headers.Clear();
                context.Response.Headers.AppendCommaSeparatedValues("Content-Type", "application/json");
                context.Response.Headers.AppendCommaSeparatedValues("Content-Language", "en-US");
                using (var writer = new StreamWriter(context.Response.Body, Encoding.UTF8, bufferSize: 1024, leaveOpen: true))
                {
                    writer.Write(JsonConvert.SerializeObject(new { message = _Message }));
                }
            }
            return;
        }
    }
}
