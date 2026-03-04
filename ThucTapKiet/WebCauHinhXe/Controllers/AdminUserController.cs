using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebCauHinhXe.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;

namespace WebCauHinhXe.Controllers
{
    [Route("api/admin/users")]
    [ApiController]
    //[Authorize(Roles = "quan_tri")]
    public class AdminUserController : ControllerBase
    {
        private readonly TtContext _context;

        public AdminUserController(TtContext context)
        {
            _context = context;
        }

        // GET: api/admin/users
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.NgayTao)
                .Select(u => new
                {
                    u.Id,
                    u.TenDangNhap,
                    u.Email,
                    u.HoTen,
                    u.SoDienThoai,
                    u.VaiTro,
                    u.TrangThaiHoatDong,
                    u.NgayTao
                })
                .ToListAsync();

            return Ok(users);
        }

        // PUT: api/admin/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateDto dto)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (id == currentUserId)
                return BadRequest("Không thể chỉnh sửa thông tin tài khoản của chính bạn qua chức năng này.");

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Người dùng không tồn tại.");

            //// Kiểm tra trùng email / username
            //if (dto.Email != null && dto.Email != user.Email &&
            //    await _context.Users.AnyAsync(u => u.Email == dto.Email))
            //    return Conflict("Email đã được sử dụng bởi tài khoản khác.");

            //if (dto.TenDangNhap != null && dto.TenDangNhap != user.TenDangNhap &&
            //    await _context.Users.AnyAsync(u => u.TenDangNhap == dto.TenDangNhap))
            //    return Conflict("Tên đăng nhập đã tồn tại.");

            var changes = new Dictionary<string, object>();

            if (dto.HoTen != null)
            {
                changes["HoTen"] = dto.HoTen;
                user.HoTen = dto.HoTen.Trim();
            }
            if (dto.SoDienThoai != null)
            {
                changes["SoDienThoai"] = dto.SoDienThoai;
                user.SoDienThoai = dto.SoDienThoai.Trim();
            }
            if (dto.VaiTro != null && (dto.VaiTro == "nguoi_dung" || dto.VaiTro == "quan_tri"))
            {
                changes["VaiTro"] = dto.VaiTro;
                user.VaiTro = dto.VaiTro;
            }
            if (dto.TrangThaiHoatDong.HasValue)
            {
                changes["TrangThaiHoatDong"] = dto.TrangThaiHoatDong;
                user.TrangThaiHoatDong = dto.TrangThaiHoatDong;
            }

            if (!changes.Any()) return BadRequest("Không có thay đổi nào được gửi.");

            user.NgayCapNhat = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Log
            await LogAction(
                hanhDong: dto.TrangThaiHoatDong == 0 ? "khoa_tai_khoan" :
                          dto.TrangThaiHoatDong == 1 ? "mo_tai_khoan" : "sua_nguoi_dung",
                idThucThe: id,
                chiTiet: new { Changes = changes }
            );

            return Ok(new { Message = "Cập nhật thông tin người dùng thành công." });
        }

        // POST: api/admin/users/{id}/reset-password
        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (id == currentUserId)
                return BadRequest("Không thể reset mật khẩu tài khoản của chính bạn.");

            var user = await _context.Users.FindAsync(id);
            if (user == null) return NotFound("Người dùng không tồn tại.");

            // Reset về mật khẩu mặc định (ví dụ: "123456" hashed SHA256)
            // Trong thực tế nên gửi email reset link thay vì reset trực tiếp
            string defaultPassword = "123456";
            user.MatKhauHash = ComputeSha256Hash(defaultPassword); // hàm hash từ AuthController

            user.NgayCapNhat = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            //await LogAction("reset_mat_khau", "nguoi_dung", id, new { Message = "Đã reset về mật khẩu mặc định" });

            return Ok(new
            {
                Message = "Đã reset mật khẩu thành công. Mật khẩu mới mặc định là: 123456 (nên yêu cầu người dùng đổi ngay)."
            });
        }

        private async Task LogAction(string hanhDong, int idThucThe, object chiTiet)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var log = new ActivityLog
            {
                NguoiDungId = adminId,
                HanhDong = hanhDong,
                LoaiThucThe = "nguoi_dung",
                IdThucThe = idThucThe,
                ChiTiet = JsonSerializer.Serialize(chiTiet),
                NgayThucHien = DateTime.UtcNow
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        // Hàm hash (copy từ AuthController)
        private static string ComputeSha256Hash(string rawPassword)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(rawPassword));
            var sb = new System.Text.StringBuilder();
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }

    public class UserUpdateDto
    {
        public string? HoTen { get; set; }
        public string? SoDienThoai { get; set; }
        public string? VaiTro { get; set; }
        public sbyte? TrangThaiHoatDong { get; set; }
    }
}