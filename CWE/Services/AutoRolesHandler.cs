namespace CWE.Services
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using CWE.Data;
    using Discord;
    using Discord.WebSocket;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The handler responsible for auto-roles.
    /// </summary>
    public class AutoRolesHandler : CWEService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoRolesHandler"/> class.
        /// </summary>
        /// <param name="client">The <see cref="DiscordSocketClient"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        /// <param name="logger">The <see cref="ILogger"/> to inject.</param>
        /// <param name="dataAccessLayer">The <see cref="DataAccessLayer"/> to inject.</param>
        public AutoRolesHandler(
            DiscordSocketClient client,
            IConfiguration configuration,
            ILogger<AutoRolesHandler> logger,
            DataAccessLayer dataAccessLayer)
            : base(client, logger, configuration, dataAccessLayer)
        {
        }

        /// <inheritdoc/>
        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Client.UserJoined += OnUserJoined;
            return Task.CompletedTask;
        }

        private Task OnUserJoined(SocketGuildUser user)
        {
            Task.Run(async () =>
            {
                var guild = Client.GetGuild(Configuration.GetValue<ulong>("Guild"));
                var autoRoles = await DataAccessLayer.GetAutoRoles();
                var roles = new List<IRole>();
                foreach (var autoRole in autoRoles)
                {
                    var currentRole = guild.GetRole(autoRole.Id);
                    if (currentRole == null)
                    {
                        await DataAccessLayer.DeleteAutoRole(autoRole.Id);
                        continue;
                    }

                    roles.Add(currentRole);
                }

                await user.AddRolesAsync(roles);
            });
            return Task.CompletedTask;
        }
    }
}
