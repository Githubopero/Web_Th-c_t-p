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
    [Route("api/admin/series")]
    [ApiController]
    //[Authorize] // Yêu cầu đăng nhập
    public class AdminSeriesController : ControllerBase
    {
        private readonly TtContext _context;

        public AdminSeriesController(TtContext context)
        {
            _context = context;
        }

        // GET: api/admin/series
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Có thể thêm check quyền ở đây nếu filter chưa làm
            var series = await _context.Series
                .OrderBy(s => s.ThuTuSapXep)
                .Select(s => new
                {
                    s.Id,
                    s.TenDongXe,
                    s.DuongDanSlug,
                    s.MoTa,
                    s.AnhDaiDien,
                    s.ThuTuSapXep,
                    s.TrangThaiHoatDong
                })
                .ToListAsync();

            return Ok(series);
        }

        // POST: api/admin/series
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] SeriesCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (await _context.Series.AnyAsync(s => s.DuongDanSlug == dto.DuongDanSlug))
                return Conflict("Slug đã tồn tại.");

            var series = new Series
            {
                TenDongXe = dto.TenDongXe.Trim(),
                DuongDanSlug = dto.DuongDanSlug.Trim().ToLower(),
                MoTa = dto.MoTa,
                AnhDaiDien = dto.AnhDaiDien, // đường dẫn từ frontend upload
                ThuTuSapXep = dto.ThuTuSapXep ?? 0,
                TrangThaiHoatDong = 1,
                NgayTao = DateTime.UtcNow
            };

            _context.Series.Add(series);
            await _context.SaveChangesAsync();

            // Log
            await LogAction("them_dong_xe", "dong_xe", series.Id, new { series.TenDongXe });

            return CreatedAtAction(nameof(GetById), new { id = series.Id }, series);
        }

        // PUT: api/admin/series/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] SeriesUpdateDto dto)
        {
            var series = await _context.Series.FindAsync(id);
            if (series == null) return NotFound();

            if (dto.DuongDanSlug != series.DuongDanSlug &&
                await _context.Series.AnyAsync(s => s.DuongDanSlug == dto.DuongDanSlug))
                return Conflict("Slug đã tồn tại.");

            series.TenDongXe = dto.TenDongXe?.Trim() ?? series.TenDongXe;
            series.DuongDanSlug = dto.DuongDanSlug?.Trim().ToLower() ?? series.DuongDanSlug;
            series.MoTa = dto.MoTa ?? series.MoTa;
            series.AnhDaiDien = dto.AnhDaiDien ?? series.AnhDaiDien;
            series.ThuTuSapXep = dto.ThuTuSapXep ?? series.ThuTuSapXep;
            series.TrangThaiHoatDong = dto.TrangThaiHoatDong ?? series.TrangThaiHoatDong;

            await _context.SaveChangesAsync();

            await LogAction("sua_dong_xe", "dong_xe", id, new { UpdatedFields = dto });

            return NoContent();
        }

        // DELETE: api/admin/series/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var series = await _context.Series.FindAsync(id);
            if (series == null) return NotFound();

            // Có thể soft-delete thay vì hard-delete
            series.TrangThaiHoatDong = 0;
            // hoặc _context.Series.Remove(series);

            await _context.SaveChangesAsync();

            await LogAction("xoa_dong_xe", "dong_xe", id, new { series.TenDongXe });

            return NoContent();
        }

        // GET chi tiết
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var series = await _context.Series.FindAsync(id);
            if (series == null) return NotFound();
            return Ok(series);
        }

        private async Task LogAction(string hanhDong, string loai, int? idThucThe, object chiTiet)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

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

    public class SeriesCreateDto
    {
        public string TenDongXe { get; set; } = null!;
        public string DuongDanSlug { get; set; } = null!;
        public string? MoTa { get; set; }
        public string? AnhDaiDien { get; set; }
        public int? ThuTuSapXep { get; set; }
    }

    public class SeriesUpdateDto
    {
        public string? TenDongXe { get; set; }
        public string? DuongDanSlug { get; set; }
        public string? MoTa { get; set; }
        public string? AnhDaiDien { get; set; }
        public int? ThuTuSapXep { get; set; }
        public sbyte? TrangThaiHoatDong { get; set; }
    }
}