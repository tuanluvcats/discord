using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace moi.ApiControllers
{
    [RoutePrefix("api/upload")]
    public class UploadController : ApiController
    {
        private const int MaxSizeBytes = 5 * 1024 * 1024;
        private static readonly string[] AllowedExt = { ".jpg", ".jpeg", ".png", ".gif" };

        [HttpPost]
        [Route("image")]
        public IHttpActionResult UploadImage()
        {
            try
            {
                // 1. Try classic MVC HttpPostedFileBase style (robust in System.Web)
                var httpRequest = HttpContext.Current?.Request;
                if (httpRequest == null)
                    return Ok(new { success = false, message = "Không thể đọc request." });

                if (httpRequest.Files.Count > 0)
                {
                    for (int i = 0; i < httpRequest.Files.Count; i++)
                    {
                        var posted = httpRequest.Files[i];
                        if (posted == null || posted.ContentLength == 0)
                            continue;

                        string result = SavePostedFile(posted, out string err);
                        if (result != null)
                            return Ok(new { success = true, fileName = result });

                        return Ok(new { success = false, message = err });
                    }

                    return Ok(new { success = false, message = "Không có file hợp lệ." });
                }

                // 2. Fallback: multipart provider (in case AJAX sent raw multipart)
                if (!Request.Content.IsMimeMultipartContent())
                    return Ok(new { success = false, message = "Sai Content-Type (cần multipart/form-data)." });

                // Use temp directory to avoid writing directly to final until validated
                string tempRoot = Path.Combine(Path.GetTempPath(), "moi_upload_temp");
                if (!Directory.Exists(tempRoot)) Directory.CreateDirectory(tempRoot);

                var provider = new System.Net.Http.MultipartFormDataStreamProvider(tempRoot);
                var readTask = Request.Content.ReadAsMultipartAsync(provider);
                readTask.Wait(); // synchronous wait inside try/catch ensures we catch errors
                var multipart = readTask.Result;

                if (!multipart.FileData.Any())
                    return Ok(new { success = false, message = "Không có file gửi lên." });

                var fileData = multipart.FileData.First(); // chỉ xử lý 1 ảnh
                string savedName = ProcessProviderFile(fileData, out string errMsg);
                if (savedName != null)
                    return Ok(new { success = true, fileName = savedName });

                return Ok(new { success = false, message = errMsg });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Upload fatal: " + ex);
                return Ok(new { success = false, message = "Lỗi upload: " + ex.Message });
            }
        }

        private string SavePostedFile(HttpPostedFile file, out string error)
        {
            error = null;

            try
            {
                var ext = Path.GetExtension(file.FileName)?.ToLower() ?? "";
                if (!AllowedExt.Contains(ext))
                {
                    error = "Định dạng không hợp lệ (.jpg/.jpeg/.png/.gif)";
                    return null;
                }

                if (file.ContentLength > MaxSizeBytes)
                {
                    error = "File quá lớn (>5MB)";
                    return null;
                }

                string folder = HttpContext.Current.Server.MapPath("~/Content/HinhAnhSP/");
                EnsureFolder(folder, out string folderErr);
                if (folderErr != null)
                {
                    error = folderErr;
                    return null;
                }

                string safeName = GenerateSafeFileName(ext);
                string fullPath = Path.Combine(folder, safeName);

                file.SaveAs(fullPath);
                return safeName;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SavePostedFile error: " + ex);
                error = "Lỗi lưu file: " + ex.Message;
                return null;
            }
        }

        private string ProcessProviderFile(System.Net.Http.MultipartFileData fileData, out string error)
        {
            error = null;
            string tempFile = fileData.LocalFileName; // random temp path
            try
            {
                // Header file name (may include quotes)
                var original = fileData.Headers.ContentDisposition.FileName?.Trim('"') ?? "image";
                var ext = Path.GetExtension(original)?.ToLower() ?? "";

                if (!AllowedExt.Contains(ext))
                {
                    SafeDelete(tempFile);
                    error = "Sai định dạng ảnh";
                    return null;
                }

                var info = new FileInfo(tempFile);
                if (info.Length > MaxSizeBytes)
                {
                    SafeDelete(tempFile);
                    error = "File ảnh > 5MB";
                    return null;
                }

                string folder = HttpContext.Current.Server.MapPath("~/Content/HinhAnhSP/");
                EnsureFolder(folder, out string folderErr);
                if (folderErr != null)
                {
                    SafeDelete(tempFile);
                    error = folderErr;
                    return null;
                }

                string safeName = GenerateSafeFileName(ext);
                string finalPath = Path.Combine(folder, safeName);
                File.Move(tempFile, finalPath);

                return safeName;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("ProcessProviderFile error: " + ex);
                SafeDelete(tempFile);
                error = "Lỗi xử lý file: " + ex.Message;
                return null;
            }
        }

        private string GenerateSafeFileName(string ext)
        {
            // Random + timestamp to reduce collision risk and avoid original name issues
            return $"{DateTime.Now:yyyyMMddHHmmss}_{Path.GetRandomFileName().Replace(".", "")}{ext}";
        }

        private void EnsureFolder(string folder, out string error)
        {
            error = null;
            try
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("EnsureFolder error: " + ex);
                error = "Không tạo được thư mục ảnh: " + ex.Message;
            }
        }

        private void SafeDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                    File.Delete(path);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("SafeDelete warning: " + ex);
            }
        }
    }
}