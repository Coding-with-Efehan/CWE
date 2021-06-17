namespace CWE.Data.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents an enum to determine the type of the <see cref="Infraction"/>
    /// </summary>
    public enum InfractionType
    {
        /// <summary>
        /// A user was Warned.
        /// </summary>
        Warn,

        /// <summary>
        /// A user was Kicked.
        /// </summary>
        Kick,

        /// <summary>
        /// A user was Banned.
        /// </summary>
        Ban,

        /// <summary>
        /// A user was Muted.
        /// </summary>
        Mute,
    }

    /// <summary>
    /// Represents an infraction created by a staff member targeting a user.
    /// </summary>
    public class Infraction
    {
        /// <summary>
        /// Gets or sets the unique <see cref="Guid"/> of this infraction.
        /// </summary>
        [Key]
        public Guid InfractionId { get; set; }

        /// <summary>
        /// Gets or sets the user this infraction targets.
        /// </summary>
        public ulong UserId { get; set; }

        /// <summary>
        /// Gets or sets the staff who created this infraction.
        /// </summary>
        public ulong StaffId { get; set; }

        /// <summary>
        /// Gets or sets the username of the target user.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the staff username who created this infraction.
        /// </summary>
        public string StaffUsername { get; set; }

        /// <summary>
        /// Gets or sets the type of this infraction.
        /// </summary>
        public InfractionType Type { get; set; }

        /// <summary>
        /// Gets or sets the date this infraction was created on.
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the reason this infraction was created.
        /// </summary>
        public string Reason { get; set; }
    }
}
