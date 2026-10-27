using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.DTOs.Auth;
using SkillHub.Application.Interfaces;
using SkillHub.Domain.Constants;
using SkillHub.Domain.Exceptions;
using SkillHub.Infrastructure.Identity;
using SkillHub.Web.Models;

namespace SkillHub.Web.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IAuthService _authService;

    public AccountController(UserManager<ApplicationUser> userManager, IAuthService authService)
    {
        _userManager = userManager;
        _authService = authService;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Workshops");
        }

        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email.Trim().ToLowerInvariant());
        if (user == null || !await _userManager.CheckPasswordAsync(user, model.Password))
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(model);
        }

        await SignInUserAsync(user);

        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToAction("Index", "Workshops");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Workshops");
        }

        return View(new RegisterViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _authService.RegisterAsync(new RegisterDto(model.FullName, model.Email, model.Password));

            // Sign in with the persisted user so the cookie carries the real user Id and roles.
            var user = await _userManager.FindByEmailAsync(model.Email.Trim().ToLowerInvariant());
            if (user != null)
            {
                await SignInUserAsync(user);
            }

            TempData["SuccessMessage"] = "Your account has been created successfully! Welcome to SkillHub.";
            return RedirectToAction("Index", "Workshops");
        }
        catch (BusinessRuleViolationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (Exception)
        {
            ModelState.AddModelError(string.Empty, "An unexpected error occurred during registration. Please try again.");
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Workshops");
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }

    private async Task SignInUserAsync(ApplicationUser user)
    {
        var roles = await _userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email!),
            new("userId", user.Id)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(4)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);
    }
}
