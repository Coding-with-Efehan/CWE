namespace CWE.Common
{
    using Discord;

    /// <summary>
    /// Colors used by CWE.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Fields are used throughout application.")]
    public static class Colors
    {
        /// <summary>
        /// The color of Discord.
        /// </summary>
        public static Color Discord = new Color(114, 137, 218);

        /// <summary>
        /// The color used to indicate an informative state.
        /// </summary>
        public static Color Information = new Color(26, 155, 226);

        /// <summary>
        /// The color used to indicate a success state.
        /// </summary>
        public static Color Success = new Color(95, 218, 153);

        /// <summary>
        /// The color used to indicate an error state.
        /// </summary>
        public static Color Error = new Color(236, 56, 69);

        /// <summary>
        /// The color used to indicate a warning state.
        /// </summary>
        public static Color Warning = new Color(254, 184, 6);

        /// <summary>
        /// The color used to indicate an active state.
        /// </summary>
        public static Color Active = new Color(254, 200, 16);
    }
}
