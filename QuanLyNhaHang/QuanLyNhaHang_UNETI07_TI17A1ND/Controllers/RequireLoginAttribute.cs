// Nội dung thực hiện: Filter kiểm tra đã đăng nhập hay chưa (dựa vào Session)
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace QuanLyNhaHang.Controllers
{
    public class RequireLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var maTaiKhoan = context.HttpContext.Session.GetInt32(SessionKeys.MaTaiKhoan);
            if (maTaiKhoan == null)
            {
                context.Result = new RedirectToActionResult("Login", "TaiKhoan", new { returnUrl = context.HttpContext.Request.Path });
                return;
            }
            base.OnActionExecuting(context);
        }
    }
}
