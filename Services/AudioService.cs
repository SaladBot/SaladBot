﻿using Discord;
using Discord.Audio;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace SaladBot.Services
{
    public class AudioService
    {
        private readonly ConcurrentDictionary<ulong, IAudioClient> ConnectedChannels = new ConcurrentDictionary<ulong, IAudioClient>();

        public async Task JoinAudio(IGuild guild, IVoiceChannel target)
        {
            IAudioClient client;

            if(ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                
                return;
            }

            if (target.Guild.Id != guild.Id)
            {
                return;
            }

            var audioClient = await target.ConnectAsync();

            if(ConnectedChannels.TryAdd(guild.Id, audioClient))
            {
                
            }
        }

        public async Task LeaveAudio(IGuild guild)
        {
            IAudioClient client;

            if(ConnectedChannels.TryRemove(guild.Id, out client))
            {
                await client.StopAsync();
            }
        }

        public async Task SendAudioAsync(IGuild guild, IMessageChannel channel, string path)
        {
            path = Path.Combine("clips", path);

            if (!File.Exists(path))
            {
                await channel.SendMessageAsync("File does not exist.");
                return;
            }

            IAudioClient client;
            if(ConnectedChannels.TryGetValue(guild.Id, out client))
            {
                using (var output = new FileStream(path, FileMode.Open))
                using (var stream = client.CreatePCMStream(AudioApplication.Music))
                {
                    await output.CopyToAsync(stream);

                    await stream.FlushAsync();
                }
            }
        }

        private Process CreateStream(string path)
        {
            return Process.Start(new ProcessStartInfo
            {
                FileName = "ffmpeg.exe",
                Arguments = $"-i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
        }
    }
}
