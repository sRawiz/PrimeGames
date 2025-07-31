using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cleanNETCoreMVC.Models;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System;

namespace cleanNETCoreMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            var userVMs = new List<UserWithRolesVM>();
            foreach (var user in users)
            {
                var roles = _userManager.GetRolesAsync(user).Result;
                userVMs.Add(new UserWithRolesVM { User = user, Roles = roles });
            }
            return View(userVMs);
        }

        public async Task<IActionResult> Details(string id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, ApplicationUser user)
        {
            if (id != user.Id) return NotFound();
            if (ModelState.IsValid)
            {
                var dbUser = await _userManager.FindByIdAsync(id);
                if (dbUser == null) return NotFound();
                dbUser.FirstName = user.FirstName;
                dbUser.LastName = user.LastName;
                dbUser.Birthdate = user.Birthdate;
                dbUser.Email = user.Email;
                dbUser.PhoneNumber = user.PhoneNumber;
                await _userManager.UpdateAsync(dbUser);
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();
            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user != null)
            {
                await _userManager.DeleteAsync(user);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult CreateModal()
        {
            return PartialView("_CreatePartial", new ApplicationUser());
        }
        [HttpGet]
        public async Task<IActionResult> EditModal(string id)
        {
            if (id == null)
            {
                Console.WriteLine("[EditModal] id is null");
                return NotFound();
            }
            Console.WriteLine($"[EditModal] id: {id}");
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                Console.WriteLine($"[EditModal] user not found for id: {id}");
                return NotFound();
            }
            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.CurrentRole = roles.FirstOrDefault() ?? "User";
            return PartialView("_EditPartial", user);
        }
        [HttpGet]
        public async Task<IActionResult> DeleteModal(string id)
        {
            if (id == null) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return PartialView("_DeletePartial", user);
        }
        [HttpPost]
        public async Task<IActionResult> CreateAjax(IFormCollection form)
        {
            var user = new ApplicationUser
            {
                UserName = form["UserName"],
                FirstName = form["FirstName"],
                LastName = form["LastName"],
                Email = form["Email"],
                PhoneNumber = form["PhoneNumber"],
                EmailConfirmed = true
            };
            user.CreatedAt = DateTime.Now;
            if (!string.IsNullOrWhiteSpace(form["Birthdate"]))
            {
                if (DateTime.TryParse(form["Birthdate"], out var birthdate))
                    user.Birthdate = birthdate;
            }
            var password = form["Password"].ToString();
            var role = form["Role"].ToString();
            var result = await _userManager.CreateAsync(user, string.IsNullOrEmpty(password) ? string.Empty : password);
            if (result.Succeeded)
            {
                if (!string.IsNullOrEmpty(role)) await _userManager.AddToRoleAsync(user, role);
                return Json(new { success = true });
            }
            return Json(new { success = false, errors = result.Errors.Select(e => e.Description).ToList() });
        }
        [HttpPost]
        public async Task<IActionResult> EditAjax(IFormCollection form)
        {
            var id = form["Id"].ToString();
            if (string.IsNullOrEmpty(id)) return Json(new { success = false, error = "ไม่พบผู้ใช้" });
            var dbUser = await _userManager.FindByIdAsync(id);
            if (dbUser == null) return Json(new { success = false, error = "ไม่พบผู้ใช้" });
            dbUser.FirstName = form["FirstName"];
            dbUser.LastName = form["LastName"];
            dbUser.Birthdate = !string.IsNullOrWhiteSpace(form["Birthdate"]) && DateTime.TryParse(form["Birthdate"], out var birthdate) ? birthdate : (DateTime?)null;
            dbUser.Email = form["Email"];
            dbUser.PhoneNumber = form["PhoneNumber"];
            var result = await _userManager.UpdateAsync(dbUser);
            if (!result.Succeeded) return Json(new { success = false, errors = result.Errors.Select(e => e.Description).ToList() });
            var password = form["Password"].ToString();
            if (!string.IsNullOrEmpty(password))
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(dbUser);
                var passResult = await _userManager.ResetPasswordAsync(dbUser, token, password);
                if (!passResult.Succeeded) return Json(new { success = false, errors = passResult.Errors.Select(e => e.Description).ToList() });
            }
            var role = form["Role"].ToString();
            var roles = await _userManager.GetRolesAsync(dbUser);
            if (!string.IsNullOrEmpty(role) && !(roles?.Contains(role ?? string.Empty) ?? false))
            {
                if (roles != null && roles.Count > 0)
                    await _userManager.RemoveFromRolesAsync(dbUser, roles);
                await _userManager.AddToRoleAsync(dbUser, role ?? string.Empty);
            }
            return Json(new { success = true });
        }
        [HttpPost]
        public async Task<IActionResult> DeleteAjax(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return Json(new { success = false, error = "ไม่พบผู้ใช้" });
            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded) return Json(new { success = true });
            return Json(new { success = false, errors = result.Errors.Select(e => e.Description).ToList() });
        }
    }

    public class UserWithRolesVM
    {
        public ApplicationUser User { get; set; } = new ApplicationUser();
        public IList<string> Roles { get; set; } = new List<string>();
    }
} 