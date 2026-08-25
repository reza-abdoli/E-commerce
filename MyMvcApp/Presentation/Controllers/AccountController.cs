using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Application.Services.Interface;
using Data.DTOs.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Presentation.Controllers;

public class AccountController : SiteBaseController
{
    private readonly IUserService _userService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IUserService userService, ILogger<AccountController> logger)
    {
        _userService = userService;
        _logger = logger;
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

        try
        {
            if (await _userService.GetUserByUsername(registerOrLoginDto.Username) != null)
            {
                TempData[ErrorMessage] = "این نام کاربری قبلاً ثبت شده است.";
                return View(registerOrLoginDto);
            }

            await _userService.CreateUser(registerOrLoginDto);

            _logger.LogInformation(
                "User registered successfully. Username: {Username}",
                registerOrLoginDto.Username);

            TempData[SuccessMessage] = "ثبت‌نام با موفقیت انجام شد. اکنون می‌توانید وارد حساب کاربری خود شوید.";
            return RedirectToAction("Login", "Account", new { area = "" });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An unexpected error occurred during signup. Username: {Username}",
                registerOrLoginDto.Username);

            TempData[ErrorMessage] = "خطایی در ثبت‌نام رخ داد. لطفاً دوباره تلاش کنید.";
            return View(registerOrLoginDto);
        }
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

        try
        {
            if (!await _userService.GetLogin(registerOrLoginDto))
            {
                TempData[ErrorMessage] = "نام کاربری یا رمز عبور وارد شده صحیح نیست.";
                return View(registerOrLoginDto);
            }

            var user = await _userService.GetUserByUsername(registerOrLoginDto.Username);

            if (user == null)
            {
                TempData[ErrorMessage] = "خطایی در ورود رخ داد. لطفاً دوباره تلاش کنید.";
                return View(registerOrLoginDto);
            }

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

            _logger.LogInformation(
                "User logged in successfully. Username: {Username}, Role: {Role}, UserId: {UserId}",
                user.Username,
                user.Role,
                user.Id);

            TempData[SuccessMessage] = "با موفقیت وارد حساب کاربری خود شدید.";

            if (user.Role == "Admin")
                return RedirectToAction("Index", "Home", new { area = "Admin" });

            return RedirectToAction("Index", "Home", new { area = "" });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An unexpected error occurred during login. Username: {Username}",
                registerOrLoginDto.Username);

            TempData[ErrorMessage] = "خطایی در ورود رخ داد. لطفاً دوباره تلاش کنید.";
            return View(registerOrLoginDto);
        }
    }

    [HttpGet("logout")]
    public async Task<IActionResult> Logout()
    {
        var username = User.Identity?.Name;
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        try
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            _logger.LogInformation(
                "User logged out successfully. Username: {Username}, UserId: {UserId}",
                username,
                userId);

            TempData[SuccessMessage] = "با موفقیت از حساب کاربری خود خارج شدید.";
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "An unexpected error occurred during logout. Username: {Username}, UserId: {UserId}",
                username,
                userId);

            TempData[ErrorMessage] = "خطایی در خروج از حساب رخ داد.";
        }

        return RedirectToAction("Index", "Home", new { area = "" });
    }
}