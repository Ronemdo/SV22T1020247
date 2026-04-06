


using SV22T1020247.DataLayers.Interfaces;
using SV22T1020247.Models.Catalog;
using SV22T1020247.Models.Common;
using SV22T1020247.Models.HR;
using SV22T1020247.Models.Partner;
using SV22T1020247.Datalayers.SqlServer;
using System.Threading.Tasks;
using SV22T1020247.DataLayers.SqlServer;
using SV22T1020247.Datalayers.SqlServer;
using SV22T1020247.Models.Catalog;

namespace SV22T1020247.BusinessLayers
{
    /// <summary>
    /// Cung cấp các tính năng xử lý dữ liệu liên quan đến đối tác của hệ thống
    /// Bao gồm: Supplier (nhà cung cấp), Customer (khách hàng), Shipper (người giao hàng), Employee (nhân viên)
    /// </summary>
    public static class PartnerDataService
    {
        private static readonly IGenericRepository<Supplier> supplierDB;
        private static readonly IGenericRepository<Shipper> shipperDB;
        private static readonly ICustomerRepository customerDB;
        private static readonly IGenericRepository<Employee> employeeDB;
        private static readonly IGenericRepository<Category> categoryDB;
        private static readonly IProductRepository productDB;

        static PartnerDataService()
        {
            supplierDB = new SupplierRepository(Configuration.ConnectionString);
            shipperDB = new ShipperRepository(Configuration.ConnectionString);
            customerDB = new CustomerRepository(Configuration.ConnectionString);
            employeeDB = new EmployeeRepository(Configuration.ConnectionString);
            categoryDB = new CategoryRepository(Configuration.ConnectionString);
            productDB = new ProductRepository(Configuration.ConnectionString);
        }
        //== CÁC CHỨC NĂNG LIÊN QUAN ĐẾN LOẠI HÀNG (CATEGORY)

        /// <summary>
        /// Tìm kiếm  và lấy danh sách loại hàng dưới dạng phân trang
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static async Task<PagedResult<Category>> ListCategoriesAsync(PaginationSearchInput input)
        {
            return await categoryDB.ListAsync(input);
        }
        public static async Task<PagedResult<Product>> ListProductsAsync(PaginationSearchInput input)
        {
            // Ép kiểu hoặc tạo mới ProductSearchInput từ PaginationSearchInput
            var productInput = new ProductSearchInput()
            {
                Page = input.Page,
                PageSize = input.PageSize,
                SearchValue = input.SearchValue,
                // Các giá trị mặc định cho lọc nâng cao (hiển thị tất cả)
                CategoryID = 0,
                SupplierID = 0,
                MinPrice = 0,
                MaxPrice = 0
            };

            // Truyền productInput vào thay vì input ban đầu
            return await productDB.ListAsync(productInput);
        }
        //== CÁC CHỨC NĂNG LIÊN QUAN ĐẾN NHÀ CUNG CẤP (SUPPLIER)

        /// <summary>
        /// Tìm kiếm  và lấy danh sách nhà cung cấp dưới dạng phân trang
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static async Task<PagedResult<Supplier>> ListSuppliersAsync(PaginationSearchInput input)
        {
            return await supplierDB.ListAsync(input);
        }


        /// <summary>
        /// Lấy thông tin của một nhà cung cấp dựa vào mã nhà cung cấp
        /// </summary>
        /// <param name="supplierID"></param>
        /// <returns>Thông tin nhà cung cấp nếu tồn tại, ngược lại trả về null</returns>
        public static async Task<Supplier?> GetSupplierAsync(int supplierID)
        {
            return await supplierDB.GetAsync(supplierID);
        }

        /// <summary>
        /// Bổ sung một nhà cung cấp mới
        /// </summary> 
        /// <param name="supplier"></param>
        /// <returns></returns>
        public static async Task<int> AddSupplierAsync(Supplier supplier)
        {
            //TODO: Kiểm tra tính hợp lệ của dữ liệu trước khi bổ sung
            return await supplierDB.AddAsync(supplier);
        }

        /// <summary>
        /// Cập nhật thông tin của nhà cung cấp
        /// </summary>
        /// <param name="supplier"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateSupplierAsync(Supplier supplier)
        {
            //TODO: Kiểm tra tính hợp lệ của dữ liệu trước khi cập nhật
            return await supplierDB.UpdateAsync(supplier);
        }

        /// <summary>
        /// Xóa nhà cung cấp có mã là <paramref name="supplierID"/>
        /// </summary>
        /// <param name="supplierID"></param>
        /// <returns></returns>
        public static async Task<bool> DeleteSupplierAsync(int supplierID)
        {
            if (await supplierDB.IsUsedAsync(supplierID))
                return false;

            return await supplierDB.DeleteAsync(supplierID);
        }

        /// <summary>
        /// Kiểm tra xem một nhà cung cấp có mặt hàng liên quan hay không
        /// (sử dụng để kiểm tra xem có được phép xóa nhà cung cấp hay không)
        /// </summary>
        /// <param name="supplierID"></param>
        /// <returns></returns>
        public static async Task<bool> IsUsedSupplierAsync(int supplierID)
        {
            return await supplierDB.IsUsedAsync(supplierID);
        }


