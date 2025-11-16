using moi.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;

public class AdminController : Controller
{
    QL_BanGaRanEntities1 db = new QL_BanGaRanEntities1();

    private void CheckAdmin()
    {
        if (HttpContext.Session["Role"]?.ToString() != "Admin")
        {
            HttpContext.Response.Redirect(Url.Action("Login", "Account"), false);
            HttpContext.ApplicationInstance.CompleteRequest();
        }
    }

    public ActionResult Index()
    {
        CheckAdmin();
        ViewBag.TongSanPham = db.SanPhams.Count();
        ViewBag.TongLoai = db.LoaiSanPhams.Count();
        ViewBag.TongHoaDon = db.HoaDons.Count();
        ViewBag.TongKhachHang = db.KhachHangs.Count();
        ViewBag.TongDoanhThu = db.HoaDons
            .Where(h => h.TrangThai == "Hoàn tất")
            .Sum(h => (decimal?)(h.TongTien ?? 0)) ?? 0;
        return View();
    }

    public ActionResult SanPham()
    {
        CheckAdmin();
        return View(); 
    }

    [HttpGet]
    public ActionResult ThemSanPham()
    {
        CheckAdmin();
        ViewBag.ListLoai = new SelectList(db.LoaiSanPhams.ToList(), "MaLoai", "TenLoai");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public ActionResult ThemSanPham(SanPham sp, HttpPostedFileBase Anh)
    {
        CheckAdmin();

        try
        {
            if (ModelState.IsValid)
            {
                // Xử lý upload ảnh
                if (Anh != null && Anh.ContentLength > 0)
                {
                    // Kiểm tra định dạng
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                    var extension = System.IO.Path.GetExtension(Anh.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("Anh", "Chỉ chấp nhận file ảnh: jpg, jpeg, png, gif");
                        ViewBag.ListLoai = new SelectList(db.LoaiSanPhams.ToList(), "MaLoai", "TenLoai", sp.MaLoai);
                        return View(sp);
                    }

                    // Kiểm tra kích thước (max 5MB)
                    if (Anh.ContentLength > 5 * 1024 * 1024)
                    {
                        ModelState.AddModelError("Anh", "File ảnh không được vượt quá 5MB");
                        ViewBag.ListLoai = new SelectList(db.LoaiSanPhams.ToList(), "MaLoai", "TenLoai", sp.MaLoai);
                        return View(sp);
                    }

                    // Tạo tên file unique
                    string fileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
                    string path = Server.MapPath("~/Content/HinhAnhSP/");

                    // Tạo thư mục nếu chưa tồn tại
                    if (!System.IO.Directory.Exists(path))
                    {
                        System.IO.Directory.CreateDirectory(path);
                    }

                    // Lưu file
                    Anh.SaveAs(System.IO.Path.Combine(path, fileName));
                    sp.Anh = fileName;
                }
                else
                {
                    sp.Anh = "default.jpg";
                }

                db.SanPhams.Add(sp);
                db.SaveChanges();

                TempData["Success"] = "Thêm sản phẩm thành công!";
                return RedirectToAction("SanPham");
            }

            ViewBag.ListLoai = new SelectList(db.LoaiSanPhams.ToList(), "MaLoai", "TenLoai", sp.MaLoai);
            return View(sp);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", "Lỗi: " + ex.Message);
            ViewBag.ListLoai = new SelectList(db.LoaiSanPhams.ToList(), "MaLoai", "TenLoai", sp.MaLoai);
            return View(sp);
        }
    }

    public ActionResult DonHang(string mode = "thang")
    {
        CheckAdmin();
        var list = db.HoaDons
              .Include(h => h.KhachHang)
              .OrderByDescending(h => h.NgayLap)
              .ToList();

        object data = null;
        if (mode == "ngay")
        {
            data = db.HoaDons
                     .Where(h => h.TrangThai == "Hoàn tất")
                     .GroupBy(h => h.NgayLap)
                     .Select(g => new
                     {
                         Ngay = g.Key,
                         TongTien = g.Sum(x => (decimal?)(x.TongTien ?? 0)) ?? 0
                     })
                     .OrderBy(x => x.Ngay)
                     .ToList();
        }
        else if (mode == "nam")
        {
            data = db.HoaDons
                     .Where(h => h.TrangThai == "Hoàn tất")
                     .GroupBy(h => h.NgayLap.Value.Year)
                     .Select(g => new
                     {
                         Nam = g.Key,
                         TongTien = g.Sum(x => (decimal?)(x.TongTien ?? 0)) ?? 0
                     })
                     .OrderBy(x => x.Nam)
                     .ToList();
        }
        else
        {
            data = db.HoaDons
                     .Where(h => h.TrangThai == "Hoàn tất")
                     .GroupBy(h => new { h.NgayLap.Value.Year, h.NgayLap.Value.Month })
                     .Select(g => new
                     {
                         Nam = g.Key.Year,
                         Thang = g.Key.Month,
                         TongTien = g.Sum(x => (decimal?)(x.TongTien ?? 0)) ?? 0
                     })
                     .OrderBy(x => x.Nam).ThenBy(x => x.Thang)
                     .ToList();
        }

        ViewBag.Mode = mode;
        ViewBag.ThongKe = data;
        return View(list);
    }

    [HttpGet]
    public ActionResult CapNhatDonHang(int? id)
    {
        CheckAdmin();
        var hd = db.HoaDons.Find(id);
        if (hd == null) return HttpNotFound();

        ViewBag.TrangThaiList = new SelectList(new[]
        {
            new { Value = "Chờ xác nhận", Text = "Chờ xác nhận" },
            new { Value = "Đang giao", Text = "Đang giao" },
            new { Value = "Hoàn tất", Text = "Hoàn tất" },
            new { Value = "Đã hủy", Text = "Đã hủy" }
        }, "Value", "Text", hd.TrangThai);

        var chiTiet = db.ChiTietHoaDons
                        .Where(ct => ct.MaHD == id)
                        .Include("SanPham")
                        .ToList();

        ViewBag.ChiTiet = chiTiet;
        return View(hd);
    }

    [HttpPost]
    public ActionResult CapNhatDonHang(HoaDon hd)
    {
        var existing = db.HoaDons.Find(hd.MaHD);
        if (existing == null) return HttpNotFound();

        existing.TrangThai = hd.TrangThai;
        var chiTiet = db.ChiTietHoaDons.Where(ct => ct.MaHD == hd.MaHD).ToList();
        existing.TongTien = chiTiet.Sum(x => (x.DonGia ?? 0) * (x.SoLuong ?? 0));

        db.SaveChanges();
        TempData["Success"] = "Cập nhật trạng thái & giá tiền hóa đơn thành công!";
        return RedirectToAction("DonHang");
    }
}