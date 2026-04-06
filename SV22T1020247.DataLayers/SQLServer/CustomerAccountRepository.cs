using Dapper;
using SV22T1020247.DataLayers.SqlServer;
using SV22T1020247.Models.Partner;


namespace SV22T1020247.DataLayers.SqlServer
{
    /// <summary>
    /// Repository xử lý tài khoản khách hàng
    /// </summary>
    public class CustomerAccountRepository : BaseRepository
    {
        public CustomerAccountRepository(string connectionString)
            : base(connectionString)
        {
        }

        /// <summary>
        /// Đăng nhập
        /// </summary>
        public async Task<Customer?> LoginAsync(string email, string password)
        {
            using var connection = GetConnection();

            string sql = @"SELECT *
                           FROM Customers
                           WHERE Email = @email
                           AND Password = @password
                           AND IsLocked = 0";

            return await connection.QueryFirstOrDefaultAsync<Customer>(
                sql,
                new { email, password });
        }

        /// <summary>
        /// Đổi mật khẩu
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int customerID, string password)
        {
            using var connection = GetConnection();

            string sql = @"UPDATE Customers
                           SET Password = @password
                           WHERE CustomerID = @customerID";

            int rows = await connection.ExecuteAsync(
                sql,
                new { customerID, password });

            return rows > 0;
        }

        /// <summary>
        /// Kiểm tra email đã tồn tại hay chưa
        /// </summary>
        public async Task<bool> ExistsAsync(string email)
        {
            using var connection = GetConnection();

            string sql = @"SELECT COUNT(*)
                           FROM Customers
                           WHERE Email = @email";

            int count = await connection.ExecuteScalarAsync<int>(
                sql,
                new { email });

            return count > 0;
        }
    }
}
