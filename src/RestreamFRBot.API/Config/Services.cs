using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RestreamFRBot.API.HostedServices;
using RestreamFRBot.DAL.Models;
using RestreamFRBot.DiscordBot;
using System.Text.Json.Serialization;

namespace RestreamFRBot.API.Config
{
    public class Services
    {
#pragma warning disable CS8618 // Always set in Program.cs
        public static IServiceProvider Provider { get; set; }
#pragma warning restore CS8618

        public static IServiceCollection ConfigureServices(IServiceCollection services, bool testMode = false)
        {
            services.AddControllers()
                    .AddJsonOptions(jo =>
                    {
                        jo.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                    });

            services.AddDbContext<Context>(o =>
                o.UseSqlite("name=RestreamFR")
            );

            services.AddSingleton<Configuration.Config>();
            services.AddSingleton<Bot>();

#if DEBUG
            if (testMode)
            {
                services.AddSingleton<DiscordBotHostedService>();
                services.AddSingleton<RestreamNotifHostedService>();

                services.AddHostedService<TestHostedService>();
            }
            else
            {
#endif
                services.AddHostedService<DiscordBotHostedService>();
                services.AddHostedService<RestreamNotifHostedService>();
#if DEBUG
            }
#endif

            return services;
        }

        public static void ConfigureApp()
        {
            using (var scope = Provider.CreateScope())
            {
                var conf = scope.ServiceProvider.GetRequiredService<Configuration.Config>();
            }
        }
    }
}
