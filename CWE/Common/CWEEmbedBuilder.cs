namespace CWE.Common
{
    using System;
    using Discord;

    /// <summary>
    /// Custom version of <see cref="EmbedBuilder"/> to include styles.
    /// </summary>
    public class CWEEmbedBuilder
    {
        private string title;
        private string description;
        private string footer;
        private EmbedStyle style;

        /// <summary>
        /// Gets or sets the title of the embed.
        /// </summary>
        public string Title
        {
            get => title;
            set
            {
                if (value?.Length > 256)
                {
                    throw new ArgumentException(message: $"Title length must be less than or equal to 256.", paramName: nameof(Title));
                }

                title = value;
            }
        }

        /// <summary>
        /// Gets or sets the description of the embed.
        /// </summary>
        public string Description
        {
            get => description;
            set
            {
                if (value?.Length > 2048)
                {
                    throw new ArgumentException(message: $"Description length must be less than or equal to 2048.", paramName: nameof(Description));
                }

                description = value;
            }
        }

        /// <summary>
        /// Gets or sets the footer of the embed.
        /// </summary>
        public string Footer
        {
            get => footer;
            set
            {
                if (value?.Length > 2048)
                {
                    throw new ArgumentException(message: $"Footer length must be less than or equal to 2048.", paramName: nameof(Footer));
                }

                footer = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="EmbedStyle"/> of the embed.
        /// </summary>
        public EmbedStyle Style
        {
            get => style;
            set
            {
                style = value;
            }
        }

        /// <summary>
        /// Attach a title to the embed.
        /// </summary>
        /// <param name="title">The title of the embed.</param>
        /// <returns>A <see cref="CWEEmbedBuilder"/> with an attached title.</returns>
        public CWEEmbedBuilder WithTitle(string title)
        {
            Title = title;
            return this;
        }

        /// <summary>
        /// Attach a description to the embed.
        /// </summary>
        /// <param name="description">The description of the embed.</param>
        /// <returns>A <see cref="CWEEmbedBuilder"/> with an attached description.</returns>
        public CWEEmbedBuilder WithDescription(string description)
        {
            Description = description;
            return this;
        }

        /// <summary>
        /// Attach a footer to the embed.
        /// </summary>
        /// <param name="footer">The footer of the embed.</param>
        /// <returns>A <see cref="CWEEmbedBuilder"/> with an attached footer.</returns>
        public CWEEmbedBuilder WithFooter(string footer)
        {
            Footer = footer;
            return this;
        }

        /// <summary>
        /// Attach an <see cref="EmbedStyle"/> to the embed.
        /// </summary>
        /// <param name="style">The <see cref="EmbedStyle"/> of the embed.</param>
        /// <returns>A <see cref="CWEEmbedBuilder"/> with an attached <see cref="EmbedStyle"/>.</returns>
        public CWEEmbedBuilder WithStyle(EmbedStyle style)
        {
            Style = style;
            return this;
        }

        /// <summary>
        /// Builds the <see cref="CWEEmbedBuilder"/>.
        /// </summary>
        /// <returns>The <see cref="Embed"/> with all attached properties.</returns>
        public Embed Build()
        {
            EmbedBuilder builder = new EmbedBuilder()
                .WithDescription(description)
                .WithFooter(footer);

            switch (style)
            {
                case EmbedStyle.Success:
                    builder
                        .WithColor(Colors.Success)
                        .WithAuthor(x =>
                        {
                            x
                            .WithIconUrl(Icons.Success)
                            .WithName(title);
                        });
                    break;
                case EmbedStyle.Error:
                    builder
                        .WithColor(Colors.Error)
                        .WithAuthor(x =>
                        {
                            x
                            .WithIconUrl(Icons.Error)
                            .WithName(title);
                        });
                    break;
                case EmbedStyle.Information:
                    builder
                        .WithColor(Colors.Information)
                        .WithAuthor(x =>
                        {
                            x
                            .WithIconUrl(Icons.Information)
                            .WithName(title);
                        });
                    break;
            }

            return builder.Build();
        }
    }
}
