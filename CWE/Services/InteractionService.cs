namespace CWE.Services
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Discord.Addons.Hosting;
    using Discord.WebSocket;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Represents a service used to get Interactions.
    /// </summary>
    public class InteractionService : InitializedService
    {
        private readonly DiscordSocketClient client;

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionService"/> class.
        /// </summary>
        /// <param name="client">The <see cref="DiscordSocketClient"/> to inject.</param>
        public InteractionService(DiscordSocketClient client)
        {
            this.client = client;
        }

        /// <inheritdoc/>
        public override Task InitializeAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Retrieves the next incoming Message component interaction that passes the <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter">The <see cref="Predicate{SocketMessageComponent}"/> which the component has to pass.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the request.</param>
        /// <returns>The <see cref="SocketMessageComponent"/> that matches the provided filter.</returns>
        public static async Task<SocketMessageComponent> NextButtonAsync(Predicate<SocketMessageComponent> filter = null, CancellationToken cancellationToken = default)
        {
            filter ??= m => true;

            var cancelSource = new TaskCompletionSource<bool>();
            var componentSource = new TaskCompletionSource<SocketMessageComponent>();
            var cancellationRegistration = cancellationToken.Register(() => cancelSource.SetResult(true));

            var componentTask = componentSource.Task;
            var cancelTask = cancelSource.Task;

            Task CheckComponent(SocketMessageComponent comp)
            {
                if (filter.Invoke(comp))
                {
                    componentSource.SetResult(comp);
                }

                return Task.CompletedTask;
            }

            Task HandleInteraction(SocketInteraction arg)
            {
                if (arg is SocketMessageComponent comp)
                {
                    return CheckComponent(comp);
                }

                return Task.CompletedTask;
            }

            try
            {
                this.client.InteractionCreated += HandleInteraction;

                var result = await Task.WhenAny(componentTask, cancelTask).ConfigureAwait(false);

                return result == componentTask
                    ? await componentTask.ConfigureAwait(false)
                    : null;
            }
            finally
            {
                this.client.InteractionCreated -= HandleInteraction;
                cancellationRegistration.Dispose();
            }
        }

        /// <summary>
        /// Retrieves the next incoming Slash command interaction that passes the <paramref name="filter"/>.
        /// </summary>
        /// <param name="filter">The <see cref="Predicate{SocketSlashCommand}"/> which the component has to pass.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the request.</param>
        /// <returns>The <see cref="SocketSlashCommand"/> that matches the provided filter.</returns>
        public async Task<SocketSlashCommand> NextSlashCommandAsync(Predicate<SocketSlashCommand> filter = null, CancellationToken cancellationToken = default)
        {
            filter ??= m => true;

            var cancelSource = new TaskCompletionSource<bool>();
            var slashcommandSource = new TaskCompletionSource<SocketSlashCommand>();
            var cancellationRegistration = cancellationToken.Register(() => cancelSource.SetResult(true));

            var slashcommandTask = slashcommandSource.Task;
            var cancelTask = cancelSource.Task;

            Task CheckCommand(SocketSlashCommand comp)
            {
                if (filter.Invoke(comp))
                {
                    slashcommandSource.SetResult(comp);
                }

                return Task.CompletedTask;
            }

            Task HandleInteraction(SocketInteraction arg)
            {
                if (arg is SocketSlashCommand comp)
                {
                    return CheckCommand(comp);
                }

                return Task.CompletedTask;
            }

            try
            {
                this.client.InteractionCreated += HandleInteraction;

                var result = await Task.WhenAny(slashcommandTask, cancelTask).ConfigureAwait(false);

                return result == slashcommandTask
                    ? await slashcommandTask.ConfigureAwait(false)
                    : null;
            }
            finally
            {
                this.client.InteractionCreated -= HandleInteraction;
                cancellationRegistration.Dispose();
            }
        }
    }
}
