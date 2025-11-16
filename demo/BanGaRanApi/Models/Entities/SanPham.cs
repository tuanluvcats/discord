using System;
using System.Collections.Generic;

namespace BanGaRanApi.Models.Entities;

public partial class SanPham
{
    public int MaSp { get; set; }

    public string? TenSp { get; set; }

    public decimal? Gia { get; set; }

    public string? MoTa { get; set; }

    public int? SoLuong { get; set; }

    public string? Anh { get; set; }

    public int? MaLoai { get; set; }

    public virtual ICollection<ChiTietHoaDon> ChiTietHoaDons { get; set; } = new List<ChiTietHoaDon>();

    public virtual ICollection<GioHangChiTiet> GioHangChiTiets { get; set; } = new List<GioHangChiTiet>();

    public virtual LoaiSanPham? MaLoaiNavigation { get; set; }
}
