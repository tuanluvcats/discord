using System;
using System.Collections.Generic;

namespace BanGaRanApi.Models.Entities;

public partial class ThanhToan
{
    public int MaTt { get; set; }

    public int? MaHd { get; set; }

    public string? PhuongThuc { get; set; }

    public decimal? SoTien { get; set; }

    public DateTime? NgayThanhToan { get; set; }

    public virtual HoaDon? MaHdNavigation { get; set; }
}
