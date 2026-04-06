using Dapper;
using System.Data;
using SV22T1020247.DataLayers.Interfaces;
using SV22T1020247.DataLayers.SqlServer;
using SV22T1020247.Models.Common;
using SV22T1020247.Models.Partner;

namespace SV22T1020247.DataLayers.SQLServer
{
    public class CustomerRepository : BaseRepository, ICustomerRepository
    {
        public CustomerRepository(string connectionString) : base(connectionString)
        {
        }

        public async Task<int> AddAsync(Customer data)
        {
            using var connection = GetConnection();
            string sql = @"
                INSERT INTO Customers (CustomerName, ContactName, Province, Address, Phone, Email, IsLocked, Password)
                VALUES (@CustomerName, @ContactName, @Province, @Address, @Phone, @Email, @IsLocked, @Password);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        public async Task<bool> UpdateAsync(Customer data)
        {
            using var connection = GetConnection();

            // GIẢI PHÁP QUAN TRỌNG: 
            // Sử dụng CASE WHEN để kiểm tra @Password. 
            // Nếu @Password gửi từ Admin xuống là NULL hoặc chuỗi rỗng (''), 
            // thì SQL sẽ gán lại giá trị của chính cột [Password] đang có trong DB (không thay đổi).
            string sql = @"
                UPDATE Customers
                SET CustomerName = @CustomerName,
                    ContactName = @ContactName,
                    Province = @Province,
                    Address = @Address,
                    Phone = @Phone,
                    Email = @Email,
                    IsLocked = @IsLocked,
                    Password = CASE 
                                    WHEN ISNULL(@Password, '') = '' THEN Password 
                                    ELSE @Password 
                               END
                WHERE CustomerID = @CustomerID";

            int rowsAffected = await connection.ExecuteAsync(sql, data);
            return rowsAffected > 0;
        }

        public async Task<PagedResult<Customer>> ListAsync(PaginationSearchInput input)
        {
            var result = new PagedResult<Customer>
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            string searchValue = string.IsNullOrEmpty(input.SearchValue) ? "" : $"%{input.SearchValue}%";

            using var connection = GetConnection();

            string countSql = @"
                SELECT COUNT(*)
                FROM Customers
                WHERE (@SearchValue = N'') 
                   OR (CustomerName LIKE @SearchValue) 
                   OR (ContactName LIKE @SearchValue)
                   OR (Phone LIKE @SearchValue)
                   OR (Email LIKE @SearchValue)";

            result.RowCount = await connection.ExecuteScalarAsync<int>(countSql, new { SearchValue = searchValue });

            if (result.RowCount > 0)
            {
                string querySql = @"
                    SELECT *
                    FROM Customers
                    WHERE (@SearchValue = N'') 
                       OR (CustomerName LIKE @SearchValue) 
                       OR (ContactName LIKE @SearchValue) 
                       OR (Phone LIKE @SearchValue)
                       OR (Email LIKE @SearchValue)
                    ORDER BY CustomerName
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                var data = await connection.QueryAsync<Customer>(querySql, new
                {
                    SearchValue = searchValue,
                    Offset = input.Offset,
                    PageSize = input.PageSize
                });

                result.DataItems = data.ToList();
            }

            return result;
        }

        public async Task<Customer?> GetAsync(int id)
        {
            using var connection = GetConnection();
            string sql = "SELECT * FROM Customers WHERE CustomerID = @CustomerID";
            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { CustomerID = id });
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = GetConnection();
            string sql = "DELETE FROM Customers WHERE CustomerID = @CustomerID";
            int rowsAffected = await connection.ExecuteAsync(sql, new { CustomerID = id });
            return rowsAffected > 0;
        }

        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = GetConnection();
            string sql = @"SELECT CASE WHEN EXISTS(SELECT 1 FROM Orders WHERE CustomerID = @CustomerID) THEN 1 ELSE 0 END";
            return await connection.ExecuteScalarAsync<bool>(sql, new { CustomerID = id });
        }

        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            using var connection = GetConnection();
            string sql = @"SELECT CASE WHEN EXISTS(SELECT 1 FROM Customers WHERE Email = @Email AND (@CustomerID = 0 OR CustomerID <> @CustomerID)) THEN 1 ELSE 0 END";
            bool isExists = await connection.ExecuteScalarAsync<bool>(sql, new { Email = email, CustomerID = id });
            return !isExists;
        }
    }
}