namespace CWE.Modules
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using CWE.Common;
    using CWE.Data.Models;
    using Discord;
    using Discord.Commands;
    using Interactivity;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The suggestions module, used to create and manage suggestions.
    /// </summary>
    [Name("Suggestions")]
    public class SuggestionsModule : CWEModuleBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SuggestionsModule"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        /// <param name="interactivityService">The <see cref="InteractivityService"/> to inject.</param>
        public SuggestionsModule(IServiceProvider serviceProvider, IConfiguration configuration, InteractivityService interactivityService)
                : base(serviceProvider, configuration, interactivityService)
        {
        }

        /// <summary>
        /// The command used to make a suggestion.
        /// </summary>
        /// <param name="suggestion">A description of the suggestion.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Command("suggest")]
        public async Task Suggest([Remainder] string suggestion)
        {
            var embed = new EmbedBuilder()
                .AddField("Initiator", this.Context.User.GetUsernameAndTag() + $" ({this.Context.User.Id})", true)
                .AddField("Suggestion", suggestion)
                .WithFooter($"ID: ... | {DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")}")
                .WithThumbnailUrl(this.Context.User.GetAvatarUrl() ?? this.Context.User.GetDefaultAvatarUrl())
                .WithColor(new Color(87, 105, 233))
                .Build();

            var suggestions = this.Context.Guild.GetTextChannel(this.Configuration.GetSection("Channels").GetValue<ulong>("Suggestions"));
            var message = await suggestions.SendMessageAsync(embed: embed);
            await message.AddReactionsAsync(new IEmote[] { new Emoji("✅"), new Emoji("❌") });

            var suggestionId = await this.DataAccessLayer.CreateSuggestion(this.Context.User.Id, message.Id);
            embed = new EmbedBuilder()
                .AddField("Initiator", this.Context.User.GetUsernameAndTag() + $" ({this.Context.User.Id})", true)
                .AddField("Suggestion", suggestion)
                .WithFooter($"ID: {suggestionId} | {DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")}")
                .WithThumbnailUrl(this.Context.User.GetAvatarUrl() ?? this.Context.User.GetDefaultAvatarUrl())
                .WithColor(new Color(87, 105, 233))
                .Build();

            await message.ModifyAsync(x => x.Embed = embed);

            var success = new CWEEmbedBuilder()
                .WithTitle("Suggestion created")
                .WithDescription($"Your suggestion was successfully created and posted in {suggestions.Mention}.")
                .WithStyle(EmbedStyle.Success)
                .Build();

            await this.Context.Channel.SendMessageAsync(embed: success);
        }

        /// <summary>
        /// The command used to approve a suggestion.
        /// </summary>
        /// <param name="suggestionId">The ID of the suggestion.</param>
        /// <param name="response">The response to be attached to the suggestion.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Command("approve")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Approve(int suggestionId, [Remainder] string response = null)
        {
            var suggestion = await this.DataAccessLayer.GetSuggestion(suggestionId);
            if (suggestion == null)
            {
                var notFound = new CWEEmbedBuilder()
                    .WithTitle("Not found")
                    .WithDescription("There does not exist a suggestion with the provided ID.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await this.Context.Channel.SendMessageAsync(embed: notFound);
                return;
            }

            if (suggestion.State != SuggestionState.New)
            {
                var alreadyReviewed = new CWEEmbedBuilder()
                    .WithTitle("Already reviewed")
                    .WithDescription("The suggestion that was provided has already been reviewed.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await this.Context.Channel.SendMessageAsync(embed: alreadyReviewed);
                return;
            }

            var suggestions = this.Context.Client.GetGuild(this.Configuration.GetValue<ulong>("Guild")).GetTextChannel(this.Configuration.GetSection("Channels").GetValue<ulong>("Suggestions"));
            var message = await suggestions.GetMessageAsync(suggestion.MessageId) as IUserMessage;

            var upvotes = message.Reactions.Where(x => x.Key == new Emoji("✅")).Count();
            var downvotes = message.Reactions.Where(x => x.Key == new Emoji("❌")).Count();

            var embed = message.Embeds.FirstOrDefault()
                .ToEmbedBuilder()
                .WithColor(Colors.Success)
                .AddField("Approved by", this.Context.User.Mention, true)
                .AddField("Results", $"✅ : {upvotes}\n❌ : {downvotes}", true)
                .AddField("Response", response ?? "N/A")
                .Build();

            await message.ModifyAsync(x => x.Embed = embed);
            await message.RemoveAllReactionsAsync();
            await this.DataAccessLayer.UpdateSuggestion(suggestionId, SuggestionState.Approved);

            var success = new CWEEmbedBuilder()
                .WithTitle("Suggestion approved")
                .WithDescription("The suggestion has been approved successfully.")
                .WithStyle(EmbedStyle.Success)
                .Build();

            await this.Context.Channel.SendMessageAsync(embed: success);
        }

        /// <summary>
        /// The command used to reject a suggestion.
        /// </summary>
        /// <param name="suggestionId">The ID of the suggestion.</param>
        /// <param name="response">The response to be attached to the suggestion.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Command("reject")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task Reject(int suggestionId, [Remainder] string response = null)
        {
            var suggestion = await this.DataAccessLayer.GetSuggestion(suggestionId);
            if (suggestion == null)
            {
                var notFound = new CWEEmbedBuilder()
                    .WithTitle("Not found")
                    .WithDescription("There does not exist a suggestion with the provided ID.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await this.Context.Channel.SendMessageAsync(embed: notFound);
                return;
            }

            if (suggestion.State != SuggestionState.New)
            {
                var alreadyReviewed = new CWEEmbedBuilder()
                    .WithTitle("Already reviewed")
                    .WithDescription("The suggestion that was provided has already been reviewed.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await this.Context.Channel.SendMessageAsync(embed: alreadyReviewed);
                return;
            }

            var suggestions = this.Context.Client.GetGuild(this.Configuration.GetValue<ulong>("Guild")).GetTextChannel(this.Configuration.GetSection("Channels").GetValue<ulong>("Suggestions"));
            var message = await suggestions.GetMessageAsync(suggestion.MessageId) as IUserMessage;

            var upvotes = message.Reactions.FirstOrDefault(x => x.Key.Name == "✅").Value.ReactionCount - 1;
            var downvotes = message.Reactions.FirstOrDefault(x => x.Key.Name == "❌").Value.ReactionCount - 1;

            var embed = message.Embeds.FirstOrDefault()
                .ToEmbedBuilder()
                .WithColor(Colors.Error)
                .AddField("Rejected by", this.Context.User.Mention, true)
                .AddField("Results", $"✅ : {upvotes}\n❌ : {downvotes}", true)
                .AddField("Response", response ?? "N/A")
                .Build();

            await message.ModifyAsync(x => x.Embed = embed);
            await message.RemoveAllReactionsAsync();
            await this.DataAccessLayer.UpdateSuggestion(suggestionId, SuggestionState.Rejected);

            var success = new CWEEmbedBuilder()
                .WithTitle("Suggestion rejected")
                .WithDescription("The suggestion has been rejected successfully.")
                .WithStyle(EmbedStyle.Success)
                .Build();

            await this.Context.Channel.SendMessageAsync(embed: success);
        }
    }
}