using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using QLKS.Models;
using System.Net;

namespace QLKS.Controllers
{
    public class KhachHangController : Controller
    {
        //
        // GET: /KhachHang/

        QLKSDataContext dulieu = new QLKSDataContext();
        public ActionResult Index()
        {
            var list = dulieu.KHACHHANGs.OrderByDescending(s => s.MAKH).ToList();
            return View(list);
        }
        [HttpPost]
        public ActionResult Index( FormCollection f)
        {
            var ten = f["timkiem"];
            var ListDH = dulieu.KHACHHANGs.Where(x => x.TENKH.Contains(ten)).ToList();

            //var links = dulieu.KHACHHANGs
            //    .Where(x => x.TENKH.Contains(ten))
            //    .OrderByDescending(s => s.MAKH)
            //    .ToList();

            return View(ListDH);
        }

        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            KHACHHANG tAIKHOAN = dulieu.KHACHHANGs.FirstOrDefault(kh => kh.TAIKHOAN == id);
            if (tAIKHOAN == null)
            {
                return HttpNotFound();
            }
            ViewBag.k = tAIKHOAN;
            return View(tAIKHOAN);
        }

        public ActionResult LockorUnlock(string id)
        {
            KHACHHANG taiKhoan = dulieu.KHACHHANGs.FirstOrDefault(kh => kh.TAIKHOAN == id);

            if (taiKhoan != null)
            {
                taiKhoan.TRANGTHAI = (taiKhoan.TRANGTHAI == 1) ? 0 : 1;

                dulieu.SubmitChanges();
            }

            return RedirectToAction("Index");
        }

    }
}
