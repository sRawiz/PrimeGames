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
using System.Transactions;

namespace cleanNETCoreMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<UserController> _logger;

        public UserController(UserManager<ApplicationUser> userManager, ILogger<UserController> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userVMs = new List<UserWithRolesVM>();
                
                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userVMs.Add(new UserWithRolesVM { User = user, Roles = roles });
                }
                
                _logger.LogInformation("Successfully loaded {UserCount} users for admin view", users.Count);
                return View(userVMs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users for admin view");
                return View(new List<UserWithRolesVM>());
            }
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
                _logger.LogWarning("EditModal called with null id");
                return NotFound();
            }
            
            _logger.LogDebug("EditModal called for user ID: {UserId}", id);
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                _logger.LogWarning("User not found for EditModal with ID: {UserId}", id);
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
            var adminUserId = _userManager.GetUserId(User);
            _logger.LogInformation("Admin {AdminUserId} attempting to create user: {UserName}", 
                adminUserId, form["UserName"].ToString());

            try
            {
                var user = new ApplicationUser
                {
                    UserName = form["UserName"].ToString(),
                    FirstName = form["FirstName"].ToString(),
                    LastName = form["LastName"].ToString(),
                    Email = form["Email"].ToString(),
                    PhoneNumber = form["PhoneNumber"].ToString(),
                    EmailConfirmed = true,
                    CreatedAt = DateTime.Now
                };

                if (!string.IsNullOrWhiteSpace(form["Birthdate"]))
                {
                    if (DateTime.TryParse(form["Birthdate"], out var birthdate))
                        user.Birthdate = birthdate;
                }

                var password = form["Password"].ToString();
                var role = form["Role"].ToString();

                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                try
                {
                    _logger.LogDebug("Starting user creation transaction for: {UserName}", user.UserName);

                    var result = await _userManager.CreateAsync(user, string.IsNullOrEmpty(password) ? string.Empty : password);
                    if (!result.Succeeded)
                    {
                        _logger.LogWarning("User creation failed for {UserName}. Errors: {Errors}", 
                            user.UserName, string.Join("; ", result.Errors.Select(e => e.Description)));
                        return Json(new { success = false, errors = result.Errors.Select(e => e.Description).ToList() });
                    }

                    _logger.LogInformation("User created successfully: {UserName} with ID: {UserId}", 
                        user.UserName, user.Id);

                    if (!string.IsNullOrEmpty(role))
                    {
                        var roleResult = await _userManager.AddToRoleAsync(user, role);
                        if (!roleResult.Succeeded)
                        {
                            _logger.LogError("Failed to add role {Role} to user {UserName}. Errors: {Errors}", 
                                role, user.UserName, string.Join("; ", roleResult.Errors.Select(e => e.Description)));
                            return Json(new { success = false, errors = roleResult.Errors.Select(e => e.Description).ToList() });
                        }

                        _logger.LogInformation("Successfully added role {Role} to user {UserName}", role, user.UserName);
                    }

                    transactionScope.Complete();
                    _logger.LogInformation("User creation transaction completed successfully for {UserName} by admin {AdminUserId}", 
                        user.UserName, adminUserId);

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Transaction failed during user creation for {UserName} by admin {AdminUserId}", 
                        user.UserName, adminUserId);
                    return Json(new { success = false, errors = new[] { "เกิดข้อผิดพลาดในการสร้างผู้ใช้ กรุณาลองใหม่อีกครั้ง" } });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during user creation by admin {AdminUserId}", adminUserId);
                return Json(new { success = false, errors = new[] { "เกิดข้อผิดพลาดที่ไม่คาดคิด กรุณาลองใหม่อีกครั้ง" } });
            }
        }
        [HttpPost]
        public async Task<IActionResult> EditAjax(IFormCollection form)
        {
            var id = form["Id"].ToString();
            var adminUserId = _userManager.GetUserId(User);
            
            _logger.LogInformation("Admin {AdminUserId} attempting to edit user ID: {UserId}", adminUserId, id);

            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Edit user request with empty ID by admin {AdminUserId}", adminUserId);
                return Json(new { success = false, error = "ไม่พบผู้ใช้" });
            }

            try
            {
                var dbUser = await _userManager.FindByIdAsync(id);
                if (dbUser == null)
                {
                    _logger.LogWarning("User not found for edit. ID: {UserId} by admin {AdminUserId}", id, adminUserId);
                    return Json(new { success = false, error = "ไม่พบผู้ใช้" });
                }

                var password = form["Password"].ToString();
                var role = form["Role"].ToString();

                using var transactionScope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                try
                {
                    _logger.LogDebug("Starting user edit transaction for ID: {UserId}", id);

                    // Update basic user information
                    dbUser.FirstName = form["FirstName"].ToString();
                    dbUser.LastName = form["LastName"].ToString();
                    dbUser.Birthdate = !string.IsNullOrWhiteSpace(form["Birthdate"]) && DateTime.TryParse(form["Birthdate"], out var birthdate) ? birthdate : (DateTime?)null;
                    dbUser.Email = form["Email"].ToString();
                    dbUser.PhoneNumber = form["PhoneNumber"].ToString();

                    var result = await _userManager.UpdateAsync(dbUser);
                    if (!result.Succeeded)
                    {
                        _logger.LogWarning("User update failed for ID: {UserId}. Errors: {Errors}", 
                            id, string.Join("; ", result.Errors.Select(e => e.Description)));
                        return Json(new { success = false, errors = result.Errors.Select(e => e.Description).ToList() });
                    }

                    _logger.LogInformation("User basic info updated successfully for ID: {UserId}", id);

                    // Update password if provided
                    if (!string.IsNullOrEmpty(password))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(dbUser);
                        var passResult = await _userManager.ResetPasswordAsync(dbUser, token, password);
                        if (!passResult.Succeeded)
                        {
                            _logger.LogError("Password reset failed for user ID: {UserId}. Errors: {Errors}", 
                                id, string.Join("; ", passResult.Errors.Select(e => e.Description)));
                            return Json(new { success = false, errors = passResult.Errors.Select(e => e.Description).ToList() });
                        }

                        _logger.LogInformation("Password updated successfully for user ID: {UserId}", id);
                    }

                    // Update roles if provided
                    var roles = await _userManager.GetRolesAsync(dbUser);
                    if (!string.IsNullOrEmpty(role) && !(roles?.Contains(role ?? string.Empty) ?? false))
                    {
                        if (roles != null && roles.Count > 0)
                        {
                            var removeResult = await _userManager.RemoveFromRolesAsync(dbUser, roles);
                            if (!removeResult.Succeeded)
                            {
                                _logger.LogError("Failed to remove existing roles from user ID: {UserId}. Errors: {Errors}", 
                                    id, string.Join("; ", removeResult.Errors.Select(e => e.Description)));
                                return Json(new { success = false, errors = removeResult.Errors.Select(e => e.Description).ToList() });
                            }

                            _logger.LogInformation("Removed existing roles from user ID: {UserId}. Roles: {Roles}", 
                                id, string.Join(", ", roles));
                        }

                        var addResult = await _userManager.AddToRoleAsync(dbUser, role ?? string.Empty);
                        if (!addResult.Succeeded)
                        {
                            _logger.LogError("Failed to add role {Role} to user ID: {UserId}. Errors: {Errors}", 
                                role, id, string.Join("; ", addResult.Errors.Select(e => e.Description)));
                            return Json(new { success = false, errors = addResult.Errors.Select(e => e.Description).ToList() });
                        }

                        _logger.LogInformation("Successfully added role {Role} to user ID: {UserId}", role, id);
                    }

                    transactionScope.Complete();
                    _logger.LogInformation("User edit transaction completed successfully for ID: {UserId} by admin {AdminUserId}", 
                        id, adminUserId);

                    return Json(new { success = true });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Transaction failed during user edit for ID: {UserId} by admin {AdminUserId}", 
                        id, adminUserId);
                    return Json(new { success = false, errors = new[] { "เกิดข้อผิดพลาดในการแก้ไขข้อมูลผู้ใช้ กรุณาลองใหม่อีกครั้ง" } });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during user edit for ID: {UserId} by admin {AdminUserId}", 
                    id, adminUserId);
                return Json(new { success = false, errors = new[] { "เกิดข้อผิดพลาดที่ไม่คาดคิด กรุณาลองใหม่อีกครั้ง" } });
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteAjax(string id)
        {
            var adminUserId = _userManager.GetUserId(User);
            _logger.LogInformation("Admin {AdminUserId} attempting to delete user ID: {UserId}", adminUserId, id);

            try
            {
                var user = await _userManager.FindByIdAsync(id);
                if (user == null)
                {
                    _logger.LogWarning("User not found for deletion. ID: {UserId} by admin {AdminUserId}", id, adminUserId);
                    return Json(new { success = false, error = "ไม่พบผู้ใช้" });
                }

                _logger.LogInformation("Found user to delete: ID: {UserId}, UserName: {UserName}", id, user.UserName);

                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    _logger.LogInformation("Successfully deleted user: ID: {UserId}, UserName: {UserName} by admin {AdminUserId}", 
                        id, user.UserName, adminUserId);
                    return Json(new { success = true });
                }

                _logger.LogWarning("User deletion failed for ID: {UserId}. Errors: {Errors}", 
                    id, string.Join("; ", result.Errors.Select(e => e.Description)));
                return Json(new { success = false, errors = result.Errors.Select(e => e.Description).ToList() });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during user deletion for ID: {UserId} by admin {AdminUserId}", 
                    id, adminUserId);
                return Json(new { success = false, errors = new[] { "เกิดข้อผิดพลาดในการลบผู้ใช้ กรุณาลองใหม่อีกครั้ง" } });
            }
        }
    }

    public class UserWithRolesVM
    {
        public ApplicationUser User { get; set; } = new ApplicationUser();
        public IList<string> Roles { get; set; } = new List<string>();
    }
} 