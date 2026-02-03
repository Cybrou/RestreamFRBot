
namespace RestreamFRBot.API.HostedServices
{
#if DEBUG
    public class TestHostedService : BackgroundService
    {
        public TestHostedService(
            RestreamNotifHostedService testRestreamNotif
        )
        {
            RestreamNotifHostedService = testRestreamNotif;
        }

        private RestreamNotifHostedService RestreamNotifHostedService;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await RestreamNotifHostedService.CronExecuteAsync(stoppingToken);
        }
    }
#endif
}
