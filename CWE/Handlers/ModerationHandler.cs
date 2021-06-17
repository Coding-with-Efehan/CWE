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

        private SocketGuild Guild
            => this.client.GetGuild(this.GuildId);

        private SocketRole MutedRole
            => this.Guild.GetRole(this.MutedRoleId);

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
    }
}
