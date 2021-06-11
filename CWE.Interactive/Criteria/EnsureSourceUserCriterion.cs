namespace CWE.Interactive
{
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;

    /// <summary>
    /// Criterion to ensure that <see cref="IMessage"/> originates from same user as source <see cref="SocketCommandContext"/>.
    /// </summary>
    public class EnsureSourceUserCriterion : ICriterion<IMessage>
    {
        /// <inheritdoc/>
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, IMessage parameter)
        {
            var ok = sourceContext.User.Id == parameter.Author.Id;
            return Task.FromResult(ok);
        }
    }
}
