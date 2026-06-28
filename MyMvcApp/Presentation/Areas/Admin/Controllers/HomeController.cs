using Microsoft.AspNetCore.Mvc;

namespace MyMvcApp.Areas.Admin.Controllers
{
    public class HomeController : AdminBaseController
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
