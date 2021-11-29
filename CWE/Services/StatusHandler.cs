namespace CWE.Services
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using CWE.Data;
    using Discord;
    using Discord.WebSocket;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The handler responsible for the status of CWE.
    /// </summary>
    public class StatusHandler : CWEService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StatusHandler"/> class.
        /// </summary>
        /// <param name="client">The <see cref="DiscordSocketClient"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        /// <param name="logger">The <see cref="ILogger"/> to inject.</param>
        /// <param name="dataAccessLayer">The <see cref="DataAccessLayer"/> to inject.</param>
        public StatusHandler(
            DiscordSocketClient client,
            IConfiguration configuration,
            ILogger<StatusHandler> logger,
            DataAccessLayer dataAccessLayer)
            : base(client, logger, configuration, dataAccessLayer)
        {
        }

        /// <inheritdoc/>
        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Client.Ready += OnReady;
            return Task.CompletedTask;
        }

        private Task OnReady()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    var guild = Client.GetGuild(Configuration.GetValue<ulong>("Guild"));
                    await Client.SetGameAsync($"{guild.Users.Where(x => !x.IsBot).Count()} programmers", null, ActivityType.Watching);
                    await Task.Delay(TimeSpan.FromMinutes(5));
                }
            });
            return Task.CompletedTask;
        }
    }
}
