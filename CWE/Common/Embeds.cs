using CWE.Data.Models;
using Discord;
using Discord.WebSocket;

namespace CWE.Common
{
    /// <summary>
    ///     Embeds used by CWE.
    /// </summary>
    public static class Embeds
    {
        /// <summary>
        ///     Creates a new <see cref="Embed" /> with the success style.
        /// </summary>
        /// <param name="title">The title to be used.</param>
        /// <param name="description">The description to be used.</param>
        /// <returns>An <see cref="Embed" /> with a success style.</returns>
        public static Embed GetSuccessEmbed(string title, string description)
        {
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x
                        .WithIconUrl(Icons.Success)
                        .WithName(title);
                })
                .WithDescription(description)
                .WithColor(Colors.Success)
                .Build();
            return embed;
        }

        /// <summary>
        ///     Creates a new <see cref="Embed" /> with the error style.
        /// </summary>
        /// <param name="title">The title to be used.</param>
        /// <param name="description">The description to be used.</param>
        /// <returns>An <see cref="Embed" /> with an error style.</returns>
        public static Embed GetErrorEmbed(string title, string description)
        {
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x
                        .WithIconUrl(Icons.Error)
                        .WithName(title);
                })
                .WithDescription(description)
                .WithColor(Colors.Error)
                .Build();
            return embed;
        }

        /// <summary>
        ///     Creates a new <see cref="Embed" /> with the information style.
        /// </summary>
        /// <param name="title">The title to be used.</param>
        /// <param name="description">The description to be used.</param>
        /// <returns>An <see cref="Embed" /> with an informative style.</returns>
        public static Embed GetInformationEmbed(string title, string description)
        {
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x
                        .WithIconUrl(Icons.Information)
                        .WithName(title);
                })
                .WithDescription(description)
                .WithColor(Colors.Information)
                .Build();
            return embed;
        }

        /// <summary>
        ///     Creates a new <see cref="Embed" /> for a <see cref="Request" />.
        /// </summary>
        /// <param name="request">The <see cref="Request" /> that the embed is made for.</param>
        /// <returns>An <see cref="Embed" /> for a <see cref="Request" />.</returns>
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
        ///     Creates a new <see cref="Embed" /> for a <see cref="Campaign" />.
        /// </summary>
        /// <param name="campaign">The <see cref="Campaign" /> that the embed is made for.</param>
        /// <returns>An <see cref="Embed" /> for a <see cref="Campaign" />.</returns>
        public static Embed GetCampaignEmbed(Campaign campaign)
        {
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x
                        .WithIconUrl(Icons.NewCampaign)
                        .WithName("New campaign");
                })
                .AddField("Initiator", $"<@{campaign.Initiator}>", true)
                .AddField("Member", $"<@{campaign.User}>", true)
                .AddField("Type", campaign.Type, true)
                .AddField("Reason", campaign.Reason)
                .AddField("Voting",
                    $"This campaign needs to receive {campaign.Minimal} votes in favour in order to succeed.")
                .WithColor(Colors.Information)
                .Build();
            return embed;
        }

        /// <summary>
        ///     Creates a new <see cref="Embed" /> for an accepted <see cref="Campaign" />.
        /// </summary>
        /// <param name="campaign">The <see cref="Campaign" /> that the embed is made for.</param>
        /// <param name="reason">An optional reason as per why the campaign was accepted.</param>
        /// <returns>An <see cref="Embed" /> for an accepted <see cref="Campaign" />.</returns>
        public static Embed GetAcceptedEmbed(Campaign campaign, string reason = null)
        {
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x
                        .WithIconUrl(Icons.AcceptedCampaign)
                        .WithName("Accepted campaign");
                })
                .AddField("Initiator", $"<@{campaign.Initiator}>", true)
                .AddField("Member", $"<@{campaign.User}>", true)
                .AddField("Type", campaign.Type, true)
                .AddField("Reason", campaign.Reason)
                .AddField("Voting",
                    reason ?? $"This campaign received {campaign.Minimal} votes and has thus been accepted.")
                .WithColor(Colors.Success)
                .Build();
            return embed;
        }

        /// <summary>
        ///     Creates a new <see cref="Embed" /> for a denied <see cref="Campaign" />.
        /// </summary>
        /// <param name="campaign">The <see cref="Campaign" /> that the embed is made for.</param>
        /// <param name="reason">An optional reason as per why the campaign was denied.</param>
        /// <returns>An <see cref="Embed" /> for a denied <see cref="Campaign" />.</returns>
        public static Embed GetDeniedEmbed(Campaign campaign, string reason = null)
        {
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x
                        .WithIconUrl(Icons.DeniedCampaign)
                        .WithName("Denied campaign");
                })
                .AddField("Initiator", $"<@{campaign.Initiator}>", true)
                .AddField("Member", $"<@{campaign.User}>", true)
                .AddField("Type", campaign.Type, true)
                .AddField("Reason", campaign.Reason)
                .AddField("Voting",
                    reason ?? "This campaign was denied because it didn't receive enough votes within 24 hours.")
                .WithColor(Colors.Error)
                .Build();
            return embed;
        }

        public static Embed HugAsync(string title, string description)
        {
            var embed = new EmbedBuilder()
                .WithColor(Colors.Hug)
                .WithDescription(description)
                .WithTitle(title)
                .WithThumbnailUrl(
                    Icons.Hug)
                .Build();
            return embed;
        }

        public static Embed eightBallEmbed(string question, string eightResponse, SocketUser user)
        {
            var embed = new EmbedBuilder()
                .WithColor(Colors.eightBall)
                .WithTitle("Magic 8-Ball")
                .WithDescription(
                    $"{user.Mention}'s Question For The Magic 8-Ball: `{question}`\n\nThe Magic 8-Ball's Response Is: `{eightResponse}\n`")
                .WithFooter($"8-Ball response for: {user.Mention}")
                .WithThumbnailUrl(
                    Icons.eightBall)
                .WithCurrentTimestamp()
                .Build();
            return embed;
        }

        public static Embed CoinflipEmbed(string output, SocketUser user)
        {
            var embed = new EmbedBuilder()
                .WithColor(Colors.Coinflip)
                .WithTitle("Coinflip")
                .WithDescription($"The coin landed on __**{output}**__")
                .WithThumbnailUrl(Icons.Coinflip)
                .WithCurrentTimestamp()
                .WithFooter($"Coin flipped by {user.Mention}")
                .Build();
            return embed;
        }

        public static Embed pollEmbed(string description, SocketUser user)
        {
            var embed = new EmbedBuilder()
                .WithAuthor(x =>
                {
                    x
                        .WithIconUrl(user.GetAvatarUrl())
                        .WithName(user.Username + "#" + user.Discriminator);
                })
                .WithTitle("__**Poll**__")
                .WithColor(Colors.Poll)
                .WithDescription($"**{description}**")
                .WithThumbnailUrl(
                    Icons.Poll)
                .WithCurrentTimestamp()
                .WithFooter($"*Poll started by {user.Username + "#" + user.Discriminator}*")
                .Build();
            return embed;
        }
    }
}