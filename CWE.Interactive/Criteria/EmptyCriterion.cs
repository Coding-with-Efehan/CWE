namespace CWE.Interactive
{
    using System.Threading.Tasks;
    using Discord.Commands;

    /// <summary>
    /// Implementation of <see cref="ICriterion{T}"/> without list of criteria.
    /// </summary>
    /// <typeparam name="T">The type of criterion.</typeparam>
    public class EmptyCriterion<T> : ICriterion<T>
    {
        /// <inheritdoc/>
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, T parameter)
            => Task.FromResult(true);
    }
}
