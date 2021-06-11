namespace CWE.Interactive
{
    using System.Threading.Tasks;
    using Discord.Commands;

    /// <summary>
    /// A criterion to ensure a certain condition.
    /// </summary>
    /// <typeparam name="T">The type of criterion.</typeparam>
    public interface ICriterion<in T>
    {
        /// <summary>
        /// Judge whether or not the <see cref="SocketCommandContext"/> meets the <see cref="T"/> criterion.
        /// </summary>
        /// <param name="sourceContext">The <see cref="SocketCommandContext"/> to be judged.</param>
        /// <param name="parameter">The <see cref="T"/> that the <see cref="SocketCommandContext"/> should be judged on.</param>
        /// <returns>A bool indicating whether or not the criterion <see cref="T"/> is met.</returns>
        Task<bool> JudgeAsync(SocketCommandContext sourceContext, T parameter);
    }
}
