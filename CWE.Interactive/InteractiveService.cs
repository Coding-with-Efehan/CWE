namespace CWE.Interactive
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;

    /// <summary>
    /// The service that implements all methods like NextMessageAsync.
    /// </summary>
    public class InteractiveService : IDisposable
    {
        /// <summary>
        /// Dictionary containing all <see cref="IReactionCallback"/>s.
        /// </summary>
        private readonly Dictionary<ulong, IReactionCallback> callbacks;

        /// <summary>
        /// The default timeout timespan.
        /// </summary>
        private TimeSpan defaultTimeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveService"/> class for a <see cref="DiscordSocketClient"/>.
        /// </summary>
        /// <param name="discord">The <see cref="DiscordSocketClient"/> to be injected.</param>
        /// <param name="config">The <see cref="InteractiveServiceConfig"/> to be injected.</param>
        public InteractiveService(DiscordSocketClient discord, InteractiveServiceConfig config = null)
            : this((BaseSocketClient)discord, config)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveService"/> class for a <see cref="DiscordShardedClient"/>.
        /// </summary>
        /// <param name="discord">The <see cref="DiscordShardedClient"/> to be injected.</param>
        /// <param name="config">The <see cref="InteractiveServiceConfig"/> to be injected.</param>
        public InteractiveService(DiscordShardedClient discord, InteractiveServiceConfig config = null)
            : this((BaseSocketClient)discord, config)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InteractiveService"/> class.
        /// </summary>
        /// <param name="discord">The <see cref="BaseSocketClient"/> to be injected.</param>
        /// <param name="config">The <see cref="InteractiveServiceConfig"/> to be injected.</param>
        public InteractiveService(BaseSocketClient discord, InteractiveServiceConfig config = null)
        {
            this.Discord = discord;
            this.Discord.ReactionAdded += this.HandleReactionAsync;

            config = config ?? new InteractiveServiceConfig();
            this.defaultTimeout = config.DefaultTimeout;

            this.callbacks = new Dictionary<ulong, IReactionCallback>();
        }

        /// <summary>
        /// Gets the Discord <see cref="BaseSocketClient"/>.
        /// </summary>
        public BaseSocketClient Discord { get; }

        /// <inheritdoc/>
        public void Dispose()
        {
            this.Discord.ReactionAdded -= this.HandleReactionAsync;
        }

        /// <summary>
        /// Wait for the next message asynchronously.
        /// </summary>
        /// <param name="context">The <see cref="SocketCommandContext"/> to be used.</param>
        /// <param name="fromSourceUser">Whether or not the message should be from the source user.</param>
        /// <param name="inSourceChannel">Whether or not the message should originate from the source channel.</param>
        /// <param name="timeout">The timeout until the method stops listening for new messages.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A <see cref="SocketMessage"/> of the message meeting the criteria.</returns>
        public Task<SocketMessage> NextMessageAsync(
            SocketCommandContext context,
            bool fromSourceUser = true,
            bool inSourceChannel = true,
            TimeSpan? timeout = null,
            CancellationToken token = default(CancellationToken))
        {
            var criterion = new Criteria<SocketMessage>();
            if (fromSourceUser)
            {
                criterion.AddCriterion(new EnsureSourceUserCriterion());
            }

            if (inSourceChannel)
            {
                criterion.AddCriterion(new EnsureSourceChannelCriterion());
            }

            return this.NextMessageAsync(context, criterion, timeout, token);
        }

        /// <summary>
        /// Wait for the next message asynchronously.
        /// </summary>
        /// <param name="context">The <see cref="SocketCommandContext"/> to be used.</param>
        /// <param name="criterion">The criterion attached to the <see cref="SocketMessage"/>.</param>
        /// <param name="timeout">The timeout until the method stops listening for new messages.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A <see cref="SocketMessage"/> of the message meeting the criteria.</returns>
        public async Task<SocketMessage> NextMessageAsync(
            SocketCommandContext context,
            ICriterion<SocketMessage> criterion,
            TimeSpan? timeout = null,
            CancellationToken token = default(CancellationToken))
        {
            timeout = timeout ?? this.defaultTimeout;

            var eventTrigger = new TaskCompletionSource<SocketMessage>();
            var cancelTrigger = new TaskCompletionSource<bool>();

            token.Register(() => cancelTrigger.SetResult(true));

            async Task Handler(SocketMessage message)
            {
                var result = await criterion.JudgeAsync(context, message).ConfigureAwait(false);
                if (result)
                {
                    eventTrigger.SetResult(message);
                }
            }

            context.Client.MessageReceived += Handler;

            var trigger = eventTrigger.Task;
            var cancel = cancelTrigger.Task;
            var delay = Task.Delay(timeout.Value);
            var task = await Task.WhenAny(trigger, delay, cancel).ConfigureAwait(false);

            context.Client.MessageReceived -= Handler;

            if (task == trigger)
            {
                return await trigger.ConfigureAwait(false);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Reply to a message and delete the response when the timeout has passed.
        /// </summary>
        /// <param name="context">The <see cref="SocketCommandContext"/> to be used.</param>
        /// <param name="content">The content of the message.</param>
        /// <param name="isTTS">Whether or not the message should be TTS.</param>
        /// <param name="embed">The embed that should be attached with the message.</param>
        /// <param name="timeout">The timeout of when the message will be deleted.</param>
        /// <param name="options">Any additional <see cref="RequestOptions"/>.</param>
        /// <returns>The <see cref="IUserMessage"/> that was sent.</returns>
        public async Task<IUserMessage> ReplyAndDeleteAsync(
            SocketCommandContext context,
            string content,
            bool isTTS = false,
            Embed embed = null,
            TimeSpan? timeout = null,
            RequestOptions options = null)
        {
            timeout = timeout ?? this.defaultTimeout;
            var message = await context.Channel.SendMessageAsync(content, isTTS, embed, options).ConfigureAwait(false);
            _ = Task.Delay(timeout.Value)
                .ContinueWith(_ => message.DeleteAsync().ConfigureAwait(false))
                .ConfigureAwait(false);
            return message;
        }

        /// <summary>
        /// Send a paginated message.
        /// </summary>
        /// <param name="context">The <see cref="SocketCommandContext"/> to be used.</param>
        /// <param name="paginator">The paginator to be sent.</param>
        /// <param name="criterion">The criterion attached to the <see cref="SocketReaction"/>.</param>
        /// <returns>The <see cref="IUserMessage"/> that was sent.</returns>
        public async Task<IUserMessage> SendPaginatedMessageAsync(
            SocketCommandContext context,
            PaginatedMessage paginator,
            ICriterion<SocketReaction> criterion = null)
        {
            var callback = new PaginatedMessageCallback(this, context, paginator, criterion);
            await callback.DisplayAsync().ConfigureAwait(false);
            return callback.Message;
        }

        /// <summary>
        /// Add a <see cref="IReactionCallback"/> to the list of callbacks, linked to a <see cref="IMessage"/>.
        /// </summary>
        /// <param name="message">The message that the callback should be attached to.</param>
        /// <param name="callback">The callback that should be attached to the message.</param>
        public void AddReactionCallback(IMessage message, IReactionCallback callback)
            => this.callbacks[message.Id] = callback;

        /// <summary>
        /// Remove a <see cref="IReactionCallback"/> from the list of callbacks.
        /// </summary>
        /// <param name="message">The message that the callback is attached to.</param>
        public void RemoveReactionCallback(IMessage message)
            => this.RemoveReactionCallback(message.Id);

        /// <summary>
        /// Remove a <see cref="IReactionCallback"/> from the list of callbacks.
        /// </summary>
        /// <param name="id">The ID of the message that the callback is attached to.</param>
        public void RemoveReactionCallback(ulong id)
            => this.callbacks.Remove(id);

        /// <summary>
        /// Clear the list of <see cref="IReactionCallback"/>s.
        /// </summary>
        public void ClearReactionCallbacks()
            => this.callbacks.Clear();

        private async Task HandleReactionAsync(
            Cacheable<IUserMessage, ulong> message,
            ISocketMessageChannel channel,
            SocketReaction reaction)
        {
            if (reaction.UserId == this.Discord.CurrentUser.Id)
            {
                return;
            }

            if (!this.callbacks.TryGetValue(message.Id, out var callback))
            {
                return;
            }

            if (!(await callback.Criterion.JudgeAsync(callback.Context, reaction).ConfigureAwait(false)))
            {
                return;
            }

            switch (callback.RunMode)
            {
                case RunMode.Async:
                    _ = Task.Run(async () =>
                    {
                        if (await callback.HandleCallbackAsync(reaction).ConfigureAwait(false))
                        {
                            this.RemoveReactionCallback(message.Id);
                        }
                    });
                    break;
                default:
                    if (await callback.HandleCallbackAsync(reaction).ConfigureAwait(false))
                    {
                        this.RemoveReactionCallback(message.Id);
                    }

                    break;
            }
        }
    }
}
