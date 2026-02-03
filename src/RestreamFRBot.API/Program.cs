using Microsoft.AspNetCore.HttpOverrides;
using Serilog;

namespace RestreamFRBot.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Serilog
            builder.Logging.ClearProviders();
            builder.Services.AddLogging(lb =>
            {
                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(builder.Configuration)
                    .CreateLogger();

                lb.AddSerilog(dispose: true);
            });

            bool testMode = args.Length > 0 && args[0].Trim().ToLower() == "--test";

            Config.Services.ConfigureServices(builder.Services, testMode);

            var app = builder.Build();

            Config.Services.Provider = app.Services;
            Config.Services.ConfigureApp();

            app.UseForwardedHeaders(new ForwardedHeadersOptions() { ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto });
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
