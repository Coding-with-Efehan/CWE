using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWE.Data.Models
{
    /// <summary>
    /// Represents a mute structure to keep track of muted users.
    /// </summary>
    [Keyless]
    public class Mute
    {
        /// <summary>
        /// Gets or sets the infractio id for this mute.
        /// </summary>
        public Guid InfractionId { get; set; }

        /// <summary>
        /// Gets or sets the user who is muted.
        /// </summary>
        public ulong User { get; set; }

        /// <summary>
        /// Gets or sets the time the mute started.
        /// </summary>
        public DateTime MuteStart { get; set; }

        /// <summary>
        /// Gets or sets the time the mute ends.
        /// </summary>
        public DateTime MuteEnd { get; set; }
    }
}
