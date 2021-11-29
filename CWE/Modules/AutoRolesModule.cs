namespace CWE.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using CWE.Common;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using Interactivity;
    using Microsoft.Extensions.Configuration;

    /// <summary>
    /// The auto-roles module, used to manage roles that are automatically added to new members.
    /// </summary>
    [Name("AutoRoles")]
    public class AutoRolesModule : CWEModuleBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AutoRolesModule"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        /// <param name="interactivityService">The <see cref="InteractivityService"/> to inject.</param>
        public AutoRolesModule(IServiceProvider serviceProvider, IConfiguration configuration, InteractivityService interactivityService)
                : base(serviceProvider, configuration, interactivityService)
        {
        }

        /// <summary>
        /// The command used to get all auto-roles.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("autoroles")]
        public async Task AutoRoles()
        {
            var autoRoles = await DataAccessLayer.GetAutoRoles();
            var roles = new List<IRole>();
            var guild = Context.Client.GetGuild(Configuration.GetValue<ulong>("Guild"));
            foreach (var autoRole in autoRoles)
            {
                var role = guild.GetRole(autoRole.Id);
                if (role == null)
                {
                    await DataAccessLayer.DeleteAutoRole(autoRole.Id);
                    continue;
                }

                roles.Add(role);
            }

            if (roles.Count == 0)
            {
                var noRanks = new CWEEmbedBuilder()
                    .WithTitle("No auto-roles found")
                    .WithDescription("This server doesn't have auto-roles yet.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await Context.Channel.SendMessageAsync(embed: noRanks);
                return;
            }

            string description = string.Join("\n", roles.Select(x => x.Name));

            var list = new CWEEmbedBuilder()
                    .WithTitle($"Auto-roles ({roles.Count})")
                    .WithDescription(description)
                    .WithStyle(EmbedStyle.Information)
                    .Build();

            await Context.Channel.SendMessageAsync(embed: list);
        }

        /// <summary>
        /// The command used to create and delete auto-roles.
        /// </summary>
        /// <param name="argument">A string argument that is later converted to an array of strings.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("autorole")]
        public async Task Rank([Remainder] string argument)
        {
            var autoRoles = await DataAccessLayer.GetAutoRoles();
            var roles = new List<IRole>();
            var socketGuildUser = Context.User as SocketGuildUser;
            foreach (var current in autoRoles)
            {
                var currentRole = Context.Guild.GetRole(current.Id);
                if (currentRole == null)
                {
                    await DataAccessLayer.DeleteRank(current.Id);
                    continue;
                }

                roles.Add(currentRole);
            }

            var arguments = argument.Split(" ");

            var autoRole = roles.FirstOrDefault(x => x.Name.ToLower() == arguments[0].ToLower());
            if (arguments[0] != "add" && arguments[0] != "delete")
            {
                var error = new CWEEmbedBuilder()
                    .WithTitle($"Invalid argument")
                    .WithDescription($"Please provide a proper argument for this command.")
                    .WithStyle(EmbedStyle.Success)
                    .Build();

                await Context.Channel.SendMessageAsync(embed: error);
                return;
            }

            var role = Context.Guild.Roles.FirstOrDefault(x => x.Name.ToLower() == arguments[1].ToLower());
            if (role == null)
            {
                var embed = new CWEEmbedBuilder()
                    .WithTitle("Not found")
                    .WithDescription("There doesn't exist a role with that name.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await Context.Channel.SendMessageAsync(embed: embed);
                return;
            }

            switch (arguments[0])
            {
                case "add":
                    if (autoRoles.Any(x => x.Id == role.Id))
                    {
                        var embed = new CWEEmbedBuilder()
                            .WithTitle("Already exists")
                            .WithDescription("There already exists an auto-role with that name.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    if (!socketGuildUser.GuildPermissions.Administrator)
                    {
                        var embed = new CWEEmbedBuilder()
                            .WithTitle("Access denied")
                            .WithDescription("You need to be an administrator in order to create auto-roles.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    await DataAccessLayer.CreateAutoRole(role.Id);
                    var created = new CWEEmbedBuilder()
                            .WithTitle("Auto-role created")
                            .WithDescription($"The auto-role \"{role.Name}\" has been created.")
                            .WithStyle(EmbedStyle.Success)
                            .Build();

                    await Context.Channel.SendMessageAsync(embed: created);
                    break;
                case "delete":
                    if (autoRoles.All(x => x.Id != role.Id))
                    {
                        var embed = new CWEEmbedBuilder()
                            .WithTitle("Not found")
                            .WithDescription("That auto-role could not be found.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    if (!socketGuildUser.GuildPermissions.Administrator)
                    {
                        var embed = new CWEEmbedBuilder()
                            .WithTitle("Access denied")
                            .WithDescription("You need to be an administrator in order to delete auto-roles.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    await DataAccessLayer.DeleteAutoRole(role.Id);
                    var deleted = new CWEEmbedBuilder()
                            .WithTitle("Auto-role deleted")
                            .WithDescription($"The auto-role was successfully deleted.")
                            .WithStyle(EmbedStyle.Success)
                            .Build();

                    await Context.Channel.SendMessageAsync(embed: deleted);
                    break;
            }
        }
    }
}