using Dapper;
using Microsoft.Data.SqlClient;
using SV22T1020247.DataLayers.Interfaces;
using SV22T1020247.Models.Common;
using SV22T1020247.Models.Partner;

using System.Data;

namespace SV22T1020247.Datalayers.SqlServer
{
    /// <summary>
    /// Lớp truy xuất dữ liệu bảng Shippers trong SQL Server
    /// sử dụng thư viện Dapper
    /// </summary>
    public class ShipperRepository :
     IGenericRepository<Shipper>,
     IDataDictionaryRepository<Shipper>
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor nhận chuỗi kết nối database
        /// </summary>
        public ShipperRepository(string connectionString)
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
        /// Thêm người giao hàng
        /// </summary>
        public async Task<int> AddAsync(Shipper data)
        {
            using var connection = OpenConnection();

            string sql = @"INSERT INTO Shippers
                          (ShipperName, Phone)
                           VALUES
                          (@ShipperName, @Phone);
                           SELECT CAST(SCOPE_IDENTITY() AS INT)";

            return await connection.ExecuteScalarAsync<int>(sql, data);
        }

        /// <summary>
        /// Cập nhật người giao hàng
        /// </summary>
        public async Task<bool> UpdateAsync(Shipper data)
        {
            using var connection = OpenConnection();

            string sql = @"UPDATE Shippers
                           SET ShipperName=@ShipperName,
                               Phone=@Phone
                           WHERE ShipperID=@ShipperID";

            int rows = await connection.ExecuteAsync(sql, data);
            return rows > 0;
        }

        /// <summary>
        /// Xóa người giao hàng
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            using var connection = OpenConnection();

            string sql = "DELETE FROM Shippers WHERE ShipperID=@id";

            int rows = await connection.ExecuteAsync(sql, new { id });
            return rows > 0;
        }

        /// <summary>
        /// Lấy thông tin 1 người giao hàng
        /// </summary>
        public async Task<Shipper?> GetAsync(int id)
        {
            using var connection = OpenConnection();

            string sql = "SELECT * FROM Shippers WHERE ShipperID=@id";

            return await connection.QueryFirstOrDefaultAsync<Shipper>(sql, new { id });
        }

        /// <summary>
        /// Kiểm tra shipper có đang được sử dụng hay không
        /// </summary>
        public async Task<bool> IsUsedAsync(int id)
        {
            using var connection = OpenConnection();

            string sql = @"SELECT COUNT(*)
                           FROM Orders
                           WHERE ShipperID=@id";

            int count = await connection.ExecuteScalarAsync<int>(sql, new { id });

            return count > 0;
        }

        /// <summary>
        /// Lấy danh sách shipper có phân trang và tìm kiếm
        /// </summary>
        public async Task<PagedResult<Shipper>> ListAsync(PaginationSearchInput input)
        {
            using var connection = OpenConnection();

            var result = new PagedResult<Shipper>()
            {
                Page = input.Page,
                PageSize = input.PageSize
            };

            string condition = "";
            if (!string.IsNullOrWhiteSpace(input.SearchValue))
                condition = "WHERE ShipperName LIKE @SearchValue";

            string countSql = $"SELECT COUNT(*) FROM Shippers {condition}";

            result.RowCount = await connection.ExecuteScalarAsync<int>(
                countSql,
                new { SearchValue = $"%{input.SearchValue}%" });

            if (result.RowCount == 0)
                return result;

            string querySql = $@"SELECT *
                                FROM Shippers
                                {condition}
                                ORDER BY ShipperName
                                OFFSET @Offset ROWS
                                FETCH NEXT @PageSize ROWS ONLY";

            result.DataItems = (await connection.QueryAsync<Shipper>(
                querySql,
                new
                {
                    SearchValue = $"%{input.SearchValue}%",
                    Offset = input.Offset,
                    PageSize = input.PageSize
                })).ToList();

            return result;
        }
        /// <summary>
        /// Lấy toàn bộ danh sách shipper (dùng cho dropdown)
        /// </summary>
        public async Task<List<Shipper>> ListAsync()
        {
            using var connection = OpenConnection();

            string sql = @"SELECT *
                   FROM Shippers
                   ORDER BY ShipperName";

            var data = await connection.QueryAsync<Shipper>(sql);

            return data.ToList();
        }


    }
}