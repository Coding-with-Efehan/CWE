namespace CWE.Data.Models
{
    /// <summary>
    /// Enumerator used to indicate the state of a request.
    /// </summary>
    public enum RequestState
    {
        /// <summary>
        /// The request is pending approval.
        /// </summary>
        Pending,

        /// <summary>
        /// The request is active.
        /// </summary>
        Active,

        /// <summary>
        /// The request has been finished.
        /// </summary>
        Finished,

        /// <summary>
        /// The request has been denied.
        /// </summary>
        Denied
    }

    /// <summary>
    /// A request that can be sent during coding night.
    /// </summary>
    public class Request
    {
        /// <summary>
        /// Gets or sets the unique ID of the request.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ID of the initiator.
        /// </summary>
        public ulong Initiator { get; set; }

        /// <summary>
        /// Gets or sets the ID of the request its message.
        /// </summary>
        public ulong MessageId { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="RequestState"/>.
        /// </summary>
        public RequestState State { get; set; } = RequestState.Pending;

        /// <summary>
        /// Gets or sets the decription of the request.
        /// </summary>
        public string Description { get; set; }
    }
}
