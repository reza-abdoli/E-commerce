using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Areas.User.ViewComponents
{
    public class GetLastUserDataViewComponent : ViewComponent
    {
        private readonly IUserService _userService;
        public GetLastUserDataViewComponent(IUserService userService)
        {
            _userService = userService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var userId = long.Parse(
                ((ClaimsPrincipal)User).FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var balance = await _userService.GetUserBalance(userId);
            return View(balance);
        }
    }
}