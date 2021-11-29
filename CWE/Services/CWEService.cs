namespace CWE.Services
{
    using CWE.Data;
    using Discord.Addons.Hosting;
    using Discord.WebSocket;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Custom implementation of <see cref="DiscordClientService"/> for CWE.
    /// </summary>
    public abstract class CWEService : DiscordClientService
    {
        /// <summary>
        /// The <see cref="IConfiguration"/> of CWE.
        /// </summary>
        public readonly IConfiguration Configuration;

        /// <summary>
        /// The <see cref="DataAccessLayer"/> of CWE.
        /// </summary>
        public readonly DataAccessLayer DataAccessLayer;

        /// <summary>
        /// Initializes a new instance of the <see cref="CWEService"/> class.
        /// </summary>
        /// <param name="client">The <see cref="DiscordSocketClient"/> to inject.</param>
        /// <param name="logger">The <see cref="ILogger{T}"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        /// <param name="dataAccessLayer">The <see cref="DataAccessLayer"/> to inject.</param>
        public CWEService(DiscordSocketClient client, ILogger<DiscordClientService> logger, IConfiguration configuration, DataAccessLayer dataAccessLayer)
            : base(client, logger)
        {
            Configuration = configuration;
            DataAccessLayer = dataAccessLayer;
        }
    }
}
