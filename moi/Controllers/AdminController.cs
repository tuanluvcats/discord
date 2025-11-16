using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using moi.Models;

namespace moi.Controllers
{
    public class AdminController : Controller
    {
        QL_BanGaRanEntities db = new QL_BanGaRanEntities();

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

        [HttpGet]
        public ActionResult ThemSanPham()
        {
            CheckAdmin();
            ViewBag.ListLoai = new SelectList(db.LoaiSanPhams.ToList(), "MaLoai", "TenLoai");
            return View(new SanPham());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemSanPham(SanPham model, HttpPostedFileBase Anh)
        {
            CheckAdmin();
            ViewBag.ListLoai = new SelectList(db.LoaiSanPhams.ToList(), "MaLoai", "TenLoai");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string savedFileName = "default.jpg";
            if (Anh != null && Anh.ContentLength > 0)
            {
                var ext = Path.GetExtension(Anh.FileName).ToLower();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                if (!allowed.Contains(ext))
                {
                    ModelState.AddModelError("", "Chỉ chấp nhận file ảnh (.jpg, .png, .gif)");
                    return View(model);
                }
                if (Anh.ContentLength > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("", "File quá lớn (tối đa 5MB)");
                    return View(model);
                }

                var folder = Server.MapPath("~/Content/HinhAnhSP/");
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                savedFileName = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N.Substring(0,8)}{ext}";
                var fullPath = Path.Combine(folder, savedFileName);
                try
                {
                    Anh.SaveAs(fullPath);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi lưu file: " + ex.Message);
                    return View(model);
                }
            }

            model.Anh = savedFileName;
            if (model.Gia == null) model.Gia = 0;
            if (model.SoLuong == null) model.SoLuong = 0;

            db.SanPhams.Add(model);
            db.SaveChanges();

            TempData["ThongBao"] = "Thêm sản phẩm thành công!";
            return RedirectToAction("SanPham");
        }
    }
}

