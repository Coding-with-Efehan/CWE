namespace CWE.Interactive
{
    using System.Threading.Tasks;
    using Discord.Commands;
    using Discord.WebSocket;

    /// <summary>
    /// Criterion to ensure that the content of <see cref="SocketMessage"/> is an integer.
    /// </summary>
    internal class EnsureIsIntegerCriterion : ICriterion<SocketMessage>
    {
        /// <inheritdoc/>
        public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketMessage parameter)
        {
            bool ok = int.TryParse(parameter.Content, out _);
            return Task.FromResult(ok);
        }
    }
}
