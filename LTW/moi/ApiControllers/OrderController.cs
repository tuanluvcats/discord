using moi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;

namespace moi.ApiControllers
{
    public class OrderDTO
    {
        public int MaHD { get; set; }
        public string TenKhachHang { get; set; }
        public decimal TongTien { get; set; }
        public DateTime NgayLap { get; set; }
        public string TrangThai { get; set; }
    }

    public class RevenueDTO
    {
        public decimal TongDoanhThu { get; set; }
        public int SoDonHoanTat { get; set; }
        public int SoDonDangXuLy { get; set; }
    }

    public class OrderController : ApiController
    {
        QL_BanGaRanEntities1 db = new QL_BanGaRanEntities1();

        [HttpGet]
        [Route("api/Order/GetLatestOrderId")]
        public int GetLatestOrderId()
        {
            var latestOrder = db.HoaDons.OrderByDescending(h => h.MaHD).FirstOrDefault();
            return latestOrder?.MaHD ?? 0;
        }

        [HttpGet]
        [Route("api/Order/NewOrdersSinceId")]
        public List<OrderDTO> GetNewOrdersSinceId(int sinceOrderId = 0)
        {
            var newOrders = db.HoaDons
                .Where(h => h.MaHD > sinceOrderId && h.TrangThai == "Chờ xác nhận")
                .OrderBy(h => h.MaHD)
                .ToList()
                .Select(h => new OrderDTO
                {
                    MaHD = h.MaHD,
                    TenKhachHang = h.KhachHang != null ? h.KhachHang.HoTen : "Khách",
                    TongTien = h.TongTien ?? 0,
                    NgayLap = db.ThanhToans
                                .Where(t => t.MaHD == h.MaHD)
                                .Select(t => t.NgayThanhToan)
                                .FirstOrDefault() ?? h.NgayLap ?? DateTime.Now,
                    TrangThai = h.TrangThai
                })
                .ToList();

            return newOrders;
        }

        [HttpGet]
        [Route("api/Order/PendingCount")]
        public int GetPendingOrdersCount()
        {
            return db.HoaDons.Count(h => h.TrangThai == "Chờ xác nhận");
        }

        [HttpGet]
        [Route("api/Order/Revenue")]
        public RevenueDTO GetRevenue()
        {
            var completedOrders = db.HoaDons
                .Where(h => h.TrangThai == "Hoàn tất" || h.TrangThai == "Đang giao" || h.TrangThai == "Chờ xác nhận")
                .ToList();

            var result = new RevenueDTO
            {
                TongDoanhThu = completedOrders.Where(h => h.TrangThai == "Hoàn tất").Sum(h => h.TongTien ?? 0),
                SoDonHoanTat = completedOrders.Count(h => h.TrangThai == "Hoàn tất"),
                SoDonDangXuLy = completedOrders.Count(h => h.TrangThai != "Hoàn tất")
            };

            return result;
        }
    }
}