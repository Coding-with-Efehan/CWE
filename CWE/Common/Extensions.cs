namespace CWE.Common
{
    using System;
    using System.IO;
    using System.Linq;
    using Discord.WebSocket;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// Extension methods used throughout CWE.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Ensure a user is promoted or has administrator permissions.
        /// </summary>
        /// <param name="socketUser">The <see cref="SocketUser"/> to be checked.</param>
        /// <returns>A bool indicating whether or not to return.</returns>
        public static bool IsPromoted(this SocketUser socketUser)
        {
            if (socketUser is not SocketGuildUser socketGuildUser)
            {
                return false;
            }

            try
            {
                var configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", false, true)
                        .Build();

                var regularRoleId = configuration.GetSection("Roles").GetValue<ulong>("Regular");
                var associateRoleId = configuration.GetSection("Roles").GetValue<ulong>("Associate");

                var regularRole = socketGuildUser.Guild.GetRole(regularRoleId);
                var associateRole = socketGuildUser.Guild.GetRole(associateRoleId);
                if (!socketGuildUser.Roles.Contains(regularRole) &&
                    !socketGuildUser.Roles.Contains(associateRole) &&
                    !socketGuildUser.GuildPermissions.Administrator)
                {
                    return false;
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Get the username and tag of a user.
        ///
        /// <para>Example: Directoire#0001.</para>
        /// </summary>
        /// <param name="socketUser">The <see cref="SocketUser"/> to get the username and tag from.</param>
        /// <returns>The username and tag of a user.</returns>
        public static string GetUsernameAndTag(this SocketUser socketUser)
        {
            return socketUser.Username + "#" + socketUser.Discriminator;
        }
    }
}
