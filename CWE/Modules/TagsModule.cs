namespace CWE.Modules
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using CWE.Common;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using Interactivity;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The tags module, used to view, create, modify and delete tags.
    /// </summary>
    [Name("Tags")]
    [Group("tag")]
    [Alias("t", "tags")]
    public class TagsModule : CWEModuleBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TagsModule"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        /// <param name="interactivityService">The <see cref="InteractivityService"/> to inject.</param>
        public TagsModule(IServiceProvider serviceProvider, IConfiguration configuration, InteractivityService interactivityService)
                : base(serviceProvider, configuration, interactivityService)
        {
        }

        /// <summary>
        /// The command used to get all tags.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command]
        public async Task Tags()
        {
            var tags = await DataAccessLayer.GetTags();

            if (!tags.Any())
            {
                var noTags = new CWEEmbedBuilder()
                    .WithTitle("No tags found")
                    .WithDescription("This server doesn't have any tags yet.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await ReplyAsync(embed: noTags);
                return;
            }

            string description = string.Join(", ", tags.Select(x => x.Name));

            var list = new CWEEmbedBuilder()
                    .WithTitle($"Tags ({tags.Count()})")
                    .WithDescription(description)
                    .WithFooter($"Use $name to view it.")
                    .WithStyle(EmbedStyle.Information)
                    .Build();

            await ReplyAsync(embed: list);
        }

        /// <summary>
        /// The command used to create a tag.
        /// </summary>
        /// <param name="tagName">The name of the new tag.</param>
        /// <param name="tagContent">The content of the new tag.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("create")]
        public async Task Create(string tagName, string tagContent)
        {
            var tag = await DataAccessLayer.GetTag(tagName);
            if (tag != null)
            {
                var embed = new CWEEmbedBuilder()
                    .WithTitle("Already exists")
                    .WithDescription("There already exists a tag with that name.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await Context.Channel.SendMessageAsync(embed: embed);
                return;
            }

            if (!Context.User.IsPromoted())
            {
                var embed = new CWEEmbedBuilder()
                    .WithTitle("Access denied")
                    .WithDescription("You need to be a helper, contributor or administrator in order to create tags.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await Context.Channel.SendMessageAsync(embed: embed);
                return;
            }

            await DataAccessLayer.CreateTag(tagName, Context.User.Id, tagContent);
            var created = new CWEEmbedBuilder()
                .WithTitle("Tag created")
                .WithDescription($"The tag has been created. You can view it by using `${tagName}`.")
                .WithStyle(EmbedStyle.Success)
                .Build();

            await ReplyAsync(embed: created);
        }

        /// <summary>
        /// The command used to edit the content of a tag.
        /// </summary>
        /// <param name="tagName">The name of the tag.</param>
        /// <param name="newContent">The new content that should be applied to the tag.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("edit")]
        public async Task Edit(string tagName, [Remainder] string newContent)
        {
            var socketGuildUser = Context.User as SocketGuildUser;
            var tag = await DataAccessLayer.GetTag(tagName);
            if (tag == null)
            {
                var embed = new CWEEmbedBuilder()
                    .WithTitle("Not found")
                    .WithDescription("That tag could not be found.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await Context.Channel.SendMessageAsync(embed: embed);
                return;
            }

            if (tag.OwnerId != Context.User.Id && !socketGuildUser.GuildPermissions.Administrator)
            {
                var embed = new CWEEmbedBuilder()
                    .WithTitle("Access denied")
                    .WithDescription("You need to be the owner of this tag or an administrator to edit the content of this tag.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await Context.Channel.SendMessageAsync(embed: embed);
                return;
            }

            await DataAccessLayer.EditTagContent(tagName, newContent);
            var edited = new CWEEmbedBuilder()
                .WithTitle("Tag content modified")
                .WithDescription($"The content of the tag was successfully modified.")
                .WithStyle(EmbedStyle.Success)
                .Build();

            await ReplyAsync(embed: edited);
        }

        /// <summary>
        /// The command used to delete a tag.
        /// </summary>
        /// <param name="tagName">The name of the tag to be deleted.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Command("delete")]
        public async Task Delete(string tagName)
        {
            var socketGuildUser = Context.User as SocketGuildUser;
            var tag = await DataAccessLayer.GetTag(tagName);
            if (tag == null)
            {
                var embed = new CWEEmbedBuilder()
                    .WithTitle("Not found")
                    .WithDescription("That tag could not be found.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await Context.Channel.SendMessageAsync(embed: embed);
                return;
            }

            if (tag.OwnerId != Context.User.Id && !socketGuildUser.GuildPermissions.Administrator)
            {
                var embed = new CWEEmbedBuilder()
                    .WithTitle("Access denied")
                    .WithDescription("You need to be the owner of this tag or an administrator to delete this tag.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await Context.Channel.SendMessageAsync(embed: embed);
                return;
            }

            await DataAccessLayer.DeleteTag(tagName);
            var deleted = new CWEEmbedBuilder()
                .WithTitle("Tag deleted")
                .WithDescription($"The tag was successfully deleted.")
                .WithStyle(EmbedStyle.Success)
                .Build();

            await ReplyAsync(embed: deleted);
        }

        /// <summary>
        /// The command used to transfer the ownership of a tag.
        /// </summary>
        /// <param name="tagName">The name of the tag.</param>
        /// <param name="newOwner">The new owner of the tag.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        [Command("transfer")]
        public async Task Transfer(string tagName, SocketGuildUser newOwner)
        {
            var socketGuildUser = Context.User as SocketGuildUser;
            var tagToTransfer = await DataAccessLayer.GetTag(tagName);
            if (tagToTransfer == null)
            {
                var embed = new CWEEmbedBuilder()
                    .WithTitle("Not found")
                    .WithDescription("That tag could not be found.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();
                await Context.Channel.SendMessageAsync(embed: embed);
                return;
            }

            if (tagToTransfer.OwnerId != Context.User.Id && !socketGuildUser.GuildPermissions.Administrator)
            {
                var embed = new CWEEmbedBuilder()
                    .WithTitle("Access denied")
                    .WithDescription("You need to be the owner of this tag or an administrator to transfer the ownership of this tag.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();
                await Context.Channel.SendMessageAsync(embed: embed);
                return;
            }

            await DataAccessLayer.EditTagOwner(tagName, newOwner.Id);
            var success = new CWEEmbedBuilder()
                .WithTitle("Tag ownership transferred")
                .WithDescription($"The ownership of the tag has been transferred to {newOwner.Mention}")
                .WithStyle(EmbedStyle.Success)
                .Build();
            await ReplyAsync(embed: success);
        }
    }
}
