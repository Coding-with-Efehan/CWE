namespace CWE.Modules
{
    using System;
    using CWE.Data;
    using Discord.Commands;
    using Interactivity;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Custom implementation of <see cref="ModuleBase"/>, containing extra's for CWE.
    /// </summary>
    public abstract class CWEModuleBase : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// The <see cref="DataAccessLayer"/> of CWE.
        /// </summary>
        public readonly DataAccessLayer DataAccessLayer;

        /// <summary>
        /// The <see cref="InteractivityService"/> used for interactivity.
        /// </summary>
        public readonly InteractivityService Interactivity;

        /// <summary>
        /// The <see cref="IConfiguration"/> of CWE.
        /// </summary>
        public readonly IConfiguration Configuration;

        private readonly IServiceScope scope;

        /// <summary>
        /// Initializes a new instance of the <see cref="CWEModuleBase"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to be injected.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to be injected.</param>
        /// <param name="interactivityService">The <see cref="InteractivityService"/> to be injected.</param>
        protected CWEModuleBase(IServiceProvider serviceProvider, IConfiguration configuration, InteractivityService interactivityService)
        {
            scope = serviceProvider.CreateScope();
            DataAccessLayer = scope.ServiceProvider.GetRequiredService<DataAccessLayer>();

            Configuration = configuration;
            Interactivity = interactivityService;
        }
    }
}
