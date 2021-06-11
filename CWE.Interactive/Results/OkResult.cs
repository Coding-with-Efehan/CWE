namespace CWE.Interactive
{
    using Discord.Commands;

    /// <summary>
    /// Implementation of <see cref="RuntimeResult"/> to indicate an OK result.
    /// </summary>
    public class OkResult : RuntimeResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="OkResult"/> class.
        /// </summary>
        /// <param name="reason">The reason of the specified result.</param>
        public OkResult(string reason = null)
            : base(null, reason)
        {
        }
    }
}
