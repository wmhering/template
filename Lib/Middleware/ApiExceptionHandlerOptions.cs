using Microsoft.Extensions.Logging;

namespace CuyahogaHHS.Middleware
{
    public class ApiExceptionHandlerOptions
    {
        public LogLevel LogLevel { get; set; } = LogLevel.Error;

        public string[] Message { get; set; } = new string[] {
            "An unexpected error has occured and been logged.",
            "If the problem persist please contact help desk." };
    }
}
