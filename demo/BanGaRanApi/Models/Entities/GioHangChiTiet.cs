using System;
using System.Collections.Generic;

namespace BanGaRanApi.Models.Entities;

public partial class GioHangChiTiet
{
    public int MaGhct { get; set; }

    public int? MaGh { get; set; }

    public int? MaSp { get; set; }

    public int? SoLuong { get; set; }

    public virtual GioHang? MaGhNavigation { get; set; }

    public virtual SanPham? MaSpNavigation { get; set; }
}
