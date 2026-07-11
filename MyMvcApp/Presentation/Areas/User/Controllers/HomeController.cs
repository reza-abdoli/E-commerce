using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Areas.User.Controllers
{
    public class HomeController : UserBaseController
    {
        private readonly IUserService _userService;

        public HomeController(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            // string username = User.Claims
            //     .SingleOrDefault(s => s.Type == ClaimTypes.Name)
            //     !.Value;
            // var model = await _userService.GetUserByUsername(username);
            return View();
        }
    }
}