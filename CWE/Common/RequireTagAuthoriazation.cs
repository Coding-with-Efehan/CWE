namespace CWE.Common
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Discord.Commands;
    using Discord.WebSocket;

    /// <summary>
    /// Attribute used to check if the user is able to create tags.
    /// </summary>
    public class RequireTagAuthoriazation : PreconditionAttribute
    {
        /// <summary>
        /// Checks if the attribute will pass or fail.
        /// </summary>
        /// <param name="context">The underlying context.</param>
        /// <param name="command">The command that this attribute was placed on.</param>
        /// <param name="services">The services inside our service pool.</param>
        /// <returns>Returns if a user is authorized to manage tags.</returns>
        public override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services)
        {
            if (context.User is SocketGuildUser user)
            {
                var regularRole = context.Guild.Roles.FirstOrDefault(x => x.Name == "Regular"); // Change this to whatever you like.
                var associateRole = context.Guild.Roles.FirstOrDefault(x => x.Name == "Associate"); // Change this to whatever you like.
                if (user.Roles.Contains(regularRole) || user.Roles.Contains(associateRole))
                {
                    return Task.FromResult(PreconditionResult.FromSuccess());
                }
                else
                {
                    return Task.FromResult(PreconditionResult.FromError("User is not authorized to manage tags."));
                }
            }
            else
            {
                return Task.FromResult(PreconditionResult.FromError("Command was run outside of a guild.")); // Should never be called, but we put this here to guarentee all code paths return a value.
            }
        }
    }
}
