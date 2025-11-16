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
        // GET: Admin
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
            ViewBag.TongDoanhThu = db.HoaDons.Sum(h => (decimal?)(h.TongTien ?? 0)) ?? 0;



            return View();
        }
        public ActionResult SanPham()
        {
            CheckAdmin();
            var list = db.SanPhams
                             .Include("LoaiSanPham")  // <-- Dùng chuỗi
                             .ToList();
            return View(list);
        }
        [HttpGet]
        public ActionResult ThemSanPham()
        {
            ViewBag.ListLoai = new SelectList(db.LoaiSanPhams.ToList(), "MaLoai", "TenLoai");
            return View();
        }
        [HttpPost]
        public ActionResult ThemSanPham(SanPham sp, HttpPostedFileBase Anh)
        {
            if (ModelState.IsValid)
            {
                if (Anh != null && Anh.ContentLength > 0)
                {
                    string fileName = Path.GetFileName(Anh.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/HinhAnhSP/"), fileName);
                    Anh.SaveAs(path);
                    sp.Anh = fileName;
                }

                db.SanPhams.Add(sp);
                db.SaveChanges();

                return RedirectToAction("SanPham");
            }

            // Nếu lỗi → trả về cùng View
            ViewBag.ListLoai = new SelectList(db.LoaiSanPhams.ToList(), "MaLoai", "TenLoai");
            return View(sp); // → Dùng lại ThemSanPham.cshtml
        }

        [HttpGet]
        public ActionResult SuaSanPham(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null) return HttpNotFound();

            ViewBag.ListLoai = new SelectList(db.LoaiSanPhams.ToList(), "MaLoai", "TenLoai", sp.MaLoai);
            return View(sp);
        }

        // === [POST] Lưu sửa sản phẩm ===
        [HttpPost]
        public ActionResult SuaSanPham(SanPham sp, HttpPostedFileBase Anh)
        {
            if (ModelState.IsValid)
            {
                var existing = db.SanPhams.Find(sp.MaSP);
                if (existing == null) return HttpNotFound();

                // Cập nhật các trường
                existing.TenSP = sp.TenSP;
                existing.Gia = sp.Gia;
                existing.SoLuong = sp.SoLuong;
                existing.MaLoai = sp.MaLoai;
                existing.MoTa = sp.MoTa;

                // Xử lý ảnh mới
                if (Anh != null && Anh.ContentLength > 0)
                {
                    // Xóa ảnh cũ (nếu có)
                    if (!string.IsNullOrEmpty(existing.Anh))
                    {
                        var oldPath = Path.Combine(Server.MapPath("~/Content/HinhAnhSP/"), existing.Anh);
                        if (System.IO.File.Exists(oldPath))
                            System.IO.File.Delete(oldPath);
                    }

                    // Lưu ảnh mới
                    string fileName = Path.GetFileName(Anh.FileName);
                    string path = Path.Combine(Server.MapPath("~/Content/HinhAnhSP/"), fileName);
                    Anh.SaveAs(path);
                    existing.Anh = fileName;
                }

                db.SaveChanges();
                return RedirectToAction("SanPham");
            }

            ViewBag.ListLoai = new SelectList(db.LoaiSanPhams.ToList(), "MaLoai", "TenLoai", sp.MaLoai);
            return View(sp);
        }
        [HttpPost]
        public ActionResult XoaSanPham(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null) return Json(new { success = false, message = "Không tìm thấy sản phẩm!" });

            // Xóa ảnh
            if (!string.IsNullOrEmpty(sp.Anh))
            {
                var path = Path.Combine(Server.MapPath("~/Content/HinhAnhSP/"), sp.Anh);
                if (System.IO.File.Exists(path))
                    System.IO.File.Delete(path);
            }

            db.SanPhams.Remove(sp);
            db.SaveChanges();

            return Json(new { success = true, message = "Xóa thành công!" });
        }
        //
        public ActionResult DonHang(string mode = "thang")
        {
            CheckAdmin();

            // Lấy danh sách hóa đơn, kèm thông tin khách hàng
            var list = db.HoaDons
                  .Include(h => h.KhachHang)
                  .OrderByDescending(h => h.NgayLap)
                  .ToList();

            // ----------- PHẦN THỐNG KÊ NGAY TRONG VIEW NÀY -----------
            object data = null;

            if (mode == "ngay")
            {
                data = db.HoaDons
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
                         .GroupBy(h => h.NgayLap.Value.Year)
                         .Select(g => new
                         {
                             Nam = g.Key,
                             TongTien = g.Sum(x => (decimal?)(x.TongTien ?? 0)) ?? 0
                         })
                         .OrderBy(x => x.Nam)
                         .ToList();
            }
            else // theo tháng
            {
                data = db.HoaDons
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
        public ActionResult CapNhatDonHang(int id)
        {
            CheckAdmin();
            var hd = db.HoaDons.Find(id);
            if (hd == null) return HttpNotFound();

            // Danh sách trạng thái đơn hàng mẫu
            ViewBag.TrangThaiList = new SelectList(new[]
            {
        new { Value = "Chờ xác nhận", Text = "Chờ xác nhận" },
        new { Value = "Đang giao", Text = "Đang giao" },
        new { Value = "Hoàn tất", Text = "Hoàn tất" },
        new { Value = "Đã hủy", Text = "Đã hủy" }
             }, "Value", "Text", hd.TrangThai);

            // Lấy chi tiết sản phẩm trong đơn hàng
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
            db.SaveChanges();

            TempData["Success"] = "Cập nhật trạng thái đơn hàng thành công!";
            return RedirectToAction("DonHang");
        }

       
    }
}