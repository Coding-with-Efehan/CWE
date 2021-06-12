namespace CWE.Interactive
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Discord.Commands;

    /// <summary>
    /// Implementation of <see cref="ICriterion{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of criterion.</typeparam>
    public class Criteria<T> : ICriterion<T>
    {
        private List<ICriterion<T>> criteria = new List<ICriterion<T>>();

        /// <summary>
        /// Add a <see cref="ICriterion{T}"/> to the list of <see cref="ICriterion{T}"/>.
        /// </summary>
        /// <param name="criterion">The <see cref="ICriterion{T}"/> to be added.</param>
        /// <returns>The <see cref="Criteria{T}"/> containing a list of criteria.</returns>
        public Criteria<T> AddCriterion(ICriterion<T> criterion)
        {
            this.criteria.Add(criterion);
            return this;
        }

        /// <inheritdoc/>
        public async Task<bool> JudgeAsync(SocketCommandContext sourceContext, T parameter)
        {
            foreach (var criterion in this.criteria)
            {
                var result = await criterion.JudgeAsync(sourceContext, parameter).ConfigureAwait(false);
                if (!result)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
