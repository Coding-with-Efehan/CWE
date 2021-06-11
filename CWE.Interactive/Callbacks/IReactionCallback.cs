namespace CWE.Interactive
{
    using System;
    using System.Threading.Tasks;
    using Discord.Commands;
    using Discord.WebSocket;

    /// <summary>
    /// Reaction callback used in i.e. paginators.
    /// </summary>
    public interface IReactionCallback
    {
        /// <summary>
        /// Gets the <see cref="RunMode"/> of the <see cref="IReactionCallback"/>.
        /// </summary>
        RunMode RunMode { get; }

        /// <summary>
        /// Gets the <see cref="ICriterion{T}"/> attached to the <see cref="IReactionCallback"/>.
        /// </summary>
        ICriterion<SocketReaction> Criterion { get; }

        /// <summary>
        /// Gets the timeout of the <see cref="IReactionCallback"/>.
        /// </summary>
        TimeSpan? Timeout { get; }

        /// <summary>
        /// Gets the <see cref="SocketCommandContext"/> of the <see cref="IReactionCallback"/>.
        /// </summary>
        SocketCommandContext Context { get; }

        /// <summary>
        /// Method responsible for handling callbacks.
        /// </summary>
        /// <param name="reaction">The <see cref="SocketReaction"/> of the <see cref="IReactionCallback"/>.</param>
        /// <returns>A bool indicating if the callback is still attached.</returns>
        Task<bool> HandleCallbackAsync(SocketReaction reaction);
    }
}
