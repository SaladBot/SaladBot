using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SaladBot.Services;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SaladBot.Modules
{
    public class EconomyModule : ModuleBase<ICommandContext>
    {
        private readonly EconomyService _economy;

        public EconomyModule(EconomyService economy)
        {
            _economy = economy;
        }

        [Command("bankstart")]
        public async Task AddToBank()
        {
            var user = Context.User as SocketGuildUser; 
            var success = await _economy.AddUser(user);
            if(success == 0)
            {
                await Context.Channel.SendMessageAsync($"{user.Username} is already a part of the bank.");
                return;
            }
            var eb = new EmbedBuilder();
            eb.WithColor(Color.Red);
            eb.WithTitle($"{user.Username} joined the bank of salad bot");
            eb.AddInlineField("Balance", "1000");
            await Context.Channel.SendMessageAsync("", false, eb);
        }

        [Command("balance")]
        public async Task GetBalance()
        {
            var user = Context.User as SocketGuildUser;

            var balance = await _economy.GetBalance(user);

            var eb = new EmbedBuilder();
            eb.WithColor(Color.Red);
            eb.WithTitle($"You have {balance} gp");
            
            await Context.Channel.SendMessageAsync("", false, eb);
        }

        [Command("pay")]
        public async Task Transfer(SocketGuildUser toUser, long amount)
        {
            var fromUser = Context.User as SocketGuildUser;
            var result = await _economy.TransferMoney(fromUser, toUser, amount);

            var eb = new EmbedBuilder();
            eb.WithColor(Color.Red);
            if (!string.IsNullOrEmpty(result.errorMessage))
            {
                eb.WithTitle($"Payment failed");
                eb.AddInlineField("Reason", result.errorMessage);

                await Context.Channel.SendMessageAsync("", false, eb);
                return;
            }

            eb.WithTitle($"{fromUser.Username} paid {toUser.Username} {amount}");
            eb.AddInlineField($"{fromUser.Username} balance", result.fromUserBalance);
            eb.AddInlineField($"{toUser.Username} balance", result.toUserBalance);
            await Context.Channel.SendMessageAsync("", false, eb);
        }

        [Command("flip")]
        public async Task Flip(long amount)
        {
            var user = Context.User as SocketGuildUser;
            var flipVal = new Random().NextDouble();
            long newBalance;
            var eb = new EmbedBuilder();
            eb.WithColor(Color.Red);
            var currentBalance = await _economy.GetBalance(user);
            if (amount < 0)
            {
                eb.WithTitle("You can only flip positive amounts.");
                await Context.Channel.SendMessageAsync("", false, eb);
                return;
            }
            if (currentBalance < amount)
            {
                eb.WithTitle($"You need {Math.Abs(currentBalance - amount)} more to flip that.");
                await Context.Channel.SendMessageAsync("", false, eb);
                return;
            }
            if(flipVal <= 0.50000)
            {
                //lose
                newBalance = await _economy.AddMoney(user, -amount);
                eb.WithTitle($"Unlucky, {user.Username}");
                eb.AddField("You lost",  $"{amount} gp");
                eb.AddField("New balance", $"{newBalance} gp");
                await Context.Channel.SendMessageAsync("", false, eb);
                return;
            }

            //win
            newBalance = await _economy.AddMoney(user, amount * 2);
            eb.WithTitle($"Lucky, {user.Username}");
            eb.AddField("You won", $"{amount * 2} gp");
            eb.AddField("New balance", $"{newBalance} gp");
            await Context.Channel.SendMessageAsync("", false, eb);
            return;
        }
    }
}
