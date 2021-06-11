namespace CWE.Interactive
{
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;

    /// <summary>
    /// Criterion to ensure that <see cref="IMessage"/> originates from a channel.
    /// </summary>
    public class EnsureFromChannelCriterion : ICriterion<IMessage>
    {
        private readonly ulong channelId;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnsureFromChannelCriterion"/> class.
        /// </summary>
        /// <param name="channel">The <see cref="IMessageChannel"/> to be injected.</param>
        public EnsureFromChannelCriterion(IMessageChannel channel)
            => this.channelId = channel.Id;

        /// <inheritdoc/>
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, IMessage parameter)
        {
            bool ok = this.channelId == parameter.Channel.Id;
            return Task.FromResult(ok);
        }
    }
}
