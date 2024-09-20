using System.Text;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RestreamFRBot.DiscordBot
{
    public class Bot
    {
        public Bot(
                Configuration.Config config,
                IServiceScopeFactory scopeFactory,
                ILogger<Bot> logger
            )
        {
            Config = config;
            ScopeFactory = scopeFactory;
            DiscordSocketConfig socketConf = new() { GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers };
            Client = new DiscordSocketClient(socketConf);
            Logger = logger;
        }

        private Configuration.Config Config { get; set; }
        private DiscordSocketClient Client { get; set; }
        private IServiceScopeFactory ScopeFactory { get; set; }
        private ILogger<Bot> Logger { get; set; }
        private SocketGuild? Guild { get; set; }

        public async Task Start()
        {
            await Start(CancellationToken.None);
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            await Client.LoginAsync(TokenType.Bot, Config.Discord.BotToken);
            Client.Ready += Client_Ready;
            Client.SlashCommandExecuted += Client_SlashCommandHandler;
            Client.Log += Client_Log;

            await Client.StartAsync();
            await Task.Delay(-1).WaitAsync(cancellationToken);
            await Client.StopAsync();
            await Client.LogoutAsync();
        }

        public async Task Reset()
        {
            try
            {
                await Client.StopAsync();
            }
            catch (Exception) { }

            try
            {
                await Client.LogoutAsync();
                Client.Ready -= Client_Ready;
                Client.SlashCommandExecuted -= Client_SlashCommandHandler;
                Client.Log -= Client_Log;
            }
            catch (Exception) { }

            DiscordSocketConfig socketConf = new() { GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers };
            Client = new DiscordSocketClient(socketConf);
        }

        private async Task Client_Ready()
        {
            Guild = Client.GetGuild(Config.Discord.BotServer);
            if (Guild != null)
            {
                await Guild.DownloadUsersAsync();
            }
        }

        private Task Client_Log(LogMessage log)
        {
            if (log.Exception != null)
            {
                Logger.LogError(log.Exception, log.Message);
            }

            return Task.CompletedTask;
        }

        public Task RegisterCommands()
        {
            return Task.CompletedTask;
        }

        private async Task Client_SlashCommandHandler(SocketSlashCommand cmd)
        {
            await cmd.DeferAsync();
        }

        public async Task ForceGuildUsersRefresh()
        {
            if (Guild != null)
            {
                await Guild.DownloadUsersAsync();
            }
        }

        private ulong? FindGuildUser(string discordName)
        {
            discordName = discordName.ToLower();
            return Guild?.Users?.Where(u => u.Username.ToLower() == discordName).Select(u => (ulong?)u.Id).FirstOrDefault();
        }

        public async Task<bool> SendRestreamNotif(string type, string round, string matchup, string host, string cohost, DateTime date, ulong channelId)
        {
            // Retrieve user id
            ulong? hostId = FindGuildUser(host);
            ulong? cohostId = FindGuildUser(cohost);

            if (hostId == null || cohostId == null || channelId == 0)
            {
                return false;
            }

            int unixIimeStamp = (int)(date - DateTime.UnixEpoch).TotalSeconds;

            StringBuilder sb = new StringBuilder();
            sb.Append($"**{matchup}**\n");
            sb.Append($"> *{type}*\n");
            sb.Append($"> {round}\n");
            sb.Append($"> Date : <t:{unixIimeStamp}>\n");
            sb.Append($"> Host par <@{hostId}> et <@{cohostId}>\n");

            IMessageChannel? chan = await Client.GetChannelAsync(channelId) as IMessageChannel;
            if (chan == null)
            {
                return false;
            }

            await chan.SendMessageAsync(sb.ToString());

            return true;
        }
    }
}
