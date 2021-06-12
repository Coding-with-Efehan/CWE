using System.Diagnostics.CodeAnalysis;

namespace CWE.Common
{
    /// <summary>
    ///     Icons used by CWE.
    /// </summary>
    [SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private",
        Justification = "Fields are used throughout application.")]
    public static class Icons
    {
        /// <summary>
        ///     The icon used to indicate a success state.
        /// </summary>
        public static string Success = "https://i.imgur.com/It9BNU8.png";

        /// <summary>
        ///     The icon used to indicate an error state.
        /// </summary>
        public static string Error = "https://i.imgur.com/mNCmgf3.png";

        /// <summary>
        ///     The icon used to indicate a warning state.
        /// </summary>
        public static string Warning = "https://i.imgur.com/X6M1yE7.png";

        /// <summary>
        ///     The icon used to indicate a restricted state.
        /// </summary>
        public static string Restricted = "https://i.imgur.com/mTUfdsc.png";

        /// <summary>
        ///     The icon used to indicate a loading state.
        /// </summary>
        public static string Wait = "https://i.imgur.com/XWK6FIH.png";

        /// <summary>
        ///     The icon used to indicate an informative state.
        /// </summary>
        public static string Information = "https://i.imgur.com/gLR4k7d.png";

        /// <summary>
        ///     The icon used to indicate a new campaign.
        /// </summary>
        public static string NewCampaign = "https://i.imgur.com/r8xwMAD.png";

        /// <summary>
        ///     The icon used to indicate a campaign has been accepted.
        /// </summary>
        public static string AcceptedCampaign = "https://i.imgur.com/k6vzLkO.png";

        /// <summary>
        ///     The icon used to indicate a campaign has been denied.
        /// </summary>
        public static string DeniedCampaign = "https://i.imgur.com/6mwoy56.png";

        /// <summary>
        ///     The icon used to indicate a new request.
        /// </summary>
        public static string NewRequest = "https://i.imgur.com/VgEclzE.png";

        /// <summary>
        ///     The icon used to indicate a request has been set to active.
        /// </summary>
        public static string ActiveRequest = "https://i.imgur.com/zhEdQeo.png";

        /// <summary>
        ///     The icon used to indicate a request has been denied.
        /// </summary>
        public static string DeniedRequest = "https://i.imgur.com/a8L3jK0.png";

        /// <summary>
        ///     The icon used to represent a hug.
        /// </summary>
        public static string Hug =
            "https://external-content.duckduckgo.com/iu/?u=https%3A%2F%2Fstatic.thenounproject.com%2Fpng%2F1580530-200.png&f=1&nofb=1";

        /// <summary>
        ///     The icon used to represent a magic 8-ball.
        /// </summary>
        public static string eightBall =
            "https://external-content.duckduckgo.com/iu/?u=https%3A%2F%2F1001freedownloads.s3.amazonaws.com%2Fvector%2Fthumb%2F110366%2F8_Ball.png&f=1&nofb=1";

        /// <summary>
        ///     The icon used to represent a coin.
        /// </summary>
        public static string Coinflip = "http://clipart-library.com/data_images/296207.png";

        /// <summary>
        ///     The icon used to represent a poll.
        /// </summary>
        public static string Poll =
            "https://external-content.duckduckgo.com/iu/?u=https%3A%2F%2Fwww.pinclipart.com%2Fpicdir%2Fmiddle%2F107-1071607_polls-icon-png-clipart.png&f=1&nofb=1";
    }
}