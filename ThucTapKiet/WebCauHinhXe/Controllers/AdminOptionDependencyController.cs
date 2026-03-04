using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCauHinhXe.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebCauHinhXe.Controllers
{
    [Route("api/admin/option-dependencies")]
    [ApiController]
    //[Authorize(Roles = "quan_tri")]
    public class AdminOptionDependencyController : ControllerBase
    {
        private readonly TtContext _context;

        public AdminOptionDependencyController(TtContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDependencies()
        {
            var deps = await _context.OptionDependencies
                .Include(d => d.TuyChonChinh)
                .Include(d => d.TuyChonBatBuoc)
                .Include(d => d.TuyChonXungDot)
                .Select(d => new
                {
                    d.Id,
                    Chinh = new { d.TuyChonChinhId, Ten = d.TuyChonChinh.TenTuyChon },
                    BatBuoc = d.TuyChonBatBuoc != null ? new { d.TuyChonBatBuocId, Ten = d.TuyChonBatBuoc.TenTuyChon } : null,
                    XungDot = d.TuyChonXungDot != null ? new { d.TuyChonXungDotId, Ten = d.TuyChonXungDot.TenTuyChon } : null,
                    d.GhiChu
                })
                .ToListAsync();

            return Ok(deps);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDependency([FromBody] DependencyCreateDto dto)
        {
            if (!await _context.Options.AnyAsync(o => o.Id == dto.TuyChonChinhId))
                return BadRequest("Tùy chọn chính không tồn tại.");

            if (dto.TuyChonBatBuocId.HasValue && !await _context.Options.AnyAsync(o => o.Id == dto.TuyChonBatBuocId))
                return BadRequest("Tùy chọn bắt buộc không tồn tại.");

            if (dto.TuyChonXungDotId.HasValue && !await _context.Options.AnyAsync(o => o.Id == dto.TuyChonXungDotId))
                return BadRequest("Tùy chọn xung đột không tồn tại.");

            // Không cho tự phụ thuộc chính nó
            if (dto.TuyChonBatBuocId == dto.TuyChonChinhId || dto.TuyChonXungDotId == dto.TuyChonChinhId)
                return BadRequest("Không thể thiết lập phụ thuộc/xung đột với chính tùy chọn đó.");

            // Kiểm tra trùng lặp
            if (await _context.OptionDependencies.AnyAsync(d =>
                d.TuyChonChinhId == dto.TuyChonChinhId &&
                d.TuyChonBatBuocId == dto.TuyChonBatBuocId &&
                d.TuyChonXungDotId == dto.TuyChonXungDotId))
                return Conflict("Quy tắc phụ thuộc này đã tồn tại.");

            var dep = new OptionDependency
            {
                TuyChonChinhId = dto.TuyChonChinhId,
                TuyChonBatBuocId = dto.TuyChonBatBuocId,
                TuyChonXungDotId = dto.TuyChonXungDotId,
                GhiChu = dto.GhiChu,
                NgayTao = DateTime.UtcNow
            };

            _context.OptionDependencies.Add(dep);
            await _context.SaveChangesAsync();

            await LogAction("them_phu_thuoc", "phu_thuoc_tuy_chon", dep.Id, new { dep.TuyChonChinhId });

            return CreatedAtAction(nameof(GetDependencyById), new { id = dep.Id }, dep);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDependency(int id, [FromBody] DependencyUpdateDto dto)
        {
            var dep = await _context.OptionDependencies.FindAsync(id);
            if (dep == null) return NotFound();

            if (dto.TuyChonBatBuocId.HasValue && dto.TuyChonBatBuocId == dep.TuyChonChinhId)
                return BadRequest("Không thể bắt buộc chính nó.");

            if (dto.TuyChonXungDotId.HasValue && dto.TuyChonXungDotId == dep.TuyChonChinhId)
                return BadRequest("Không thể xung đột với chính nó.");

            dep.TuyChonBatBuocId = dto.TuyChonBatBuocId;
            dep.TuyChonXungDotId = dto.TuyChonXungDotId;
            dep.GhiChu = dto.GhiChu;

            await _context.SaveChangesAsync();

            await LogAction("sua_phu_thuoc", "phu_thuoc_tuy_chon", id, dto);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDependency(int id)
        {
            var dep = await _context.OptionDependencies.FindAsync(id);
            if (dep == null) return NotFound();

            _context.OptionDependencies.Remove(dep);
            await _context.SaveChangesAsync();

            await LogAction("xoa_phu_thuoc", "phu_thuoc_tuy_chon", id, new { dep.TuyChonChinhId });

            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDependencyById(int id)
        {
            var dep = await _context.OptionDependencies.FindAsync(id);
            if (dep == null) return NotFound();
            return Ok(dep);
        }

        private async Task LogAction(string hanhDong, string loai, int? idThucThe, object chiTiet)
        {
            // Tương tự các controller trước
            var log = new ActivityLog
            {
                NguoiDungId = 1, // thay bằng userId thật
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

    public class DependencyCreateDto
    {
        public int TuyChonChinhId { get; set; }
        public int? TuyChonBatBuocId { get; set; }
        public int? TuyChonXungDotId { get; set; }
        public string? GhiChu { get; set; }
    }

    public class DependencyUpdateDto
    {
        public int? TuyChonBatBuocId { get; set; }
        public int? TuyChonXungDotId { get; set; }
        public string? GhiChu { get; set; }
    }
}