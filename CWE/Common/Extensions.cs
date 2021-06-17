namespace CWE.Common
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.RegularExpressions;
    using Discord;
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
        /// Makes a <see cref="EmbedAuthorBuilder"/> from a <see cref="IUser"/>.
        /// </summary>
        /// <param name="user">The user to make an <see cref="EmbedAuthorBuilder"/> from</param>
        /// <returns>A <see cref="EmbedAuthorBuilder"/> whos properties are set to the current users.</returns>
        public static EmbedAuthorBuilder GetAuthorEmbed(this IUser user)
        {
            return new EmbedAuthorBuilder()
            {
                Name = $"{user.Username}{user.Discriminator}",
                IconUrl = user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl(),
            };
        }

        /// <summary>
        /// Converts the timespan to a readable format.
        /// </summary>
        /// <param name="time">The current timespan to convert.</param>
        /// <returns>A formatted string representing the timespan.</returns>
        public static string ToReadableFormat(this TimeSpan time)
        {
            List<string> formats = new List<string>();

            if(time.Days > 0)
            {
                formats.Add($"{time.Days} days");
            }

            if (time.Hours > 0)
            {
                formats.Add($"{time.Hours} hours");
            }

            if (time.Minutes > 0)
            {
                formats.Add($"{time.Minutes} minutes");
            }

            if (time.Seconds > 0)
            {
                formats.Add($"{time.Seconds} seconds");
            }

            if (formats.Count >= 2)
            {
                var last = formats.Last();
                formats.Remove(last);
                last = "and " + last;
            }

            return string.Join(", ", formats);
        }

        /// <summary>
        /// Converts the current string into a timespan.
        /// </summary>
        /// <param name="str">The string matching the below regex: <code>(?>(\d+?)([h|m|s|d]))</code></param>
        /// <returns>A timespan created from the string.</returns>
        public static TimeSpan ToTimespan(this string str)
        {
            TimeSpan t = new TimeSpan(0);
            List<TimeSpan> spans = new List<TimeSpan>();

            var matches = Regex.Matches(str, @"(?>(\d+?)([h|m|s|d]))");

            foreach (Match match in matches)
            {
                switch (match.Groups[2].Value)
                {
                    case "h":
                        spans.Add(TimeSpan.FromHours(int.Parse(match.Groups[1].Value)));
                        break;
                    case "m":
                        spans.Add(TimeSpan.FromMinutes(int.Parse(match.Groups[1].Value)));
                        break;
                    case "s":
                        spans.Add(TimeSpan.FromSeconds(int.Parse(match.Groups[1].Value)));
                        break;
                    case "d":
                        spans.Add(TimeSpan.FromDays(int.Parse(match.Groups[1].Value)));
                        break;
                }
            }

            foreach (var ts in spans)
            {
                t = t.Add(ts);
            }

            return t;
        }
    }
}
