namespace CWE.Modules
{
    using System;
    using System.Threading.Tasks;
    using Discord.Commands;
    using Interactivity;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The general module, containing commands related to campaigns and requests.
    /// </summary>
    public class GeneralModule : CWEModuleBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GeneralModule"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        /// <param name="interactivityService">The <see cref="InteractivityService"/> to inject.</param>
        public GeneralModule(IServiceProvider serviceProvider, IConfiguration configuration, InteractivityService interactivityService)
                : base(serviceProvider, configuration, interactivityService)
        {
        }

        /// <summary>
        /// The command used to determine the latency.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("ping")]
        public async Task Ping()
        {
            await this.ReplyAsync($"Pong! 🏓 `{this.Context.Client.Latency}ms`");
        }
    }
}
