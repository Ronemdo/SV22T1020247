using SV22T1020247.Models.Partner;

namespace SV22T1020247.DataLayers.Interfaces
{
    /// <summary>
    /// Định nghĩa các phép xử lý dữ liệu trên Customer
    /// </summary>
    public interface ICustomerRepository : IGenericRepository<Customer>
    {
        /// <summary>
        /// Kiểm tra xem một địa chỉ email có hợp lệ hay không?
        /// </summary>
        Task<bool> ValidateEmailAsync(string email, int id = 0);
    }
}