namespace CWE.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Enumerator used to indicate what role a <see cref="Campaign"/> is for.
    /// </summary>
    public enum CampaignType
    {
        /// <summary>
        /// The regular role.
        /// </summary>
        Regular,

        /// <summary>
        /// The associate role.
        /// </summary>
        Associate,
    }

    /// <summary>
    /// A campaign to promote a user.
    /// </summary>
    public class Campaign
    {
        /// <summary>
        /// Gets or sets the ID of the user to be promoted.
        /// </summary>
        [Key]
        public ulong User { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="CampaignType"/> to indicate what role the campaign is for.
        /// </summary>
        public CampaignType Type { get; set; }

        /// <summary>
        /// Gets or sets the ID of the initiator.
        /// </summary>
        public ulong Initiator { get; set; }

        /// <summary>
        /// Gets or sets the reason of the campaign.
        /// </summary>
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets the ID of the message of the campaign.
        /// </summary>
        public ulong Message { get; set; }

        /// <summary>
        /// Gets or sets the start <see cref="DateTime"/> of the campaign.
        /// </summary>
        public DateTime Start { get; set; }

        /// <summary>
        /// Gets or sets the end <see cref="DateTime"/> of the campaign.
        /// </summary>
        public DateTime End { get; set; }

        /// <summary>
        /// Gets or sets the minimum amount of votes in favour required for the campaign to be accepted.
        /// </summary>
        public int Minimal { get; set; }
    }
}
