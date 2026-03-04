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
    [Route("api/admin/models")]
    [ApiController]
    //[Authorize]
    public class AdminModelController : ControllerBase
    {
        private readonly TtContext _context;

        public AdminModelController(TtContext context)
        {
            _context = context;
        }

        // GET: api/admin/models?seriesId=3
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int? seriesId = null)
        {
            var query = _context.Models.AsQueryable();

            if (seriesId.HasValue)
                query = query.Where(m => m.DongXeId == seriesId);

            var models = await query
                .Include(m => m.DongXe)
                .OrderBy(m => m.TenMauXe)
                .Select(m => new
                {
                    m.Id,
                    m.TenMauXe,
                    m.DuongDanSlug,
                    m.GiaCoBan,
                    m.NamSanXuat,
                    m.MoTa,
                    m.AnhDaiDien,
                    m.TrangThaiHoatDong,
                    SeriesName = m.DongXe.TenDongXe
                })
                .ToListAsync();

            return Ok(models);
        }

        // POST: api/admin/models
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ModelCreateDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!await _context.Series.AnyAsync(s => s.Id == dto.DongXeId))
                return BadRequest("Dòng xe không tồn tại.");

            if (await _context.Models.AnyAsync(m => m.DuongDanSlug == dto.DuongDanSlug))
                return Conflict("Slug đã tồn tại.");

            if (dto.GiaCoBan <= 0)
                return BadRequest("Giá cơ bản phải lớn hơn 0.");

            // Kiểm tra JSON hợp lệ
            if (!string.IsNullOrWhiteSpace(dto.ThongSoKyThuat))
            {
                try
                {
                    JsonDocument.Parse(dto.ThongSoKyThuat);
                }
                catch
                {
                    return BadRequest("Thông số kỹ thuật phải là JSON hợp lệ.");
                }
            }

            var model = new Model
            {
                DongXeId = dto.DongXeId,
                TenMauXe = dto.TenMauXe.Trim(),
                DuongDanSlug = dto.DuongDanSlug.Trim().ToLower(),
                GiaCoBan = dto.GiaCoBan,
                NamSanXuat = dto.NamSanXuat,
                MoTa = dto.MoTa,
                ThongSoKyThuat = dto.ThongSoKyThuat,
                AnhDaiDien = dto.AnhDaiDien,
                TrangThaiHoatDong = 1,
                NgayTao = DateTime.UtcNow
            };

            _context.Models.Add(model);
            await _context.SaveChangesAsync();

            await LogAction("them_mau_xe", "mau_xe", model.Id, new { model.TenMauXe, model.DongXeId });

            return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
        }

        // PUT: api/admin/models/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ModelUpdateDto dto)
        {
            var model = await _context.Models.FindAsync(id);
            if (model == null) return NotFound();

            if (dto.DuongDanSlug != null && dto.DuongDanSlug != model.DuongDanSlug &&
                await _context.Models.AnyAsync(m => m.DuongDanSlug == dto.DuongDanSlug))
                return Conflict("Slug đã tồn tại.");

            if (dto.GiaCoBan.HasValue && dto.GiaCoBan <= 0)
                return BadRequest("Giá cơ bản phải lớn hơn 0.");

            if (!string.IsNullOrWhiteSpace(dto.ThongSoKyThuat))
            {
                try
                {
                    JsonDocument.Parse(dto.ThongSoKyThuat);
                }
                catch
                {
                    return BadRequest("Thông số kỹ thuật phải là JSON hợp lệ.");
                }
            }

            model.TenMauXe = dto.TenMauXe?.Trim() ?? model.TenMauXe;
            model.DuongDanSlug = dto.DuongDanSlug?.Trim().ToLower() ?? model.DuongDanSlug;
            model.GiaCoBan = dto.GiaCoBan ?? model.GiaCoBan;
            model.NamSanXuat = dto.NamSanXuat ?? model.NamSanXuat;
            model.MoTa = dto.MoTa ?? model.MoTa;
            model.ThongSoKyThuat = dto.ThongSoKyThuat ?? model.ThongSoKyThuat;
            model.AnhDaiDien = dto.AnhDaiDien ?? model.AnhDaiDien;
            model.TrangThaiHoatDong = dto.TrangThaiHoatDong ?? model.TrangThaiHoatDong;

            await _context.SaveChangesAsync();

            await LogAction("sua_mau_xe", "mau_xe", id, dto);

            return NoContent();
        }

        // DELETE: api/admin/models/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _context.Models.FindAsync(id);
            if (model == null) return NotFound();

            model.TrangThaiHoatDong = 0;
            // hoặc _context.Models.Remove(model);

            await _context.SaveChangesAsync();

            await LogAction("xoa_mau_xe", "mau_xe", id, new { model.TenMauXe });

            return NoContent();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var model = await _context.Models
                .Include(m => m.DongXe)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (model == null) return NotFound();
            return Ok(model);
        }

        private async Task LogAction(string hanhDong, string loai, int? idThucThe, object chiTiet)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId)) return;

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

    public class ModelCreateDto
    {
        public int DongXeId { get; set; }
        public string TenMauXe { get; set; } = null!;
        public string DuongDanSlug { get; set; } = null!;
        public decimal GiaCoBan { get; set; }
        public short? NamSanXuat { get; set; }
        public string? MoTa { get; set; }
        public string? ThongSoKyThuat { get; set; }  // JSON string
        public string? AnhDaiDien { get; set; }
    }

    public class ModelUpdateDto
    {
        public string? TenMauXe { get; set; }
        public string? DuongDanSlug { get; set; }
        public decimal? GiaCoBan { get; set; }
        public short? NamSanXuat { get; set; }
        public string? MoTa { get; set; }
        public string? ThongSoKyThuat { get; set; }
        public string? AnhDaiDien { get; set; }
        public sbyte? TrangThaiHoatDong { get; set; }
    }
}