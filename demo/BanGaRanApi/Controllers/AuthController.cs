using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BanGaRanApi.Models.Entities;
using BanGaRanApi.Models;

namespace BanGaRanApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AuthController(AppDbContext db)
        {
            _db = db;
        }

        // ============================
        // ĐĂNG KÝ
        // POST: api/auth/register
        // ============================
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (await _db.TaiKhoans.AnyAsync(x => x.TenDangNhap == req.TenDangNhap))
                return BadRequest(new { message = "Tên đăng nhập đã tồn tại." });

            var tk = new TaiKhoan
            {
                TenDangNhap = req.TenDangNhap,
                MatKhau = req.MatKhau,     // bài học: chưa mã hoá (version A)
                VaiTro = "KhachHang",
                TrangThai = 1
            };

            _db.TaiKhoans.Add(tk);
            await _db.SaveChangesAsync();

            var kh = new KhachHang
            {
                HoTen = req.HoTen,
                Email = req.Email,
                MaTK = tk.MaTK
            };

            _db.KhachHangs.Add(kh);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Đăng ký thành công", userId = tk.MaTK });
        }

        // ============================
        // ĐĂNG NHẬP
        // POST: api/auth/login
        // ============================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _db.TaiKhoans
                .Include(x => x.KhachHangs)
                .FirstOrDefaultAsync(x => x.TenDangNhap == req.TenDangNhap
                                        && x.MatKhau == req.MatKhau);

            if (user == null)
                return NotFound(new { message = "Sai tài khoản hoặc mật khẩu" });

            return Ok(new
            {
                MaTK = user.MaTK,
                TenDangNhap = user.TenDangNhap,
                VaiTro = user.VaiTro
            });
        }
    }

    // ============================
    // REQUEST MODELS
    // ============================
    public class RegisterRequest
    {
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }
        public string HoTen { get; set; }
        public string Email { get; set; }
    }

    public class LoginRequest
    {
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }
    }
}
