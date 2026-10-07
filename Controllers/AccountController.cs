using ChurchManagementMVC.Models;
using ChurchManagementMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<Admins> _userManager;
        private readonly SignInManager<Admins> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AccountController(
        UserManager<Admins> userManager,
        SignInManager<Admins> signInManager,
        RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }

        // =========================
        // LOGIN
        // =========================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(
            LoginViewModel model,
            string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(
                model.Email,
                model.Password,
                model.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(returnUrl) &&
                    Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction(
                    "Dashboard",
                    "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "This account has been locked.");
            }
            else
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Invalid email or password.");
            }

            return View(model);
        }

        // =========================
        // REGISTER
        // =========================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = new Admins
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.Name
            };

            var result = await _userManager.CreateAsync(
                user,
                model.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(
                    user,
                    "Member");

                await _signInManager.SignInAsync(
                    user,
                    isPersistent: false);

                return RedirectToAction(
                    "Dashboard",
                    "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(
                    string.Empty,
                    error.Description);
            }

            return View(model);
        }

        // =========================
        // LOGOUT
        // =========================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction(
                "Login",
                "Account");
        }

        // =========================
        // ACCESS DENIED
        // =========================

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}