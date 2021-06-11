namespace CWE.Interactive
{
    using System.Threading.Tasks;
    using Discord.Commands;
    using Discord.WebSocket;

    /// <summary>
    /// Criterion to ensure that <see cref="SocketReaction"/> originates from same user as <see cref="SocketCommandContext"/>.
    /// </summary>
    internal class EnsureReactionFromSourceUserCriterion : ICriterion<SocketReaction>
    {
        /// <inheritdoc/>
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketReaction parameter)
        {
            bool ok = parameter.UserId == sourceContext.User.Id;
            return Task.FromResult(ok);
        }
    }
}
