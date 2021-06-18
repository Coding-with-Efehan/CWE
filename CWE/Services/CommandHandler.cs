namespace CWE.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Threading.Tasks;
    using CWE.Common;
    using CWE.Data;
    using CWE.Data.Models;
    using CWE.Modules;
    using Discord;
    using Discord.Addons.Hosting;
    using Discord.Commands;
    using Discord.WebSocket;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class responsible for handling all commands and various events.
    /// </summary>
    public class CommandHandler : InitializedService
    {
        /// <summary>
        /// Bool indicating whether or not requests should be submittable.
        /// Should be moved to database.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "Is used across application.")]
        public static bool Requests;

        private readonly IServiceProvider provider;
        private readonly DiscordSocketClient client;
        private readonly CommandService commandService;
        private readonly IConfiguration configuration;
        private readonly ILogger<CommandHandler> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandler"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="IServiceProvider"/> to inject.</param>
        /// <param name="client">The <see cref="DiscordSocketClient"/> to inject.</param>
        /// <param name="commandService">The <see cref="CommandService"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        /// <param name="logger">The <see cref="ILogger"/> to inject.</param>
        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService commandService, IConfiguration configuration, ILogger<CommandHandler> logger)
        {
            this.provider = provider;
            this.client = client;
            this.commandService = commandService;
            this.configuration = configuration;
            this.logger = logger;
        }

        /// <inheritdoc/>
        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            this.client.MessageReceived += this.HandleMessage;
            this.client.ReactionAdded += this.OnReactionAdded;
            this.client.InteractionCreated += this.OnInteractionCreated;
            this.client.Ready += this.OnReady;
            this.client.UserJoined += this.OnUserJoined;
            this.client.UserLeft += this.OnUserLeft;

            var campaignHandler = new Task(async () => await this.CampaignHandler());
            campaignHandler.Start();

            this.commandService.CommandExecuted += this.CommandExecutedAsync;
            await this.commandService.AddModulesAsync(Assembly.GetEntryAssembly(), this.provider);
        }

        private async Task OnInteractionCreated(SocketInteraction socketInteraction)
        {
            if (socketInteraction.Type == InteractionType.MessageComponent)
            {
                var socketMessageComponent = (SocketMessageComponent)socketInteraction;
                var request = await this.GetRequest(socketMessageComponent.Message.Id);

                if (request == null)
                {
                    return;
                }

                var guild = (socketMessageComponent.Channel as SocketGuildChannel).Guild;
                var requestMessage = this.configuration.GetSection("Messages").GetValue<ulong>("ActiveRequest");
                var informationChannel = guild.GetTextChannel(this.configuration.GetSection("Channels").GetValue<ulong>("Information"));
                var initiator = guild.GetUser(request.Initiator);

                switch (request.State)
                {
                    case RequestState.Pending:
                        if (socketMessageComponent.Data.CustomId == "deny")
                        {
                            request.State = RequestState.Denied;
                            var embed = RequestsModule.GetRequestEmbed(request);
                            var component = new ComponentBuilder().Build();

                            await (socketMessageComponent.Message as IUserMessage).ModifyAsync(x =>
                            {
                                x.Components = component;
                                x.Embed = embed;
                            });

                            try
                            {
                                await initiator.SendMessageAsync($"Hello there! Your request \"{request.Description}\" has been denied. You can always try to create a new request. Make sure your request is in compliance with the rules mentioned in {informationChannel.Mention}.");
                            }
                            catch
                            {
                                this.logger.LogInformation($"Failed to send DM to {initiator.Username}#{initiator.Discriminator}.");
                            }

                            await this.UpdateRequest(socketMessageComponent.Message.Id, RequestState.Denied);

                            return;
                        }
                        else
                        {
                            request.State = RequestState.Active;
                            var embed = RequestsModule.GetRequestEmbed(request);
                            var component = new ComponentBuilder()
                                .WithButton("Finish", "finish", ButtonStyle.Success)
                                .Build();

                            await (socketMessageComponent.Message as IUserMessage).ModifyAsync(x =>
                            {
                                x.Components = component;
                                x.Embed = embed;
                            });

                            try
                            {
                                await initiator.SendMessageAsync($"Hello there! Your request \"{request.Description}\" is now active!");
                            }
                            catch
                            {
                                this.logger.LogInformation($"Failed to send DM to {initiator.Username}#{initiator.Discriminator}.");
                            }

                            await this.UpdateRequest(socketMessageComponent.Message.Id, RequestState.Active);

                            var activeRequest = new EmbedBuilder()
                                .WithAuthor(x =>
                                {
                                    x
                                    .WithIconUrl(Icons.ActiveRequest)
                                    .WithName("Active request");
                                })
                                .WithColor(Colors.Active)
                                .WithDescription(request.Description)
                                .WithFooter($"Requested by {(initiator == null ? "Unknown" : initiator.Username + "#" + initiator.Discriminator)}")
                                .Build();

                            var message = await informationChannel.GetMessageAsync(requestMessage) as IUserMessage;
                            await message.ModifyAsync(x => x.Embed = activeRequest);

                            return;
                        }

                    case RequestState.Active:
                        if (socketMessageComponent.Data.CustomId == "finish")
                        {
                            request.State = RequestState.Finished;
                            var embed = RequestsModule.GetRequestEmbed(request);
                            var component = new ComponentBuilder().Build();

                            await (socketMessageComponent.Message as IUserMessage).ModifyAsync(x =>
                            {
                                x.Components = component;
                                x.Embed = embed;
                            });

                            try
                            {
                                await initiator.SendMessageAsync($"Hello there! Your request \"{request.Description}\" has just been finished. Thank you for sending your request, feel free to send more.");
                            }
                            catch
                            {
                                this.logger.LogInformation($"Failed to send DM to {initiator.Username}#{initiator.Discriminator}.");
                            }

                            await this.UpdateRequest(socketMessageComponent.Message.Id, RequestState.Finished);

                            var activeRequest = new EmbedBuilder()
                            .WithAuthor(x =>
                            {
                                x
                                .WithIconUrl(Icons.ActiveRequest)
                                .WithName("No active request");
                            })
                            .WithColor(Colors.Active)
                            .Build();

                            var message = await informationChannel.GetMessageAsync(requestMessage) as IUserMessage;
                            await message.ModifyAsync(x => x.Embed = activeRequest);

                            return;
                        }

                        break;
                }
            }
        }

        private async Task OnReady()
        {
            await this.client.SetGameAsync($"{this.client.Guilds.FirstOrDefault().Users.Where(x => !x.IsBot).Count()} programmers", null, ActivityType.Watching);
        }

        private async Task OnUserJoined(SocketGuildUser user)
        {
            var guild = this.client.GetGuild(this.configuration.GetValue<ulong>("Guild"));
            await this.client.SetGameAsync($"{this.client.Guilds.FirstOrDefault().Users.Where(x => !x.IsBot).Count()} programmers", null, ActivityType.Watching);

            if (user.Guild != guild)
            {
                return;
            }

            var role = guild.Roles.FirstOrDefault(x => x.Name == "Member");
            await user.AddRoleAsync(role);
        }

        private async Task OnUserLeft(SocketGuildUser user)
        {
            await this.client.SetGameAsync($"{this.client.Guilds.FirstOrDefault().Users.Where(x => !x.IsBot).Count()} programmers", null, ActivityType.Watching);
        }

        private async Task<IEnumerable<Campaign>> GetCampaigns()
        {
            using var scope = this.provider.CreateScope();
            var dataAccessLayer = scope.ServiceProvider.GetRequiredService<DataAccessLayer>();
            return await dataAccessLayer.GetCampaigns();
        }

        private async Task DeleteCampaign(ulong id)
        {
            using var scope = this.provider.CreateScope();
            var dataAccessLayer = scope.ServiceProvider.GetRequiredService<DataAccessLayer>();
            await dataAccessLayer.DeleteCampaign(id);
        }

        private async Task<IEnumerable<Request>> GetRequests()
        {
            using var scope = this.provider.CreateScope();
            var dataAccessLayer = scope.ServiceProvider.GetRequiredService<DataAccessLayer>();
            return await dataAccessLayer.GetRequests();
        }

        private async Task<Request> GetRequest(ulong messageId)
        {
            using var scope = this.provider.CreateScope();
            var dataAccessLayer = scope.ServiceProvider.GetRequiredService<DataAccessLayer>();
            return await dataAccessLayer.GetRequest(messageId);
        }

        private async Task UpdateRequest(ulong messageId, RequestState state)
        {
            using var scope = this.provider.CreateScope();
            var dataAccessLayer = scope.ServiceProvider.GetRequiredService<DataAccessLayer>();
            await dataAccessLayer.UpdateRequest(messageId, state);
        }

        private async Task DeleteRequest(ulong messageId)
        {
            using var scope = this.provider.CreateScope();
            var dataAccessLayer = scope.ServiceProvider.GetRequiredService<DataAccessLayer>();
            await dataAccessLayer.DeleteRequest(messageId);
        }

        private async Task CampaignHandler()
        {
            while (true)
            {
                var campaigns = await this.GetCampaigns();
                foreach (var campaign in campaigns)
                {
                    if (DateTime.Now < campaign.End)
                    {
                        continue;
                    }

                    var msg = await this.client.GetGuild(this.configuration.GetValue<ulong>("Guild")).GetTextChannel(this.configuration.GetSection("Channels").GetValue<ulong>("Campaigns")).GetMessageAsync(campaign.Message) as IUserMessage;
                    if (msg != null)
                    {
                        var denied = CampaignsModule.GetDeniedEmbed(campaign);
                        await msg.ModifyAsync(x => x.Embed = denied);
                        await msg.RemoveAllReactionsAsync();
                    }

                    await this.DeleteCampaign(campaign.User);
                }

                await Task.Delay(60 * 60 * 1000);
            }
        }

        private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            var campaigns = await this.GetCampaigns();
            if (!campaigns.Any(x => x.Message == arg3.MessageId))
            {
                return;
            }

            var campaign = campaigns.FirstOrDefault(x => x.Message == arg3.MessageId);

            if (arg3.UserId == campaign.Initiator)
            {
                return;
            }

            var msg = await arg3.Channel.GetMessageAsync(campaign.Message) as IUserMessage;

            int current = msg.Reactions.FirstOrDefault(x => x.Key.Name == "✅").Value.ReactionCount;

            if (current < campaign.Minimal)
            {
                return;
            }

            var accepted = CampaignsModule.GetAcceptedEmbed(campaign);
            await msg.ModifyAsync(x => x.Embed = accepted);
            await msg.RemoveAllReactionsAsync();

            var user = (msg.Channel as SocketGuildChannel).Guild.GetUser(campaign.User);
            if (campaign.Type == CampaignType.Regular)
            {
                await user.AddRoleAsync(user.Guild.GetRole(this.configuration.GetSection("Roles").GetValue<ulong>("Regular")));
            }
            else
            {
                await user.RemoveRoleAsync(user.Guild.GetRole(this.configuration.GetSection("Roles").GetValue<ulong>("Regular")));
                await user.AddRoleAsync(user.Guild.GetRole(this.configuration.GetSection("Roles").GetValue<ulong>("Associate")));
            }

            await this.DeleteCampaign(campaign.User);
        }

        private async Task HandleMessage(SocketMessage incomingMessage)
        {
            if (!(incomingMessage is SocketUserMessage message))
            {
                return;
            }

            if (message.Source != MessageSource.User)
            {
                return;
            }

            int argPos = 0;
            if (!message.HasStringPrefix(this.configuration["Prefix"], ref argPos) && !message.HasMentionPrefix(this.client.CurrentUser, ref argPos))
            {
                return;
            }

            var context = new SocketCommandContext(this.client, message);
            await this.commandService.ExecuteAsync(context, argPos, this.provider);
        }

        private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified || result.IsSuccess)
            {
                return;
            }

            await context.Channel.SendMessageAsync($"Error: {result}");
        }
    }
}