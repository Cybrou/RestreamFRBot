using CsvHelper.Configuration;
using CsvHelper;
using RestreamFRBot.DAL.Models;
using RestreamFRBot.DiscordBot;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace RestreamFRBot.API.HostedServices
{
    public class RestreamNotifHostedService : CronBackgroundService
    {
        public RestreamNotifHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<RestreamNotifHostedService> logger)
        {
            ScopeFactory = scopeFactory;
            Logger = logger;
            CookieContainer = new CookieContainer();
        }

        private IServiceScopeFactory ScopeFactory { get; set; }
        private ILogger<RestreamNotifHostedService> Logger { get; set; }
        private CookieContainer CookieContainer { get; set; }

        protected override string GetCronExpression() => "*/5 * * * *";

        public async Task Debug()
        {
            await CronExecuteAsync(CancellationToken.None);
        }

        protected override async Task CronExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                Logger.LogInformation("Check restream notifications");

                using (var scope = ScopeFactory.CreateScope())
                {
                    var config = scope.ServiceProvider.GetRequiredService<Configuration.Config>();
                    var db = scope.ServiceProvider.GetRequiredService<Context>();
                    var bot = scope.ServiceProvider.GetRequiredService<Bot>();

                    await bot.ForceGuildUsersRefresh();

                    // Force some reasons Google Server respond with a Bad Request when redirect is done too quickly
                    // so we process the redirect manually with a delay
                    HttpClientHandler handler = new HttpClientHandler() { CookieContainer = CookieContainer, AllowAutoRedirect = false, UseCookies = true };
                    using HttpClient http = new HttpClient(handler, true);
                    http.DefaultRequestHeaders.UserAgent.Clear();
                    http.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:130.0) Gecko/20100101 Firefox/130.0");

                    foreach (var resteamModule in config.RestreamModules)
                    {
                        // Download sheet data
                        HttpRequestMessage req = new HttpRequestMessage(HttpMethod.Get, resteamModule.SheetUri);
                        HttpResponseMessage resp = await http.SendAsync(req);

                        if (resp.StatusCode == HttpStatusCode.TemporaryRedirect && resp.Headers.Location != null)
                        {
                            string newUri = resp.Headers.Location.ToString();
                            resp.Dispose();
                            req.Dispose();

                            await Task.Delay(5000);
                            req = new HttpRequestMessage(HttpMethod.Get, newUri);
                            resp = await http.SendAsync(req);
                        }

                        if (!resp.IsSuccessStatusCode)
                        {
                            Logger.LogError("Error while downloading sheet data.");

                            resp.Dispose();
                            req.Dispose();

                            return;
                        }

                        using StreamReader sr = new StreamReader(await resp.Content.ReadAsStreamAsync(), System.Text.Encoding.UTF8);
                        CsvConfiguration csvConfig = new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture) { Delimiter = ",", Escape = '"', NewLine = "\r\n" };
                        using CsvReader csv = new CsvReader(sr, csvConfig);
                        while (await csv.ReadAsync())
                        {
                            string guid = csv.GetField(0) ?? "";
                            bool isRestream = csv.GetField(8)?.ToLower() == "true";
                            string strDatetime = csv.GetField(16) ?? "";

                            if (!string.IsNullOrWhiteSpace(guid)
                                && isRestream
                                && DateTime.TryParseExact(strDatetime, "dd/MM/yyyy HH:mm:ss", null, System.Globalization.DateTimeStyles.AssumeUniversal, out DateTime dateTime)
                                && dateTime >= resteamModule.MinDate
                                && !(await db.RestreamNotifs.AnyAsync(r => r.RestreamModuleId == resteamModule.ModuleId && r.Guid == guid)))
                            {
                                dateTime = dateTime.ToUniversalTime();

                                string type = csv.GetField(2) ?? "";
                                string matchtup = csv.GetField(3) ?? "";
                                string round = csv.GetField(4) ?? "";
                                string host = TrimBlank(csv.GetField(14)) ?? "";
                                string cohost = TrimBlank(csv.GetField(15)) ?? "";

                                // Send notif
                                if (await bot.SendRestreamNotif(type, round, matchtup, host, cohost, dateTime))
                                {
                                    // Save in bdd
                                    RestreamNotif newNotif = new RestreamNotif() { RestreamModuleId = resteamModule.ModuleId, Guid = guid, SentDate = DateTime.UtcNow };
                                    await db.RestreamNotifs.AddAsync(newNotif);
                                    await db.SaveChangesAsync();
                                }
                            }
                        }

                        resp.Dispose();
                        req.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error while processing restream norifications.");
            }
        }

        private static string? TrimBlank(string? str)
        {
            if (str == null)
            {
                return null;
            }

            return str.Trim(' ', '\t', '\r', '\n');
        }
    }
}
