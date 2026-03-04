using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using WebCauHinhXe.Models;

namespace WebCauHinhXe.Controllers
{
    [Route("api/admin/images")]
    [ApiController]
    //[Authorize(Roles = "quan_tri")]
    public class AdminImageController : ControllerBase
    {
        private readonly TtContext _context;

        public AdminImageController(TtContext context)
        {
            _context = context;
        }

        // GET: api/admin/images?type=dong_xe&id=5
        [HttpGet]
        public async Task<IActionResult> GetImages([FromQuery] string type, [FromQuery] int id)
        {
            if (string.IsNullOrEmpty(type) || id <= 0)
                return BadRequest("Thiếu loại thực thể hoặc ID.");

            var images = await _context.Images
                .Where(img => img.LoaiThucThe == type && img.IdThucThe == id)
                .OrderBy(img => img.ThuTu)
                .Select(img => new
                {
                    img.Id,
                    img.DuongDanAnh,
                    img.MoTaAnh,
                    img.GocChup,
                    img.MaMau,
                    img.ThuTu
                })
                .ToListAsync();

            if (!images.Any())
                return Ok(new { Message = "Chưa có ảnh preview cho thực thể này.", Data = new List<object>() });

            return Ok(images);
        }

        // POST: api/admin/images (upload ảnh mới)
        // Lưu ý: Hiện tại giả lập đường dẫn, thực tế cần dùng IFormFile
        [HttpPost]
        public async Task<IActionResult> AddImage([FromBody] ImageCreateDto dto)
        {
            if (!IsValidEntity(dto.LoaiThucThe, dto.IdThucThe))
                return BadRequest("Loại thực thể hoặc ID không hợp lệ.");

            // Kiểm tra giới hạn (ví dụ: tối đa 10 ảnh / thực thể)
            var count = await _context.Images.CountAsync(i => i.LoaiThucThe == dto.LoaiThucThe && i.IdThucThe == dto.IdThucThe);
            if (count >= 10)
                return BadRequest("Đã đạt giới hạn 10 ảnh cho thực thể này.");

            var image = new Image
            {
                LoaiThucThe = dto.LoaiThucThe,
                IdThucThe = dto.IdThucThe,
                DuongDanAnh = dto.DuongDanAnh, // từ upload thực tế
                MoTaAnh = dto.MoTaAnh,
                GocChup = dto.GocChup,
                MaMau = dto.MaMau,
                ThuTu = dto.ThuTu ?? (await GetNextThuTu(dto.LoaiThucThe, dto.IdThucThe)),
                NgayTao = DateTime.UtcNow
            };

            _context.Images.Add(image);
            await _context.SaveChangesAsync();

            await LogAction("them_anh_preview", image.Id, new { image.LoaiThucThe, image.IdThucThe });

            return CreatedAtAction(nameof(GetImageById), new { id = image.Id }, image);
        }

        // PUT: api/admin/images/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateImage(int id, [FromBody] ImageUpdateDto dto)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null) return NotFound("Ảnh không tồn tại.");

            image.DuongDanAnh = dto.DuongDanAnh ?? image.DuongDanAnh;
            image.MoTaAnh = dto.MoTaAnh ?? image.MoTaAnh;
            image.GocChup = dto.GocChup ?? image.GocChup;
            image.MaMau = dto.MaMau ?? image.MaMau;
            image.ThuTu = dto.ThuTu ?? image.ThuTu;

            await _context.SaveChangesAsync();

            await LogAction("sua_anh_preview", id, new { UpdatedFields = dto });

            return NoContent();
        }

        // DELETE: api/admin/images/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null) return NotFound();

            // Kiểm tra nếu là ảnh đại diện mặc định
            bool isDefault = await IsDefaultImage(image.LoaiThucThe, image.IdThucThe, image.DuongDanAnh);
            if (isDefault)
                return BadRequest("Ảnh này đang được dùng làm ảnh đại diện chính. Hãy thay đổi trước khi xóa.");

            _context.Images.Remove(image);
            await _context.SaveChangesAsync();

            await LogAction("xoa_anh_preview", id, new { image.LoaiThucThe, image.IdThucThe });

            return Ok(new { Message = "Đã xóa ảnh thành công." });
        }

        // POST: api/admin/images/reorder
        [HttpPost("reorder")]
        public async Task<IActionResult> ReorderImages([FromBody] ReorderImagesRequest request)
        {
            if (request.ImageOrders == null || !request.ImageOrders.Any())
                return BadRequest("Không có dữ liệu sắp xếp.");

            foreach (var order in request.ImageOrders)
            {
                var img = await _context.Images.FindAsync(order.ImageId);
                if (img != null)
                {
                    img.ThuTu = order.ThuTu;
                }
            }

            await _context.SaveChangesAsync();

            await LogAction("sap_xep_anh_preview", null, new { Count = request.ImageOrders.Count });

            return Ok(new { Message = "Đã cập nhật thứ tự ảnh thành công." });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetImageById(int id)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null) return NotFound();
            return Ok(image);
        }

        private bool IsValidEntity(string loai, int id)
        {
            return loai switch
            {
                "dong_xe" => _context.Series.Any(s => s.Id == id),
                "mau_xe" => _context.Models.Any(m => m.Id == id),
                "tuy_chon" => _context.Options.Any(o => o.Id == id),
                _ => false
            };
        }

        private async Task<int> GetNextThuTu(string loai, int id)
        {
            return await _context.Images
                .Where(i => i.LoaiThucThe == loai && i.IdThucThe == id)
                .MaxAsync(i => (int?)i.ThuTu) ?? 1 + 1;
        }

        private async Task<bool> IsDefaultImage(string loai, int id, string duongDan)
        {
            return loai switch
            {
                "dong_xe" => await _context.Series.AnyAsync(s => s.Id == id && s.AnhDaiDien == duongDan),
                "mau_xe" => await _context.Models.AnyAsync(m => m.Id == id && m.AnhDaiDien == duongDan),
                _ => false
            };
        }

        private async Task LogAction(string hanhDong, int? imageId, object chiTiet)
        {
            var adminId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var log = new ActivityLog
            {
                NguoiDungId = adminId,
                HanhDong = hanhDong,
                LoaiThucThe = "image",
                IdThucThe = imageId,
                ChiTiet = JsonSerializer.Serialize(chiTiet),
                NgayThucHien = DateTime.UtcNow
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }

    public class ImageCreateDto
    {
        public string LoaiThucThe { get; set; } = null!; // dong_xe / mau_xe / tuy_chon
        public int IdThucThe { get; set; }
        public string DuongDanAnh { get; set; } = null!;
        public string? MoTaAnh { get; set; }
        public string? GocChup { get; set; }
        public string? MaMau { get; set; }
        public int? ThuTu { get; set; }
    }

    public class ImageUpdateDto
    {
        public string? DuongDanAnh { get; set; }
        public string? MoTaAnh { get; set; }
        public string? GocChup { get; set; }
        public string? MaMau { get; set; }
        public int? ThuTu { get; set; }
    }

    public class ImageOrderDto
    {
        public int ImageId { get; set; }
        public int ThuTu { get; set; }
    }

    public class ReorderImagesRequest
    {
        public List<ImageOrderDto> ImageOrders { get; set; } = new();
    }
}