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
    using Interactivity;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The requests module, used to create and toggle requests.
    /// </summary>
    [Name("Requests")]
    public class RequestsModule : CWEModuleBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestsModule"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        /// <param name="interactivityService">The <see cref="InteractivityService"/> to inject.</param>
        public RequestsModule(IServiceProvider serviceProvider, IConfiguration configuration, InteractivityService interactivityService)
                : base(serviceProvider, configuration, interactivityService)
        {
        }

        /// <summary>
        /// Creates a new <see cref="Embed"/> for a <see cref="Request"/>.
        /// </summary>
        /// <param name="request">The <see cref="Request"/> that the embed is made for.</param>
        /// <returns>An <see cref="Embed"/> for a <see cref="Request"/>.</returns>
        public static Embed GetRequestEmbed(Request request)
        {
            var builder = new EmbedBuilder();

            switch (request.State)
            {
                case RequestState.Pending:
                    builder
                        .WithAuthor(x =>
                        {
                            x
                            .WithIconUrl(Icons.NewRequest)
                            .WithName("New request");
                        })
                        .WithColor(Colors.Information);
                    break;
                case RequestState.Active:
                    builder
                        .WithAuthor(x =>
                        {
                            x
                            .WithIconUrl(Icons.ActiveRequest)
                            .WithName("Active request");
                        })
                        .WithColor(Colors.Active);
                    break;
                case RequestState.Finished:
                    builder
                        .WithAuthor(x =>
                        {
                            x
                            .WithIconUrl(Icons.Success)
                            .WithName("Finished request");
                        })
                        .WithColor(Colors.Success);
                    break;
                case RequestState.Denied:
                    builder
                        .WithAuthor(x =>
                        {
                            x
                            .WithIconUrl(Icons.DeniedRequest)
                            .WithName("Denied request");
                        })
                        .WithColor(Colors.Error);
                    break;
            }

            builder
                .AddField("Initiator", $"<@{request.Initiator}>", true)
                .AddField("Description", request.Description);

            return builder.Build();
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
                var error = new CWEEmbedBuilder()
                    .WithTitle("Requests disabled")
                    .WithDescription($"Requests are currently disabled, please come back later.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await this.Context.Channel.SendMessageAsync(embed: error);
                return;
            }

            var socketGuildUser = this.Context.User as SocketGuildUser;
            if (socketGuildUser.Roles.All(x => x.Name != "Patron"))
            {
                var error = new CWEEmbedBuilder()
                    .WithTitle("Not a Patron")
                    .WithDescription($"This command can only be used by patrons. Take a look at [our Patreon page](https://www.patreon.com/codingwithefehan) to become a patron.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await this.Context.Channel.SendMessageAsync(embed: error);
                return;
            }

            var request = new Request
            {
                Description = description,
                Initiator = this.Context.User.Id,
            };

            var channel = this.Context.Guild.GetTextChannel(this.Configuration.GetSection("Channels").GetValue<ulong>("Requests"));
            var requestEmbed = GetRequestEmbed(request);
            var component = new ComponentBuilder()
                .WithButton("Deny", "deny", ButtonStyle.Danger)
                .WithButton("Switch", "switch", ButtonStyle.Success)
                .Build();

            var message = await channel.SendMessageAsync(embed: requestEmbed, component: component);
            request.MessageId = message.Id;

            try
            {
                await this.DataAccessLayer.CreateRequest(request);
                var success = new CWEEmbedBuilder()
                    .WithTitle("Request sent!")
                    .WithDescription($"Your request has been sent!")
                    .WithStyle(EmbedStyle.Success)
                    .Build();

                await this.Context.Channel.SendMessageAsync(embed: success);
            }
            catch
            {
                await message.DeleteAsync();
                var error = new CWEEmbedBuilder()
                    .WithTitle("Error")
                    .WithDescription($"An error occurred while sending your request.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await this.Context.Channel.SendMessageAsync(embed: error);
            }
        }

        /// <summary>
        /// The command used to toggle on or off the ability to send requests.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("togglerequests")]
        [RequireOwner]
        public async Task ToggleRequests()
        {
            CommandHandler.Requests = !CommandHandler.Requests;
            var success = new CWEEmbedBuilder()
                    .WithTitle((CommandHandler.Requests ? "Enabled" : "Disabled") + " requests")
                    .WithDescription($"Successfully {(CommandHandler.Requests ? "enabled" : "disabled")} requests!")
                    .WithStyle(EmbedStyle.Success)
                    .Build();

            await this.Context.Channel.SendMessageAsync(embed: success);
        }
    }
}
