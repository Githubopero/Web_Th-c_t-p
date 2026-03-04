using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using WebCauHinhXe.Models;

namespace WebCauHinhXe.Controllers
{
    [Route("api/admin/configurations")]
    [ApiController]
    //[Authorize(Roles = "quan_tri")]
    public class AdminConfigurationController : ControllerBase
    {
        private readonly TtContext _context;

        public AdminConfigurationController(TtContext context)
        {
            _context = context;
        }

        // GET: api/admin/configurations
        [HttpGet]
        public async Task<IActionResult> GetAllConfigs()
        {
            var configs = await _context.Configurations
                .Include(c => c.MauXe)
                .Include(c => c.NguoiDung)
                .OrderByDescending(c => c.NgayCapNhat)
                .Select(c => new
                {
                    c.Id,
                    c.TenCauHinh,
                    c.TongGia,
                    c.NgayTao,
                    c.MaChiaSe,
                    c.CongKhai,
                    UserName = c.NguoiDung != null ? c.NguoiDung.HoTen ?? c.NguoiDung.TenDangNhap : "Khách vãng lai",
                    ModelName = c.MauXe.TenMauXe
                })
                .ToListAsync();

            return Ok(configs);
        }

        // GET: api/admin/configurations/{id}/detail
        [HttpGet("{id}/detail")]
        public async Task<IActionResult> GetConfigDetail(int id)
        {
            var config = await _context.Configurations
                .Include(c => c.MauXe)
                .Include(c => c.NguoiDung)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (config == null) return NotFound("Không tìm thấy cấu hình.");

            var optionIds = config.TuyChonDaChon.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();

            var selectedOptions = await _context.Options
                .Where(o => optionIds.Contains(o.Id))
                .Select(o => new
                {
                    o.Id,
                    o.TenTuyChon,
                    o.GiaThem,
                    o.AnhMoTa,
                    o.MaMau
                })
                .ToListAsync();

            // Ảnh preview (có thể lọc theo ma_mau nếu cần)
            var previewImages = await _context.Images
                .Where(img => img.LoaiThucThe == "mau_xe" && img.IdThucThe == config.MauXeId)
                .OrderBy(img => img.ThuTu)
                .Select(img => new { img.DuongDanAnh, img.GocChup, img.MaMau })
                .ToListAsync();

            // Log xem chi tiết
            await LogAction("xem_cau_hinh_admin", config.Id, new { config.TenCauHinh });

            return Ok(new
            {
                config,
                SelectedOptions = selectedOptions,
                PreviewImages = previewImages
            });
        }

        // DELETE: api/admin/configurations/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteConfig(int id)
        {
            var config = await _context.Configurations.FindAsync(id);
            if (config == null) return NotFound("Cấu hình không tồn tại.");

            _context.Configurations.Remove(config);

            await _context.SaveChangesAsync();

            await LogAction("xoa_cau_hinh_admin", config.Id, new
            {
                TenCauHinh = config.TenCauHinh,
                NguoiDungId = config.NguoiDungId
            });

            return Ok(new { Message = "Đã xóa cấu hình thành công." });
        }

        private async Task LogAction(string hanhDong, int configId, object chiTiet)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var log = new ActivityLog
            {
                NguoiDungId = adminId,
                HanhDong = hanhDong,
                LoaiThucThe = "cau_hinh",
                IdThucThe = configId,
                ChiTiet = JsonSerializer.Serialize(chiTiet),
                NgayThucHien = DateTime.UtcNow
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}