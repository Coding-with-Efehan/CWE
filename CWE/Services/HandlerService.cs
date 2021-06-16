namespace CWE.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Discord.Addons.Hosting;
    using Discord.WebSocket;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    ///     A service used to manage and instagate handlers.
    /// </summary>
    public class HandlerService : InitializedService
    {
        private static readonly Dictionary<DiscordHandler, object> Handlers = new Dictionary<DiscordHandler, object>();
        private readonly DiscordSocketClient client;
        private readonly IServiceProvider provider;
        private readonly IConfiguration configuration;
        private readonly ILogger<HandlerService> logger;

        /// <summary>
        ///     Initializes a new instance of the <see cref="HandlerService"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="IServiceProvider"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        /// <param name="client">The <see cref="DiscordSocketClient"/> to inject.</param>
        /// <param name="logger">The <see cref="ILogger"/> to inject.</param>
        public HandlerService(IServiceProvider provider, IConfiguration configuration, DiscordSocketClient client, ILogger<HandlerService> logger)
        {
            this.client = client;
            this.provider = provider;
            this.configuration = configuration;
            this.logger = logger;

            client.Ready += this.Client_Ready;
        }

        /// <summary>
        ///     Gets a handler with the specified type.
        /// </summary>
        /// <typeparam name="T">The type of the handler to get</typeparam>
        /// <returns>The handler with the type of <typeparamref name="T"/>. If no handler is found then <see langword="null"/></returns>
        public static T GetHandlerInstance<T>()
            where T : DiscordHandler => Handlers.FirstOrDefault(x => x.Key.GetType() == typeof(T)).Value as T;

        /// <inheritdoc/>
        public override Task InitializeAsync(CancellationToken cancellationToken)
        {
            List<Type> typs = new List<Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsAssignableTo(typeof(DiscordHandler)) && type != typeof(DiscordHandler))
                    {
                        // add to a cache.
                        typs.Add(type);
                    }
                }
            }
            foreach (var handler in typs)
            {
                var inst = Activator.CreateInstance(handler);
                Handlers.Add(inst as DiscordHandler, inst);
            }

            return Task.CompletedTask;
        }

        private Task Client_Ready()
        {
            _ = Task.Run(() =>
            {
                var work = new List<Func<Task>>();

                foreach (var item in Handlers)
                {
                    work.Add(async () =>
                    {
                        try
                        {
                            await item.Key.InitializeAsync(this.client, this.provider, this.configuration);
                            item.Key.Initialize(this.client, this.provider, this.configuration);
                        }
                        catch (Exception x)
                        {
                            this.logger.LogError($"Exception occured while initializing {item.Key.GetType().Name}: ", x);
                        }
                    });
                }

                Task.WaitAll(work.Select(x => x()).ToArray());
            });

            return Task.CompletedTask;
        }
    }

    /// <summary>
    ///     Marks the current class as a handler
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:File may only contain a single type", Justification = "Directly related to the handler service.")]
    public abstract class DiscordHandler
    {
        /// <summary>
        ///     Intitialized this handler asynchronously.
        /// </summary>
        /// <param name="client">The <see cref="DiscordSocketClient"/> to inject.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        /// <returns>A task representing the asynchronous operation of initializing this handler.</returns>
        public virtual Task InitializeAsync(DiscordSocketClient client, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Intitialized this handler.
        /// </summary>
        /// <param name="client">The <see cref="DiscordSocketClient"/> to inject.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        public virtual void Initialize(DiscordSocketClient client, IServiceProvider serviceProvider, IConfiguration configuration)
        {
        }
    }
}
