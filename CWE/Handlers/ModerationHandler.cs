namespace CWE.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Timers;
    using CWE.Common;
    using CWE.Data;
    using CWE.Data.Models;
    using CWE.Services;
    using Discord;
    using Discord.WebSocket;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Represents a handler for dealing with moderation related actions.
    /// </summary>
    public class ModerationHandler : DiscordHandler
    {
        private DiscordSocketClient client;
        private DataAccessLayer dataAccessLayer;
        private IConfiguration configuration;
        private Timer muteTimer;

        private List<Mute> muteCache;

        private ulong GuildId
            => this.configuration.GetValue<ulong>("Guild");

        private ulong MutedRoleId
            => this.configuration.GetSection("Roles").GetValue<ulong>("Muted");

        private ulong LogChannelId
            => this.configuration.GetSection("Channels").GetValue<ulong>("LogChannel");

        private SocketGuild Guild
            => this.client.GetGuild(this.GuildId);

        private SocketRole MutedRole
            => this.Guild.GetRole(this.MutedRoleId);

        private SocketTextChannel LogChannel
            => this.Guild.GetTextChannel(this.LogChannelId);

        /// <summary>
        /// Formats an <see cref="InfractionType"/> to a user friendly string.
        /// </summary>
        /// <param name="type">The infraction type to convert.</param>
        /// <returns>A readable string representing the infraction type.</returns>
        public static string FormatType(InfractionType type)
        {
            switch (type)
            {
                case InfractionType.Ban:
                    return "Banned";
                case InfractionType.Kick:
                    return "Kicked";
                case InfractionType.Mute:
                    return "Muted";
                case InfractionType.Warn:
                    return "Warned";
                default: return "Unknown";
            }
        }

        /// <inheritdoc/>
        public override async Task InitializeAsync(DiscordSocketClient client, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            this.client = client;
            this.configuration = configuration;
            this.dataAccessLayer = serviceProvider.GetRequiredService<DataAccessLayer>();
            this.muteTimer = new (60000);
            this.muteTimer.Elapsed += this.HandleElapsed;
            this.muteTimer.Start();

            this.muteCache = await this.dataAccessLayer.GetMutes();

            this.client.InteractionCreated += (SocketInteraction arg) =>
            {
                if (arg is SocketMessageComponent comp)
                {
                    return this.HandleUnmuteRequest(comp);
                }

                return Task.CompletedTask;
            };
        }

        /// <summary>
        /// Creates an infraction.
        /// </summary>
        /// <param name="type">The type of the infraction to create.</param>
        /// <param name="target">The user id to target this infraction towards.</param>
        /// <param name="staff">The staff member creating this infraction.</param>
        /// <param name="reason">The reason this infraction was created.</param>
        /// <param name="muteDuration">The optional mute duration for the target user.</param>
        /// <returns>A <see cref="bool"/> if the infraction create process was successful.</returns>
        public async Task<bool> CreateInfractionAsync(InfractionType type, ulong target, IUser staff, string reason, TimeSpan? muteDuration = null)
        {
            var infrac = new Infraction()
            {
                InfractionId = Guid.NewGuid(),
                Date = DateTime.UtcNow,
                Reason = reason,
                StaffId = staff.Id,
                StaffUsername = $"{staff.Username}{staff.Discriminator}",
                Username = $"{staff.Username}{staff.Discriminator}",
                Type = type,
                UserId = target,
            };

            await this.dataAccessLayer.CreateInfraction(infrac);

            var guildUser = this.Guild.GetUser(target);

            switch (type)
            {
                case InfractionType.Kick:
                    if (guildUser != null)
                    {
                        await guildUser.KickAsync($"{reason} - {staff.Username}");
                    }

                    break;
                case InfractionType.Ban:
                    await this.Guild.AddBanAsync(target);
                    break;
                case InfractionType.Mute:
                    if (muteDuration == null)
                    {
                        return false;
                    }

                    var user = this.Guild.GetUser(target);

                    if (user == null)
                    {
                        return false;
                    }

                    await user.AddRoleAsync(this.MutedRole);

                    var mute = new Mute()
                    {
                        InfractionId = infrac.InfractionId,
                        MuteEnd = DateTime.UtcNow.Add(muteDuration.Value),
                        MuteStart = DateTime.UtcNow,
                        User = target,
                    };
                    await this.dataAccessLayer.CreateMute(mute);
                    this.muteCache.Add(mute);
                    break;

                default:
                    return true;
            }

            this.DispatchModeratorLog(infrac);
            return true;
        }

        /// <summary>
        /// Deletes an infraction.
        /// </summary>
        /// <param name="id">The id of the infraction to delete.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation of deleting the infraction.</returns>
        public Task DeleteInfractionAsync(Guid id)
        {
            return this.dataAccessLayer.DeleteInfraction(id);
        }

        private async void HandleElapsed(object sender, ElapsedEventArgs e)
        {
            var expiredMutes = this.muteCache.Where(x => x.MuteEnd < DateTime.UtcNow);

            foreach (var expiredMute in expiredMutes)
            {
                await this.ClearMute(expiredMute);
            }
        }

        private async Task ClearMute(Mute mute)
        {
            await this.dataAccessLayer.DeleteMute(mute.User);
            this.muteCache.Remove(mute);

            var user = this.client.GetGuild(this.GuildId).GetUser(mute.User);

            if (user == null)
            {
                return;
            }

            if (user.Roles.Contains(this.MutedRole))
            {
                await user.RemoveRoleAsync(this.MutedRole);
            }

            var unmuteEmbed = new EmbedBuilder()
                .WithColor(Color.Green)
                .WithAuthor(user.GetAuthorEmbed())
                .WithTitle("Mute expired")
                .WithDescription($"You are no longer muted in {this.Guild}")
                .WithCurrentTimestamp();

            try
            {
                await user.SendMessageAsync(embed: unmuteEmbed.Build());
            }
            catch
            {
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1025:Code should not contain multiple whitespace in a row", Justification = "Easier to read.")]
        private void DispatchModeratorLog(Infraction infraction)
        {
            if (this.LogChannel == null)
            {
                return;
            }

            _ = Task.Run(async () =>
            {
                var user = this.client.GetUser(infraction.UserId);
                var author = user?.GetAuthorEmbed() ?? new EmbedAuthorBuilder().WithName(infraction.Username);

                var embed = new EmbedBuilder()
                    .WithColor(infraction.Type == InfractionType.Ban ? Color.Red
                             : infraction.Type == InfractionType.Kick ? Color.Orange
                             : infraction.Type == InfractionType.Mute ? Color.Blue
                             : infraction.Type == InfractionType.Warn ? Color.Green
                             : Color.Teal)
                    .WithAuthor(author)
                    .WithDescription($"The user <@{infraction.UserId}> has been {FormatType(infraction.Type)}");

                embed.Fields = new List<EmbedFieldBuilder>()
                {
                    new EmbedFieldBuilder()
                    {
                        Name = "Reason",
                        Value = infraction.Reason,
                    },
                    new EmbedFieldBuilder()
                    {
                        Name = "Staff member",
                        Value = $"{infraction.StaffUsername} (<@{infraction.StaffId}>)",
                    },
                };

                MessageComponent comp = null;

                if (infraction.Type == InfractionType.Mute)
                {
                    comp = new ComponentBuilder()
                        .WithButton($"Unmute {infraction.Username}", $"unmute_{infraction.UserId}", ButtonStyle.Danger)
                        .Build();

                    var mute = this.muteCache.FirstOrDefault(x => x.InfractionId == infraction.InfractionId);

                    if (mute != null)
                    {
                        embed.AddField("Mute duration", (mute.MuteEnd - mute.MuteStart).ToReadableFormat());
                    }
                }

                await this.LogChannel.SendMessageAsync(embed: embed.Build(), component: comp);
            });
        }

        private async Task HandleUnmuteRequest(SocketMessageComponent component)
        {
            if (!component.Data.CustomId.StartsWith("unmute_"))
            {
                return;
            }

            var guildUser = this.Guild.GetUser(component.User.Id);

            if (guildUser == null)
            {
                return;
            }

            var staffRoleId = this.configuration.GetSection("Roles").GetValue<ulong>("Staff");

            if (!guildUser.Roles.Any(x => x.Id == staffRoleId))
            {
                return;
            }

            var targetUserId = ulong.Parse(component.Data.CustomId.Replace("unmute_", string.Empty));
            var targetGuildUser = this.Guild.GetUser(targetUserId);

            await this.dataAccessLayer.DeleteMute(targetUserId);

            if (targetGuildUser == null)
            {
                return;
            }

            if (targetGuildUser.Roles.Any(x => x.Id == this.MutedRoleId))
            {
                await targetGuildUser.RemoveRoleAsync(this.MutedRole);
            }
        }
    }
}
