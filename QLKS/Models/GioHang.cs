using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QLKS.Models
{
    public class GioHang
    {
        QLKSDataContext db = new QLKSDataContext();
        public string maLoaiPhong { set; get; }

        public string hinhAnh { set; get; }

        public int soNguoiMax { set; get; }

        public double donGia { set; get; }

        public int soLuong { set; get; }

        public double thanhTien
        {
            get { return soLuong * donGia; }
        }
        public GioHang(string malphong)
        {
            maLoaiPhong = malphong;
            LOAIPHONG loaiphong = db.LOAIPHONGs.SingleOrDefault(s => s.MALOAI == maLoaiPhong);
            soNguoiMax = loaiphong.SONGUOIMAX;
            hinhAnh = loaiphong.HINHANH;
            donGia = double.Parse(loaiphong.DONGIA.ToString());
            soLuong = 1;

        }

    }
}