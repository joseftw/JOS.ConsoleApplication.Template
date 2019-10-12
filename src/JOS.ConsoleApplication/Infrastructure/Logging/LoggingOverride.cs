using Serilog.Events;

namespace JOS.ConsoleApplication.Infrastructure.Logging
{
    public class LoggingOverride
    {
        public string Path { get; set; }
        public LogEventLevel Level { get; set; }
    }
}
