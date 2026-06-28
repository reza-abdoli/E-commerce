using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MyMvcApp.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]
    [Area("Admin")]
    public class AdminBaseController : Controller
    {
        protected string ErrorMessage = "ErrorMessage";
        protected string SuccessMessage = "SuccessMessage";
        protected string InfoMessage = "InfoMessage";
        protected string WarningMessage = "WarningMessage";

        protected string SuccessText = "عملیات با موفقیت انجام شد";
        protected string ErrorText = "در انجام عملیات خطایی رخ داده است";
        protected string DeleteText = "دیتا با موفقیت حذف شد";
        protected string ImageNotSaveText = "در ذخیره سازی تصویر خطایی رخ داده است";
    }
}
