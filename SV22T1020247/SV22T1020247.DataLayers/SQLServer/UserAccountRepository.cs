using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020247.DataLayers.Interfaces;
using SV22T1020247.Models.Security;

namespace SV22T1020247.DataLayers.SqlServer
{
    public class UserAccountRepository : IUserAccountRepository
    {
        private readonly string _connectionString;

        public UserAccountRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Mở kết nối SQL Server
        /// </summary>
        private IDbConnection OpenConnection()
        {
            return new SqlConnection(_connectionString);
        }

        /// <summary>
        /// Kiểm tra đăng nhập
        /// </summary>
        public async Task<UserAccount?> Authorize(string userName, string password)
        {
            using var connection = OpenConnection();

            string sql = @"SELECT 
                            UserId,
                            UserName,
                            DisplayName,
                            Email,
                            Photo,
                            RoleNames
                           FROM UserAccounts
                           WHERE UserName=@userName AND Password=@password";

            return await connection.QueryFirstOrDefaultAsync<UserAccount>(
                sql,
                new { userName, password });
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        public async Task<bool> ChangePassword(string userName, string password)
        {
            using var connection = OpenConnection();

            string sql = @"UPDATE UserAccounts
                           SET Password=@password
                           WHERE UserName=@userName";

            int rows = await connection.ExecuteAsync(sql, new { userName, password });

            return rows > 0;
        }

        public Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ChangePasswordAsync(string userName, string password)
        {
            throw new NotImplementedException();
        }
    }
}