        //== CÁC CHỨC NĂNG LIÊN QUAN ĐẾN NGƯỜI GIAO HÀNG (SHIPPER)

        /// <summary>
        /// Tìm kiếm và lấy danh sách người giao hàng dưới dạng phân trang
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static async Task<PagedResult<Shipper>> ListShippersAsync(PaginationSearchInput input)
        {
            return await shipperDB.ListAsync(input);
        }

        /// <summary>
        /// Lấy thông tin của một người giao hàng theo mã
        /// </summary>
        /// <param name="shipperID"></param>
        /// <returns></returns>
        public static async Task<Shipper?> GetShipperAsync(int shipperID)
        {
            return await shipperDB.GetAsync(shipperID);
        }

        /// <summary>
        /// Bổ sung một người giao hàng mới
        /// </summary>
        /// <param name="shipper"></param>
        /// <returns>Mã người giao hàng được bổ sung</returns>
        public static async Task<int> AddShipperAsync(Shipper shipper)
        {
            return await shipperDB.AddAsync(shipper);
        }

        /// <summary>
        /// Cập nhật thông tin người giao hàng
        /// </summary>
        /// <param name="shipper"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateShipperAsync(Shipper shipper)
        {
            return await shipperDB.UpdateAsync(shipper);
        }

        /// <summary>
        /// Xóa người giao hàng theo mã
        /// </summary>
        /// <param name="shipperID"></param>
        /// <returns></returns>
        public static async Task<bool> DeleteShipperAsync(int shipperID)
        {
            if (await shipperDB.IsUsedAsync(shipperID))
                return false;

            return await shipperDB.DeleteAsync(shipperID);
        }

        /// <summary>
        /// Kiểm tra người giao hàng có phát sinh dữ liệu liên quan hay không
        /// </summary>
        /// <param name="shipperID"></param>
        /// <returns></returns>
        public static async Task<bool> IsUsedShipperAsync(int shipperID)
        {
            return await shipperDB.IsUsedAsync(shipperID);
        }


        //= CÁC CHỨC NĂNG LIÊN QUAN ĐẾN KHÁCH HÀNG (CUSTOMER)

        /// <summary>
        /// Tìm kiếm và lấy danh sách khách hàng dưới dạng phân trang
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static async Task<PagedResult<Customer>> ListCustomersAsync(PaginationSearchInput input)
        {
            return await customerDB.ListAsync(input);
        }

        /// <summary>
        /// Lấy thông tin của một khách hàng theo mã
        /// </summary>
        /// <param name="customerID"></param>
        /// <returns></returns>
        public static async Task<Customer?> GetCustomerAsync(int customerID)
        {
            return await customerDB.GetAsync(customerID);
        }

        /// <summary>
        /// Bổ sung khách hàng mới
        /// </summary>
        /// <param name="customer"></param>
        /// <returns>Mã khách hàng được bổ sung</returns>
        public static async Task<int> AddCustomerAsync(Customer customer)
        {
            return await customerDB.AddAsync(customer);
        }

        /// <summary>
        /// Cập nhật thông tin khách hàng
        /// </summary>
        /// <param name="customer"></param>
        /// <returns></returns>
        public static async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            return await customerDB.UpdateAsync(customer);
        }

        /// <summary>
        /// Xóa khách hàng theo mã
        /// </summary>
        /// <param name="customerID"></param>
        /// <returns></returns>
        public static async Task<bool> DeleteCustomerAsync(int customerID)
        {
            if (await customerDB.IsUsedAsync(customerID))
                return false;

            return await customerDB.DeleteAsync(customerID);
        }

        /// <summary>
        /// Kiểm tra khách hàng có phát sinh dữ liệu liên quan hay không
        /// </summary>
        /// <param name="customerID"></param>
        /// <returns></returns>
        public static async Task<bool> IsUsedCustomerAsync(int customerID)
        {
            return await customerDB.IsUsedAsync(customerID);
        }

        /// <summary>
        /// Kiểm tra xem email của khách hàng có hợp lệ hay không
        /// (Email hợp lệ nếu không bị trùng với email của khách hàng khác)
        /// </summary>
        /// <param name="email"></param>
        /// <param name="customerID"></param>
        /// Nếu bằng 0, tức là kiểm email đối với khách hàng mới
        /// Nếu khác 0, tức là kiểm tra email của khách hàng có mã là <paramref name="customerID"/>
        /// <returns></returns>
        public static async Task<bool> ValidateCustomerEmailAsync(string email, int customerID = 0)
        {
            return await customerDB.ValidateEmailAsync(email);
        }
        /// <summary>
        /// Tìm kiếm và lấy danh sách nhân viên
        /// </summary>
        public static async Task<PagedResult<Employee>> ListEmployeesAsync(PaginationSearchInput input)
        {
            return await employeeDB.ListAsync(input);
        }

        public static async Task<string?> GetCategoryAsync(int id)
        {
            throw new NotImplementedException();
        }

        public static async Task DeleteCategoryAsync(int id)
        {
            throw new NotImplementedException();
        }

        public static async Task<bool> IsUsedCategoryAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
