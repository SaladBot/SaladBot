using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SaladBot.Modules
{
    public class PingModule :ModuleBase<ICommandContext>
    {
        [Command("ping")]
        public async Task Ping()
        {
            await Context.Message.Channel.SendMessageAsync($"pong {Context.User.Username}");
        }
    }
}
