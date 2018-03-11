using Dapper;
using Discord;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaladBot.Services
{
    public class EconomyService
    {
        private readonly DatabaseService _db;

        public EconomyService(DatabaseService db)
        {
            _db = db;
        }

        public Task<long> AddMoney(IGuildUser user, long amount)
        {
            var sql = @"
                update Bank set Balance = Balance + @amount where UserId = @Id;
                select Balance from Bank where UserId = @Id;
            ";

            return _db.WithConnAsync(conn => conn.QueryFirstAsync<long>(sql, new { amount, user.Id }));
        }

        public Task RemoveMoney(IGuildUser user, int amount)
        {
            return Task.CompletedTask;
        }

        public async Task<(long fromUserBalance, long toUserBalance, string errorMessage)> TransferMoney(IGuildUser fromUser, IGuildUser toUser, long amount)
        {
            if (amount < 0)
            {
                return (0, 0, "Can only transfer amount greater than 0");
            }

            var sql = @"
                insert ignore into Bank values(@fromUserId, 1000), (@toUserId, 1000);

                select
                    Balance
                from
                    Bank
                where
                    UserId = @fromUserId;

                select
                    Balance
                from
                    Bank
                where
                    UserId = @toUserId;
            ";

            var updateBalanceSql = @"
                update Bank set Balance = @fromUserBalance where UserId = @fromUserId;
                update Bank set Balance = @toUserBalance where UserId = @toUserId;
            ";

            using (var conn = await _db.OpenConn())
            using (var data = await conn.QueryMultipleAsync(sql, new { fromUserId = fromUser.Id, toUserId = toUser.Id }))
            {
                var fromUserBalance = await data.ReadFirstOrDefaultAsync<long>();
                var toUserBalance = await data.ReadFirstOrDefaultAsync<long>();

                if (fromUserBalance - amount < 0)
                {
                    return (0, 0, $"You need {Math.Abs(fromUserBalance - amount)} more to complete the transfer.");
                }
                var newFromBalance = fromUserBalance - amount;
                var newToBalance = toUserBalance + amount;
                await conn.ExecuteAsync(updateBalanceSql, new { fromUserId = fromUser.Id, toUserId = toUser.Id, fromUserBalance = newFromBalance, toUserBalance = newToBalance });
                return (newFromBalance, newToBalance, null);
            }
        }

        public async Task<long> GetBalance(IGuildUser user)
        {
            var sql = @"
                insert ignore into Bank values(@Id, 1000);
                select
                    Balance
                from
                    Bank
                where
                    UserId = @Id;
            ";
            return await _db.WithConnAsync(conn => conn.QueryFirstOrDefaultAsync<long>(sql, new { user.Id }));
        }

        public Task<int> AddUser(IGuildUser user)
        {
            var sql = @"
                insert ignore into Bank values(@Id, 1000);
            ";
            
            return _db.WithConnAsync(conn => conn.ExecuteAsync(sql, new { user.Id }));
        }
    }
}
