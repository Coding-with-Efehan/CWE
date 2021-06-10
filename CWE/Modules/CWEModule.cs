namespace CWE.Modules
{
    using System;
    using CWE.Data;
    using Discord.Addons.Interactive;
    using Discord.Commands;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Custom implementation of <see cref="InteractiveBase"/>, containing extra's for CWE.
    /// </summary>
    public abstract class CWEModule : InteractiveBase<SocketCommandContext>
    {
        /// <summary>
        /// The <see cref="DataAccessLayer"/> of CWE.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Field used via inheritance.")]
        public readonly DataAccessLayer DataAccessLayer;

        /// <summary>
        /// The <see cref="IConfiguration"/> of CWE.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Field used via inheritance.")]
        public readonly IConfiguration Configuration;

        private readonly IServiceScope scope;

        /// <summary>
        /// Initializes a new instance of the <see cref="CWEModule"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to be injected.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to be injected.</param>
        protected CWEModule(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            this.scope = serviceProvider.CreateScope();
            this.DataAccessLayer = this.scope.ServiceProvider.GetRequiredService<DataAccessLayer>();

            this.Configuration = configuration;
        }
    }
}
