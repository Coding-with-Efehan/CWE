namespace CWE.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using Discord.WebSocket;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// Attribute used to ensure the user is staff.
    /// </summary>
    public class RequireStaff : PreconditionAttribute
    {
        /// <inheritdoc/>
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            var staffRoleId = services.GetRequiredService<IConfiguration>().GetSection("Roles").GetValue<ulong>("Staff");

            if (context.User is SocketGuildUser guildUser)
            {
                if (guildUser.Roles.Any(x => x.Id == staffRoleId))
                {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }
                else
                {
                    return Task.FromResult(PreconditionResult.FromError("User doesn't have staff roles"));
                }
            }
            else
            {
                return Task.FromResult(PreconditionResult.FromError("Command was run outside of a guild.")); // Should never be called, but we put this here to guarentee all code paths return a value.
            }
        }
    }
}
