namespace CWE.Services
{
    using System.Threading;
    using System.Threading.Tasks;
    using CWE.Common;
    using CWE.Data;
    using CWE.Data.Models;
    using CWE.Modules;
    using Discord;
    using Discord.WebSocket;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The handler for interactions, e.g. handling requests.
    /// </summary>
    public class InteractionsHandler : CWEService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InteractionsHandler"/> class.
        /// </summary>
        /// <param name="client">The <see cref="DiscordSocketClient"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        /// <param name="logger">The <see cref="ILogger"/> to inject.</param>
        /// <param name="dataAccessLayer">The <see cref="DataAccessLayer"/> to inject.</param>
        public InteractionsHandler(
            DiscordSocketClient client,
            IConfiguration configuration,
            ILogger<InteractionsHandler> logger,
            DataAccessLayer dataAccessLayer)
            : base(client, logger, configuration, dataAccessLayer)
        {
        }

        /// <inheritdoc/>
        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Client.InteractionCreated += OnInteractionCreated;
            return Task.CompletedTask;
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
    }
}
