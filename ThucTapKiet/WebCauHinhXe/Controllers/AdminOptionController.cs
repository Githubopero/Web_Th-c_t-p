using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebCauHinhXe.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebCauHinhXe.Controllers
{
    [Route("api/admin/options")]
    [ApiController]
    //[Authorize(Roles = "quan_tri")]
    public class AdminOptionController : ControllerBase
    {
        private readonly TtContext _context;

        public AdminOptionController(TtContext context)
        {
            _context = context;
        }

        // ────────────────────────────────────────────────
        // Nhóm tùy chọn (Option Categories)
        // ────────────────────────────────────────────────

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.OptionCategories
                .OrderBy(c => c.ThuTuSapXep)
                .Select(c => new { c.Id, c.TenNhom, c.DuongDanSlug, c.MoTa, c.BieuTuong, c.ThuTuSapXep })
                .ToListAsync();

            return Ok(categories);
        }

        [HttpPost("categories")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDto dto)
        {
            if (await _context.OptionCategories.AnyAsync(c => c.DuongDanSlug == dto.DuongDanSlug))
                return Conflict("Slug đã tồn tại.");

            var cat = new OptionCategory
            {
                TenNhom = dto.TenNhom.Trim(),
                DuongDanSlug = dto.DuongDanSlug.Trim().ToLower(),
                MoTa = dto.MoTa,
                BieuTuong = dto.BieuTuong,
                ThuTuSapXep = dto.ThuTuSapXep ?? 0,
                NgayTao = DateTime.UtcNow
            };

            _context.OptionCategories.Add(cat);
            await _context.SaveChangesAsync();

            await LogAction("them_nhom_tuy_chon", "nhom_tuy_chon", cat.Id, new { cat.TenNhom });

            return CreatedAtAction(nameof(GetCategoryById), new { id = cat.Id }, cat);
        }

        [HttpPut("categories/{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] CategoryUpdateDto dto)
        {
            var cat = await _context.OptionCategories.FindAsync(id);
            if (cat == null) return NotFound();

            if (dto.DuongDanSlug != null && dto.DuongDanSlug != cat.DuongDanSlug &&
                await _context.OptionCategories.AnyAsync(c => c.DuongDanSlug == dto.DuongDanSlug))
                return Conflict("Slug đã tồn tại.");

            cat.TenNhom = dto.TenNhom?.Trim() ?? cat.TenNhom;
            cat.DuongDanSlug = dto.DuongDanSlug?.Trim().ToLower() ?? cat.DuongDanSlug;
            cat.MoTa = dto.MoTa ?? cat.MoTa;
            cat.BieuTuong = dto.BieuTuong ?? cat.BieuTuong;
            cat.ThuTuSapXep = dto.ThuTuSapXep ?? cat.ThuTuSapXep;

            await _context.SaveChangesAsync();
            await LogAction("sua_nhom_tuy_chon", "nhom_tuy_chon", id, dto);

            return NoContent();
        }

        [HttpDelete("categories/{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var cat = await _context.OptionCategories.FindAsync(id);
            if (cat == null) return NotFound();

            // Kiểm tra có tùy chọn nào thuộc nhóm không
            if (await _context.Options.AnyAsync(o => o.NhomTuyChonId == id))
                return BadRequest("Không thể xóa nhóm vì vẫn còn tùy chọn thuộc nhóm này.");

            _context.OptionCategories.Remove(cat);
            await _context.SaveChangesAsync();

            await LogAction("xoa_nhom_tuy_chon", "nhom_tuy_chon", id, new { cat.TenNhom });

            return NoContent();
        }

        [HttpGet("categories/{id}")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            var cat = await _context.OptionCategories.FindAsync(id);
            if (cat == null) return NotFound();
            return Ok(cat);
        }

        // ────────────────────────────────────────────────
        // Tùy chọn chi tiết (Options)
        // ────────────────────────────────────────────────

        [HttpGet("by-category/{categoryId}")]
        public async Task<IActionResult> GetOptionsByCategory(int categoryId)
        {
            var options = await _context.Options
                .Where(o => o.NhomTuyChonId == categoryId)
                .OrderBy(o => o.TenTuyChon)
                .Select(o => new
                {
                    o.Id,
                    o.TenTuyChon,
                    o.DuongDanSlug,
                    o.GiaThem,
                    o.MoTa,
                    o.AnhMoTa,
                    o.MaMau,
                    o.LaMacDinh,
                    o.TonKho,
                    o.TrangThaiHoatDong
                })
                .ToListAsync();

            return Ok(options);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOption([FromBody] OptionCreateDto dto)
        {
            if (!await _context.OptionCategories.AnyAsync(c => c.Id == dto.NhomTuyChonId))
                return BadRequest("Nhóm tùy chọn không tồn tại.");

            if (await _context.Options.AnyAsync(o => o.NhomTuyChonId == dto.NhomTuyChonId && o.DuongDanSlug == dto.DuongDanSlug))
                return Conflict("Slug đã tồn tại trong nhóm này.");

            if (dto.GiaThem < 0) return BadRequest("Giá thêm không được âm.");

            // Nếu là nhóm màu (giả sử bạn có cách nhận diện nhóm màu), kiểm tra MaMau
            // Ví dụ: if (nhom là "Màu ngoại thất" || "Màu nội thất") && string.IsNullOrEmpty(dto.MaMau) → lỗi

            var opt = new Option
            {
                NhomTuyChonId = dto.NhomTuyChonId,
                TenTuyChon = dto.TenTuyChon.Trim(),
                DuongDanSlug = dto.DuongDanSlug.Trim().ToLower(),
                GiaThem = dto.GiaThem,
                MoTa = dto.MoTa,
                AnhMoTa = dto.AnhMoTa,
                MaMau = dto.MaMau,
                LaMacDinh = dto.LaMacDinh ?? 0,
                TonKho = dto.TonKho ?? 999,
                TrangThaiHoatDong = 1,
                NgayTao = DateTime.UtcNow
            };

            _context.Options.Add(opt);
            await _context.SaveChangesAsync();

            await LogAction("them_tuy_chon", "tuy_chon", opt.Id, new { opt.TenTuyChon, opt.NhomTuyChonId });

            return CreatedAtAction(nameof(GetOptionById), new { id = opt.Id }, opt);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOption(int id, [FromBody] OptionUpdateDto dto)
        {
            var opt = await _context.Options.FindAsync(id);
            if (opt == null) return NotFound();

            if (dto.DuongDanSlug != null && dto.DuongDanSlug != opt.DuongDanSlug &&
                await _context.Options.AnyAsync(o => o.NhomTuyChonId == opt.NhomTuyChonId && o.DuongDanSlug == dto.DuongDanSlug))
                return Conflict("Slug đã tồn tại trong nhóm.");

            if (dto.GiaThem.HasValue && dto.GiaThem < 0)
                return BadRequest("Giá thêm không được âm.");

            opt.TenTuyChon = dto.TenTuyChon?.Trim() ?? opt.TenTuyChon;
            opt.DuongDanSlug = dto.DuongDanSlug?.Trim().ToLower() ?? opt.DuongDanSlug;
            opt.GiaThem = dto.GiaThem ?? opt.GiaThem;
            opt.MoTa = dto.MoTa ?? opt.MoTa;
            opt.AnhMoTa = dto.AnhMoTa ?? opt.AnhMoTa;
            opt.MaMau = dto.MaMau ?? opt.MaMau;
            opt.LaMacDinh = dto.LaMacDinh ?? opt.LaMacDinh;
            opt.TonKho = dto.TonKho ?? opt.TonKho;
            opt.TrangThaiHoatDong = dto.TrangThaiHoatDong ?? opt.TrangThaiHoatDong;

            await _context.SaveChangesAsync();
            await LogAction("sua_tuy_chon", "tuy_chon", id, dto);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOption(int id)
        {
            var opt = await _context.Options.FindAsync(id);
            if (opt == null) return NotFound();

            // Kiểm tra đang được dùng trong cấu hình nào không
            var usedInConfig = await _context.Configurations
                .AnyAsync(c => c.TuyChonDaChon.Contains(id.ToString()));

            if (usedInConfig)
                return BadRequest("Tùy chọn này đang được sử dụng trong một số cấu hình. Không thể xóa.");

            _context.Options.Remove(opt);
            await _context.SaveChangesAsync();

            await LogAction("xoa_tuy_chon", "tuy_chon", id, new { opt.TenTuyChon });

            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOptionById(int id)
        {
            var opt = await _context.Options.FindAsync(id);
            if (opt == null) return NotFound();
            return Ok(opt);
        }

        private async Task LogAction(string hanhDong, string loai, int? idThucThe, object chiTiet)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId)) return;

            var log = new ActivityLog
            {
                NguoiDungId = userId,
                HanhDong = hanhDong,
                LoaiThucThe = loai,
                IdThucThe = idThucThe,
                ChiTiet = System.Text.Json.JsonSerializer.Serialize(chiTiet),
                NgayThucHien = DateTime.UtcNow
            };
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }

    // DTOs
    public class CategoryCreateDto
    {
        public string TenNhom { get; set; } = null!;
        public string DuongDanSlug { get; set; } = null!;
        public string? MoTa { get; set; }
        public string? BieuTuong { get; set; }
        public int? ThuTuSapXep { get; set; }
    }

    public class CategoryUpdateDto
    {
        public string? TenNhom { get; set; }
        public string? DuongDanSlug { get; set; }
        public string? MoTa { get; set; }
        public string? BieuTuong { get; set; }
        public int? ThuTuSapXep { get; set; }
    }

    public class OptionCreateDto
    {
        public int NhomTuyChonId { get; set; }
        public string TenTuyChon { get; set; } = null!;
        public string DuongDanSlug { get; set; } = null!;
        public decimal GiaThem { get; set; }
        public string? MoTa { get; set; }
        public string? AnhMoTa { get; set; }
        public string? MaMau { get; set; }
        public sbyte? LaMacDinh { get; set; }
        public int? TonKho { get; set; }
    }

    public class OptionUpdateDto
    {
        public string? TenTuyChon { get; set; }
        public string? DuongDanSlug { get; set; }
        public decimal? GiaThem { get; set; }
        public string? MoTa { get; set; }
        public string? AnhMoTa { get; set; }
        public string? MaMau { get; set; }
        public sbyte? LaMacDinh { get; set; }
        public int? TonKho { get; set; }
        public sbyte? TrangThaiHoatDong { get; set; }
    }
}