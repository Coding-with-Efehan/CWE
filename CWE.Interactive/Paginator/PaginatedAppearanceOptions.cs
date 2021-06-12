namespace CWE.Interactive
{
    using System;
    using Discord;

    /// <summary>
    /// Enumerator to indicate when jump options should be displayed.
    /// </summary>
    public enum JumpDisplayOptions
    {
        /// <summary>
        /// Never show the jump options.
        /// </summary>
        Never,

        /// <summary>
        /// Only show the jump options when user has <see cref="ChannelPermissions.ManageMessages"/> permissions.
        /// </summary>
        WithManageMessages,

        /// <summary>
        /// Always show the jump options.
        /// </summary>
        Always,
    }

    /// <summary>
    /// Appearance options of a paginator, such as its buttons, footer and related.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Fiels are used and are required to have public access modifiers.")]
    public class PaginatedAppearanceOptions
    {
        /// <summary>
        /// Default appearance options.
        /// </summary>
        public static PaginatedAppearanceOptions Default = new PaginatedAppearanceOptions();

        /// <summary>
        /// Emote to go back to the first page.
        /// </summary>
        public IEmote First = new Emoji("⏮");

        /// <summary>
        /// Emote to go back to the previous page.
        /// </summary>
        public IEmote Back = new Emoji("◀");

        /// <summary>
        /// Emote to go to the next page.
        /// </summary>
        public IEmote Next = new Emoji("▶");

        /// <summary>
        /// Emote to go to the final page.
        /// </summary>
        public IEmote Last = new Emoji("⏭");

        /// <summary>
        /// Emote to stop the paginator.
        /// </summary>
        public IEmote Stop = new Emoji("⏹");


        /// <summary>
        /// Emote to jump to a page.
        /// </summary>
        public IEmote Jump = new Emoji("🔢");

        /// <summary>
        /// Emote to show the information section.
        /// </summary>
        public IEmote Info = new Emoji("ℹ");

        /// <summary>
        /// The formatting of the footer.
        /// </summary>
        public string FooterFormat = "Page {0}/{1}";

        /// <summary>
        /// The text displayed when the Info emote is pressed.
        /// </summary>
        public string InformationText = "This is a paginator. React with the respective icons to change page.";

        /// <summary>
        /// When the Jump emote is usable.
        /// </summary>
        public JumpDisplayOptions JumpDisplayOptions = JumpDisplayOptions.WithManageMessages;

        /// <summary>
        /// Whether or not to display the information icon.
        /// </summary>
        public bool DisplayInformationIcon = true;

        /// <summary>
        /// The timeout of the paginator.
        /// </summary>
        public TimeSpan? Timeout = null;

        /// <summary>
        /// The timeout of the information section (when it disappears).
        /// </summary>
        public TimeSpan InfoTimeout = TimeSpan.FromSeconds(30);

        /// <summary>
        /// The amount of fields displayed per page.
        /// </summary>
        public int FieldsPerPage = 6;
    }
}
