using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLKS.Models;

namespace QLKS.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        QLKSDataContext dulieu = new QLKSDataContext();
        public ActionResult Index()
        {
            List<LOAIPHONG> loaiphong = dulieu.LOAIPHONGs.ToList();
            return View(loaiphong);

        }
        public ActionResult About()
        {
            return View();
        }

        [HttpGet]
        public ActionResult DangNhap()
        {
            if (Session["kh"] != null)
            {
                TAIKHOAN kh = Session["kh"] as TAIKHOAN;
                TAIKHOAN k = dulieu.TAIKHOANs.FirstOrDefault(kk => kk.TENDN == kh.TENDN);
                KHACHHANG khach = dulieu.KHACHHANGs.FirstOrDefault(kk => kk.TAIKHOAN == kh.TENDN);
                if (khach != null && khach.TRANGTHAI == 1)
                {
                    return RedirectToAction("Index", "KhachHang");
                }
                else
                    if (k != null && k.CHUCVU == "Quản lý")//admin
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else
                        if (k != null)//chức vụ còn lại
                        {
                            return RedirectToAction("Index", "NhanVien");
                        }

            }
            return View();
        }

        [HttpPost]
        public ActionResult DangNhap(FormCollection c)
        {

            var id = c["txtidUser"];
            var pass = c["txtPass"];
            if (Session["kh"] != null)
            {
                return RedirectToAction("Index", "Home");
            }
            TAIKHOAN acc = dulieu.TAIKHOANs.FirstOrDefault(tk => tk.TENDN == id && tk.MATKHAU == pass);
            KHACHHANG khach = dulieu.KHACHHANGs.FirstOrDefault(tk => tk.TAIKHOAN == id && tk.MATKHAU == pass);

            if (acc != null || khach != null)
            {
                if (khach != null &&khach.TRANGTHAI==1)
                {
                    Session["kh"] = khach;
                    return RedirectToAction("Index", "Home");

                }
                if (acc != null && acc.CHUCVU == "Quản lý") // admin && acc.CHUCVU == ""
                {
                    Session["kh"] = acc;
                    return RedirectToAction("Index", "Admin");
                }
                if (acc != null) //chức vụ còn lại
                {
                    Session["kh"] = acc;
                    return RedirectToAction("Index", "Nhân Viên");
                }
                else
                {
                    Session["tb"] = "Tài khoản chưa được xác nhận hoặc đã bị khoá !"; //chưa đk xác nhận hoặc đã bị khoác
                    return RedirectToAction("DangNhap", "Home");
                }
            }
            else
            {
                Session["tb"] = "Tài khoản hoặc mật khẩu không đúng !";
                return RedirectToAction("DangNhap", "Home");
            }
        } //khach hang now
        public ActionResult KhachHangNow()
        {
            KHACHHANG acc = Session["kh"] as KHACHHANG;

            return PartialView();
        }


        //dang ky
        [HttpGet]
        public ActionResult DangKy()
        {
            return View();
        }
        [HttpPost]
        public ActionResult DangKy(KHACHHANG tk, FormCollection f)
        {
            var hoten = f["txtName"];
            var tendn = f["UserName"];
            var matkhau = f["Password"];
            var rematkhau = f["ConfirmPassword"];
            var gioitinh = f["txtGioiTinh"];
            var ngaysinh = f["txtNgaySinh"];
            var cccd = f["txtCCCD"];
            var diachi = f["txtDiaChi"];
            var sdt = f["PhoneNumber"];
            var email = f["Email"];


            bool check = dulieu.KHACHHANGs.Where(k => k.TAIKHOAN == tendn).Count() > 0;
            bool checkmail = dulieu.KHACHHANGs.Where(k => k.EMAIL == email).Count() > 0;
            if (check)
            {
                ViewBag.hasUser = "Tên đăng nhập" + tendn + "đã tồn tại.";
                return View();
            }
            if (checkmail)
            {
                ViewBag.hasMail = "Email " + email + " đã được đăng ký.";
                return View();
            }
            else
            {
                tk.MAKH = tendn;
                tk.TENKH = hoten;
                tk.GIOITINH = gioitinh;
                tk.NGAYSINH = DateTime.Parse(ngaysinh);
                tk.CCCD = cccd;
                tk.DIACHI = diachi;
                tk.EMAIL = email;
                tk.SDT = sdt;
                tk.TAIKHOAN = tendn;
                tk.MATKHAU = matkhau;
                tk.TRANGTHAI = 0; //false;
                //tk.TRANGTHAI = false;

                //dulieu.KHACHHANGs.Add(tk);
                dulieu.KHACHHANGs.InsertOnSubmit(tk);

                dulieu.SubmitChanges();

                System.Net.Mail.MailMessage m = new System.Net.Mail.MailMessage(
                        new System.Net.Mail.MailAddress("Nvgha25422@gmail.com", "Khách sạn"),
                        new System.Net.Mail.MailAddress(tk.EMAIL));
                m.Subject = "Email confirmation";
                m.Body = string.Format("Thân gửi {0}<BR/>Cảm ơn bạn đã đăng ký tài khoản, " +
                    "Bạn vui lòng click vào link bên dưới để hoàn tất việc đăng ký :<br > " +
                    "<a href=\"{1}\" title=\"User Email Confirm\">{1}</a>"
                    + "<br >Bạn sẽ được chuyển về trang ĐĂNG NHẬP, vui lòng nhập đúng tài khoản đã đăng ký để truy cập mua sắm.<br > Chúc một ngày tốt lành. ",
                    tk.TAIKHOAN, Url.Action("ConfirmEmail", "Home", new { Token = tk.TAIKHOAN, Email = tk.EMAIL }, Request.Url.Scheme));
                m.IsBodyHtml = true;
                System.Net.Mail.SmtpClient smtp = new System.Net.Mail.SmtpClient("smtp.gmail.com");
                smtp.Port = 587;
                smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential("Nvgha25422@gmail.com", "qqfu atjn sfan yoxd");
                smtp.Send(m);

                return RedirectToAction("XacThuc", "Home", new { Email = tk.EMAIL });
            }

        }
        public ActionResult XacThuc(string Email)
        {
            ViewBag.Email = Email;
            return View();
        }
        public ActionResult ConfirmEmail(string Token, string Email)
        {

            bool check1 = dulieu.KHACHHANGs.Where(k => k.TAIKHOAN == Token).Count() > 0;
            bool check2 = dulieu.KHACHHANGs.Where(k => k.EMAIL == Email).Count() > 0;
            KHACHHANG user = dulieu.KHACHHANGs.Where(k => k.TAIKHOAN == Token).Single();
            if (check1)
            {
                if (check2)
                {
                    user.TRANGTHAI = 1;

                    dulieu.SubmitChanges();
                    Session["ok"] = "Tài khoản " + Token + "đã được đăng ký thành công!";
                    ViewBag.success = "OK";
                    return RedirectToAction("DangNhap", "Home");
                }
                else
                {
                    return RedirectToAction("XacThuc", "Home", new { Email = user.EMAIL });
                }
            }
            else
            {
                return RedirectToAction("XacThuc", "Home", new { Email = "" });
            }

        }
        public ActionResult Logout()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult GioiThieu()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }

        public ActionResult LienHe()
        {
            Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}
