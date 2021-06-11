namespace CWE.Interactive
{
    using System.Collections.Generic;
    using Discord;

    /// <summary>
    /// A message containing multiple pages, controlled by emotes.
    /// </summary>
    public class PaginatedMessage
    {
        /// <summary>
        /// Gets or sets a collection of elements to page over in the embed. It is expected
        /// that a string-like object is used in this collection, as objects will be converted
        /// to a displayable string only through their generic ToString method, with the
        /// exception of EmbedFieldBuilders.
        ///
        /// If this collection is of EmbedFieldBuilder, then the pages will be displayed in
        /// batches of <see cref="PaginatedAppearanceOptions.FieldsPerPage"/>, and the
        /// embed's description will be populated with the <see cref="AlternateDescription"/> field.
        /// </summary>
        public IEnumerable<object> Pages { get; set; }

        /// <summary>
        /// Gets or sets the content of the message, displayed above the embed. This may remain empty.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the <see cref="EmbedBuilder.Author"/> property directly.
        /// </summary>
        public EmbedAuthorBuilder Author { get; set; } = null;

        /// <summary>
        /// Gets or sets the <see cref="Color"/> of the embed.
        /// </summary>
        public Color Color { get; set; } = Color.Default;

        /// <summary>
        /// Gets or sets the title of the embed.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets an alternative description.
        ///
        /// AlternateDescription will be used as the description of the paginator only when
        /// <see cref="Pages"/> is a collection of <see cref="EmbedFieldBuilder"/>.
        /// </summary>
        public string AlternateDescription { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the <see cref="PaginatedAppearanceOptions"/> of the paginator.
        /// </summary>
        public PaginatedAppearanceOptions Options { get; set; } = PaginatedAppearanceOptions.Default;
    }
}
