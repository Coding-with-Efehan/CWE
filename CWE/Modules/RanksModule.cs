namespace CWE.Modules
{
    using System;
    using System.Collections.Generic;
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
    /// The ranks module, used to assign and remove ranks from users and manage existing ranks.
    /// </summary>
    [Name("Ranks")]
    public class RanksModule : CWEModuleBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RanksModule"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration"/> to inject.</param>
        /// <param name="interactivityService">The <see cref="InteractivityService"/> to inject.</param>
        public RanksModule(IServiceProvider serviceProvider, IConfiguration configuration, InteractivityService interactivityService)
                : base(serviceProvider, configuration, interactivityService)
        {
        }

        /// <summary>
        /// The command used to get all tags.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("ranks")]
        public async Task Ranks()
        {
            var ranks = await DataAccessLayer.GetRanks();
            var roles = new List<IRole>();
            var guild = Context.Client.GetGuild(Configuration.GetValue<ulong>("Guild"));
            foreach (var rank in ranks)
            {
                var role = guild.GetRole(rank.Id);
                if (role == null)
                {
                    await DataAccessLayer.DeleteRank(rank.Id);
                    continue;
                }

                roles.Add(role);
            }

            if (roles.Count() == 0)
            {
                var noRanks = new CWEEmbedBuilder()
                    .WithTitle("No ranks found")
                    .WithDescription("This server doesn't have ranks yet.")
                    .WithStyle(EmbedStyle.Error)
                    .Build();

                await Context.Channel.SendMessageAsync(embed: noRanks);
                return;
            }

            string description = string.Join("\n", roles.Select(x => x.Name));

            var list = new CWEEmbedBuilder()
                    .WithTitle($"Ranks ({roles.Count()})")
                    .WithDescription(description)
                    .WithFooter($"Use \"{Configuration.GetValue<string>("Prefix")}r name\" to join a rank")
                    .WithStyle(EmbedStyle.Information)
                    .Build();

            await Context.Channel.SendMessageAsync(embed: list);
        }

        /// <summary>
        /// The command used to create and delete ranks.
        /// </summary>
        /// <param name="argument">A string argument that is later converted to an array of strings.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        [Command("rank")]
        [Alias("r")]
        public async Task Rank([Remainder] string argument)
        {
            var ranks = await DataAccessLayer.GetRanks();
            var roles = new List<IRole>();
            var socketGuildUser = Context.User as SocketGuildUser;
            foreach (var currentRank in ranks)
            {
                var currentRole = Context.Guild.GetRole(currentRank.Id);
                if (currentRole == null)
                {
                    await DataAccessLayer.DeleteRank(currentRank.Id);
                    continue;
                }

                roles.Add(currentRole);
            }

            var arguments = argument.Split(" ");

            var rank = roles.FirstOrDefault(x => x.Name.ToLower() == arguments[0].ToLower());
            if (arguments.Count() == 1 && arguments[0] != "add" && arguments[0] != "delete")
            {
                if (rank == null)
                {
                    var embed = new CWEEmbedBuilder()
                        .WithTitle("Not found")
                        .WithDescription("The rank you requested could not be found.")
                        .WithStyle(EmbedStyle.Error)
                        .Build();

                    await Context.Channel.SendMessageAsync(embed: embed);
                    return;
                }

                var join = !socketGuildUser.Roles.Contains(rank);
                if (join)
                {
                    await socketGuildUser.AddRoleAsync(rank);
                }
                else
                {
                    await socketGuildUser.RemoveRoleAsync(rank);
                }

                var success = new CWEEmbedBuilder()
                    .WithTitle($"{(join ? "Joined" : "Left")} rank")
                    .WithDescription($"You have successfully {(join ? "joined" : "left")} {rank.Name}.")
                    .WithStyle(EmbedStyle.Success)
                    .Build();

                await Context.Channel.SendMessageAsync(embed: success);
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
                    if (ranks.Any(x => x.Id == role.Id))
                    {
                        var embed = new CWEEmbedBuilder()
                            .WithTitle("Already exists")
                            .WithDescription("There already exists a rank with that name.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    if (!socketGuildUser.GuildPermissions.Administrator)
                    {
                        var embed = new CWEEmbedBuilder()
                            .WithTitle("Access denied")
                            .WithDescription("You need to be an administrator in order to create ranks.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    await DataAccessLayer.CreateRank(role.Id);
                    var created = new CWEEmbedBuilder()
                            .WithTitle("Rank created")
                            .WithDescription($"The rank \"{role.Name}\" has been created.")
                            .WithStyle(EmbedStyle.Success)
                            .Build();

                    await Context.Channel.SendMessageAsync(embed: created);
                    break;
                case "delete":
                    if (ranks.All(x => x.Id != role.Id))
                    {
                        var embed = new CWEEmbedBuilder()
                            .WithTitle("Not found")
                            .WithDescription("That rank could not be found.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    if (!socketGuildUser.GuildPermissions.Administrator)
                    {
                        var embed = new CWEEmbedBuilder()
                            .WithTitle("Access denied")
                            .WithDescription("You need to be an administrator in order to delete ranks.")
                            .WithStyle(EmbedStyle.Error)
                            .Build();

                        await Context.Channel.SendMessageAsync(embed: embed);
                        return;
                    }

                    await DataAccessLayer.DeleteRank(role.Id);
                    var deleted = new CWEEmbedBuilder()
                            .WithTitle("Rank deleted")
                            .WithDescription($"The rank was successfully deleted.")
                            .WithStyle(EmbedStyle.Success)
                            .Build();

                    await Context.Channel.SendMessageAsync(embed: deleted);
                    break;
            }
        }
    }
}