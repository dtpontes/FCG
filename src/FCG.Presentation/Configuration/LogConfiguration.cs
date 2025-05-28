using Serilog;

namespace FCG.Presentation.Configuration
{
    public static class LogConfiguration
    {
        public static void ConfigureSerilog(this IHostBuilder hostBuilder)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .Enrich.FromLogContext()
                .CreateLogger();

            hostBuilder.UseSerilog();
        }
    }
}
