using System;
using System.Collections.Generic;

namespace BanGaRanApi.Models.Entities;

public partial class NhanVien
{
    public int MaNv { get; set; }

    public string? HoTen { get; set; }

    public string? Email { get; set; }

    public string? DienThoai { get; set; }

    public int? MaTk { get; set; }

    public virtual ICollection<HoaDon> HoaDons { get; set; } = new List<HoaDon>();

    public virtual TaiKhoan? MaTkNavigation { get; set; }
}
