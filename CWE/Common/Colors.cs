namespace CWE.Common
{
    using Discord;

    /// <summary>
    /// Colors used by CWE.
    /// </summary>
    public static class Colors
    {
        /// <summary>
        /// The color of Discord.
        /// </summary>
        public static Color Discord = new (114, 137, 218);

        /// <summary>
        /// The color used to indicate an informative state.
        /// </summary>
        public static Color Information = new (26, 155, 226);

        /// <summary>
        /// The color used to indicate a success state.
        /// </summary>
        public static Color Success = new (95, 218, 153);

        /// <summary>
        /// The color used to indicate an error state.
        /// </summary>
        public static Color Error = new (236, 56, 69);

        /// <summary>
        /// The color used to indicate a warning state.
        /// </summary>
        public static Color Warning = new (254, 184, 6);

        /// <summary>
        /// The color used to indicate an active state.
        /// </summary>
        public static Color Active = new (254, 200, 16);
    }
}
