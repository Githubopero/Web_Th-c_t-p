using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebCauHinhXe.Models;
using System.Linq;
using System.Threading.Tasks;

namespace WebCauHinhXe.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeriesApiController : ControllerBase
    {
        private readonly TtContext _context;

        public SeriesApiController(TtContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách tất cả dòng xe đang hoạt động
        /// GET: api/SeriesApi
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllSeries()
        {
            var seriesList = await _context.Series
                .Where(s => s.TrangThaiHoatDong == 1)
                .OrderBy(s => s.ThuTuSapXep)
                .Select(s => new
                {
                    s.Id,
                    s.TenDongXe,
                    s.DuongDanSlug,
                    s.MoTa,
                    s.AnhDaiDien,
                    s.ThuTuSapXep
                })
                .ToListAsync();

            if (!seriesList.Any())
            {
                return Ok(new
                {
                    Message = "Hiện chưa có dòng xe nào khả dụng.",
                    Data = new List<object>()
                });
            }

            return Ok(new
            {
                Message = "Lấy danh sách dòng xe thành công",
                Data = seriesList
            });
        }

        /// <summary>
        /// Lấy thông tin chi tiết một dòng xe (bao gồm ảnh)
        /// GET: api/SeriesApi/{id}
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSeriesById(int id)
        {
            var series = await _context.Series
                .Where(s => s.Id == id && s.TrangThaiHoatDong == 1)
                .Select(s => new
                {
                    s.Id,
                    s.TenDongXe,
                    s.DuongDanSlug,
                    s.MoTa,
                    s.AnhDaiDien,
                    s.ThuTuSapXep,
                    Images = _context.Images
                        .Where(img => img.LoaiThucThe == "dong_xe" && img.IdThucThe == s.Id)
                        .OrderBy(img => img.ThuTu)
                        .Select(img => new { img.DuongDanAnh, img.MoTaAnh, img.GocChup })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (series == null)
            {
                return NotFound("Không tìm thấy dòng xe hoặc dòng xe không hoạt động.");
            }

            return Ok(series);
        }
    }
}