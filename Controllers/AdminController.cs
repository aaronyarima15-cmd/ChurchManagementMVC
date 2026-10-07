using ChurchManagementMVC.Models;
using ChurchManagementMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ChurchManagementMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<Admins> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            UserManager<Admins> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Users()
        {
            var users = _userManager.Users.ToList();

            var userList = new List<UserRoleViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userList.Add(new UserRoleViewModel
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email ?? "",
                    Role = roles.FirstOrDefault() ?? "Member"
                });
            }

            return View(userList);
        }

        [HttpGet]
        public async Task<IActionResult> ChangeRole(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            var roles = await _userManager.GetRolesAsync(user);

            var model = new UserRoleViewModel
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? "",
                Role = roles.FirstOrDefault() ?? "Member"
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeRole(UserRoleViewModel model)
        {
            if (string.IsNullOrEmpty(model.Id))
            {
                return NotFound();
            }

            var user = await _userManager.FindByIdAsync(model.Id);

            if (user == null)
            {
                return NotFound();
            }

            var allowedRoles = new[]
            {
                "Admin",
                "Pastor",
                "Secretary",
                "Treasurer",
                "Member"
    };

            if (!allowedRoles.Contains(model.Role))
            {
                ModelState.AddModelError(
                    "Role",
                    "Invalid role selected.");

                return View(model);
            }

            // Prevent the current Admin from accidentally removing
            // their own Admin access.
            if (user.Id == _userManager.GetUserId(User) &&
                model.Role != "Admin")
            {
                ModelState.AddModelError(
                    "Role",
                    "You cannot remove the Admin role from your own account.");

                return View(model);
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(
                    user,
                    currentRoles);

                if (!removeResult.Succeeded)
                {
                    foreach (var error in removeResult.Errors)
                    {
                        ModelState.AddModelError(
                            string.Empty,
                            error.Description);
                    }

                    return View(model);
                }
            }

            var addResult = await _userManager.AddToRoleAsync(
                user,
                model.Role);

            if (!addResult.Succeeded)
            {
                foreach (var error in addResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return View(model);
            }

            return RedirectToAction(nameof(Users));
        }
    }
}