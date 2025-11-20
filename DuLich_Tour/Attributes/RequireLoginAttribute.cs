using System.Web.Mvc;

namespace DuLich_Tour.Attributes
{
    /// <summary>
    /// Attribute để yêu cầu người dùng phải đăng nhập trước khi truy cập action
    /// </summary>
    public class RequireLoginAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            if (filterContext.HttpContext.Session["TenDangNhap"] == null)
            {
                // Lưu URL hiện tại để redirect lại sau khi login
                string returnUrl = filterContext.HttpContext.Request.RawUrl;
                filterContext.HttpContext.Session["ReturnUrl"] = returnUrl;
                
                // Redirect đến trang login
                filterContext.Result = new RedirectToRouteResult(
                    new System.Web.Routing.RouteValueDictionary(
                        new { controller = "Account", action = "Login" }
                    )
                );
            }
            
            base.OnActionExecuting(filterContext);
        }
    }
}

