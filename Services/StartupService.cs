﻿using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaladBot.Services
{
    public class StartupService
    {
        private readonly DiscordSocketClient _discord;
        private readonly CommandService _commands;
        private readonly IConfigurationRoot _config;
        private readonly EconomyService _economy;

        public StartupService(DiscordSocketClient discord, CommandService commands, IConfigurationRoot config, EconomyService economy)
        {
            _discord = discord;
            _commands = commands;
            _config = config;
            _economy = economy;
        }

        public async Task StartAsync()
        {
            string discordToken = _config.GetSection("tokens")["bot"];
            if (string.IsNullOrEmpty(discordToken))
            {
                throw new Exception("Please enter bot's token in config");
            }

            await _discord.LoginAsync(Discord.TokenType.Bot, discordToken);
            await _discord.StartAsync();
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly());
            _discord.UserJoined += UserJoined;
        }

        private async Task UserJoined(SocketGuildUser user)
        {
            Console.WriteLine($"New user joined {user.Username}");
            await _economy.AddUser(user);
        }
    }
}
