using SV22T1020247.Models.Common;


namespace SV22T1020247.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu đơn giản trên một
    /// kiểu dữ liệu T nào đó (T là một Entity/DomainModel)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IGenericRepository<T> where T : class
    {
        /// <summary>
        /// Truy vấn, tìm kiếm dữ liệu và trả về kết quả dưới dạng phân trang
        /// </summary>
        /// <param name="input">Thông tin tìm kiếm và phân trang</param>
        /// <returns>Kết quả dữ liệu phân trang</returns>
        Task<PagedResult<T>> ListAsync(PaginationSearchInput input);

        /// <summary>
        /// Lấy thông tin của một bản ghi theo ID
        /// </summary>
        /// <param name="id">Mã của bản ghi cần lấy</param>
        /// <returns>Dữ liệu tìm được hoặc null nếu không tồn tại</returns>
        Task<T?> GetAsync(int id);

        /// <summary>
        /// Bổ sung một bản ghi mới vào CSDL
        /// </summary>
        /// <param name="data">Dữ liệu cần thêm</param>
        /// <returns>ID của bản ghi mới được thêm</returns>
        Task<int> AddAsync(T data);

        /// <summary>
        /// Cập nhật dữ liệu của một bản ghi
        /// </summary>
        /// <param name="data">Dữ liệu cần cập nhật</param>
        /// <returns>true nếu cập nhật thành công</returns>
        Task<bool> UpdateAsync(T data);

        /// <summary>
        /// Kiểm tra xem bản ghi có dữ liệu liên quan hay không
        /// </summary>
        /// <param name="id">Mã bản ghi</param>
        /// <returns>true nếu đang được sử dụng</returns>
        Task<bool> IsUsedAsync(int id);

        /// <summary>
        /// Xóa một bản ghi theo ID
        /// </summary>
        /// <param name="id">Mã bản ghi cần xóa</param>
        /// <returns>true nếu xóa thành công</returns>
        Task<bool> DeleteAsync(int id);
    }
}