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
            Client.MessageReceived += OnMessageReceived;

            _commandService.CommandExecuted += CommandExecutedAsync;
            await _commandService.AddModulesAsync(Assembly.GetEntryAssembly(), _serviceProvider);
        }

        private async Task OnMessageReceived(SocketMessage incomingMessage)
        {
            if (incomingMessage is not SocketUserMessage message)
            {
                return;
            }

            if (message.Source != MessageSource.User)
            {
                return;
            }

            int argPos = 0;
            if (!message.HasStringPrefix(Configuration["Prefix"], ref argPos) && !message.HasMentionPrefix(Client.CurrentUser, ref argPos))
            {
                return;
            }

            var context = new SocketCommandContext(Client, message);
            await _commandService.ExecuteAsync(context, argPos, _serviceProvider);
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