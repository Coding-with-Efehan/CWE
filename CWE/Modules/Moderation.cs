using CWE.Common;
using CWE.Data.Models;
using CWE.Handlers;
using CWE.Services;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Interactivity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CWE.Modules
{
    public class Moderation : CWEModuleBase
    {
        private ModerationHandler modHandler;

        private ModerationHandler ModerationHandler
        {
            get
            {
                if (this.modHandler == null)
                {
                    this.modHandler = HandlerService.GetHandlerInstance<ModerationHandler>();
                }

                return this.modHandler;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Moderation"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        /// <param name="interactivityService">The <see cref="InteractivityService"/> to inject.</param>
        public Moderation(IServiceProvider serviceProvider, IConfiguration configuration, InteractivityService interactivityService)
                : base(serviceProvider, configuration, interactivityService)
        {
        }

        /// <summary>
        /// Gets a success embed for a moderation action.
        /// </summary>
        /// <param name="type">The type of the infraction.</param>
        /// <param name="target">The target user for this infraction</param>
        /// <param name="staff">The staff member executing the infraction</param>
        /// <param name="reason">The reason for the infraction</param>
        /// <param name="mute">The optional mute parameter for mute cases</param>
        /// <returns>An embed that sumarizes the infraction.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:Parameter should not span multiple lines", Justification = "Styling of embed.")]
        public Embed GetSuccessEmbed(InfractionType type, SocketGuildUser target, SocketUser staff, string reason, TimeSpan? mute = null)
        {
            return new EmbedBuilder()
                .WithAuthor(target.GetAuthorEmbed())
                .WithTitle($"Successfully {this.FormatType(type)} {target}")
                .WithFields(
                    new EmbedFieldBuilder()
                        .WithIsInline(true)
                        .WithName("Staff member")
                        .WithValue(staff),
                    new EmbedFieldBuilder()
                        .WithIsInline(true)
                        .WithName("Reason")
                        .WithValue(reason),
                    mute.HasValue
                        ? new EmbedFieldBuilder()
                            .WithIsInline(true)
                            .WithName("Mute Duration")
                            .WithValue(mute.Value.ToReadableFormat())
                        : null)
                .WithCurrentTimestamp()
                .WithColor(Color.Green)
                .Build();
        }

        /// <summary>
        /// Warns a user.
        /// </summary>
        /// <param name="user">The user to warn.</param>
        /// <param name="reason">The reason to warn this user.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [RequireStaff]
        [Command("warn", RunMode = RunMode.Async)]
        public Task Warn(SocketGuildUser user, [Remainder] string reason)
            => this.ExecuteActionInternal(user, reason, InfractionType.Warn);

        /// <summary>
        /// Kicks a user.
        /// </summary>
        /// <param name="user">The user to kick.</param>
        /// <param name="reason">The reason to kick this user.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [RequireStaff]
        [Command("kick", RunMode = RunMode.Async)]
        public Task Kick(SocketGuildUser user, [Remainder] string reason)
            => this.ExecuteActionInternal(user, reason, InfractionType.Kick);

        /// <summary>
        /// Bans a user.
        /// </summary>
        /// <param name="user">The user to ban.</param>
        /// <param name="reason">The reason to ban this user.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [RequireStaff]
        [Command("ban", RunMode = RunMode.Async)]
        public Task Ban(SocketGuildUser user, [Remainder] string reason)
            => this.ExecuteActionInternal(user, reason, InfractionType.Kick);

        /// <summary>
        /// Mutes a user.
        /// </summary>
        /// <param name="user">The user to mute.</param>
        /// <param name="duration">The duration to mute the user.</param>
        /// <param name="reason">The reason to mute this user.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [RequireStaff]
        [Command("mute", RunMode = RunMode.Async)]
        public Task Mute(SocketGuildUser user, string duration, [Remainder] string reason)
        {
            var timespan = duration.ToTimespan();
            return this.ExecuteActionInternal(user, reason, InfractionType.Mute);
        }

        private async Task ExecuteActionInternal(SocketGuildUser user, string reason, InfractionType type, TimeSpan? time = null)
        {
            var result = await this.ModerationHandler.CreateInfractionAsync(type, user.Id, this.Context.User, reason, time);

            if (result)
            {
                await this.ReplyAsync(embed: this.GetSuccessEmbed(type, user, this.Context.User, reason, time));
            }
            else
            {
                await this.ReplyAsync(embed: Embeds.GetErrorEmbed("Action failed", $"Failed to {type} {user}"));
            }
        }

        private string FormatType(InfractionType type)
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
    }
}
