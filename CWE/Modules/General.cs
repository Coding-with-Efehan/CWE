namespace CWE.Modules
{
    using System;
    using System.Linq;
    using System.Text;
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
        /// The command used to get all tags.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("tags")]
        public async Task GetTags()
        {
            var tags = await this.DataAccessLayer.GetTags();

            if (tags.Count() == 0)
            {
                var noTags = Embeds.GetErrorEmbed("No tags found", "This server doesn't have any tags yet.");
                await this.Context.Channel.SendMessageAsync(embed: noTags);
                return;
            }

            string description = string.Join(", ", tags.Select(x => x.Name));

            var list = Embeds.GetInformationEmbed($"Tags ({tags.Count()})", description, $"Use \"{this.Configuration.GetValue<string>("Prefix")}t name\" to view a tag");
            await this.Context.Channel.SendMessageAsync(embed: list);
        }

        /// <summary>
        /// The command used to get, create, modify and delete a tag.
        /// </summary>
        /// <param name="argument">A string argument that is later converted to an array of strings.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("tag")]
        [Alias("t")]
        public async Task TagAsync([Remainder] string argument)
        {
            var arguments = argument.Split(" ");

            if (arguments.Count() == 1 && arguments[0] != "create" && arguments[0] != "edit" && arguments[0] != "transfer" && arguments[0] != "delete")
            {
                var tag = await this.DataAccessLayer.GetTag(arguments[0]);
                if (tag == null)
                {
                    var embed = Embeds.GetErrorEmbed("Not found", "The tag you requested could not be found.");
                    await this.Context.Channel.SendMessageAsync(embed: embed);
                    return;
                }

                await this.Context.Channel.SendMessageAsync(tag.Content);
                return;
            }

            var socketGuildUser = this.Context.User as SocketGuildUser;

            switch (arguments[0])
            {
                case "create":
                    var tag = await this.DataAccessLayer.GetTag(arguments[1]);
                    if (tag != null)
                    {
                        var embed = Embeds.GetErrorEmbed("Already exists", "There already exists a tag with that name.");
                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    if (!this.Context.User.IsPromoted())
                    {
                        var embed = Embeds.GetErrorEmbed("Access denied", "You need to be a regular, associate or administrator in order to create tags.");
                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    await this.DataAccessLayer.CreateTag(arguments[1], this.Context.User.Id, string.Join(" ", arguments.Skip(2)));
                    var created = Embeds.GetSuccessEmbed("Tag created", $"The tag has been created. You can view it by using `{this.Configuration.GetValue<string>("Prefix")}tag {arguments[1]}`.");
                    await this.Context.Channel.SendMessageAsync(embed: created);
                    break;
                case "edit":
                    var foundTag = await this.DataAccessLayer.GetTag(arguments[1]);
                    if (foundTag == null)
                    {
                        var embed = Embeds.GetErrorEmbed("Not found", "That tag could not be found.");
                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    if (foundTag.OwnerId != this.Context.User.Id && !socketGuildUser.GuildPermissions.Administrator)
                    {
                        var embed = Embeds.GetErrorEmbed("Access denied", "You need to be the owner of this tag or an administrator to edit the content of this tag.");
                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    await this.DataAccessLayer.EditTagContent(arguments[1], string.Join(" ", arguments.Skip(2)));
                    var edited = Embeds.GetSuccessEmbed("Tag content modified", $"The content of the tag was successfully modified.");
                    await this.Context.Channel.SendMessageAsync(embed: edited);
                    break;
                case "transfer":
                    var tagToTransfer = await this.DataAccessLayer.GetTag(arguments[1]);
                    if (tagToTransfer == null)
                    {
                        var embed = Embeds.GetErrorEmbed("Not found", "That tag could not be found.");
                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    if (!MentionUtils.TryParseUser(arguments[2], out ulong userId) || this.Context.Guild.GetUser(userId) == null)
                    {
                        var embed = Embeds.GetErrorEmbed("Invalid user", "Please provide a valid user.");
                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    if (tagToTransfer.OwnerId != this.Context.User.Id && !socketGuildUser.GuildPermissions.Administrator)
                    {
                        var embed = Embeds.GetErrorEmbed("Access denied", "You need to be the owner of this tag or an administrator to transfer the ownership of this tag.");
                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    await this.DataAccessLayer.EditTagOwner(arguments[1], userId);
                    var success = Embeds.GetSuccessEmbed("Tag ownership transferred", $"The ownership of the tag has been transferred to <@{userId}>.");
                    await this.Context.Channel.SendMessageAsync(embed: success);
                    break;
                case "delete":
                    var tagToDelete = await this.DataAccessLayer.GetTag(arguments[1]);
                    if (tagToDelete == null)
                    {
                        var embed = Embeds.GetErrorEmbed("Not found", "That tag could not be found.");
                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    if (tagToDelete.OwnerId != this.Context.User.Id && !socketGuildUser.GuildPermissions.Administrator)
                    {
                        var embed = Embeds.GetErrorEmbed("Access denied", "You need to be the owner of this tag or an administrator to delete this tag.");
                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    await this.DataAccessLayer.DeleteTag(arguments[1]);
                    var deleted = Embeds.GetSuccessEmbed("Tag deleted", $"The tag was successfully deleted.");
                    await this.Context.Channel.SendMessageAsync(embed: deleted);
                    break;
            }
        }
    }
}
