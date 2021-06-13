using System;
using System.Linq;
using System.Threading.Tasks;
using CWE.Common;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;

namespace CWE.Modules
{
    /// <summary>
    ///     The community module, containing fun commands that can be used by or used for guild members.
    /// </summary>
    public class Community : CWEModule
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="Community" /> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider" /> to inject.</param>
        /// <param name="configuration">The <see cref="IConfiguration" /> to inject.</param>
        public Community(IServiceProvider serviceProvider, IConfiguration configuration)
            : base(serviceProvider, configuration)
        {
        }

        /// <summary>
        ///     The command used to hug a guild member.
        /// </summary>
        /// <param name="user">The <see cref="SocketGuildUser" /> in the campaign.</param>
        /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
        [Command("hug", RunMode = RunMode.Async)]
        public async Task Hug([Remainder] SocketGuildUser user)
        {
            if (user == null)
            {
                var error = Embeds.GetErrorEmbed("No User Mentioned", "You must mention a user to hug");
                await Context.Channel.SendMessageAsync(embed: error);
                return;
            }

            var hug = Embeds.HugAsync("Hug", $"{Context.User.Mention} gave {user.Mention} a hug");
            await Context.Channel.SendMessageAsync(embed: hug);
        }

        /// <summary>
        ///     The command used to gain wisdom of a magic 8-ball.
        /// </summary>
        /// <param name="question">The <see cref="string" /> in the campaign.</param>
        /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
        [Command("8ball", RunMode = RunMode.Async)]
        [Summary("Ask the magical 8-Ball for their wisdom\nYou must input your question")]
        public async Task eightBall([Remainder] string question = null)
        {
            if (question == null)
            {
                var error = Embeds.GetErrorEmbed(
                    "Missing parameter",
                    "You must include a question for the magic 8-ball");
                await Context.Channel.SendMessageAsync(embed: error);
                return;
            }

            //List of 8-ball response strings
            string[] responses =
            {
                //negitive responses
                "My reply is no",
                "Don’t count on it",
                "Very doubtful",
                "Outlook not so good",
                "My sources say no",

                //affirmative responses
                "Yes",
                "Most likely",
                "It is certain",
                "Signs point to yes",
                "Outlook good",
                "As I see it, yes",
                "Yes, definitely",
                "It is decidedly so",
                "Without a doubt",
                "You may rely on it",

                //non commital responses
                "Better not tell you now",
                "Reply hazy try again",
                "Concentrate and ask again",
                "Cannot predict now",
                "Ask again later"
            };

            var randomizedResponse = responses[new Random().Next(0, responses.Count())];
            var eightBall = Embeds.eightBallEmbed(question, randomizedResponse, Context.User);
            await Context.Channel.SendMessageAsync(embed: eightBall);
        }

        /// <summary>
        ///     The command used to make a decision via coinflip.
        /// </summary>
        /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
        [Command("coinflip", RunMode = RunMode.Async)]
        public async Task Coinflip()
        {
            //List of response strings
            string[] coin =
            {
                "Heads",
                "Tails"
            };

            var randomizedResponse = coin[new Random().Next(0, coin.Count())];
            var coinflip = Embeds.CoinflipEmbed(randomizedResponse,
                Context.User.Username + "#" + Context.User.Discriminator);
            await Context.Channel.SendMessageAsync(embed: coinflip);
        }

        /// <summary>
        ///     The command used to create a new community poll.
        /// </summary>
        /// ///
        /// <param name="pollmsg">The <see cref="string" /> in the campaign.</param>
        /// <returns>A <see cref="Task" /> representing the result of the asynchronous operation.</returns>
        [Command("poll", RunMode = RunMode.Async)]
        public async Task Poll([Remainder] string pollmsg)
        {
            var emoteUp = new Emoji("⬆️");
            var emoteDown = new Emoji("⬇️");

            var poll = Embeds.pollEmbed(pollmsg, Context.User);
            var pollemb = await Context.Channel.SendMessageAsync(embed: poll);
            await pollemb.AddReactionAsync(emoteUp);
            await pollemb.AddReactionAsync(emoteDown);
        }
    }
}