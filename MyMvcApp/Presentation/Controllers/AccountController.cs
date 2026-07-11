using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Services.Interface;
using Data.DTOs.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Presentation.Controllers;

public class AccountController : SiteBaseController
{
    private readonly IUserService _userService;

    public AccountController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("signup")]
    public IActionResult Signup() => View();

    [HttpPost("signup"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Signup(RegisterOrLoginDto registerOrLoginDto)
    {
        if (!ModelState.IsValid)
        {
            TempData[ErrorMessage] = "اطلاعات وارد شده معتبر نیست.";
            return View(registerOrLoginDto);
        }

        if (await _userService.GetUserByUsername(registerOrLoginDto.Username) != null)
        {
            TempData[ErrorMessage] = "این نام کاربری قبلاً ثبت شده است.";
            return View(registerOrLoginDto);
        }

        await _userService.CreateUser(registerOrLoginDto);

        TempData[SuccessMessage] = "ثبت‌نام با موفقیت انجام شد. اکنون می‌توانید وارد حساب کاربری خود شوید.";
        return RedirectToAction("Login", "Account", new { area = "" });
    }

    [HttpGet("login")]
    public IActionResult Login() => View();

    [HttpPost("login"), ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(RegisterOrLoginDto registerOrLoginDto)
    {
        if (!ModelState.IsValid)
        {
            TempData[ErrorMessage] = "اطلاعات وارد شده معتبر نیست.";
            return View(registerOrLoginDto);
        }

        if (!await _userService.GetLogin(registerOrLoginDto))
        {
            TempData[ErrorMessage] = "نام کاربری یا رمز عبور وارد شده صحیح نیست.";
            return View(registerOrLoginDto);
        }

        var user = await _userService.GetUserByUsername(registerOrLoginDto.Username);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme);

        var principal = new ClaimsPrincipal(identity);

        var properties = new AuthenticationProperties
        {
            IsPersistent = true
        };

        await HttpContext.SignInAsync(principal, properties);

        TempData[SuccessMessage] = "با موفقیت وارد حساب کاربری خود شدید.";

        if (user.Role == "Admin")
            return RedirectToAction("Index", "Home", new { area = "Admin" });

        return RedirectToAction("Index", "Home", new { area = "" });
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

        TempData[SuccessMessage] = "با موفقیت از حساب کاربری خود خارج شدید.";

        return RedirectToAction("Index", "Home", new { area = "" });
    }
}