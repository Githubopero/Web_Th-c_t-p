namespace WebCauHinhXe.Models
{
    public class RegisterModel
    {
        //public string TenDangNhap { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string MatKhau { get; set; } = null!;

        // Các trường tùy chọn
        public string? HoTen { get; set; }
        public string? SoDienThoai { get; set; }
    }
}
