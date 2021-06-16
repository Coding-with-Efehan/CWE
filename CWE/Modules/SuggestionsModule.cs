namespace CWE.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Discord.Commands;
    using Interactivity;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The suggestions module, used to create and manage suggestions.
    /// </summary>
    [Name("Suggestions")]
    public class SuggestionsModule : CWEModuleBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SuggestionsModule"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        /// <param name="interactivityService">The <see cref="InteractivityService"/> to inject.</param>
        public SuggestionsModule(IServiceProvider serviceProvider, IConfiguration configuration, InteractivityService interactivityService)
                : base(serviceProvider, configuration, interactivityService)
        {
        }
    }
}
