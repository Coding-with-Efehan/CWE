namespace CWE.Data.Models
{
    /// <summary>
    /// Enumerator used to indicate the state of a suggestion.
    /// </summary>
    public enum SuggestionState
    {
        /// <summary>
        /// The suggestion is new.
        /// </summary>
        New,

        /// <summary>
        /// The suggestion has been accepted.
        /// </summary>
        Approved,

        /// <summary>
        /// The suggestion has been rejected.
        /// </summary>
        Rejected,
    }

    /// <summary>
    /// A suggestion to request i.e. an improvement, change or removal.
    /// </summary>
    public class Suggestion
    {
        /// <summary>
        /// Gets or sets the ID of the suggestion.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the initiator.
        /// </summary>
        public ulong Initiator { get; set; }

        /// <summary>
        /// Gets or sets the ID of the suggestion its message.
        /// </summary>
        public ulong MessageId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SuggestionState"/>.
        /// </summary>
        public SuggestionState State { get; set; } = SuggestionState.New;
    }
}
