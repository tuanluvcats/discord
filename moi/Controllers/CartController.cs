using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using moi.Models;

namespace moi.Controllers
{
    public class CartController : Controller
    {
        QL_BanGaRanEntities db = new QL_BanGaRanEntities();

        private GioHang GetOrCreateCart()
        {
            if (Session["MaTK"] == null)
                return null;

            int maTK = int.Parse(Session["MaTK"].ToString());
            var cart = db.GioHangs.FirstOrDefault(g => g.MaTK == maTK);

            if (cart == null)
            {
                cart = new GioHang
                {
                    MaTK = maTK,
                    NgayCapNhat = DateTime.Now
                };
                db.GioHangs.Add(cart);
                db.SaveChanges();
            }

            return cart;
        }

        public ActionResult Index()
        {
            var cart = GetOrCreateCart();
            if (cart == null)
                return RedirectToAction("Login", "Account");

            var items = db.GioHangChiTiets
                          .Where(g => g.MaGH == cart.MaGH)
                          .ToList();

            return View(items);
        }

        [HttpPost]
        public ActionResult ThemVaoGio(int maSP, int soLuong = 1)
        {
            var cart = GetOrCreateCart();
            if (cart == null)
                return Json(new { success = false, message = "Vui lòng đăng nhập!" });

            var item = db.GioHangChiTiets
                         .FirstOrDefault(g => g.MaGH == cart.MaGH && g.MaSP == maSP);

            if (item != null)
            {
                item.SoLuong += soLuong;
            }
            else
            {
                item = new GioHangChiTiet
                {
                    MaGH = cart.MaGH,
                    MaSP = maSP,
                    SoLuong = soLuong
                };
                db.GioHangChiTiets.Add(item);
            }

            cart.NgayCapNhat = DateTime.Now;
            db.SaveChanges();

            return Json(new { success = true, message = "Đã thêm vào giỏ hàng!" });
        }

        [HttpPost]
        public ActionResult CapNhatSoLuong(int maGHCT, int soLuong)
        {
            var item = db.GioHangChiTiets.Find(maGHCT);
            if (item == null)
                return Json(new { success = false });

            if (soLuong <= 0)
            {
                db.GioHangChiTiets.Remove(item);
            }
            else
            {
                item.SoLuong = soLuong;
            }

            db.SaveChanges();
            return Json(new { success = true });
        }

        [HttpPost]
        public ActionResult XoaKhoiGio(int maGHCT)
        {
            var item = db.GioHangChiTiets.Find(maGHCT);
            if (item != null)
            {
                db.GioHangChiTiets.Remove(item);
                db.SaveChanges();
            }

            return Json(new { success = true });
        }

        [HttpGet]
        public ActionResult ThanhToan()
        {
            var cart = GetOrCreateCart();
            if (cart == null)
                return RedirectToAction("Login", "Account");

            var items = db.GioHangChiTiets
                          .Where(g => g.MaGH == cart.MaGH)
                          .ToList();

            if (!items.Any())
                return RedirectToAction("Index");

            return View(items);
        }

        [HttpPost]
        public ActionResult XacNhanThanhToan(string phuongThuc)
        {
            var cart = GetOrCreateCart();
            if (cart == null)
                return Json(new { success = false, message = "Lỗi giỏ hàng!" });

            var items = db.GioHangChiTiets
                          .Where(g => g.MaGH == cart.MaGH)
                          .ToList();

            if (!items.Any())
                return Json(new { success = false, message = "Giỏ hàng trống!" });

            int maTK = int.Parse(Session["MaTK"].ToString());
            var kh = db.KhachHangs.FirstOrDefault(k => k.MaTK == maTK);

            decimal tongTien = 0;
            foreach (var item in items)
            {
                var sp = db.SanPhams.Find(item.MaSP);
                tongTien += (sp.Gia ?? 0) * (item.SoLuong ?? 0);
            }

            var hoaDon = new HoaDon
            {
                NgayLap = DateTime.Now, 
                TongTien = tongTien,
                TrangThai = "Chờ xác nhận",
                MaKH = kh?.MaKH,
                MaNV = null
            };
            db.HoaDons.Add(hoaDon);
            db.SaveChanges();

            foreach (var item in items)
            {
                var sp = db.SanPhams.Find(item.MaSP);
                var chiTiet = new ChiTietHoaDon
                {
                    MaHD = hoaDon.MaHD,
                    MaSP = item.MaSP,
                    SoLuong = item.SoLuong ?? 0,
                    DonGia = sp.Gia ?? 0
                };
                db.ChiTietHoaDons.Add(chiTiet);
            }
            db.SaveChanges();

            var thanhToan = new ThanhToan
            {
                MaHD = hoaDon.MaHD,
                PhuongThuc = phuongThuc,
                SoTien = tongTien,
                NgayThanhToan = DateTime.Now
            };
            db.ThanhToans.Add(thanhToan);

            foreach (var item in items.ToList())
            {
                db.GioHangChiTiets.Remove(item);
            }
            db.SaveChanges();

            return Json(new { success = true, message = "Đặt hàng thành công!" });
        }
    }
}