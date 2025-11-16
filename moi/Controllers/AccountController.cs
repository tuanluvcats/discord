using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using moi.Models;

namespace moi.Controllers
{
    public class AccountController : Controller
    {
        QL_BanGaRanEntities db = new QL_BanGaRanEntities();

        [HttpGet]
        public ActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult Login(string tendangnhap, string matkhau, string returnUrl = null)
        {
            var tk = db.TaiKhoans.FirstOrDefault(t => t.TenDangNhap == tendangnhap && t.MatKhau == matkhau && t.TrangThai == true);
            if (tk != null)
            {
                // Tạo Forms Authentication Ticket với UserData chứa Role
                var ticket = new FormsAuthenticationTicket(
                    1, // version
                    tk.MaTK.ToString(), // username = MaTK
                    DateTime.Now, // issue date
                    DateTime.Now.AddHours(8), // expiration
                    false, // persistent
                    tk.VaiTro, // userData = Role
                    FormsAuthentication.FormsCookiePath
                );

                string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                Response.Cookies.Add(cookie);

                // Session backup (để tương thích code cũ)
                HttpContext.Session["Role"] = tk.VaiTro;
                HttpContext.Session["MaTK"] = tk.MaTK.ToString();

                if (tk.VaiTro == "Admin")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else // KhachHang
                {
                    var kh = db.KhachHangs.FirstOrDefault(k => k.MaTK == tk.MaTK);
                    HttpContext.Session["UserName"] = kh?.HoTen ?? tendangnhap;

                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);

                    return RedirectToAction("Index", "Home");
                }
            }
            ViewBag.Error = "Sai tên đăng nhập hoặc mật khẩu!";
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            HttpContext.Session.Clear();
            return RedirectToAction("Login", "Account");
        }

        [HttpGet]
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Register(string hoten, string diachi, string dienthoai, string email, string tendangnhap, string matkhau)
        {
            if (db.TaiKhoans.Any(t => t.TenDangNhap == tendangnhap))
            {
                ViewBag.Error = "Tên đăng nhập đã tồn tại!";
                return View();
            }

            var tk = new TaiKhoan
            {
                TenDangNhap = tendangnhap,
                MatKhau = matkhau,
                VaiTro = "KhachHang",
                TrangThai = true
            };
            db.TaiKhoans.Add(tk);
            db.SaveChanges();

            var kh = new KhachHang
            {
                HoTen = hoten,
                DiaChi = diachi,
                DienThoai = dienthoai,
                Email = email,
                MaTK = tk.MaTK
            };
            db.KhachHangs.Add(kh);
            db.SaveChanges();

            return RedirectToAction("Login");
        }
    }
}