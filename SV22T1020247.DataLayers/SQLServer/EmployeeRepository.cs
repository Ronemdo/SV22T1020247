using Dapper;
using SV22T1020247.DataLayers.Interfaces;
using SV22T1020247.DataLayers.SqlServer;
using SV22T1020247.Models.Common;
using SV22T1020247.Models.HR;

using System.Data;

namespace SV22T1020247.DataLayers.SqlServer
{
    public class EmployeeRepository : BaseRepository, IEmployeeRepository
    {
        public EmployeeRepository(string connectionString)
            : base(connectionString)
        {
        }

        // ================= LIST =================
        public async Task<PagedResult<Employee>> ListAsync(PaginationSearchInput input)
        {
            using var connection = GetConnection();

            var result = new PagedResult<Employee>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            string condition = "";

            if (!string.IsNullOrWhiteSpace(input.SearchValue))
                condition = "WHERE FullName LIKE @SearchValue";

            string countSql = $@"
                SELECT COUNT(*)
                FROM Employees
                {condition}";

            result.RowCount = await connection.ExecuteScalarAsync<int>(
                countSql,
                new { SearchValue = $"%{input.SearchValue}%" });

            if (result.RowCount == 0)
                return result;

            string querySql = $@"
                SELECT *
                FROM Employees
                {condition}
                ORDER BY FullName
                OFFSET @Offset ROWS
                FETCH NEXT @PageSize ROWS ONLY";

            result.DataItems = (await connection.QueryAsync<Employee>(querySql, new
            {
                SearchValue = $"%{input.SearchValue}%",
                Offset = input.Offset,
                PageSize = input.PageSize
            })).ToList();

            return result;
        }

        // ================= GET =================
        public async Task<Employee?> GetAsync(int id)
        {
            using var connection = GetConnection();

            string sql = "SELECT * FROM Employees WHERE EmployeeID = @id";

            return await connection.QueryFirstOrDefaultAsync<Employee>(sql, new { id });
        }

        // ================= ADD =================
        // ================= ADD =================
        public async Task<int> AddAsync(Employee data)
        {
            using var connection = GetConnection();

            string sql = @"
                INSERT INTO Employees
                (FullName, BirthDate, Address, Phone, Email, Photo, IsWorking, RoleNames)
                VALUES
                (@FullName, @BirthDate, @Address, @Phone, @Email, @Photo, @IsWorking, @RoleNames);

                SELECT CAST(SCOPE_IDENTITY() AS INT)";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        // ================= UPDATE =================
        public async Task<bool> UpdateAsync(Employee data)
        {
            using var connection = GetConnection();

            string sql = @"
                UPDATE Employees
                SET
                    FullName=@FullName,
                    BirthDate=@BirthDate,
                    Address=@Address,
                    Phone=@Phone,
                    Email=@Email,
                    Photo=@Photo,
                    IsWorking=@IsWorking,
                    RoleNames=@RoleNames -- Đã bổ sung cột này để lưu quyền
                WHERE EmployeeID=@EmployeeID";

            int rows = await connection.ExecuteAsync(sql, data);

            return rows > 0;
        }

        // ================= DELETE =================
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = GetConnection();

            string sql = "DELETE FROM Employees WHERE EmployeeID=@id";

            int rows = await connection.ExecuteAsync(sql, new { id });

            return rows > 0;
        }

        // ================= CHECK USED =================
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = GetConnection();

            string sql = "SELECT COUNT(*) FROM Orders WHERE EmployeeID=@id";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });

            return count > 0;
        }

        // ================= VALIDATE EMAIL =================
        public async Task<bool> ValidateEmailAsync(string email, int id = 0)
        {
            using var connection = GetConnection();

            string sql;

            if (id == 0)
            {
                sql = "SELECT COUNT(*) FROM Employees WHERE Email=@email";
                int count = await connection.ExecuteScalarAsync<int>(sql, new { email });
                return count == 0;
            }
            else
            {
                sql = "SELECT COUNT(*) FROM Employees WHERE Email=@email AND EmployeeID<>@id";
                int count = await connection.ExecuteScalarAsync<int>(sql, new { email, id });
                return count == 0;
            }
        }


    }
}
