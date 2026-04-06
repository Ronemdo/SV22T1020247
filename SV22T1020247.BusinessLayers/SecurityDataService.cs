using SV22T1020247.DataLayers.SQLServer;
using SV22T1020247.Models.Security;
using System.Threading.Tasks;

namespace SV22T1020247.BusinessLayers
{
    public static class SecurityDataService
    {
        // Thêm "= null!;" để báo với trình biên dịch rằng biến này chắc chắn sẽ được cấp phát sau
        private static UserAccountRepository employeeAccountDB = null!;

        public static void Init(string connectionString)
        {
            // Khởi tạo bằng Repository
            employeeAccountDB = new UserAccountRepository(connectionString);
        }

        public static async Task<UserAccount?> EmployeeAuthorizeAsync(string userName, string password)
        {
            return await Task.Run(() =>
            {
                return employeeAccountDB.Authorize(userName, password);
            });
        }

        /// <summary>
        /// Đổi mật khẩu tài khoản
        /// </summary>
        public static bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            return employeeAccountDB.ChangePassword(userName, oldPassword, newPassword);
        }

        /// <summary>
        /// Admin đặt lại mật khẩu cho nhân viên
        /// </summary>
        public static bool ResetPassword(string employeeId, string newPassword)
        {
            return employeeAccountDB.ResetPassword(employeeId, newPassword);
        }
    }
}