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
        public async Task GetTags()
        {
            var tags = await this.DataAccessLayer.GetTags();

            if (tags.Count() == 0)
            {
                var noTags = Embeds.GetErrorEmbed("No tags found", "This server doesn't have any tags yet.");
                await this.Context.Channel.SendMessageAsync(embed: noTags);
                return;
            }

            string description = string.Join(", ", tags.Select(x => x.Name));

            var list = Embeds.GetInformationEmbed($"Tags ({tags.Count()})", description, $"Use \"{this.Configuration.GetValue<string>("Prefix")}t name\" to view a tag");
            await this.Context.Channel.SendMessageAsync(embed: list);
        }

        /// <summary>
        /// The command used to get, create, modify and delete a tag.
        /// </summary>
        /// <param name="argument">A string argument that is later converted to an array of strings.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("tag")]
        [Alias("t")]
        public async Task TagAsync([Remainder] string argument)
        {
            var arguments = argument.Split(" ");

            if (arguments.Count() == 1 && arguments[0] != "create" && arguments[0] != "edit" && arguments[0] != "transfer" && arguments[0] != "delete")
            {
                var tag = await this.DataAccessLayer.GetTag(arguments[0]);
                if (tag == null)
                {
                    var embed = Embeds.GetErrorEmbed("Not found", "The tag you requested could not be found.");
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
                        var embed = Embeds.GetErrorEmbed("Already exists", "There already exists a tag with that name.");
                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    if (!this.Context.User.IsPromoted())
                    {
                        var embed = Embeds.GetErrorEmbed("Access denied", "You need to be a regular, associate or administrator in order to create tags.");
                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    await this.DataAccessLayer.CreateTag(arguments[1], this.Context.User.Id, string.Join(" ", arguments.Skip(2)));
                    var created = Embeds.GetSuccessEmbed("Tag created", $"The tag has been created. You can view it by using `{this.Configuration.GetValue<string>("Prefix")}tag {arguments[1]}`.");
                    await this.Context.Channel.SendMessageAsync(embed: created);
                    break;
                case "edit":
                    var foundTag = await this.DataAccessLayer.GetTag(arguments[1]);
                    if (foundTag == null)
                    {
                        var embed = Embeds.GetErrorEmbed("Not found", "That tag could not be found.");
                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    if (foundTag.OwnerId != this.Context.User.Id && !socketGuildUser.GuildPermissions.Administrator)
                    {
                        var embed = Embeds.GetErrorEmbed("Access denied", "You need to be the owner of this tag or an administrator to edit the content of this tag.");
                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    await this.DataAccessLayer.EditTagContent(arguments[1], string.Join(" ", arguments.Skip(2)));
                    var edited = Embeds.GetSuccessEmbed("Tag content modified", $"The content of the tag was successfully modified.");
                    await this.Context.Channel.SendMessageAsync(embed: edited);
                    break;
                case "transfer":
                    var tagToTransfer = await this.DataAccessLayer.GetTag(arguments[1]);
                    if (tagToTransfer == null)
                    {
                        var embed = Embeds.GetErrorEmbed("Not found", "That tag could not be found.");
                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    if (!MentionUtils.TryParseUser(arguments[2], out ulong userId) || this.Context.Guild.GetUser(userId) == null)
                    {
                        var embed = Embeds.GetErrorEmbed("Invalid user", "Please provide a valid user.");
                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    if (tagToTransfer.OwnerId != this.Context.User.Id && !socketGuildUser.GuildPermissions.Administrator)
                    {
                        var embed = Embeds.GetErrorEmbed("Access denied", "You need to be the owner of this tag or an administrator to transfer the ownership of this tag.");
                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    await this.DataAccessLayer.EditTagOwner(arguments[1], userId);
                    var success = Embeds.GetSuccessEmbed("Tag ownership transferred", $"The ownership of the tag has been transferred to <@{userId}>.");
                    await this.Context.Channel.SendMessageAsync(embed: success);
                    break;
                case "delete":
                    var tagToDelete = await this.DataAccessLayer.GetTag(arguments[1]);
                    if (tagToDelete == null)
                    {
                        var embed = Embeds.GetErrorEmbed("Not found", "That tag could not be found.");
                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    if (tagToDelete.OwnerId != this.Context.User.Id && !socketGuildUser.GuildPermissions.Administrator)
                    {
                        var embed = Embeds.GetErrorEmbed("Access denied", "You need to be the owner of this tag or an administrator to delete this tag.");
                        await this.Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    await this.DataAccessLayer.DeleteTag(arguments[1]);
                    var deleted = Embeds.GetSuccessEmbed("Tag deleted", $"The tag was successfully deleted.");
                    await this.Context.Channel.SendMessageAsync(embed: deleted);
                    break;
            }
        }
    }
}
