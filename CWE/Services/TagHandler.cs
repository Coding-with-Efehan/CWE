namespace CWE.Services
{
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;
    using CWE.Data;
    using Discord;
    using Discord.WebSocket;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// The handler for inline-tags.
    /// </summary>
    public class TagHandler : CWEService
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TagHandler"/> class.
        /// </summary>
        /// <param name="client">The <see cref="DiscordSocketClient"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        /// <param name="logger">The <see cref="ILogger"/> to inject.</param>
        /// <param name="dataAccessLayer">The <see cref="DataAccessLayer"/> to inject.</param>
        public TagHandler(
            DiscordSocketClient client,
            IConfiguration configuration,
            ILogger<TagHandler> logger,
            DataAccessLayer dataAccessLayer)
            : base(client, logger, configuration, dataAccessLayer)
        {
        }

        /// <inheritdoc/>
        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Client.MessageReceived += OnMessageReceived;
            return Task.CompletedTask;
        }

        private Task OnMessageReceived(SocketMessage incomingMessage)
        {
            Task.Run(async () =>
            {
                if (incomingMessage is not SocketUserMessage message)
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
                    if (string.IsNullOrWhiteSpace(content))
                    {
                        return;
                    }

                    var match = new Regex(@"\$(\S+)\b").Match(content);
                    if (!match.Success)
                    {
                        return;
                    }

                    var tagName = match.Groups[1].Value;
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
            });
            return Task.CompletedTask;
        }
    }
}
