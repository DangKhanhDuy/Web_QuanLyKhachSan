using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLKS.Models;
using PagedList;

namespace QLKS.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /Phong/
        QLKSDataContext dulieu = new QLKSDataContext();
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult DanhMucLoapPhong(int? page)
        {
            List<LOAIPHONG> loaiphong = dulieu.LOAIPHONGs.OrderByDescending(s => s.MALOAI).ToList();
            if (page == null) { page = 1; }

            var links = (from l in dulieu.LOAIPHONGs
                         select l).OrderByDescending(x => x.MALOAI);

            int size = 12;

            int number = (page ?? 1);

            return View(links.ToPagedList(number, size));
        }


        public ActionResult XemCT(string maphong)
        {
            LOAIPHONG mlp = dulieu.LOAIPHONGs.Single(s => s.MALOAI == maphong);
            var a = mlp.MALOAI;
            var sl = dulieu.PHONGs.Count(p => p.MALOAI == maphong && p.TRANGTHAI.ToLower() == "trống");
            ViewBag.SoLuongPhongTrong = sl;
            //gợi ý thêm loại phòng khác
            List<LOAIPHONG> listDeXuat = dulieu.LOAIPHONGs.Where(sp => sp.MALOAI == a).Take(4).ToList();
            ViewBag.dsDeXuat = listDeXuat;
            if (mlp == null)
            {
                //khong tim thay chi tiet san pham
                return HttpNotFound();
            }
            List<DICHVU> listdv = dulieu.DICHVUs.Take(6).ToList();
            ViewBag.dsDichVu = listdv;

            return View(mlp);
        }
        //dịch vụ
        public ActionResult QLDichVu(int? page)
        {
            var list = dulieu.DICHVUs.OrderByDescending(s => s.MADV).ToList().ToPagedList(page ?? 1, 10);
            return View(list);
        }
        [HttpPost]
        public ActionResult QLDichVu(int? page, FormCollection f)
        {
            var ten = f["timkiem"];
            var ListDH = dulieu.DICHVUs.Where(x => x.TENDICHVU.Contains(ten)).ToList();
            if (page == null) page = 1;
            var links = (from l in dulieu.DICHVUs.Where(x => x.TENDICHVU.Contains(ten))
                         select l).OrderByDescending(s => s.TENDICHVU);

            int pageSize = 8;
            int pageNumber = (page ?? 1);
            return View(links.ToPagedList(pageNumber, pageSize));
        }
        public int tongslDV()
        {
            List<DICHVU> lst = dulieu.DICHVUs.ToList();
            int count = lst.Count;
            return count;
        }

        public ActionResult ThemDV()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemDV([Bind(Include = "MADV,TENDICHVU,DONGIA,MOTA")] DICHVU dv)
        {
            if (ModelState.IsValid)
            {
                dv.MADV = "DV" + (tongslDV() + 1).ToString();
                dulieu.DICHVUs.InsertOnSubmit(dv);
                dulieu.SubmitChanges();
                return RedirectToAction("QLDichVu");
            }

            return View(dv);
        }
        public ActionResult XoaDV(string id)
        {
            try
            {
                if (dulieu.DICHVUs.FirstOrDefault(x => x.MADV == id) != null)
                {
                    DICHVU g = dulieu.DICHVUs.SingleOrDefault(x => x.MADV == id);
                    dulieu.DICHVUs.DeleteOnSubmit(g);
                    dulieu.SubmitChanges();
                    return RedirectToAction("QLDichVu", "DichVu");
                }
            }
            catch
            {
            }
            return View();
        }

        // đặt phòng
        public List<GioHang> LayGioHang()
        {
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang == null)
            {
                lstGioHang = new List<GioHang>();
                Session["GioHang"] = lstGioHang;
            }
            return lstGioHang;
        }

        public ActionResult ThemGioHang(string malphong, string strURL)
        {
            if (Session["kh"] == null)
            {
                return RedirectToAction("DangNhap", "Home");
            }
            else
            {
                List<GioHang> lstGioHang = LayGioHang();
                GioHang loaiphong = lstGioHang.Find(sp => sp.maLoaiPhong == malphong);
                if (lstGioHang.SingleOrDefault(s => s.maLoaiPhong == malphong) == null)
                {

                    loaiphong = new GioHang(malphong);
                    lstGioHang.Add(loaiphong);
                    return Redirect(strURL);
                }
                else
                {
                    ViewBag.TB = "Đã có loại phòng trong giỏ!";
                    loaiphong.soLuong++;
                    return Redirect(strURL);
                }
            }
        }
        private int allSL()
        {
            int sl = 0;
            List<GioHang> listGioHang = Session["GioHang"] as List<GioHang>;
            if (listGioHang != null)
            {
                sl += listGioHang.Sum(sp => sp.soLuong);
            }
            return sl;
        }

        private double tinhTongTien()
        {
            double ttt = 0;
            List<GioHang> lstGioHang = Session["GioHang"] as List<GioHang>;
            if (lstGioHang != null)
            {
                ttt += lstGioHang.Sum(sp => sp.thanhTien);
            }
            return ttt;
        }


        public ActionResult GioHang()
        {
            if (Session["kh"] == null)
            {
                return RedirectToAction("DangNhap", "Home");
            }
            if (Session["GioHang"] == null)
            {
                ViewBag.cart = "Chưa có phòng được chọn";
                return View();
            }
            else
            {
                List<GioHang> lstGioHang = LayGioHang();
                ViewBag.tongSoLuong = allSL();
                ViewBag.TongThanhTien = tinhTongTien();

                return View(lstGioHang);
            }
        }
        [HttpPost]
        public ActionResult ChinhSua(string malphong, FormCollection c)
        {
            if (Session["kh"] == null)
            {
                return RedirectToAction("Signin", "KhachHang");
            }
            else
            {
                TAIKHOAN kh = Session["kh"] as TAIKHOAN;
                List<GioHang> lstGioHang = LayGioHang();
                GioHang sp = lstGioHang.Single(s => s.maLoaiPhong == malphong);
                if (sp != null)
                {
                    sp.soLuong = int.Parse(c["quantity"].ToString());
                }
                return RedirectToAction("GioHang", "GioHang");

            }
        }

        public ActionResult SL_TT_GioHang()
        {
            ViewBag.tongSoLuong = allSL();
            ViewBag.TongThanhTien = tinhTongTien();


            return PartialView();
        }

        public ActionResult XoaGioHang(string malphong)
        {
            List<GioHang> lstGioHang = LayGioHang();
            GioHang sp = lstGioHang.SingleOrDefault(s => s.maLoaiPhong == malphong);
            if (sp != null)
            {
                lstGioHang.RemoveAll(s => s.maLoaiPhong == malphong);
                return RedirectToAction("GioHang", "GioHang");
            }
            else
            {
                return RedirectToAction("DanhMucLoapPhong", "Admin");

            }
        }

        public ActionResult XoaAllGioHang()
        {
            List<GioHang> lstGioHang = LayGioHang();
            lstGioHang.Clear();
            return RedirectToAction("DanhMucLoapPhong", "Admin");

        }
        public int tongsldonDat()
        {
            List<DATPHONG> lst = dulieu.DATPHONGs.ToList();
            int count = lst.Count;
            return count;
        }
        public ActionResult DatHang()
        {
            if (Session["kh"] == null)
            {
                return RedirectToAction("DangNhap", "Home");
            }
            if (Session["GioHang"] == null)
            {
                return RedirectToAction("Index", "Home");
            }
            else
            {
                List<GioHang> lstGioHang = LayGioHang();
                ViewBag.tongSoLuong = allSL();
                ViewBag.TongThanhTien = tinhTongTien();
                ViewBag.tienCoc = tinhTongTien() * 0.3;

                return View(lstGioHang);
            }
        }
        [HttpPost]
        public ActionResult DatHang(FormCollection f)
        {
            DATPHONG dp = new DATPHONG();
            KHACHHANG kh = (KHACHHANG)Session["kh"];
            List<GioHang> gh = LayGioHang();
            string maloai = "";
            foreach (var j in gh)
            {
                maloai = j.maLoaiPhong; //char 10
            }

            DateTime ngayDat = DateTime.Now; //datetime

            DateTime ngayTra = DateTime.Parse(f["txtNgaytra"]); //datetime
            var check = f["phuongthucthanhtoan"];

            dp.MADATPHONG = "DP" + (tongsldonDat() + 1).ToString(); // char 10
            dp.MAKH = kh.MAKH; // char10
            dp.MALOAI = maloai; //char 10
            dp.SONGUOIO = int.Parse(f["txtSoNguoiO"]); // int
            dp.THOIGIANDAT = ngayDat;
            dp.THOIGIANTRA = ngayTra;
            TimeSpan soNgayO = ngayTra - ngayDat;
            dp.SoNgayO = soNgayO.Days;
            object tongTien = tinhTongTien();
            if (double.TryParse(tongTien.ToString(), out double thanhTien))
            {
                dp.ThanhTien = (decimal?)thanhTien;
            }
            //them trang thai thanh toan don dat phong voi tien coc = 30% tien phong
            //huy phong hoan 20% coc trong vong 1 ngay
            //huy phong voi thoi gian dat > 2 ngay va gan ngay dat <1 ngay = hoan 10%

            if (check == "NH")
            {
                dp.TinhTrangThanhToan = 1; //NganHang

            }
            else
            if (check == "PP")
            {
                dp.TinhTrangThanhToan = 0; //paypal

            }
            else
            {
                dp.TinhTrangThanhToan = -1; // chua thanh toan
            }


            dulieu.DATPHONGs.InsertOnSubmit(dp);
            dulieu.SubmitChanges();

            Session["HoaDon"] = dp;
            //Session["ThanhToan"] = dulieu.DATPHONGs.Single(x => x.LOAIPHONG == dp.LOAIPHONG).TinhTrangThanhToan;
            return RedirectToAction("XacNhanDonHang", "Admin");


        }

        public ActionResult XacNhanDonHang()
        {
            List<GioHang> lstGioHang = LayGioHang();
            object thanhtien = 0;
            object tongTien = tinhTongTien();
            if (double.TryParse(tongTien.ToString(), out double thanhTien))
            {
                thanhtien = (decimal?)thanhTien;
            }
            object tienCoc = thanhTien * 0.3;
            ViewBag.tiencoc = tienCoc;
            return View(lstGioHang);
        }
        [HttpPost]
        public ActionResult XacNhanDonHang(FormCollection f)
        {
            Session["HoaDon"] = null;
            Session["ThanhToan"] = null;
            Session["GioHang"] = null;
            return RedirectToAction("Index", "Home");
        }
        //loại phòng
        public ActionResult LoaiPhong()
        {
            List<LOAIPHONG> lp = dulieu.LOAIPHONGs.ToList();
            return View(lp);
        }
        public ActionResult maLoaiPhong(string ma)
        {
            var listlp = dulieu.LOAIPHONGs.Where(s => s.MALOAI == ma).OrderBy(s => s.MALOAI).ToList();
            return View(listlp);
        }
        public ActionResult Phong(string ma)
        {
            var listlp = dulieu.PHONGs.Where(s => s.MALOAI == ma).OrderBy(s => s.MAPHONG).ToList();
            return View(listlp);
        }
        public ActionResult maPhong(string ma)
        {
            var listlp = dulieu.PHONGs.Where(s => s.MAPHONG == ma).OrderBy(s => s.MAPHONG).ToList();
            return View(listlp);
        }
        public ActionResult NhanPhong()
        {
            // Lấy mã HD lớn nhất từ cơ sở dữ liệu
            string lastMAHD = dulieu.HOADONs.Select(h => h.MAHD).OrderByDescending(h => h).FirstOrDefault();
            // Xác định mã HD mới
            string newMAHD = GenerateNextMAHD(lastMAHD);

            // Lấy danh sách MADATPHONG từ cơ sở dữ liệu hoặc bất kỳ nguồn dữ liệu nào khác
            List<DATPHONG> datPhongList = dulieu.DATPHONGs.Where(dp => !dulieu.HOADONs.Any(hd => hd.MADATPHONG == dp.MADATPHONG)).ToList();

            // Truyền danh sách MADATPHONG đến view
            ViewBag.DatPhongList = new SelectList(datPhongList, "MADATPHONG", "MAKH");


            // Lấy danh sách MANV từ cơ sở dữ liệu hoặc nguồn dữ liệu khác
            List<NHANVIEN> nhanVienList = dulieu.NHANVIENs.ToList();
            // Truyền danh sách MANV đến view
            ViewBag.NhanVienList = new SelectList(nhanVienList, "MANV", "TENNV");


            return View(new HOADON() {MAHD = newMAHD, NGAYXUAT= DateTime.Now , TRANGTHAI="Chưa thanh toán"});
        }

        [HttpPost]
        public ActionResult NhanPhong(HOADON model)
        {
            if (ModelState.IsValid)
            {
                dulieu.HOADONs.InsertOnSubmit(model);
                dulieu.SubmitChanges();
                var phong = dulieu.PHONGs.FirstOrDefault(p => p.MAPHONG == model.MAPHONG);
                if(phong != null)
                {
                    phong.TRANGTHAI = "Đã đặt";
                    dulieu.SubmitChanges();
                }    
                return RedirectToAction("ListHoaDon");
            }
            return View(model);
        }
        private string GenerateNextMAHD(string lastMAHD)
        {
            if (string.IsNullOrEmpty(lastMAHD))
            {
                return "HD1";
            }

            // Trích xuất số cuối cùng từ mã HD hiện tại và tăng giá trị lên 1
            int lastNumber = int.Parse(lastMAHD.Substring(2));
            int nextNumber = lastNumber + 1;

            // Tạo mã HD mới
            string newMAHD = "HD" + nextNumber.ToString("D2");

            return newMAHD;
        }

        // Hành động AJAX để lấy TENKH dựa trên MADATPHONG
        public ActionResult GetTENKH(string MADATPHONG)
        {
            // Lấy MADATPHONG tương ứng với MAKH
            var maKH = dulieu.DATPHONGs
                            .Where(dp => dp.MADATPHONG == MADATPHONG)
                            .Select(dp => dp.MAKH)
                            .FirstOrDefault();

            // Lấy TENKH từ bảng KHACHHANG dựa trên MAKH
            var tenKH = dulieu.KHACHHANGs
                            .Where(kh => kh.MAKH == maKH)
                            .Select(kh => kh.TENKH)
                            .FirstOrDefault();

            return Content(tenKH ?? "Không tìm thấy khách hàng");
        }


        public ActionResult GetMaPhongByMadatphong(string madatphong)
        {
            var maPhongs = dulieu.DATPHONGs
                            .Where(dp => dp.MADATPHONG == madatphong)
                            .Join(dulieu.PHONGs,
                                  dp => dp.MALOAI,
                                  p => p.MALOAI,
                                  (dp, p) => new { p.MAPHONG, p.TRANGTHAI })
                            .Where(joinResult => joinResult.TRANGTHAI == "Trống")
                            .Select(joinResult => new SelectListItem
                            {
                                Value = joinResult.MAPHONG,
                                Text = joinResult.MAPHONG
                            })
                            .ToList();

            return Json(maPhongs, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult GetLOAIPHONG(string madatphong)
        {
            // Thực hiện logic để lấy thông tin MALOAI từ bảng DATPHONG dựa trên MADATPHONG
            var maloai = dulieu.DATPHONGs.Where(dp => dp.MADATPHONG == madatphong).Select(dp => dp.MALOAI).FirstOrDefault();

            // Trả về dữ liệu dưới dạng Json
            return Json(new { MALOAI = maloai }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ListLoaiPhong()
        {
            List<LOAIPHONG> lp = dulieu.LOAIPHONGs.ToList();
            return View(lp);
        }

        public ActionResult ThemLoaiPhong()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ThemLoaiPhong(LOAIPHONG model)
        {
            if (string.IsNullOrEmpty(model.MALOAI) == true)
            {
                ModelState.AddModelError("", "Tên loại không được để trống");
                return View(model);
            }

            dulieu.LOAIPHONGs.InsertOnSubmit(model);
            dulieu.SubmitChanges();
            return RedirectToAction("ListLoaiPhong");
        }

        public ActionResult SuaLoaiPhong(string id)
        {
            var LoaiPhong = dulieu.LOAIPHONGs.SingleOrDefault(lp => lp.MALOAI == id);
            return View(LoaiPhong);
        }

        [HttpPost]
        public ActionResult SuaLoaiPhong(LOAIPHONG model)
        {
            var lp = dulieu.LOAIPHONGs.SingleOrDefault(l => l.MALOAI == model.MALOAI);
            lp.DONGIA = model.DONGIA;
            lp.SONGUOIMAX = model.SONGUOIMAX;
            lp.HINHANH = model.HINHANH;
            dulieu.SubmitChanges();
            return RedirectToAction("ListLoaiPhong");
            
        }
        public ActionResult XoaLoaiPhong(string id)
        {
            var lp = dulieu.LOAIPHONGs.SingleOrDefault(l => l.MALOAI == id);
            dulieu.LOAIPHONGs.DeleteOnSubmit(lp);
            dulieu.SubmitChanges();
            return RedirectToAction("ListLoaiPhong");
        }

        public ActionResult ListPhong()
        {
            List<PHONG> lp = dulieu.PHONGs.ToList();
            return View(lp);
        }
        public ActionResult ThemPhong()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ThemPhong(PHONG model)
        {
            if (string.IsNullOrEmpty(model.MAPHONG) == true)
            {
                ModelState.AddModelError("", "Mã phòng không được để trống");
                return View(model);
            }

            dulieu.PHONGs.InsertOnSubmit(model);
            dulieu.SubmitChanges();
            return RedirectToAction("ListPhong");
        }

        public ActionResult SuaPhong(string id)
        {
            var Phong = dulieu.PHONGs.SingleOrDefault(p => p.MAPHONG == id);
            return View(Phong);
        }

        [HttpPost]
        public ActionResult SuaPhong(PHONG model)
        {
            var lp = dulieu.PHONGs.SingleOrDefault(p => p.MAPHONG == model.MAPHONG);
            lp.SDTBAN = model.SDTBAN;
            lp.HINHANH = model.HINHANH;
            lp.TRANGTHAI = model.TRANGTHAI;
            lp.MALOAI = model.MALOAI;
            dulieu.SubmitChanges();
            return RedirectToAction("ListPhong");
        }
        public ActionResult XoaPhong(string id)
        {
            var lp = dulieu.PHONGs.SingleOrDefault(p => p.MAPHONG == id);
            dulieu.PHONGs.DeleteOnSubmit(lp);
            dulieu.SubmitChanges();
            return RedirectToAction("ListPhong");
        }

        
        

        //quản lý thiết bị
        public ActionResult QuanLyThietBi()
        {
            List<THIETBI> thietbi = dulieu.THIETBIs.ToList();
            return View(thietbi);
        }
        [HttpPost]
        public ActionResult QuanLyThietBi(FormCollection f)
        {
            var tb = f["search"];
            var a = dulieu.THIETBIs.Where(x => x.TENTHIETBI.Contains(tb)).ToList();
            return View(a);
        }

        public ActionResult ChiTietThietBi(string id)
        {
            CTTHIETBI cttb = dulieu.CTTHIETBIs.FirstOrDefault(t => t.MATHIETBI == id);
            List<THIETBI> dsTB = dulieu.THIETBIs.Where(t => t.MATHIETBI == id).ToList();
            ViewBag.phong = dsTB;

            return View(cttb);
        }

        public ActionResult taoMoi()
        {
            return View();
        }

        public ActionResult XLThemMoi(THIETBI t)
        {
            dulieu.THIETBIs.InsertOnSubmit(t);
            dulieu.SubmitChanges();
            return RedirectToAction("QuanLyThietBi");
        }

        public ActionResult SuaThietBi(string id)
        {
            THIETBI tb = dulieu.THIETBIs.FirstOrDefault(t => t.MATHIETBI == id);
            return View(tb);
        }
        [HttpPost]
        public ActionResult SuaThietBi(THIETBI sp, string id)
        {

            THIETBI tb = dulieu.THIETBIs.FirstOrDefault(t => t.MATHIETBI == id);
            UpdateModel(tb);
            dulieu.SubmitChanges();
            return RedirectToAction("QuanLyThietBi");
        }

        public ActionResult XoaThietBi(string id)
        {
            THIETBI tb = dulieu.THIETBIs.FirstOrDefault(t => t.MATHIETBI == id);
            return View(tb);
        }
        [HttpPost]
        public ActionResult XoaThietBi(string id, THIETBI tb)
        {
            THIETBI thietbi = dulieu.THIETBIs.FirstOrDefault(t => t.MATHIETBI == id);
            dulieu.THIETBIs.DeleteOnSubmit(thietbi);
            dulieu.SubmitChanges();
            return RedirectToAction("QuanLyThietBi");
        }

        //quản lý nhân viên
        public ActionResult QuanLyNV()
        {
            List<NHANVIEN> nv = dulieu.NHANVIENs.ToList();
            return View(nv);
        }
        [HttpPost]
        public ActionResult QuanLyNV(FormCollection f)
        {
            var tb = f["search"];
            var a = dulieu.NHANVIENs.Where(x => x.TENNV.Contains(tb)).ToList();
            return View(a);
        }

        public ActionResult ThongTinCTNV(string id)
        {
            NHANVIEN nhanvien = dulieu.NHANVIENs.FirstOrDefault(t => t.MANV == id);
            List<LICHLAMVIEC> calam = dulieu.LICHLAMVIECs.Where(t => t.MANV == id).ToList();
            List<TAIKHOAN> tk = dulieu.TAIKHOANs.Where(t => t.TENDN == id).ToList();

            ViewBag.taikhoan = tk;
            ViewBag.calam = calam;

            return View(nhanvien);
        }

        public ActionResult taoTK()
        {
            return View();
        }
        [HttpPost]
        public ActionResult taoTK(TAIKHOAN t)
        {
            dulieu.TAIKHOANs.InsertOnSubmit(t);
            dulieu.SubmitChanges();
            return RedirectToAction("themNV");
        }

        public ActionResult themNV()
        {
            return View();
        }
        [HttpPost]
        public ActionResult themNV(NHANVIEN t)
        {
            dulieu.NHANVIENs.InsertOnSubmit(t);
            dulieu.SubmitChanges();
            return RedirectToAction("QuanLyNV");
        }

        public ActionResult SuaNV(string id)
        {
            NHANVIEN nv = dulieu.NHANVIENs.FirstOrDefault(t => t.MANV == id);
            return View(nv);
        }
        [HttpPost]
        public ActionResult SuaNV(NHANVIEN nv, string id)
        {

            NHANVIEN nhanvien = dulieu.NHANVIENs.FirstOrDefault(t => t.MANV == id);
            UpdateModel(nhanvien);
            dulieu.SubmitChanges();
            return RedirectToAction("QuanLyNV");
        }

        public ActionResult XoaNV(string id)
        {
            NHANVIEN nv = dulieu.NHANVIENs.FirstOrDefault(t => t.MANV == id);
            return View(nv);
        }
        [HttpPost]
        public ActionResult XoaNV(string id, NHANVIEN nv)
        {
            NHANVIEN nhanvien = dulieu.NHANVIENs.FirstOrDefault(t => t.MANV == id);
            dulieu.NHANVIENs.DeleteOnSubmit(nhanvien);
            dulieu.SubmitChanges();
            return RedirectToAction("QuanLyNV");
        }

        //quản lý lịch làm việc của nhân viên
        public ActionResult QuanLyLichLamViec()
        {
            List<LICHLAMVIEC> a = dulieu.LICHLAMVIECs.ToList();
            return View(a);
        }

        public ActionResult themLich()
        {
            return View();
        }
        [HttpPost]
        public ActionResult themLich(LICHLAMVIEC t)
        {
            dulieu.LICHLAMVIECs.InsertOnSubmit(t);
            dulieu.SubmitChanges();
            return RedirectToAction("QuanLyLichLamViec");
        }

        public ActionResult SuaLich(string id)
        {
            LICHLAMVIEC a = dulieu.LICHLAMVIECs.FirstOrDefault(t => t.CALAM == id);
            return View(a);
        }
        [HttpPost]
        public ActionResult SuaLich(LICHLAMVIEC a, string id)
        {

            LICHLAMVIEC calam = dulieu.LICHLAMVIECs.FirstOrDefault(t => t.CALAM == id);
            UpdateModel(calam);
            dulieu.SubmitChanges();
            return RedirectToAction("QuanLyLichLamViec");
        }

        public ActionResult XoaLich(string id)
        {
            LICHLAMVIEC a = dulieu.LICHLAMVIECs.FirstOrDefault(t => t.CALAM == id);
            return View(a);
        }
        [HttpPost]
        public ActionResult XoaLich(string id, LICHLAMVIEC nv)
        {
            LICHLAMVIEC calam = dulieu.LICHLAMVIECs.FirstOrDefault(t => t.CALAM == id);
            dulieu.LICHLAMVIECs.DeleteOnSubmit(calam);
            dulieu.SubmitChanges();
            return RedirectToAction("QuanLyLichLamViec");
        }

        //báo cáo doanh thu số tiền trả lương nhân viên
        public ActionResult ThongKe()
        {
            //get Tong thu nhap $$
            List<LICHLAMVIEC> lstLich = dulieu.LICHLAMVIECs.OrderBy(c => c.CALAM).ToList();
            decimal tong = 0;
            foreach (LICHLAMVIEC c in lstLich)
            {
                tong += (c.TONGTIEN.HasValue ? c.TONGTIEN.Value : 0);
            }
            ViewBag.tongtien = tong;
            ViewBag.doanhThuDV = ThongKeDoanhThuDichVu();
            ViewBag.TongDoanhThu = ThongKeDoanhThuLuongNV(); //thong ke doanh thu luong NV
            ViewBag.TongDTKS = ThongKeTongDT();  // Thống Kê tổng doanh thu
            return View(dulieu.LICHLAMVIECs.ToList());
        }

        public decimal ThongKeDoanhThuLuongNV()
        {
            decimal doanhThuLuongNV = decimal.Parse(dulieu.LICHLAMVIECs.Sum(t => t.TONGTIEN).Value.ToString());
            return doanhThuLuongNV;
        }

        public decimal ThongKeDoanhThuLuongNVThang(int Thang, int Nam)
        {
            //theo thang, nam tuong ung
            var lstLich = dulieu.LICHLAMVIECs.Where(n => n.NGAYLAM.Month == Thang && n.NGAYLAM.Year == Nam);
            decimal tongTien = 0;
            foreach (var item in lstLich)
            {
                tongTien += decimal.Parse(item.TONGTIEN.Value.ToString());
            }
            return tongTien;
        }

        //thong ke tong tien dich vu
        public decimal ThongKeDoanhThuDichVu()
        {
            decimal doanhThuDV = decimal.Parse(dulieu.CTDICHVUs.Sum(t => t.THANHTIEN).Value.ToString());
            return doanhThuDV;
        }

        // Thống Kê tổng doanh thu
        public decimal ThongKeTongDT()
        {
            decimal doanhthu = decimal.Parse(dulieu.HOADONs.Sum(t => t.TONGTIEN).Value.ToString());
            return doanhthu;
        }

        public ActionResult HoaDon()
        {
            List<HOADON> lp = dulieu.HOADONs.ToList();
            return View(lp);
        }
        public ActionResult ListHoaDon(int? page)
        {
            List<HOADON> item = dulieu.HOADONs.ToList();
            return View(item);
        }

        public ActionResult CTHoaDon(string MAHD, string MADATPHONG)
        {
            var cthd = dulieu.CTHDs.SingleOrDefault(lp => lp.MAHD == MAHD && lp.MADATPHONG == MADATPHONG);
            return View(cthd);
        }
        [HttpPost]
        public ActionResult CTHoaDon(CTHD model)
        {

            dulieu.CTHDs.InsertOnSubmit(model);
            dulieu.SubmitChanges();
            return RedirectToAction("ListHoaDon");
        }

        public ActionResult CapNhatCTHoaDon(string MAHD, string MADATPHONG)
        {
            var cthd = dulieu.CTHDs.SingleOrDefault(lp => lp.MAHD == MAHD && lp.MADATPHONG == MADATPHONG);
            return View(cthd);
        }
        [HttpPost]
        public ActionResult CapNhatCTHoaDon(CTHD model)
        {
            var cthd = dulieu.CTHDs.SingleOrDefault(lp => lp.MAHD == model.MAHD && lp.MADATPHONG == model.MADATPHONG);
            cthd.GIAMGIA = model.GIAMGIA;
            cthd.PHUTHU = model.PHUTHU;
            cthd.GHICHU = model.GHICHU;
            dulieu.SubmitChanges();
            return RedirectToAction("ListHoaDon");
        }


        public ActionResult XuatHoaDon(string id)
        {
            var Hoadon = dulieu.HOADONs.SingleOrDefault(hd => hd.MAHD == id);

            if (Hoadon != null)
            {
                string maDatPhong = Hoadon.MADATPHONG;
                string tenKhachHang = GetTenKhachHangByMaDatPhong(maDatPhong);

                // Truyền biến tenKhachHang đến view
                ViewBag.TenKhachHang = tenKhachHang;

                // Lấy thông tin từ bảng CTHD dựa trên MAHD
                var cthdInfo = dulieu.CTHDs
                    .Where(cthd => cthd.MAHD == Hoadon.MAHD)
                    .GroupBy(cthd => cthd.MAHD)
                    .Select(group => new
                    {
                        GIAMGIA = group.Sum(cthd => cthd.GIAMGIA),
                        TONGTIENPHONG = group.Sum(cthd => cthd.TONGTIENPHONG),
                        PHIDICHVU = group.Sum(cthd => cthd.PHIDICHVU),
                        PHITHIETHAI = group.Sum(cthd => cthd.PHITHIETHAI),
                        PHUTHU = group.Sum(cthd => cthd.PHUTHU)
                    })
                    .SingleOrDefault();

                // Gán giá trị cho ViewBag
                ViewBag.GIAMGIA = cthdInfo.GIAMGIA;
                ViewBag.TONGTIENPHONG = cthdInfo.TONGTIENPHONG;
                ViewBag.PHIDICHVU = cthdInfo.PHIDICHVU;
                ViewBag.PHITHIETHAI = cthdInfo.PHITHIETHAI;
                ViewBag.PHUTHU = cthdInfo.PHUTHU;
            }
            Hoadon.TRANGTHAI = "Đã thanh toán";
            Hoadon.NGAYXUAT = DateTime.Now;
            return View(Hoadon);
        }

        [HttpPost]
        public ActionResult XuatHoaDon(HOADON model)
        {
            var hd = dulieu.HOADONs.SingleOrDefault(l => l.MAHD == model.MAHD);
            hd.TRANGTHAI = model.TRANGTHAI;
            hd.NGAYXUAT = model.NGAYXUAT;
            var phong = dulieu.PHONGs.FirstOrDefault(p => p.MAPHONG == model.MAPHONG);
            if (phong != null)
            {
                phong.TRANGTHAI = "Trống";
                dulieu.SubmitChanges();
            }
            dulieu.SubmitChanges();
            return RedirectToAction("ListHoaDon");

        }
        public string GetTenKhachHangByMaDatPhong(string maDatPhong)
        {
            var query = from dp in dulieu.DATPHONGs
                        join kh in dulieu.KHACHHANGs on dp.MAKH equals kh.MAKH
                        where dp.MADATPHONG == maDatPhong
                        select kh.TENKH;
            return query.FirstOrDefault();
        }

        public ActionResult DatDichVu()
        {
            List<DATPHONG> datPhongList = dulieu.DATPHONGs.ToList();
            // Truyền danh sách MADATPHONG đến view
            ViewBag.DatPhongList = new SelectList(datPhongList, "MADATPHONG", "MADATPHONG");

            // Lấy mã HD lớn nhất từ cơ sở dữ liệu
            string lastMACTDV = dulieu.CTDICHVUs.Select(h => h.MACTDV).OrderByDescending(h => h).FirstOrDefault();
            // Xác định mã HD mới
            string newMACTDV = GenerateNextMACTDV(lastMACTDV);

            // Tạo danh sách MADV từ bảng DICHVU
            List<DICHVU> dichVuList = dulieu.DICHVUs.ToList();

            // Truyền danh sách MADV đến view
            ViewBag.DichVuList = new SelectList(dichVuList, "MADV", "TENDICHVU");

            return View(new CTDICHVU() { MACTDV = newMACTDV });
        }
        [HttpPost]
        public ActionResult DatDichVu(CTDICHVU t)
        {
            dulieu.CTDICHVUs.InsertOnSubmit(t);
            dulieu.SubmitChanges();
            return RedirectToAction("DatDichVu");
        }
        private string GenerateNextMACTDV(string lastMACTDV)
        {
            if (string.IsNullOrEmpty(lastMACTDV))
            {
                return "CTDV01";
            }

            // Trích xuất số cuối cùng từ mã HD hiện tại và tăng giá trị lên 1
            int lastNumber;
            if (int.TryParse(lastMACTDV.Substring(4), out lastNumber))
            {
                int nextNumber = lastNumber + 1;

                // Tạo mã HD mới với định dạng số
                string newMACTDV = "CTDV" + nextNumber.ToString("D2");

                return newMACTDV;
            }
            else
            {
                // Xử lý trường hợp ngoại lệ nếu không thể chuyển đổi thành số
                throw new InvalidOperationException("MACTDV không hợp lệ.");
            }
        }

        public ActionResult ThietHai()
        {

            List<HOADON> hoaDonList = dulieu.HOADONs.Where(h => h.TRANGTHAI == "Chưa thanh toán").ToList();
            // Truyền danh sách MADATPHONG đến view
            ViewBag.HoaDonList = new SelectList(hoaDonList, "MAHD", "MAHD");

            // Lấy mã HD lớn nhất từ cơ sở dữ liệu
            string lastMACTTH = dulieu.CTTHIETHAIs.Select(h => h.MACTTH).OrderByDescending(h => h).FirstOrDefault();
            // Xác định mã HD mới
            string newMACTTH = GenerateNextMACTTH(lastMACTTH);

            // Tạo danh sách MATHIETBI 
            List<THIETBI> thietBiList = dulieu.THIETBIs.ToList();

            // Truyền danh sách MADV đến view
            ViewBag.ThietBiList = new SelectList(thietBiList, "MATHIETBI", "TENTHIETBI");

            return View(new CTTHIETHAI() { MACTTH = newMACTTH });
        }
        [HttpPost]
        public ActionResult ThietHai(CTTHIETHAI t)
        {
            dulieu.CTTHIETHAIs.InsertOnSubmit(t);
            dulieu.SubmitChanges();
            return RedirectToAction("ThietHai");
        }
        private string GenerateNextMACTTH(string lastMACTTH)
        {
            if (string.IsNullOrEmpty(lastMACTTH))
            {
                return "CTTH01";
            }

            // Trích xuất số cuối cùng từ mã HD hiện tại và tăng giá trị lên 1
            int lastNumber;
            if (int.TryParse(lastMACTTH.Substring(4), out lastNumber))
            {
                int nextNumber = lastNumber + 1;

                // Tạo mã HD mới với định dạng số
                string newMACTTH = "CTTH" + nextNumber.ToString("D2");

                return newMACTTH;
            }
            else
            {
                // Xử lý trường hợp ngoại lệ nếu không thể chuyển đổi thành số
                throw new InvalidOperationException("MACTTH không hợp lệ.");
            }
        }
    }
}
