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
        [Command("tags")]
        public async Task Tags()
        {
            var tags = await this.DataAccessLayer.GetTags();

            if (tags.Count() == 0)
            {
                var noTags = new CWEEmbedBuilder()
                    .WithTitle("No tags found")
                    .WithDescription("This server doesn't have any tags yet.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await this.Context.Channel.SendMessageAsync(embed: noTags);
                return;
            }

            string description = string.Join(", ", tags.Select(x => x.Name));

            var list = new CWEEmbedBuilder()
                    .WithTitle($"Tags ({tags.Count()})")
                    .WithDescription(description)
                    .WithFooter($"Use \"{this.Configuration.GetValue<string>("Prefix")}t name\" to view a tag")
                    .WithStyle(EmbedStyle.Information)
                    .Build();

            await this.Context.Channel.SendMessageAsync(embed: list);
        }

        /// <summary>
        /// The command used to get, create, modify and delete a tag.
        /// </summary>
        /// <param name="argument">A string argument that is later converted to an array of strings.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("tag")]
        [Alias("t")]
        public async Task Tag([Remainder] string argument)
        {
            var arguments = argument.Split(" ");

            if (arguments.Count() == 1 && arguments[0] != "create" && arguments[0] != "edit" && arguments[0] != "transfer" && arguments[0] != "delete")
            {
                var tag = await this.DataAccessLayer.GetTag(arguments[0]);
                if (tag == null)
                {
                    var embed = new CWEEmbedBuilder()
                        .WithTitle("Not found")
                        .WithDescription("The tag you requested could not be found.")
                        .WithStyle(EmbedStyle.Error)
                        .Build();

                    await this.Context.Channel.SendMessageAsync(embed: embed);
                    return;
                }

                await this.Context.Channel.SendMessageAsync(tag.Content);
                return;
            }

            var socketGuildUser = this.Context.User as SocketGuildUser;

            switch (arguments[0])
            {
                case "create":
                    var tag = await this.DataAccessLayer.GetTag(arguments[1]);
                    if (tag != null)
                    {
                        var embed = new CWEEmbedBuilder()
                            .WithTitle("Already exists")
                            .WithDescription("There already exists a tag with that name.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    if (!this.Context.User.IsPromoted())
                    {
                        var embed = new CWEEmbedBuilder()
                            .WithTitle("Access denied")
                            .WithDescription("You need to be a regular, associate or administrator in order to create tags.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    await this.DataAccessLayer.CreateTag(arguments[1], this.Context.User.Id, string.Join(" ", arguments.Skip(2)));
                    var created = new CWEEmbedBuilder()
                            .WithTitle("Tag created")
                            .WithDescription($"The tag has been created. You can view it by using `{this.Configuration.GetValue<string>("Prefix")}tag {arguments[1]}`.")
                            .WithStyle(EmbedStyle.Success)
                            .Build();

                    await this.Context.Channel.SendMessageAsync(embed: created);
                    break;
                case "edit":
                    var foundTag = await this.DataAccessLayer.GetTag(arguments[1]);
                    if (foundTag == null)
                    {
                        var embed = new CWEEmbedBuilder()
                            .WithTitle("Not found")
                            .WithDescription("That tag could not be found.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    if (foundTag.OwnerId != this.Context.User.Id && !socketGuildUser.GuildPermissions.Administrator)
                    {
                        var embed = new CWEEmbedBuilder()
                            .WithTitle("Access denied")
                            .WithDescription("You need to be the owner of this tag or an administrator to edit the content of this tag.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    await this.DataAccessLayer.EditTagContent(arguments[1], string.Join(" ", arguments.Skip(2)));
                    var edited = new CWEEmbedBuilder()
                            .WithTitle("Tag content modified")
                            .WithDescription($"The content of the tag was successfully modified.")
                            .WithStyle(EmbedStyle.Success)
                            .Build();

                    await this.Context.Channel.SendMessageAsync(embed: edited);
                    break;
                case "transfer":
                    var tagToTransfer = await this.DataAccessLayer.GetTag(arguments[1]);
                    if (tagToTransfer == null)
                    {
                        var embed = new CWEEmbedBuilder()
                            .WithTitle("Not found")
                            .WithDescription("That tag could not be found.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    if (!MentionUtils.TryParseUser(arguments[2], out ulong userId) || this.Context.Guild.GetUser(userId) == null)
                    {
                        var embed = new CWEEmbedBuilder()
                            .WithTitle("Invalid user")
                            .WithDescription("Please provide a valid user.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    if (tagToTransfer.OwnerId != this.Context.User.Id && !socketGuildUser.GuildPermissions.Administrator)
                    {
                        var embed = new CWEEmbedBuilder()
                            .WithTitle("Access denied")
                            .WithDescription("You need to be the owner of this tag or an administrator to transfer the ownership of this tag.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    await this.DataAccessLayer.EditTagOwner(arguments[1], userId);
                    var success = new CWEEmbedBuilder()
                            .WithTitle("Tag ownership transferred")
                            .WithDescription($"The ownership of the tag has been transferred to <@{userId}>.")
                            .WithStyle(EmbedStyle.Success)
                            .Build();

                    await this.Context.Channel.SendMessageAsync(embed: success);
                    break;
                case "delete":
                    var tagToDelete = await this.DataAccessLayer.GetTag(arguments[1]);
                    if (tagToDelete == null)
                    {
                        var embed = new CWEEmbedBuilder()
                            .WithTitle("Not found")
                            .WithDescription("That tag could not be found.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    if (tagToDelete.OwnerId != this.Context.User.Id && !socketGuildUser.GuildPermissions.Administrator)
                    {
                        var embed = new CWEEmbedBuilder()
                            .WithTitle("Access denied")
                            .WithDescription("You need to be the owner of this tag or an administrator to delete this tag.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    await this.DataAccessLayer.DeleteTag(arguments[1]);
                    var deleted = new CWEEmbedBuilder()
                            .WithTitle("Tag deleted")
                            .WithDescription($"The tag was successfully deleted.")
                            .WithStyle(EmbedStyle.Success)
                            .Build();

                    await this.Context.Channel.SendMessageAsync(embed: deleted);
                    break;
            }
        }
    }
}
