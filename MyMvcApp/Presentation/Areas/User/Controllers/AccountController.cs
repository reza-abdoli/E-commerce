using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Services.Interface;
using Data.DTOs.Account;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Areas.User.Controllers
{
    public class AccountController : UserBaseController
    {
        private readonly IUserService _userService;

        public AccountController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("edit-user-detail")]
        public async Task<IActionResult> EditUserDetail(bool returnToCheckout)
        {
            // var user = await _userService.GetUserByUsername(User.FindFirst(ClaimTypes.Name)!.Value);
            // ViewData["User"] = user;
            var model = await _userService.GetEditUserDetail(long.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value));
            // model.ReturnToCheckout = returnToCheckout;
            ViewData["ReturnToCheckout"] = returnToCheckout;
            return View(model);
        }

        [HttpPost("edit-user-detail")]
        public async Task<IActionResult> EditUserDetail(EditUserInfoDTO dto, bool returnToCheckout)
        {
            // ViewData["User"] = await _userService.GetUserById(User.GetUserId());

            if (!ModelState.IsValid) return View(dto);
            await _userService.EditUserDetail(dto);
            TempData[SuccessMessage] = SuccessText;

            if (returnToCheckout) return RedirectToAction("Checkout", "Order", new { area = "User" });
            return RedirectToAction("EditUserDetail", "Account", new { area = "User" });
        }


    }
}

