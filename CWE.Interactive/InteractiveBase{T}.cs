namespace CWE.Interactive
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;

    /// <summary>
    /// Implementation of <see cref="InteractiveBase"/> for <see cref="ModuleBase"/> where the <see cref="T"/> is <see cref="SocketCommandContext"/>.
    /// </summary>
    /// <typeparam name="T">The <see cref="T"/>.</typeparam>
    public abstract class InteractiveBase<T> : ModuleBase<T>
        where T : SocketCommandContext
    {
        /// <summary>
        /// Gets or sets the <see cref="InteractiveService"/>.
        /// </summary>
        public InteractiveService Interactive { get; set; }

        /// <summary>
        /// Wait for the next message asynchronously.
        /// </summary>
        /// <param name="criterion">The criterion attached to the <see cref="SocketMessage"/>.</param>
        /// <param name="timeout">The timeout until the method stops listening for new messages.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A <see cref="SocketMessage"/> of the message meeting the criteria.</returns>
        public Task<SocketMessage> NextMessageAsync(ICriterion<SocketMessage> criterion, TimeSpan? timeout = null, CancellationToken token = default(CancellationToken))
        => this.Interactive.NextMessageAsync(this.Context, criterion, timeout, token);

        /// <summary>
        /// Wait for the next message asynchronously.
        /// </summary>
        /// <param name="fromSourceUser">Whether or not the message should be from the source user.</param>
        /// <param name="inSourceChannel">Whether or not the message should originate from the source channel.</param>
        /// <param name="timeout">The timeout until the method stops listening for new messages.</param>
        /// <param name="token">The cancellation token.</param>
        /// <returns>A <see cref="SocketMessage"/> of the message meeting the criteria.</returns>
        public Task<SocketMessage> NextMessageAsync(bool fromSourceUser = true, bool inSourceChannel = true, TimeSpan? timeout = null, CancellationToken token = default(CancellationToken))
            => this.Interactive.NextMessageAsync(this.Context, fromSourceUser, inSourceChannel, timeout, token);

        /// <summary>
        /// Reply to a message and delete the response when the timeout has passed.
        /// </summary>
        /// <param name="content">The content of the message.</param>
        /// <param name="isTTS">Whether or not the message should be TTS.</param>
        /// <param name="embed">The embed that should be attached with the message.</param>
        /// <param name="timeout">The timeout of when the message will be deleted.</param>
        /// <param name="options">Any additional <see cref="RequestOptions"/>.</param>
        /// <returns>The <see cref="IUserMessage"/> that was sent.</returns>
        public Task<IUserMessage> ReplyAndDeleteAsync(string content, bool isTTS = false, Embed embed = null, TimeSpan? timeout = null, RequestOptions options = null)
            => this.Interactive.ReplyAndDeleteAsync(this.Context, content, isTTS, embed, timeout, options);

        /// <summary>
        /// A <see cref="RuntimeResult"/> indicating a success state.
        /// </summary>
        /// <param name="reason">An optional reason for the success state.</param>
        /// <returns>A <see cref="RuntimeResult"/> indicating a success state, with an optional reason.</returns>
        public RuntimeResult Ok(string reason = null) => new OkResult(reason);
    }
}