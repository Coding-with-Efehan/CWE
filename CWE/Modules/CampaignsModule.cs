namespace CWE.Modules
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using CWE.Common;
    using CWE.Data.Models;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using Interactivity;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The campaigns module, used to create and manage campaigns.
    /// </summary>
    [Name("Campaigns")]
    public class CampaignsModule : CWEModuleBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CampaignsModule"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        /// <param name="interactivityService">The <see cref="InteractivityService"/> to inject.</param>
        public CampaignsModule(IServiceProvider serviceProvider, IConfiguration configuration, InteractivityService interactivityService)
                : base(serviceProvider, configuration, interactivityService)
        {
        }

        /// <summary>
        /// The command used to create new campaigns.
        /// </summary>
        /// <param name="user">The <see cref="SocketGuildUser"/> that the campaign is for.</param>
        /// <param name="type">The <see cref="CampaignType"/>, indicating to what role the user should be promoted.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("campaign", RunMode = RunMode.Async)]
        public async Task Campaign(SocketGuildUser user, CampaignType type)
        {
            if (user.Roles.Any(x => x.Name == type.ToString()))
            {
                var error = Embeds.GetErrorEmbed("User already promoted", $"The user is already promoted to {type.ToString().ToLower()}.");
                await this.Context.Channel.SendMessageAsync(embed: error);
                return;
            }

            if (user.Roles.Any(x => x.Name == "Associate") && type == CampaignType.Regular)
            {
                var error = Embeds.GetErrorEmbed("User already associate", $"Because the user is already an associate, he/she cannot be promoted to a regular.");
                await this.Context.Channel.SendMessageAsync(embed: error);
                return;
            }

            var campaigns = await this.DataAccessLayer.GetCampaigns();
            if (campaigns.Any(x => x.User == user.Id))
            {
                var error = Embeds.GetErrorEmbed("User already in campaign", $"There is already a vote in progress for that user.");
                await this.Context.Channel.SendMessageAsync(embed: error);
                return;
            }

            await this.Context.Channel.SendMessageAsync($"Briefly describe why {user.Mention} should be promoted to {type.ToString().ToLower()}? You have 2 minutes to answer.");

            var response = await this.Interactivity.NextMessageAsync(x => x.Author.Id == this.Context.User.Id, timeout: TimeSpan.FromMinutes(2));
            if (!response.IsSuccess)
            {
                await this.Context.Channel.SendMessageAsync("You waited too long, causing the campaign to automatically be cancelled.");
                return;
            }

            double total = this.Context.Guild.Users.Where(x => x.Roles.Any(x => x.Name == "Associate") || x.Roles.Any(x => x.Name == "Regular") || x.Roles.Any(x => x.Name == "Staff")).Count();
            int minimal = (int)Math.Ceiling(total / 2);

            var campaign = new Campaign() { User = user.Id, Initiator = this.Context.User.Id, Reason = response.Value.Content, Start = DateTime.Now, End = DateTime.Now + TimeSpan.FromDays(2), Type = type, Minimal = minimal };
            var campaignEmbed = Embeds.GetCampaignEmbed(campaign);

            var requestChannelId = this.Configuration.GetSection("Channels").GetValue<ulong>("Campaigns");
            var vote = await this.Context.Guild.GetTextChannel(requestChannelId).SendMessageAsync(embed: campaignEmbed);
            await vote.AddReactionsAsync(new IEmote[] { new Emoji("✅"), new Emoji("❌") });

            campaign.Message = vote.Id;
            await this.DataAccessLayer.CreateCampaign(campaign);

            var success = Embeds.GetSuccessEmbed("Started campaign", $"Successfully launched the campaign in <#{requestChannelId}>!");
            await this.Context.Channel.SendMessageAsync(embed: success);
        }

        /// <summary>
        /// The command used to forcefully accept a campaign.
        /// </summary>
        /// <param name="user">The <see cref="SocketGuildUser"/> in the campaign.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("acceptcampaign", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task AcceptCampaign(SocketGuildUser user)
        {
            var campaigns = await this.DataAccessLayer.GetCampaigns();
            if (campaigns.All(x => x.User != user.Id))
            {
                var error = Embeds.GetErrorEmbed("User not in campaign", $"That user is not in a campaign.");
                await this.Context.Channel.SendMessageAsync(embed: error);
                return;
            }

            var campaign = campaigns.FirstOrDefault(x => x.User == user.Id);
            await this.DataAccessLayer.DeleteCampaign(user.Id);

            try
            {
                var denied = Embeds.GetAcceptedEmbed(campaign, "This campaign was accepted by an administrator.");
                var message = await this.Context.Guild.GetTextChannel(this.Configuration.GetSection("Channels").GetValue<ulong>("Campaigns")).GetMessageAsync(campaign.Message) as IUserMessage;
                await message.ModifyAsync(x => x.Embed = denied);
                await message.RemoveAllReactionsAsync();

                if (campaign.Type == CampaignType.Regular)
                {
                    await user.AddRoleAsync(user.Guild.GetRole(this.Configuration.GetSection("Roles").GetValue<ulong>("Regular")));
                }
                else
                {
                    await user.RemoveRoleAsync(user.Guild.GetRole(this.Configuration.GetSection("Roles").GetValue<ulong>("Regular")));
                    await user.AddRoleAsync(user.Guild.GetRole(this.Configuration.GetSection("Roles").GetValue<ulong>("Associate")));
                }

                var success = Embeds.GetSuccessEmbed("Campaign accepted", $"Successfully accepted the campaign.");
                await this.Context.Channel.SendMessageAsync(embed: success);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                var error = Embeds.GetErrorEmbed("Error while accepting campaign", $"An error occurred while trying to accept that campaign.");
                await this.Context.Channel.SendMessageAsync(embed: error);
            }
        }

        /// <summary>
        /// The command used to forcefully cancel a campaign.
        /// </summary>
        /// <param name="user">The <see cref="SocketGuildUser"/> in the campaign.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("cancelcampaign", RunMode = RunMode.Async)]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task CancelCampaign(SocketGuildUser user)
        {
            var campaigns = await this.DataAccessLayer.GetCampaigns();
            if (campaigns.All(x => x.User != user.Id))
            {
                var error = Embeds.GetErrorEmbed("User not in campaign", $"That user is not in a campaign.");
                await this.Context.Channel.SendMessageAsync(embed: error);
                return;
            }

            var campaign = campaigns.FirstOrDefault(x => x.User == user.Id);
            await this.DataAccessLayer.DeleteCampaign(user.Id);

            try
            {
                var denied = Embeds.GetDeniedEmbed(campaign, "This campaign was cancelled by an administrator.");
                var message = await this.Context.Guild.GetTextChannel(this.Configuration.GetSection("Channels").GetValue<ulong>("Campaigns")).GetMessageAsync(campaign.Message) as IUserMessage;
                await message.ModifyAsync(x => x.Embed = denied);
                await message.RemoveAllReactionsAsync();

                var success = Embeds.GetSuccessEmbed("Campaign cancelled", $"Successfully cancelled the campaign.");
                await this.Context.Channel.SendMessageAsync(embed: success);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                var error = Embeds.GetErrorEmbed("Error while cancelling campaign", $"An error occurred while trying to cancel that campaign.");
                await this.Context.Channel.SendMessageAsync(embed: error);
            }
        }
    }
}
