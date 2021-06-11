namespace CWE.Interactive
{
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;

    /// <summary>
    /// Criterion to ensure that <see cref="IMessage"/> originates from same channel as source <see cref="SocketCommandContext"/>.
    /// </summary>
    public class EnsureSourceChannelCriterion : ICriterion<IMessage>
    {
        /// <inheritdoc/>
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, IMessage parameter)
        {
            var ok = sourceContext.Channel.Id == parameter.Channel.Id;
            return Task.FromResult(ok);
        }
    }
}
