using System.Diagnostics.CodeAnalysis;
using Discord;

namespace CWE.Common
{
    /// <summary>
    ///     Colors used by CWE.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private",
        Justification = "Fields are used throughout application.")]
    public static class Colors
    {
        /// <summary>
        ///     The color of Discord.
        /// </summary>
        public static Color Discord = new(114, 137, 218);

        /// <summary>
        ///     The color used to indicate an informative state.
        /// </summary>
        public static Color Information = new(26, 155, 226);

        /// <summary>
        ///     The color used to indicate a success state.
        /// </summary>
        public static Color Success = new(95, 218, 153);

        /// <summary>
        ///     The color used to indicate an error state.
        /// </summary>
        public static Color Error = new(236, 56, 69);

        /// <summary>
        ///     The color used to indicate a warning state.
        /// </summary>
        public static Color Warning = new(254, 184, 6);

        /// <summary>
        ///     The color used to indicate an active state.
        /// </summary>
        public static Color Active = new(254, 200, 16);

        /// <summary>
        ///     The color used to indicate a hug.
        /// </summary>
        public static Color Hug = new(43, 182, 115);

        /// <summary>
        ///     The color used to indicate a eight ball response.
        /// </summary>
        public static Color eightBall = new(0, 0, 0);

        /// <summary>
        ///     The color used to indicate a coinflip.
        /// </summary>
        public static Color Coinflip = new(216, 206, 13);

        /// <summary>
        ///     The color used to indicate a new community poll.
        /// </summary>
        public static Color Poll = new(33, 176, 252);
    }
}