namespace CWE.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines a standard Tag, which is a phrase that triggers a response from CWE.
    /// </summary>
    public class Tag
    {
        /// <summary>
        /// Gets or sets the unique ID of this tag.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name of the tag.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the content that the tag will hold.
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// Gets or sets the user who created the tag, who can edit, and delete the tag.
        /// </summary>
        public ulong OwnerId { get; set; }
    }
}
