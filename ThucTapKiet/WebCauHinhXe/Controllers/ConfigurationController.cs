using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCauHinhXe.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace WebCauHinhXe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigurationController : ControllerBase
    {
        private readonly TtContext _context;

        public ConfigurationController(TtContext context)
        {
            _context = context;
        }

        // ────────────────────────────────────────────────
        // 2. Lưu cấu hình (UC Lưu cấu hình)
        // ────────────────────────────────────────────────
        [HttpPost("save")]
        public async Task<IActionResult> SaveConfiguration([FromBody] SaveConfigRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.TenCauHinh) || !request.SelectedOptionIds.Any())
            {
                return BadRequest("Thiếu thông tin cấu hình: tên và tùy chọn đã chọn.");
            }

            var model = await _context.Models
                .FirstOrDefaultAsync(m => m.Id == request.MauXeId && m.TrangThaiHoatDong == 1);

            if (model == null)
            {
                return NotFound("Mẫu xe không tồn tại hoặc không hoạt động.");
            }

            // Tính tổng giá thêm
            var addOnPrice = await _context.Options
                .Where(o => request.SelectedOptionIds.Contains(o.Id))
                .SumAsync(o => o.GiaThem ?? 0m);

            var tongGia = model.GiaCoBan + addOnPrice;

            // Tạo mã chia sẻ unique 10 ký tự
            string maChiaSe;
            do
            {
                maChiaSe = Guid.NewGuid().ToString("N")[..10].ToUpper();
            } while (await _context.Configurations.AnyAsync(c => c.MaChiaSe == maChiaSe));

            var config = new Configuration
            {
                NguoiDungId = request.NguoiDungId,  // null nếu khách vãng lai
                MauXeId = request.MauXeId,
                TenCauHinh = request.TenCauHinh.Trim(),
                TuyChonDaChon = JsonSerializer.Serialize(request.SelectedOptionIds),
                TongGia = tongGia,
                DonViTienTe = "VND",
                MaChiaSe = maChiaSe,
                CongKhai = request.CongKhai ? (sbyte)1 : (sbyte)0,
                NgayTao = DateTime.UtcNow,
                NgayCapNhat = DateTime.UtcNow
            };

            _context.Configurations.Add(config);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Lưu cấu hình thành công!",
                ConfigId = config.Id,
                ShareCode = config.MaChiaSe,
                ShareUrl = $"https://yourdomain.com/cau-hinh/{config.MaChiaSe}",
                TotalPrice = tongGia
            });
        }

        // ────────────────────────────────────────────────
        // 3. Xem cấu hình đã lưu (danh sách của user + xem chi tiết theo share code)
        // ────────────────────────────────────────────────

        /// <summary>
        /// Lấy danh sách cấu hình của người dùng
        /// GET: api/Configuration/my/{userId}
        /// </summary>
        [HttpGet("my/{userId}")]
        public async Task<IActionResult> GetMyConfigurations(int userId)
        {
            var configs = await _context.Configurations
                .Where(c => c.NguoiDungId == userId)
                .Include(c => c.MauXe)
                .OrderByDescending(c => c.NgayCapNhat)
                .Select(c => new
                {
                    c.Id,
                    c.TenCauHinh,
                    c.MaChiaSe,
                    c.TongGia,
                    ModelName = c.MauXe.TenMauXe,
                    CreatedAt = c.NgayTao,
                    IsPublic = c.CongKhai == 1
                })
                .ToListAsync();

            return Ok(new { Message = "Lấy danh sách cấu hình thành công", Data = configs });
        }

        /// <summary>
        /// Xem chi tiết một cấu hình theo mã chia sẻ
        /// GET: api/Configuration/view/{shareCode}
        /// </summary>
        [HttpGet("view/{shareCode}")]
        public async Task<IActionResult> ViewConfiguration(string shareCode)
        {
            var config = await _context.Configurations
                .Include(c => c.MauXe)
                .FirstOrDefaultAsync(c => c.MaChiaSe == shareCode);

            if (config == null || (config.CongKhai.GetValueOrDefault() != 1 && config.NguoiDungId == null))
            {
                return NotFound("Không tìm thấy cấu hình hoặc không có quyền xem.");
            }

            var selectedIds = JsonSerializer.Deserialize<List<int>>(config.TuyChonDaChon ?? "[]") ?? new List<int>();

            var selectedOptions = await _context.Options
                .Where(o => selectedIds.Contains(o.Id))
                .Select(o => new
                {
                    o.Id,
                    o.TenTuyChon,
                    o.GiaThem,
                    o.AnhMoTa,
                    o.MaMau
                })
                .ToListAsync();

            // Thay phần query previewImages bằng code này (dòng ~140-156)
            var previewImages = await _context.Images
                .Where(img => img.LoaiThucThe == "mau_xe" && img.IdThucThe == config.MauXeId)
                .OrderBy(img => img.ThuTu ?? 0)
                .Select(img => new PreviewImageDto  // ← dùng DTO nhỏ
                {
                    DuongDanAnh = img.DuongDanAnh,
                    GocChup = img.GocChup,
                    MaMau = img.MaMau,
                    MoTaAnh = img.MoTaAnh
                })
                .ToListAsync();

            // Đảm bảo không null và có fallback
            if (previewImages == null || !previewImages.Any())
            {
                previewImages = new List<PreviewImageDto>
    {
        new PreviewImageDto
        {
            DuongDanAnh = "/images/default-car-preview.jpg",
            GocChup = "default",
            MaMau = null,
            MoTaAnh = "Ảnh mặc định"
        }
    };
            }

            return Ok(new
            {
                Configuration = config,
                Model = config.MauXe,
                SelectedOptions = selectedOptions,
                PreviewImages = previewImages
            });
        }

        // ────────────────────────────────────────────────
        // 4. Xóa cấu hình cá nhân
        // ────────────────────────────────────────────────
        [HttpDelete("delete/{configId}")]
        public async Task<IActionResult> DeleteConfiguration(int configId, [FromQuery] int userId)
        {
            var config = await _context.Configurations
                .FirstOrDefaultAsync(c => c.Id == configId && c.NguoiDungId == userId);

            if (config == null)
            {
                return NotFound("Không tìm thấy cấu hình hoặc bạn không có quyền xóa.");
            }

            var configName = config.TenCauHinh;
            var modelId = config.MauXeId;

            _context.Configurations.Remove(config);

            // Ghi log hoạt động
            var log = new ActivityLog
            {
                NguoiDungId = userId,
                HanhDong = "xoa_cau_hinh",
                LoaiThucThe = "cau_hinh",
                IdThucThe = configId,
                ChiTiet = JsonSerializer.Serialize(new { TenCauHinh = configName, MauXeId = modelId }),
                NgayThucHien = DateTime.UtcNow
            };
            _context.ActivityLogs.Add(log);

            await _context.SaveChangesAsync();

            return Ok("Cấu hình đã được xóa thành công.");
        }

        // ────────────────────────────────────────────────
        // 5. Gửi báo giá (phiên bản cơ bản - chưa gửi email thật)
        // ────────────────────────────────────────────────
        [HttpPost("send-quote")]
        public async Task<IActionResult> SendQuote([FromBody] SendQuoteRequest request)
        {
            if (string.IsNullOrEmpty(request.EmailTo))
            {
                return BadRequest("Vui lòng cung cấp email nhận báo giá.");
            }

            var config = await _context.Configurations
                .Include(c => c.MauXe)
                .FirstOrDefaultAsync(c => c.Id == request.ConfigId || c.MaChiaSe == request.ShareCode);

            if (config == null)
            {
                return NotFound("Không tìm thấy cấu hình.");
            }

            var selectedIds = JsonSerializer.Deserialize<List<int>>(config.TuyChonDaChon) ?? new List<int>();

            var selectedOptions = await _context.Options
                .Where(o => selectedIds.Contains(o.Id))
                .Select(o => new { o.TenTuyChon, o.GiaThem })
                .ToListAsync();

            // TODO: Tạo nội dung email / PDF ở đây
            // Ví dụ nội dung giả lập:
            string emailContent = $"Báo giá cấu hình: {config.TenCauHinh}\n" +
                                  $"Mẫu xe: {config.MauXe.TenMauXe}\n" +
                                  $"Tổng giá: {config.TongGia:N0} VND\n" +
                                  $"Tùy chọn: {string.Join(", ", selectedOptions.Select(o => o.TenTuyChon))}";

            // TODO: Gọi service gửi email (MailKit, SendGrid, v.v.)
            // await _emailService.SendAsync(request.EmailTo, "Báo giá cấu hình xe", emailContent);

            // Log hành động
            var log = new ActivityLog
            {
                NguoiDungId = request.UserId.Value,
                HanhDong = "gui_bao_gia",
                LoaiThucThe = "cau_hinh",
                IdThucThe = config.Id,
                ChiTiet = JsonSerializer.Serialize(new { EmailTo = request.EmailTo, TongGia = config.TongGia }),
                NgayThucHien = DateTime.UtcNow
            };
            _context.ActivityLogs.Add(log);

            await _context.SaveChangesAsync();

            return Ok(new { Message = "Báo giá đã được gửi thành công đến " + request.EmailTo });
        }
    }

    // ────────────────────────────────────────────────
    // DTOs (Request Models)
    // ────────────────────────────────────────────────

    public class SaveConfigRequest
    {
        public int? NguoiDungId { get; set; }
        public int MauXeId { get; set; }
        public string TenCauHinh { get; set; } = null!;
        public List<int> SelectedOptionIds { get; set; } = new();
        public bool CongKhai { get; set; }
    }

    public class SendQuoteRequest
    {
        public int? ConfigId { get; set; }
        public string? ShareCode { get; set; }
        public int? UserId { get; set; }
        public string EmailTo { get; set; } = null!;
    }


    // DTO đơn giản cho ảnh preview
    public class PreviewImageDto
    {
        public string DuongDanAnh { get; set; } = string.Empty;
        public string? GocChup { get; set; }
        public string? MaMau { get; set; }
        public string? MoTaAnh { get; set; }
    }
}