namespace CWE.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using CWE.Common;
    using CWE.Data;
    using CWE.Data.Models;
    using CWE.Modules;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Class responsible for handling all commands and various events.
    /// </summary>
    public class CommandHandler : CWEService
    {
        /// <summary>
        /// Bool indicating whether or not requests should be submittable.
        /// Should be moved to database.
        /// </summary>
        public static bool Requests;

        private readonly CommandService _commandService;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandler"/> class.
        /// </summary>
        /// <param name="client">The <see cref="DiscordSocketClient"/> to inject.</param>
        /// <param name="commandService">The <see cref="CommandService"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        /// <param name="logger">The <see cref="ILogger"/> to inject.</param>
        /// <param name="dataAccessLayer">The <see cref="DataAccessLayer"/> to inject.</param>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to inject.</param>
        public CommandHandler(
            DiscordSocketClient client,
            CommandService commandService,
            IConfiguration configuration,
            ILogger<CommandHandler> logger,
            DataAccessLayer dataAccessLayer,
            IServiceProvider serviceProvider)
            : base(client, logger, configuration, dataAccessLayer)
        {
            _commandService = commandService;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Client.MessageReceived += HandleMessage;
            Client.InteractionCreated += OnInteractionCreated;
            Client.Ready += OnReady;
            Client.UserJoined += OnUserJoined;
            Client.UserLeft += OnUserLeft;

            _commandService.CommandExecuted += CommandExecutedAsync;
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }

        private async Task OnInteractionCreated(SocketInteraction socketInteraction)
        {
            if (socketInteraction.Type == InteractionType.MessageComponent)
            {
                var socketMessageComponent = (SocketMessageComponent)socketInteraction;
                var request = await DataAccessLayer.GetRequest(socketMessageComponent.Message.Id);

                if (request == null)
                {
                    return;
                }

                var guild = (socketMessageComponent.Channel as SocketGuildChannel).Guild;
                var requestMessage = Configuration.GetSection("Messages").GetValue<ulong>("ActiveRequest");
                var informationChannel = guild.GetTextChannel(Configuration.GetSection("Channels").GetValue<ulong>("Information"));
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
                                Logger.LogInformation($"Failed to send DM to {initiator.Username}#{initiator.Discriminator}.");
                            }

                            await DataAccessLayer.UpdateRequest(socketMessageComponent.Message.Id, RequestState.Denied);

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
                                Logger.LogInformation($"Failed to send DM to {initiator.Username}#{initiator.Discriminator}.");
                            }

                            await DataAccessLayer.UpdateRequest(socketMessageComponent.Message.Id, RequestState.Active);

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
                                Logger.LogInformation($"Failed to send DM to {initiator.Username}#{initiator.Discriminator}.");
                            }

                            await DataAccessLayer.UpdateRequest(socketMessageComponent.Message.Id, RequestState.Finished);

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
            await Client.SetGameAsync($"{Client.Guilds.FirstOrDefault().Users.Where(x => !x.IsBot).Count()} programmers", null, ActivityType.Watching);
        }

        private async Task OnUserJoined(SocketGuildUser user)
        {
            var guild = Client.GetGuild(Configuration.GetValue<ulong>("Guild"));
            var autoRoles = await DataAccessLayer.GetAutoRoles();
            var roles = new List<IRole>();
            foreach (var current in autoRoles)
            {
                var currentRole = guild.GetRole(current.Id);
                if (currentRole == null)
                {
                    await DataAccessLayer.DeleteAutoRole(current.Id);
                    continue;
                }

                roles.Add(currentRole);
            }

            await Client.SetGameAsync($"{Client.Guilds.FirstOrDefault().Users.Where(x => !x.IsBot).Count()} programmers", null, ActivityType.Watching);

            if (user.Guild != guild)
            {
                return;
            }

            await user.AddRolesAsync(roles);
        }

        private async Task OnUserLeft(SocketGuildUser user)
        {
            await Client.SetGameAsync($"{Client.Guilds.FirstOrDefault().Users.Where(x => !x.IsBot).Count()} programmers", null, ActivityType.Watching);
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

            if (message.Content.Contains("$"))
            {
                var content = Regex.Replace(message.Content, @"(`{1,3}).*?(.\1)", string.Empty, RegexOptions.Singleline);
                content = Regex.Replace(content, "^>.*$", string.Empty, RegexOptions.Multiline);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var match = new Regex(@"\$(\S+)\b").Match(content);
                    if (match.Success)
                    {
                        await HandleTag(message, match);
                    }
                }
            }

            int argPos = 0;
            if (!message.HasStringPrefix(Configuration["Prefix"], ref argPos) && !message.HasMentionPrefix(Client.CurrentUser, ref argPos))
            {
                return;
            }

            var context = new SocketCommandContext(Client, message);
            await _commandService.ExecuteAsync(context, argPos, _serviceProvider);
        }

        private async Task HandleTag(SocketUserMessage message, Match regexMatch)
        {
            var tagName = regexMatch.Groups[1].Value;
            if (string.IsNullOrWhiteSpace(tagName))
            {
                return;
            }

            var tag = await DataAccessLayer.GetTag(tagName);
            if (tag == null)
            {
                return;
            }

            await message.Channel.SendMessageAsync(tag.Content);
        }

        private async Task CommandExecutedAsync(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (!command.IsSpecified || result.IsSuccess)
            {
                return;
            }

            string title = string.Empty;
            string description = string.Empty;

            switch (result.Error)
            {
                case CommandError.BadArgCount:
                    title = "Invalid use of command";
                    description = "Please provide the correct amount of parameters.";
                    break;
                case CommandError.MultipleMatches:
                    title = "Invalid argument";
                    description = "Please provide a valid argument.";
                    break;
                case CommandError.ObjectNotFound:
                    title = "Not found";
                    description = "The argument that was provided could not be found.";
                    break;
                case CommandError.ParseFailed:
                    title = "Invalid argument";
                    description = "The argument that you provided could not be parsed correctly.";
                    break;
                case CommandError.UnmetPrecondition:
                    title = "Access denied";
                    description = "You or the bot does not meet the required preconditions.";
                    break;
                default:
                    title = "An error occurred";
                    description = "An error occurred while trying to run this command.";
                    break;
            }

            var error = new CWEEmbedBuilder()
                .WithTitle(title)
                .WithDescription(description)
                .WithStyle(EmbedStyle.Error)
                .Build();

            await context.Channel.SendMessageAsync(embed: error);
        }
    }
}