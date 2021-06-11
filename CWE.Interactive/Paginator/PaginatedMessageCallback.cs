namespace CWE.Interactive
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;

    /// <summary>
    /// Implementation of <see cref="IReactionCallback"/> for paginated messages.
    /// </summary>
    public class PaginatedMessageCallback : IReactionCallback
    {
        private readonly ICriterion<SocketReaction> criterion;
        private readonly PaginatedMessage paginator;
        private readonly int pages;
        private int page = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaginatedMessageCallback"/> class.
        /// </summary>
        /// <param name="interactive">The <see cref="Interactive"/> to be injected.</param>
        /// <param name="sourceContext">The <see cref="SocketCommandContext"/> to be injected.</param>
        /// <param name="paginator">The <see cref="PaginatedMessage"/> to be injected.</param>
        /// <param name="criterion">The <see cref="ICriterion{T}"/> to be injected.</param>
        public PaginatedMessageCallback(
            InteractiveService interactive,
            SocketCommandContext sourceContext,
            PaginatedMessage paginator,
            ICriterion<SocketReaction> criterion = null)
        {
            this.Interactive = interactive;
            this.Context = sourceContext;
            this.criterion = criterion ?? new EmptyCriterion<SocketReaction>();
            this.paginator = paginator;
            this.pages = this.paginator.Pages.Count();
            if (this.paginator.Pages is IEnumerable<EmbedFieldBuilder>)
            {
                this.pages = ((this.paginator.Pages.Count() - 1) / this.Options.FieldsPerPage) + 1;
            }
        }

        /// <inheritdoc/>
        public SocketCommandContext Context { get; }

        /// <summary>
        /// Gets the <see cref="InteractiveService"/> to use.
        /// </summary>
        public InteractiveService Interactive { get; private set; }

        /// <summary>
        /// Gets the <see cref="IUserMessage"/> representing the paginator.
        /// </summary>
        public IUserMessage Message { get; private set; }

        /// <inheritdoc/>
        public RunMode RunMode => RunMode.Sync;

        /// <inheritdoc/>
        public ICriterion<SocketReaction> Criterion => this.criterion;

        /// <inheritdoc/>
        public TimeSpan? Timeout => this.Options.Timeout;

        private PaginatedAppearanceOptions Options => this.paginator.Options;

        /// <summary>
        /// The method that will send the paginator.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task DisplayAsync()
        {
            var embed = this.BuildEmbed();
            var message = await this.Context.Channel.SendMessageAsync(this.paginator.Content, embed: embed).ConfigureAwait(false);
            this.Message = message;
            this.Interactive.AddReactionCallback(message, this);
            _ = Task.Run(async () =>
            {
                await message.AddReactionAsync(this.Options.First);
                await message.AddReactionAsync(this.Options.Back);
                await message.AddReactionAsync(this.Options.Next);
                await message.AddReactionAsync(this.Options.Last);

                var manageMessages = (this.Context.Channel is IGuildChannel guildChannel)
                    ? (this.Context.User as IGuildUser).GetPermissions(guildChannel).ManageMessages
                    : false;

                if (this.Options.JumpDisplayOptions == JumpDisplayOptions.Always
                    || (this.Options.JumpDisplayOptions == JumpDisplayOptions.WithManageMessages && manageMessages))
                {
                    await message.AddReactionAsync(this.Options.Jump);
                }

                await message.AddReactionAsync(this.Options.Stop);

                if (this.Options.DisplayInformationIcon)
                {
                    await message.AddReactionAsync(this.Options.Info);
                }
            });
            if (this.Timeout.HasValue && this.Timeout.Value != TimeSpan.Zero)
            {
                _ = Task.Delay(this.Timeout.Value).ContinueWith(_ =>
                {
                    this.Interactive.RemoveReactionCallback(message);
                    _ = this.Message.DeleteAsync();
                });
            }
        }

        /// <inheritdoc/>
        public async Task<bool> HandleCallbackAsync(SocketReaction reaction)
        {
            var emote = reaction.Emote;

            if (emote.Equals(this.Options.First))
            {
                this.page = 1;
            }
            else if (emote.Equals(this.Options.Next))
            {
                if (this.page >= this.pages)
                {
                    return false;
                }

                ++this.page;
            }
            else if (emote.Equals(this.Options.Back))
            {
                if (this.page <= 1)
                {
                    return false;
                }

                --this.page;
            }
            else if (emote.Equals(this.Options.Last))
            {
                this.page = this.pages;
            }
            else if (emote.Equals(this.Options.Stop))
            {
                await this.Message.DeleteAsync().ConfigureAwait(false);
                return true;
            }
            else if (emote.Equals(this.Options.Jump))
            {
                _ = Task.Run(async () =>
                {
                    var criteria = new Criteria<SocketMessage>()
                        .AddCriterion(new EnsureSourceChannelCriterion())
                        .AddCriterion(new EnsureFromUserCriterion(reaction.UserId))
                        .AddCriterion(new EnsureIsIntegerCriterion());
                    var response = await this.Interactive.NextMessageAsync(this.Context, criteria, TimeSpan.FromSeconds(15));
                    var request = int.Parse(response.Content);
                    if (request < 1 || request > this.pages)
                    {
                        _ = response.DeleteAsync().ConfigureAwait(false);
                        await this.Interactive.ReplyAndDeleteAsync(this.Context, this.Options.Stop.Name);
                        return;
                    }
                    this.page = request;
                    _ = response.DeleteAsync().ConfigureAwait(false);
                    await this.RenderAsync().ConfigureAwait(false);
                });
            }
            else if (emote.Equals(this.Options.Info))
            {
                await this.Interactive.ReplyAndDeleteAsync(this.Context, this.Options.InformationText, timeout: this.Options.InfoTimeout);
                return false;
            }

            _ = this.Message.RemoveReactionAsync(reaction.Emote, reaction.User.Value);
            await this.RenderAsync().ConfigureAwait(false);
            return false;
        }

        /// <summary>
        /// The method that will build the embed of the paginator.
        /// </summary>
        /// <returns>An <see cref="Embed"/> representing the set page of the paginator.</returns>
        protected virtual Embed BuildEmbed()
        {
            var builder = new EmbedBuilder()
                .WithAuthor(this.paginator.Author)
                .WithColor(this.paginator.Color)
                .WithFooter(f => f.Text = string.Format(this.Options.FooterFormat, this.page, this.pages))
                .WithTitle(this.paginator.Title);
            if (this.paginator.Pages is IEnumerable<EmbedFieldBuilder> efb)
            {
                builder.Fields = efb.Skip((this.page - 1) * this.Options.FieldsPerPage).Take(this.Options.FieldsPerPage).ToList();
                builder.Description = this.paginator.AlternateDescription;
            }
            else
            {
                builder.Description = this.paginator.Pages.ElementAt(this.page - 1).ToString();
            }

            return builder.Build();
        }

        /// <summary>
        /// The method responsible for updating the paginator.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        private async Task RenderAsync()
        {
            var embed = this.BuildEmbed();
            await this.Message.ModifyAsync(m => m.Embed = embed).ConfigureAwait(false);
        }
    }
}
