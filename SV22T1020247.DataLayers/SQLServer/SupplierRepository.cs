using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020247.DataLayers.Interfaces;
using SV22T1020247.Models.Common;
using SV22T1020247.Models.Partner;

using System.Data;

namespace SV22T1020247.DataLayers.SqlServer
{
    /// <summary>
    /// Lớp truy xuất dữ liệu bảng Suppliers trong SQL Server sử dụng Dapper
    /// </summary>
    public class SupplierRepository :
     IGenericRepository<Supplier>,
     IDataDictionaryRepository<Supplier>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor nhận chuỗi kết nối database
        /// </summary>
        public SupplierRepository(string connectionString)
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
        /// Thêm nhà cung cấp
        /// </summary>
        public async Task<int> AddAsync(Supplier data)
        {
            using var connection = OpenConnection();

            string sql = @"INSERT INTO Suppliers
                          (SupplierName,ContactName,Province,Address,Phone,Email)
                           VALUES
                          (@SupplierName,@ContactName,@Province,@Address,@Phone,@Email);
                           SELECT CAST(SCOPE_IDENTITY() AS INT)";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật nhà cung cấp
        /// </summary>
        public async Task<bool> UpdateAsync(Supplier data)
        {
            using var connection = OpenConnection();

            string sql = @"UPDATE Suppliers
                           SET SupplierName=@SupplierName,
                               ContactName=@ContactName,
                               Province=@Province,
                               Address=@Address,
                               Phone=@Phone,
                               Email=@Email
                           WHERE SupplierID=@SupplierID";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Xóa nhà cung cấp
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = OpenConnection();

            string sql = "DELETE FROM Suppliers WHERE SupplierID=@id";

            int rows = await connection.ExecuteAsync(sql, new { id });
            return rows > 0;
        }

        /// <summary>
        /// Lấy thông tin 1 nhà cung cấp
        /// </summary>
        public async Task<Supplier?> GetAsync(int id)
        {
            using var connection = OpenConnection();

            string sql = "SELECT * FROM Suppliers WHERE SupplierID=@id";

            return await connection.QueryFirstOrDefaultAsync<Supplier>(sql, new { id });
        }

        /// <summary>
        /// Kiểm tra supplier có đang được sử dụng hay không
        /// </summary>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = OpenConnection();

            string sql = "SELECT COUNT(*) FROM Products WHERE SupplierID=@id";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });
            return count > 0;
        }

        /// <summary>
        /// Lấy danh sách supplier có phân trang và tìm kiếm
        /// </summary>
        public async Task<PagedResult<Supplier>> ListAsync(PaginationSearchInput input)
        {
            using var connection = OpenConnection();

            var result = new PagedResult<Supplier>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            string condition = "";
            if (!string.IsNullOrWhiteSpace(input.SearchValue))
                condition = "WHERE SupplierName LIKE @SearchValue";

            string countSql = $"SELECT COUNT(*) FROM Suppliers {condition}";

            result.RowCount = await connection.ExecuteScalarAsync<int>(
                countSql,
                new { SearchValue = $"%{input.SearchValue}%" });

            if (result.RowCount == 0)
                return result;

            string querySql = $@"SELECT *
                                FROM Suppliers
                                {condition}
                                ORDER BY SupplierName
                                OFFSET @Offset ROWS
                                FETCH NEXT @PageSize ROWS ONLY";

            result.DataItems = (await connection.QueryAsync<Supplier>(querySql,
                new
                {
                    SearchValue = $"%{input.SearchValue}%",
                    Offset = input.Offset,
                    PageSize = input.PageSize
                })).ToList();

            return result;
        }
        /// <summary>
        /// Lấy toàn bộ danh sách supplier (dùng cho dropdown)
        /// </summary>
        public async Task<List<Supplier>> ListAsync()
        {
            using var connection = OpenConnection();

            string sql = @"SELECT *
                   FROM Suppliers
                   ORDER BY SupplierName";

            var data = await connection.QueryAsync<Supplier>(sql);

            return data.ToList();
        }


    }
}