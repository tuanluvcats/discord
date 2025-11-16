using System;
using System.Collections.Generic;

namespace BanGaRanApi.Models.Entities;

public partial class ChiTietHoaDon
{
    public int MaCthd { get; set; }

    public int? MaHd { get; set; }

    public int? MaSp { get; set; }

    public int? SoLuong { get; set; }

    public decimal? DonGia { get; set; }

    public virtual HoaDon? MaHdNavigation { get; set; }

    public virtual SanPham? MaSpNavigation { get; set; }
}
