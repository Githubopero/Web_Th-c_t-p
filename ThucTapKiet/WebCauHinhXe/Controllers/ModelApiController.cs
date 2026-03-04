using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCauHinhXe.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json; // Cần install package Newtonsoft.Json nếu chưa có

namespace WebCauHinhXe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModelApiController : ControllerBase
    {
        private readonly TtContext _context;

        public ModelApiController(TtContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách mẫu xe theo dòng xe (seriesId)
        /// GET: api/ModelApi/by-series/{seriesId}
        /// </summary>
        [HttpGet("by-series/{seriesId}")]
        public async Task<IActionResult> GetModelsBySeries(int seriesId)
        {
            var models = await _context.Models
                .Where(m => m.DongXeId == seriesId && m.TrangThaiHoatDong == 1)
                .OrderBy(m => m.TenMauXe)
                .Select(m => new
                {
                    m.Id,
                    m.TenMauXe,
                    m.DuongDanSlug,
                    GiaCoBan = m.GiaCoBan, // frontend sẽ format thành VND
                    m.NamSanXuat,
                    m.MoTa,
                    m.AnhDaiDien
                })
                .ToListAsync();

            if (!models.Any())
            {
                return Ok(new
                {
                    Message = "Dòng xe này chưa có phiên bản nào.",
                    Data = new List<object>()
                });
            }

            return Ok(new
            {
                Message = "Lấy danh sách mẫu xe thành công",
                Data = models
            });
        }

        /// <summary>
        /// Lấy chi tiết một mẫu xe (bao gồm thông số kỹ thuật JSON và ảnh)
        /// GET: api/ModelApi/{modelId}
        /// </summary>
        [HttpGet("{modelId}")]
        public async Task<IActionResult> GetModelDetail(int modelId)
        {
            var model = await _context.Models
                .Where(m => m.Id == modelId && m.TrangThaiHoatDong == 1)
                .Select(m => new
                {
                    m.Id,
                    m.TenMauXe,
                    m.DuongDanSlug,
                    m.GiaCoBan,
                    m.NamSanXuat,
                    m.MoTa,
                    m.AnhDaiDien,
                    ThongSoKyThuat = m.ThongSoKyThuat != null
                        ? JsonConvert.DeserializeObject<object>(m.ThongSoKyThuat)
                        : null,
                    Images = _context.Images
                        .Where(img => img.LoaiThucThe == "mau_xe" && img.IdThucThe == m.Id)
                        .OrderBy(img => img.ThuTu)
                        .Select(img => new { img.DuongDanAnh, img.MoTaAnh, img.GocChup, img.MaMau })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (model == null)
            {
                return NotFound("Không tìm thấy mẫu xe hoặc mẫu xe không hoạt động.");
            }

            return Ok(model);
        }
    }
}