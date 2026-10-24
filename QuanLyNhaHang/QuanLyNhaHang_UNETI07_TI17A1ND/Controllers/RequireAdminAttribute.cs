// Họ và tên: Vũ Công Vinh (Sinh viên 1)
// Mã sinh viên: 23203100012
// Nội dung thực hiện: Entity Tài khoản - Đăng nhập - Phân quyền
// Nội dung thực hiện: Filter kiểm tra quyền Admin tại Controller (không chỉ ẩn menu trên View)
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using QuanLyNhaHang.Models;

namespace QuanLyNhaHang.Controllers
{
    public class RequireAdminAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var maTaiKhoan = context.HttpContext.Session.GetInt32(SessionKeys.MaTaiKhoan);
            if (maTaiKhoan == null)
            {
                context.Result = new RedirectToActionResult("Login", "TaiKhoan", new { returnUrl = context.HttpContext.Request.Path });
                return;
            }

            var vaiTro = context.HttpContext.Session.GetString(SessionKeys.VaiTro);
            if (vaiTro != VaiTro.Admin.ToString())
            {
                context.Result = new RedirectToActionResult("AccessDenied", "TaiKhoan", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
