using Discord;
using Discord.Audio;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaladBot.Modules;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace SaladBot
{
    class Program
    {
        static void Main(string[] args) => Startup.RunAsync(args).GetAwaiter().GetResult();
    }
}
