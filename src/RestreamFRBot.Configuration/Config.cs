using Microsoft.Extensions.Configuration;

namespace RestreamFRBot.Configuration
{
    public class Config
    {
        public Config(IConfiguration conf)
        {
            Discord = conf.GetSection("Discord").Get<DiscordConfig>() ?? new DiscordConfig();
            RestreamModules = conf.GetSection("RestreamModules").Get<List<RestreamModuleConfig>>() ?? new List<RestreamModuleConfig>();
        }

        public DiscordConfig Discord { get; set; }
        public List<RestreamModuleConfig> RestreamModules { get; set; }
    }
}
