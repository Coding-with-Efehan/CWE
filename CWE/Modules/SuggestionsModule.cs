namespace CWE.Modules
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using CWE.Common;
    using CWE.Data.Models;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
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
        /// <param name="argument">Arguments for the command.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Command("suggest")]
        public async Task Suggest([Remainder] string argument)
        {
            var arguments = argument.Split(" ");
            var socketGuildUser = Context.User as SocketGuildUser;
            var suggestionsChannel = Context.Guild.GetTextChannel(Configuration.GetSection("Channels").GetValue<ulong>("Suggestions"));

            switch (arguments[0])
            {
                case "create":
                    string suggestion = string.Join(" ", arguments.Skip(1));
                    var embed = new EmbedBuilder()
                        .AddField("Initiator", Context.User.GetUsernameAndTag() + $" ({Context.User.Id})", true)
                        .AddField("Suggestion", suggestion)
                        .WithFooter($"ID: ... | {DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")}")
                        .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                        .WithColor(new Color(87, 105, 233))
                        .Build();

                    var suggestions = Context.Guild.GetTextChannel(Configuration.GetSection("Channels").GetValue<ulong>("Suggestions"));
                    var message = await suggestions.SendMessageAsync(embed: embed);
                    await message.AddReactionsAsync(new IEmote[] { new Emoji("✅"), new Emoji("❌") });

                    var suggestionId = await DataAccessLayer.CreateSuggestion(Context.User.Id, message.Id);
                    embed = new EmbedBuilder()
                        .AddField("Initiator", Context.User.GetUsernameAndTag() + $" ({Context.User.Id})", true)
                        .AddField("Suggestion", suggestion)
                        .WithFooter($"ID: {suggestionId} | {DateTime.Now.ToString("MM/dd/yyyy hh:mm tt")}")
                        .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                        .WithColor(new Color(87, 105, 233))
                        .Build();

                    await message.ModifyAsync(x => x.Embed = embed);

                    var success = new CWEEmbedBuilder()
                        .WithTitle("Suggestion created")
                        .WithDescription($"Your suggestion was successfully created and posted in {suggestions.Mention}.")
                        .WithStyle(EmbedStyle.Success)
                        .Build();

                    await Context.Channel.SendMessageAsync(embed: success);
                    break;
                case "edit":
                    if (arguments.Count() < 3)
                    {
                        var invalidArgumentsEmbed = new CWEEmbedBuilder()
                            .WithTitle("Invalid arguments")
                            .WithDescription("Please provide the ID of the suggestion and the new content.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await Context.Channel.SendMessageAsync(embed: invalidArgumentsEmbed);
                        return;
                    }

                    if (!int.TryParse(arguments[1], out int currentSuggestionId))
                    {
                        var invalidArgumentTypeEmbed = new CWEEmbedBuilder()
                            .WithTitle("Invalid ID")
                            .WithDescription("Please provide a properly formatted ID.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await Context.Channel.SendMessageAsync(embed: invalidArgumentTypeEmbed);
                        return;
                    }

                    var currentSuggestion = await DataAccessLayer.GetSuggestion(currentSuggestionId);
                    if (currentSuggestion == null)
                    {
                        var notFoundEmbed = new CWEEmbedBuilder()
                            .WithTitle("Not found")
                            .WithDescription("The ID you provided is not associated to an existing suggestion.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await Context.Channel.SendMessageAsync(embed: notFoundEmbed);
                        return;
                    }

                    if (currentSuggestion.Initiator != Context.User.Id)
                    {
                        var accessDeniedEmbed = new CWEEmbedBuilder()
                            .WithTitle("Access denied")
                            .WithDescription("Only the initiator of the suggestion can edit it.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await Context.Channel.SendMessageAsync(embed: accessDeniedEmbed);
                        return;
                    }

                    if (currentSuggestion.State != SuggestionState.New)
                    {
                        var accessDeniedEmbed = new CWEEmbedBuilder()
                            .WithTitle("Access denied")
                            .WithDescription("Only suggestions that haven't been approved or denied can be edited.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await Context.Channel.SendMessageAsync(embed: accessDeniedEmbed);
                        return;
                    }

                    var suggestionMessage = await suggestionsChannel.GetMessageAsync(currentSuggestion.MessageId);
                    var newSuggestionEmbed = suggestionMessage.Embeds
                        .FirstOrDefault()
                        .ToEmbedBuilder();

                    newSuggestionEmbed
                         .Fields
                         .FirstOrDefault(x => x.Name == "Suggestion")
                         .Value = string.Join(" ", arguments.Skip(2));

                    await (suggestionMessage as IUserMessage).ModifyAsync(x => x.Embed = newSuggestionEmbed.Build());

                    var editSuccess = new CWEEmbedBuilder()
                        .WithTitle("Suggestion updated")
                        .WithDescription($"Your suggestion was successfully updated.")
                        .WithStyle(EmbedStyle.Success)
                        .Build();

                    await Context.Channel.SendMessageAsync(embed: editSuccess);
                    break;
                case "delete":
                    if (arguments.Count() != 2)
                    {
                        var noIdProvidedEmbed = new CWEEmbedBuilder()
                            .WithTitle("No ID provided")
                            .WithDescription("Please provide the ID of the suggestion.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await Context.Channel.SendMessageAsync(embed: noIdProvidedEmbed);
                        return;
                    }

                    if (!int.TryParse(arguments[1], out int deleteSuggestionId))
                    {
                        var invalidArgumentTypeEmbed = new CWEEmbedBuilder()
                            .WithTitle("Invalid ID")
                            .WithDescription("Please provide a properly formatted ID.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await Context.Channel.SendMessageAsync(embed: invalidArgumentTypeEmbed);
                        return;
                    }

                    var deleteSuggestion = await DataAccessLayer.GetSuggestion(deleteSuggestionId);
                    if (deleteSuggestion == null)
                    {
                        var notFoundEmbed = new CWEEmbedBuilder()
                            .WithTitle("Not found")
                            .WithDescription("The ID you provided is not associated to an existing suggestion.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await Context.Channel.SendMessageAsync(embed: notFoundEmbed);
                        return;
                    }

                    if (deleteSuggestion.Initiator != Context.User.Id && !socketGuildUser.GuildPermissions.Administrator)
                    {
                        var accessDeniedEmbed = new CWEEmbedBuilder()
                            .WithTitle("Access denied")
                            .WithDescription("Only the initiator of the suggestion can delete it.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await Context.Channel.SendMessageAsync(embed: accessDeniedEmbed);
                        return;
                    }

                    if (deleteSuggestion.State != SuggestionState.New)
                    {
                        var accessDeniedEmbed = new CWEEmbedBuilder()
                            .WithTitle("Access denied")
                            .WithDescription("Only suggestions that haven't been approved or denied can be edited.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await Context.Channel.SendMessageAsync(embed: accessDeniedEmbed);
                        return;
                    }

                    var deleteSuggestionMessage = await suggestionsChannel.GetMessageAsync(deleteSuggestion.MessageId);
                    await deleteSuggestionMessage.DeleteAsync();

                    var deleteSuccess = new CWEEmbedBuilder()
                        .WithTitle("Suggestion deleted")
                        .WithDescription($"Your suggestion was successfully deleted.")
                        .WithStyle(EmbedStyle.Success)
                        .Build();

                    await Context.Channel.SendMessageAsync(embed: deleteSuccess);
                    break;
            }
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
            var suggestion = await DataAccessLayer.GetSuggestion(suggestionId);
            if (suggestion == null)
            {
                var notFound = new CWEEmbedBuilder()
                    .WithTitle("Not found")
                    .WithDescription("There does not exist a suggestion with the provided ID.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await Context.Channel.SendMessageAsync(embed: notFound);
                return;
            }

            if (suggestion.State != SuggestionState.New)
            {
                var alreadyReviewed = new CWEEmbedBuilder()
                    .WithTitle("Already reviewed")
                    .WithDescription("The suggestion that was provided has already been reviewed.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await Context.Channel.SendMessageAsync(embed: alreadyReviewed);
                return;
            }

            var suggestions = Context.Client.GetGuild(Configuration.GetValue<ulong>("Guild")).GetTextChannel(Configuration.GetSection("Channels").GetValue<ulong>("Suggestions"));
            var message = await suggestions.GetMessageAsync(suggestion.MessageId) as IUserMessage;

            var upvotes = message.Reactions.FirstOrDefault(x => x.Key.Name == "✅").Value.ReactionCount - 1;
            var downvotes = message.Reactions.FirstOrDefault(x => x.Key.Name == "❌").Value.ReactionCount - 1;

            var embed = message.Embeds.FirstOrDefault()
                .ToEmbedBuilder()
                .WithColor(Colors.Success)
                .AddField("Approved by", Context.User.Mention, true)
                .AddField("Results", $"✅ : {upvotes}\n❌ : {downvotes}", true)
                .AddField("Response", response ?? "N/A")
                .Build();

            await message.ModifyAsync(x => x.Embed = embed);
            await message.RemoveAllReactionsAsync();
            await DataAccessLayer.UpdateSuggestion(suggestionId, SuggestionState.Approved);

            var success = new CWEEmbedBuilder()
                .WithTitle("Suggestion approved")
                .WithDescription("The suggestion has been approved successfully.")
                .WithStyle(EmbedStyle.Success)
                .Build();

            await Context.Channel.SendMessageAsync(embed: success);
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
            var suggestion = await DataAccessLayer.GetSuggestion(suggestionId);
            if (suggestion == null)
            {
                var notFound = new CWEEmbedBuilder()
                    .WithTitle("Not found")
                    .WithDescription("There does not exist a suggestion with the provided ID.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await Context.Channel.SendMessageAsync(embed: notFound);
                return;
            }

            if (suggestion.State != SuggestionState.New)
            {
                var alreadyReviewed = new CWEEmbedBuilder()
                    .WithTitle("Already reviewed")
                    .WithDescription("The suggestion that was provided has already been reviewed.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await Context.Channel.SendMessageAsync(embed: alreadyReviewed);
                return;
            }

            var suggestions = Context.Client.GetGuild(Configuration.GetValue<ulong>("Guild")).GetTextChannel(Configuration.GetSection("Channels").GetValue<ulong>("Suggestions"));
            var message = await suggestions.GetMessageAsync(suggestion.MessageId) as IUserMessage;

            var upvotes = message.Reactions.FirstOrDefault(x => x.Key.Name == "✅").Value.ReactionCount - 1;
            var downvotes = message.Reactions.FirstOrDefault(x => x.Key.Name == "❌").Value.ReactionCount - 1;

            var embed = message.Embeds.FirstOrDefault()
                .ToEmbedBuilder()
                .WithColor(Colors.Error)
                .AddField("Rejected by", Context.User.Mention, true)
                .AddField("Results", $"✅ : {upvotes}\n❌ : {downvotes}", true)
                .AddField("Response", response ?? "N/A")
                .Build();

            await message.ModifyAsync(x => x.Embed = embed);
            await message.RemoveAllReactionsAsync();
            await DataAccessLayer.UpdateSuggestion(suggestionId, SuggestionState.Rejected);

            var success = new CWEEmbedBuilder()
                .WithTitle("Suggestion rejected")
                .WithDescription("The suggestion has been rejected successfully.")
                .WithStyle(EmbedStyle.Success)
                .Build();

            await Context.Channel.SendMessageAsync(embed: success);
        }
    }
}