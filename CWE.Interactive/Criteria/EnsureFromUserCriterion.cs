namespace CWE.Interactive
{
    using System.ComponentModel;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;

    /// <summary>
    /// Criterion to ensure that <see cref="IMessage"/> originates from a user.
    /// </summary>
    public class EnsureFromUserCriterion : ICriterion<IMessage>
    {
        private readonly ulong id;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnsureFromUserCriterion"/> class.
        /// </summary>
        /// <param name="user">The <see cref="IUser"/> to be injected.</param>
        public EnsureFromUserCriterion(IUser user)
            => this.id = user.Id;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnsureFromUserCriterion"/> class.
        /// </summary>
        /// <param name="id">The ID of the user.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public EnsureFromUserCriterion(ulong id)
            => this.id = id;

        /// <inheritdoc/>
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, IMessage parameter)
        {
            bool ok = this.id == parameter.Author.Id;
            return Task.FromResult(ok);
        }
    }
}
