using Discord;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SaladBot.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;
        private readonly LoggingService _logger;

        public DatabaseService(IConfigurationRoot configRoot, LoggingService logger)
        {
            _connectionString = configRoot.GetConnectionString("Default");
            _logger = logger;
        }

        public async Task<MySqlConnection> OpenConn()
        {
            var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            return conn;
        }

        public async Task<TResult> WithConnAsync<TResult>(Func<MySqlConnection,  Task<TResult>> fn)
        {
            try
            {
                using (var conn = await OpenConn())
                {
                    return await fn(conn);
                }
            }
            catch(MySqlException e)
            {
                await _logger.LogAsync(new LogMessage(LogSeverity.Error, nameof(DatabaseService), e.Message));
            }

            throw new Exception();
        }

        public async Task WithConnAsync(Func<MySqlConnection, Task> fn)
        {
            try
            {
                using (var conn = await OpenConn())
                {
                    await fn(conn);
                    return;
                }
            }
            catch(MySqlException e)
            {
                await _logger.LogAsync(new LogMessage(LogSeverity.Error, nameof(DatabaseService), e.Message));
            }
            throw new Exception();
        }
    }
}
