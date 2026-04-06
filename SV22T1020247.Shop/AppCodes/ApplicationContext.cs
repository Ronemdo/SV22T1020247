namespace SV22T1020247.Shop
{
    public static class ApplicationContext
    {
        /// <summary>
        /// Chuỗi kết nối Database
        /// </summary>
        public static string ConnectionString { get; private set; } = string.Empty;

        /// <summary>
        /// Khởi tạo cấu hình cho hệ thống
        /// </summary>
        public static void Initialize(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }
}