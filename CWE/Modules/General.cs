namespace CWE.Modules
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using CWE.Common;
    using CWE.Data.Models;
    using CWE.Services;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The general module, containing commands related to campaigns and requests.
    /// </summary>
    public class General : CWEModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="General"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        public General(IServiceProvider serviceProvider, IConfiguration configuration)
                : base(serviceProvider, configuration)
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

            var response = await this.NextMessageAsync(timeout: TimeSpan.FromMinutes(2));
            if (response == null)
            {
                await this.Context.Channel.SendMessageAsync("You waited too long, causing the campaign to automatically be cancelled.");
                return;
            }

            double total = this.Context.Guild.Users.Where(x => x.Roles.Any(x => x.Name == "Associate") || x.Roles.Any(x => x.Name == "Regular") || x.Roles.Any(x => x.Name == "Staff")).Count();
            int minimal = (int)Math.Ceiling(total / 2);

            var campaign = new Campaign() { User = user.Id, Initiator = this.Context.User.Id, Reason = response.Content, Start = DateTime.Now, End = DateTime.Now + TimeSpan.FromDays(2), Type = type, Minimal = minimal };
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
        /// The command used to create a new request.
        /// </summary>
        /// <param name="description">The description of the request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("request", RunMode = RunMode.Async)]
        public async Task Request([Remainder] string description)
        {
            if (CommandHandler.Requests == false)
            {
                var error = Embeds.GetErrorEmbed("Requests disabled", $"Requests are currently disabled, please come back later.");
                await this.Context.Channel.SendMessageAsync(embed: error);
                return;
            }

            var socketGuildUser = this.Context.User as SocketGuildUser;
            if (socketGuildUser.Roles.All(x => x.Name != "Patron"))
            {
                var error = Embeds.GetErrorEmbed("Not a Patron", $"This command can only be used by patrons. Take a look at [our Patreon page](https://www.patreon.com/codingwithefehan) to become a patron.");
                await this.Context.Channel.SendMessageAsync(embed: error);
                return;
            }

            var request = new Request
            {
                Description = description,
                Initiator = this.Context.User.Id,
            };

            var channel = this.Context.Guild.GetTextChannel(this.Configuration.GetSection("Channels").GetValue<ulong>("Requests"));
            var requestEmbed = Embeds.GetRequestEmbed(request);
            var component = new ComponentBuilder()
                .WithButton("Deny", "deny", ButtonStyle.Danger)
                .WithButton("Switch", "switch", ButtonStyle.Success)
                .Build();

            var message = await channel.SendMessageAsync(embed: requestEmbed, component: component);
            request.MessageId = message.Id;

            try
            {
                await this.DataAccessLayer.CreateRequest(request);
                var success = Embeds.GetSuccessEmbed("Request sent!", $"Your request has been sent!");
                await this.Context.Channel.SendMessageAsync(embed: success);
            }
            catch
            {
                await message.DeleteAsync();
                var error = Embeds.GetErrorEmbed("Error", $"An error occurred while sending your request.");
                await this.Context.Channel.SendMessageAsync(embed: error);
            }
        }

        /// <summary>
        /// The command used to toggle on or off the ability to send requests.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("togglerequests")]
        [RequireOwner]
        public async Task Requests()
        {
            CommandHandler.Requests = !CommandHandler.Requests;
            var success = Embeds.GetSuccessEmbed((CommandHandler.Requests ? "Enabled" : "Disabled") + " requests", $"Successfully {(CommandHandler.Requests ? "enabled" : "disabled")} requests!");
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

        /// <summary>
        /// Command used to call a tag.
        /// </summary>
        /// <param name="tagName">The tag to execute.</param>
        /// <returns>The execution of a tag, <see cref="Task"/>.</returns>
        [Command("tag")]
        public async Task ExecuteTag(string tagName)
        {
            var tag = await this.DataAccessLayer.FetchTagAsync(tagName);
            await this.ReplyAsync(tag.Content);
        }

        /// <summary>
        /// Command used to create a tag.
        /// </summary>
        /// <param name="tagName">The tag's name.</param>
        /// <param name="content">The content that the tag should hold.</param>
        /// <returns>The creation of a tag, <see cref="Task"/>.</returns>
        [Command("tag create")]
        [RequireTagAuthoriazation]
        public async Task CreateTag(string tagName, [Remainder] string content)
        {
            var tag = await this.DataAccessLayer.FetchTagAsync(tagName);
            if (tag != null)
            {
                throw new ArgumentException("The tag provided already exists, so I can't create one with the matching name.");
            }

            await this.DataAccessLayer.CreateTagAsync(tagName, this.Context.User.Id, content);
        }

        /// <summary>
        /// Command used to transfer ownership of a tag.
        /// </summary>
        /// <param name="tagName">The name of the tag to transfer.</param>
        /// <param name="newOwnerId">The new owner's ID value.</param>
        /// <returns>The ownership action of transering a tag. <see cref="Task"/>.</returns>
        [Command("tag transfer")]
        [RequireTagAuthoriazation]
        public async Task TranserTag(string tagName, ulong newOwnerId)
        {
            // The method already handles verification, so no need to check here.
            await this.DataAccessLayer.TransferTagOwnershipAsync(tagName, this.Context.User.Id, newOwnerId);
        }

        /// <summary>
        /// The command used to edit a currently existing tag's response.
        /// </summary>
        /// <param name="tagName">The tag to edit.</param>
        /// <param name="newContent">The content that should be put in place of the old content.</param>
        /// <returns>Transfership of a tag<see cref="Task"/>.</returns>
        [Command("tag edit")]
        [RequireTagAuthoriazation]
        public async Task EditTag(string tagName, [Remainder] string newContent)
        {
            await this.DataAccessLayer.EditTagContentAsync(tagName, this.Context.User.Id, newContent);
        }
    }
}
