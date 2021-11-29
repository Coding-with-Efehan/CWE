namespace CWE
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using CWE.Data;
    using CWE.Data.Context;
    using CWE.Services;
    using Discord;
    using Discord.Addons.Hosting;
    using Discord.Commands;
    using Discord.WebSocket;
    using Interactivity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Serilog;

    /// <summary>
    /// Entry point of the CWE bot.
    /// </summary>
    internal class Program
    {
        private static async Task Main()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            var builder = new HostBuilder()
                .UseSerilog()
                .ConfigureAppConfiguration(x =>
                {
                    var configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", false, true)
                        .Build();

                    x.AddConfiguration(configuration);
                })
                .ConfigureDiscordHost((context, config) =>
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Info,
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 200,
                        GatewayIntents = GatewayIntents.All,
                    };

                    config.Token = context.Configuration["Token"];
                })
                .UseCommandService((context, config) =>
                {
                    config.LogLevel = LogSeverity.Info;
                    config.CaseSensitiveCommands = false;
                    config.DefaultRunMode = RunMode.Async;
                })
                .ConfigureServices((context, services) =>
                {
                    services
                    .AddHostedService<CommandHandler>()
                    .AddHostedService<InteractionsHandler>()
                    .AddHostedService<TagHandler>()
                    .AddHostedService<AutoRolesHandler>()
                    .AddHostedService<StatusHandler>()
                    .AddSingleton<InteractivityService>()
                    .AddSingleton(new InteractivityConfig { DefaultTimeout = TimeSpan.FromSeconds(20) })
                    .AddDbContextFactory<CWEDbContext>(x =>
                        x.UseMySql(
                            context.Configuration["Database"],
                            new MySqlServerVersion(new Version(8, 0, 23))))
                    .AddSingleton<DataAccessLayer>();
                })
                .UseConsoleLifetime();

            var host = builder.Build();
            using (host)
            {
                await host.RunAsync();
            }
        }
    }
}