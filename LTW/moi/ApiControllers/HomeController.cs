using moi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace moi.ApiControllers
{
    // DTO class
    public class SanPhamDTO
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

    public class HomeController : ApiController
    {
        public List<SanPhamDTO> Get()
        {
            QL_BanGaRanEntities1 db = new QL_BanGaRanEntities1();

            List<SanPhamDTO> list = db.SanPhams
                .Select(sp => new SanPhamDTO
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

            return list;
        }
    }
}