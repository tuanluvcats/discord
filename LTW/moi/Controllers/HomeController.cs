using moi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace moi.Controllers
{
    public class HomeController : Controller
    {
        QL_BanGaRanEntities1 db = new QL_BanGaRanEntities1(); 
        public ActionResult Index()
        {
            var sp = db.SanPhams.ToList();
            return View(sp);
        }
  
        public ActionResult VeJollibee()
        {
            return View();
        }
        public ActionResult LienHe()
        {
          return View();
        }
        // HomeController.cs

        // Hiển thị sản phẩm theo loại
        public ActionResult DanhMuc(string tenLoai)
        {
            if (string.IsNullOrEmpty(tenLoai))
                return HttpNotFound();

            var loai = db.LoaiSanPhams.FirstOrDefault(l => l.TenLoai == tenLoai);
            if (loai == null)
                return HttpNotFound();

            var sanPhams = db.SanPhams
                .Where(p => p.MaLoai == loai.MaLoai)
                .ToList();

            ViewBag.TenLoai = tenLoai;
            ViewBag.Loai = loai;

            return View("View", sanPhams);
        }
        [HttpGet]
        public ActionResult SearchProducts(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword) || keyword.Length < 2)
                return Json(new List<object>(), JsonRequestBehavior.AllowGet);

            // Bước 1: Lấy dữ liệu từ DB (chỉ những trường cần)
            var products = db.SanPhams
                .Where(p => p.TenSP.Contains(keyword))
                .Take(8)
                .Select(p => new
                {
                    p.MaSP,
                    p.TenSP,
                    p.Gia,
                    Anh = p.Anh ?? "default.jpg"
                })
                .ToList(); // Đưa vào bộ nhớ trước

            // Bước 2: Dùng C# để tạo URL (bên ngoài LINQ)
            var results = products.Select(p => new
            {
                maSP = p.MaSP,
                tenSP = p.TenSP,
                gia = p.Gia,
                anh = p.Anh,
                url = Url.Action("ChiTietSanPham", "Home", new { id = p.MaSP }, Request.Url.Scheme)
            }).ToList();

            return Json(results, JsonRequestBehavior.AllowGet);
        }
        public ActionResult ChiTietSanPham(int id)
        {
            var sp = db.SanPhams
                .Include("LoaiSanPham")
                .FirstOrDefault(p => p.MaSP == id);

            if (sp == null) return HttpNotFound();

            // Lấy 4 sản phẩm cùng loại, không bao gồm sản phẩm hiện tại
            var relatedProducts = db.SanPhams
                .Where(p => p.MaLoai == sp.MaLoai && p.MaSP != sp.MaSP)
                .OrderBy(p => Guid.NewGuid()) // Ngẫu nhiên
                .Take(4)
                .ToList();

            ViewBag.RelatedProducts = relatedProducts;

            return View(sp);
        }
    }
}