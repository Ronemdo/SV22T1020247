using Dapper;
using SV22T1020247.DataLayers.Interfaces;
using SV22T1020247.DataLayers.SqlServer;
using SV22T1020247.Models.Security;

using System.Data;

namespace SV22T1020247.DataLayers.SqlServer
{
    public class EmployeeAccountRepository : BaseRepository, IUserAccountRepository
    {
        public EmployeeAccountRepository(string connectionString)
            : base(connectionString)
        {
        }

        /// kiểm tra đăng nhập
        public async Task<UserAccount?> Authorize(string userName, string password)
        {
            using IDbConnection connection = OpenConnection();

            string sql = @"SELECT 
                                EmployeeID AS UserId,
                                Email AS UserName,
                                FullName AS DisplayName,
                                Photo,
                                RoleNames
                           FROM Employees
                           WHERE Email = @userName
                           AND Password = @password
                           AND IsWorking = 1";

            return await connection.QueryFirstOrDefaultAsync<UserAccount>(
                sql,
                new { userName, password });
        }

        public Task<UserAccount?> AuthorizeAsync(string userName, string password)
        {
            throw new NotImplementedException();
        }

        /// đổi mật khẩu
        public async Task<bool> ChangePassword(string userName, string password)
        {
            using IDbConnection connection = OpenConnection();

            string sql = @"UPDATE Employees
                           SET Password = @password
                           WHERE Email = @userName";

            int rows = await connection.ExecuteAsync(sql,
                new { userName, password });

            return rows > 0;
        }

        public Task<bool> ChangePasswordAsync(string userName, string password)
        {
            throw new NotImplementedException();
        }
    }
}
