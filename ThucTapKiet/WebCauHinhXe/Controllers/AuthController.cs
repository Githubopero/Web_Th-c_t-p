using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using System.Text;
using WebCauHinhXe.Models;

namespace WebCauHinhXe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TtContext _context;

        public AuthController(TtContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Đăng nhập - POST: api/Auth/Login
        /// </summary>
        [HttpPost("Login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            if (model == null || string.IsNullOrEmpty(model.TenDangNhap) || string.IsNullOrEmpty(model.MatKhau))
            {
                return BadRequest("Tên đăng nhập và mật khẩu là bắt buộc.");
            }

            var user = _context.Users
                .FirstOrDefault(u => u.TenDangNhap == model.TenDangNhap && u.TrangThaiHoatDong == 1);

            if (user == null)
            {
                return Unauthorized("Tên đăng nhập không tồn tại hoặc tài khoản bị khóa.");
            }

            string hashedInputPassword = ComputeSha256Hash(model.MatKhau);

            if (hashedInputPassword != user.MatKhauHash)
            {
                return Unauthorized("Mật khẩu không đúng.");
            }

            return Ok(new { UserId = user.Id });
        }

        /// <summary>
        /// Đăng ký tài khoản - POST: api/Auth/Register
        /// </summary>
        [HttpPost("Register")]
        public IActionResult Register([FromBody] RegisterModel model)
        {
            if (model == null
                || string.IsNullOrWhiteSpace(model.TenDangNhap)
                || string.IsNullOrWhiteSpace(model.Email)
                || string.IsNullOrWhiteSpace(model.MatKhau))
            {
                return BadRequest("Vui lòng điền đầy đủ: Tên đăng nhập, Email và Mật khẩu.");
            }

            // Validation cơ bản
            if (model.TenDangNhap.Length < 4 || model.TenDangNhap.Length > 50)
            {
                return BadRequest("Tên đăng nhập phải từ 4-50 ký tự.");
            }

            if (model.MatKhau.Length < 6)
            {
                return BadRequest("Mật khẩu phải có ít nhất 6 ký tự.");
            }

            if (!model.Email.Contains("@") || !model.Email.Contains("."))
            {
                return BadRequest("Email không hợp lệ.");
            }

            // Kiểm tra trùng lặp
            if (_context.Users.Any(u => u.TenDangNhap == model.TenDangNhap))
            {
                return Conflict("Tên đăng nhập đã được sử dụng.");
            }

            if (_context.Users.Any(u => u.Email == model.Email))
            {
                return Conflict("Email này đã được đăng ký.");
            }

            // Hash mật khẩu
            string hashedPassword = ComputeSha256Hash(model.MatKhau);

            // Tạo user mới
            var newUser = new User
            {
                TenDangNhap = model.TenDangNhap.Trim(),
                Email = model.Email.Trim(),
                MatKhauHash = hashedPassword,
                HoTen = model.HoTen?.Trim(),
                SoDienThoai = model.SoDienThoai?.Trim(),
                VaiTro = "nguoi_dung",                  // mặc định người dùng thường
                TrangThaiHoatDong = 1,                   // kích hoạt ngay
                NgayTao = DateTime.UtcNow,
                NgayCapNhat = DateTime.UtcNow
            };

            try
            {
                _context.Users.Add(newUser);
                _context.SaveChanges();

                return Ok(new
                {
                    Message = "Đăng ký thành công!",
                    UserId = newUser.Id
                });
            }
            catch (Exception ex)
            {
                // Trong production nên log lỗi thay vì trả về message trực tiếp
                return StatusCode(500, $"Có lỗi xảy ra khi lưu tài khoản: {ex.Message}");
            }
        }

        /// <summary>
        /// Lấy thông tin user theo id - GET: api/Auth/User/{id}
        /// </summary>
        [HttpGet("User/{id}")]
        public IActionResult GetUser(int id)
        {
            var user = _context.Users
                .Where(u => u.Id == id)
                .Select(u => new
                {
                    u.Id,
                    u.HoTen,
                    u.Email
                })
                .FirstOrDefault();

            if (user == null) return NotFound();

            return Ok(user);
        }
        /// <summary>
        /// Hàm hash SHA256 cho mật khẩu
        /// </summary>
        private static string ComputeSha256Hash(string rawPassword)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawPassword));
            var sb = new StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2")); // hex lowercase
            }
            return sb.ToString();
        }
    }

    // Các model được đặt ngoài class controller
    public class LoginModel
    {
        public string TenDangNhap { get; set; } = null!;
        public string MatKhau { get; set; } = null!;
    }

    public class RegisterModel
    {
        public string TenDangNhap { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string MatKhau { get; set; } = null!;

        // Các trường không bắt buộc
        public string? HoTen { get; set; }
        public string? SoDienThoai { get; set; }
    }
}