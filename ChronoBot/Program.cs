﻿using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using ChronoBot.Modules.ChronoGG;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NLog.Extensions.Logging;

namespace ChronoBot
{
    class Program
    {
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        private DiscordSocketClient client;
        private CommandService commands;
        private IServiceProvider services;

        private ILogger logger;

        public async Task MainAsync()
        {
            client = new DiscordSocketClient();
            commands = new CommandService();

            await InstallCommandsAsync();

            logger = services.GetService<ILogger<Program>>();

            string token = JsonConvert.DeserializeObject<Credentials>(File.ReadAllText(Path.Combine(Directory.GetCurrentDirectory(), "Credentials.json"))).Token;
            await client.LoginAsync(Discord.TokenType.Bot, token);
            await client.StartAsync();

            var chronoGGService = services.GetService<ChronoGGService>();
            chronoGGService.StartService();

            logger.LogDebug("Bot running at {0}", DateTime.Now);

            await Task.Delay(-1);
        }

        private async Task InstallCommandsAsync()
        {
            client.MessageReceived += MessageReceived;

            services = new ServiceCollection()
                .AddSingleton(client)
                .AddSingleton(commands)
                .AddSingleton<ChronoGGService>()
                .AddSingleton<ILoggerFactory, LoggerFactory>()
                .AddSingleton(typeof(ILogger<>), typeof(Logger<>))
                .AddLogging((builder) => builder.SetMinimumLevel(LogLevel.Trace))
                .BuildServiceProvider();
            
            ILoggerFactory loggerFactory = services.GetRequiredService<ILoggerFactory>();

            loggerFactory.AddNLog(new NLogProviderOptions { CaptureMessageTemplates = true, CaptureMessageProperties = true });

            // TODO: fix this garbage (debug/release)
            //loggerFactory.ConfigureNLog("../../../../nlog.config");
            loggerFactory.ConfigureNLog("nlog.config");

            await commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task MessageReceived(SocketMessage messageParam)
        {
            SocketUserMessage message = messageParam as SocketUserMessage;
            if (message == null) return;

            int argPos = 0;

            if (!message.HasCharPrefix('!', ref argPos)) return;

            SocketCommandContext context = new SocketCommandContext(client, message);

            IResult result = await commands.ExecuteAsync(context, argPos, services);
            if (!result.IsSuccess)
                Console.WriteLine(result.ErrorReason);
        }
    }
}
