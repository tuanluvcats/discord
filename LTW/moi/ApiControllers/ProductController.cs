using moi.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace moi.ApiControllers
{
    public class ProductDTO
    {
        public int MaSP { get; set; }
        public string TenSP { get; set; }
        public decimal? Gia { get; set; }
        public string MoTa { get; set; }
        public int? SoLuong { get; set; }
        public string Anh { get; set; }
        public int? MaLoai { get; set; }
        public string TenLoai { get; set; }
    }

    [RoutePrefix("api/Product")]
    public class ProductController : ApiController
    {
        private readonly QL_BanGaRanEntities1 db = new QL_BanGaRanEntities1();

        // GET /api/Product
        [HttpGet]
        [Route("")]
        public IHttpActionResult GetAll()
        {
            var products = db.SanPhams
                .Select(sp => new ProductDTO
                {
                    MaSP = sp.MaSP,
                    TenSP = sp.TenSP,
                    Gia = sp.Gia,
                    MoTa = sp.MoTa,
                    SoLuong = sp.SoLuong,
                    Anh = sp.Anh,
                    MaLoai = sp.MaLoai,
                    TenLoai = sp.LoaiSanPham != null ? sp.LoaiSanPham.TenLoai : null
                })
                .ToList();
            return Ok(products);
        }

        // GET /api/Product/{id}
        [HttpGet]
        [Route("{id:int}")]
        public IHttpActionResult Get(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null) return NotFound();

            var dto = new ProductDTO
            {
                MaSP = sp.MaSP,
                TenSP = sp.TenSP,
                Gia = sp.Gia,
                MoTa = sp.MoTa,
                SoLuong = sp.SoLuong,
                Anh = sp.Anh,
                MaLoai = sp.MaLoai,
                TenLoai = sp.LoaiSanPham?.TenLoai
            };
            return Ok(dto);
        }

        // GET /api/Product/Categories
        [HttpGet]
        [Route("Categories")]
        public IHttpActionResult GetCategories()
        {
            var list = db.LoaiSanPhams
                .Select(l => new { l.MaLoai, l.TenLoai })
                .ToList();
            return Ok(list);
        }

        // POST JSON create
        [HttpPost]
        [Route("")]
        public IHttpActionResult Create([FromBody] ProductDTO dto)
        {
            if (dto == null) return BadRequest("Dữ liệu rỗng");
            if (string.IsNullOrWhiteSpace(dto.TenSP)) return BadRequest("Tên sản phẩm trống");

            var sp = new SanPham
            {
                TenSP = dto.TenSP,
                Gia = dto.Gia ?? 0,
                MoTa = dto.MoTa,
                SoLuong = dto.SoLuong ?? 0,
                Anh = string.IsNullOrEmpty(dto.Anh) ? "default.jpg" : dto.Anh,
                MaLoai = dto.MaLoai
            };
            db.SanPhams.Add(sp);
            db.SaveChanges();
            return Ok(new { success = true, message = "Thêm sản phẩm thành công!", maSP = sp.MaSP });
        }

        // PUT JSON update
        [HttpPut]
        [Route("{id:int}")]
        public IHttpActionResult Update(int id, [FromBody] ProductDTO dto)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null) return NotFound();
            if (string.IsNullOrWhiteSpace(dto.TenSP))
                return BadRequest("Tên sản phẩm trống");

            sp.TenSP = dto.TenSP;
            sp.Gia = dto.Gia;
            sp.MoTa = dto.MoTa;
            sp.SoLuong = dto.SoLuong;
            sp.MaLoai = dto.MaLoai;
            if (!string.IsNullOrEmpty(dto.Anh))
                sp.Anh = dto.Anh;

            db.SaveChanges();
            return Ok(new { success = true, message = "Cập nhật thành công!" });
        }

        // DELETE
        [HttpDelete]
        [Route("{id:int}")]
        public IHttpActionResult Delete(int id)
        {
            var sp = db.SanPhams.Find(id);
            if (sp == null) return NotFound();

            db.SanPhams.Remove(sp);
            db.SaveChanges();
            return Ok(new { success = true, message = "Xóa thành công!" });
        }

        // POST multipart create
        [HttpPost]
        [Route("create-with-image")]
        public async System.Threading.Tasks.Task<IHttpActionResult> CreateWithImage()
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                    return BadRequest("Content-Type phải là multipart/form-data");

                var root = HttpContext.Current.Server.MapPath("~/Content/HinhAnhSP/");
                if (!Directory.Exists(root)) Directory.CreateDirectory(root);

                var provider = new MultipartFormDataStreamProvider(root);
                await Request.Content.ReadAsMultipartAsync(provider);

                var tenSP = provider.FormData["TenSP"];
                if (string.IsNullOrWhiteSpace(tenSP))
                    return Ok(new { success = false, message = "Tên sản phẩm không được trống" });

                decimal gia = 0; decimal.TryParse(provider.FormData["Gia"], out gia);
                int soLuong = 0; int.TryParse(provider.FormData["SoLuong"], out soLuong);
                int? maLoai = null; if (int.TryParse(provider.FormData["MaLoai"], out var ml)) maLoai = ml;
                var moTa = provider.FormData["MoTa"];

                string fileName = "default.jpg";
                if (provider.FileData.Any())
                {
                    var fileData = provider.FileData.First();
                    if (!TryProcessImageFile(fileData, root, out fileName, out string err))
                        return Ok(new { success = false, message = err });
                }

                var sp = new SanPham
                {
                    TenSP = tenSP,
                    Gia = gia,
                    SoLuong = soLuong,
                    MoTa = moTa,
                    MaLoai = maLoai,
                    Anh = fileName
                };
                db.SanPhams.Add(sp);
                db.SaveChanges();

                return Ok(new { success = true, message = "Thêm sản phẩm thành công!", maSP = sp.MaSP, fileName });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("CreateWithImage error: " + ex);
                return Ok(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        // PUT multipart update-with-image
        [HttpPut]
        [Route("update-with-image/{id:int}")]
        public async System.Threading.Tasks.Task<IHttpActionResult> UpdateWithImage(int id)
        {
            try
            {
                if (!Request.Content.IsMimeMultipartContent())
                    return BadRequest("Content-Type phải là multipart/form-data");

                var sp = db.SanPhams.Find(id);
                if (sp == null) return NotFound();

                var root = HttpContext.Current.Server.MapPath("~/Content/HinhAnhSP/");
                if (!Directory.Exists(root)) Directory.CreateDirectory(root);

                var provider = new MultipartFormDataStreamProvider(root);
                await Request.Content.ReadAsMultipartAsync(provider);

                var tenSP = provider.FormData["TenSP"];
                if (string.IsNullOrWhiteSpace(tenSP))
                    tenSP = sp.TenSP;

                decimal gia; if (!decimal.TryParse(provider.FormData["Gia"], out gia)) gia = sp.Gia ?? 0;
                int soLuong; if (!int.TryParse(provider.FormData["SoLuong"], out soLuong)) soLuong = sp.SoLuong ?? 0;
                int? maLoai = sp.MaLoai;
                if (int.TryParse(provider.FormData["MaLoai"], out var ml)) maLoai = ml;
                var moTa = provider.FormData["MoTa"];
                if (moTa == null) moTa = sp.MoTa;

                string newFileName = sp.Anh;
                bool changedImage = false;

                if (provider.FileData.Any())
                {
                    var fileData = provider.FileData.First();
                    if (!TryProcessImageFile(fileData, root, out newFileName, out string err))
                        return Ok(new { success = false, message = err });

                    changedImage = !string.Equals(newFileName, sp.Anh, StringComparison.OrdinalIgnoreCase);
                }

                sp.TenSP = tenSP;
                sp.Gia = gia;
                sp.SoLuong = soLuong;
                sp.MaLoai = maLoai;
                sp.MoTa = moTa;
                if (changedImage)
                {
                    TryDeleteOldImage(root, sp.Anh);
                    sp.Anh = newFileName;
                }

                db.SaveChanges();
                return Ok(new { success = true, message = "Cập nhật sản phẩm thành công!", fileName = sp.Anh });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("UpdateWithImage error: " + ex);
                return Ok(new { success = false, message = "Lỗi: " + ex.Message });
            }
        }

        private bool TryProcessImageFile(MultipartFileData fileData, string root, out string savedName, out string error)
        {
            savedName = "default.jpg";
            error = null;
            try
            {
                var original = fileData.Headers.ContentDisposition.FileName?.Trim('"') ?? "image";
                var ext = Path.GetExtension(original).ToLower();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif" };
                if (!allowed.Contains(ext))
                {
                    File.Delete(fileData.LocalFileName);
                    error = "Sai định dạng ảnh";
                    return false;
                }
                var info = new FileInfo(fileData.LocalFileName);
                if (info.Length > 5 * 1024 * 1024)
                {
                    File.Delete(fileData.LocalFileName);
                    error = "File ảnh > 5MB";
                    return false;
                }
                savedName = $"{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N.Substring(0, 8)}{ext}";
                File.Move(fileData.LocalFileName, Path.Combine(root, savedName));
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("TryProcessImageFile error: " + ex);
                error = "Lỗi xử lý ảnh: " + ex.Message;
                return false;
            }
        }

        private void TryDeleteOldImage(string root, string oldName)
        {
            try
            {
                if (!string.IsNullOrEmpty(oldName) &&
                    !string.Equals(oldName, "default.jpg", StringComparison.OrdinalIgnoreCase))
                {
                    var path = Path.Combine(root, oldName);
                    if (File.Exists(path))
                        File.Delete(path);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Delete old image failed: " + ex.Message);
            }
        }
    }
}