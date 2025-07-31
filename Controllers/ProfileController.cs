using cleanNETCoreMVC.Models;
using cleanNETCoreMVC.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PrimeGames.Services;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace cleanNETCoreMVC.Controllers
{
    [Authorize]
    [Route("profile")]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IBlobStorageService _blobStorageService;
        private readonly IConfiguration _configuration;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IBlobStorageService blobStorageService,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _blobStorageService = blobStorageService;
            _configuration = configuration;
        }

        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            return View(user);
        }

        [HttpPost("")]
        public async Task<IActionResult> Index(string FirstName, string LastName, string Email, Microsoft.AspNetCore.Http.IFormFile? AvatarFile)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (AvatarFile != null && AvatarFile.Length > 0)
            {
                var ext = Path.GetExtension(AvatarFile.FileName).ToLower();
                var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!allowed.Contains(ext))
                    ModelState.AddModelError("AvatarFile", "อนุญาตเฉพาะไฟล์ .jpg, .jpeg, .png, .webp เท่านั้น");
                if (AvatarFile.Length > 2 * 1024 * 1024)
                    ModelState.AddModelError("AvatarFile", "ไฟล์ต้องมีขนาดไม่เกิน 2MB");

                if (ModelState.IsValid)
                {
                    try
                    {
                        var containerName = "user-avatars";

                        var newAvatarUrl = await _blobStorageService.UploadImageAsync(AvatarFile, containerName);

                        if (!string.IsNullOrEmpty(user.AvatarUrl))
                        {
                            await _blobStorageService.DeleteImageAsync(user.AvatarUrl, containerName);
                        }

                        user.AvatarUrl = newAvatarUrl;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("AvatarFile", $"เกิดข้อผิดพลาดในการอัพโหลดรูป: {ex.Message}");
                        return View(user);
                    }
                }
            }

            user.FirstName = FirstName;
            user.LastName = LastName;

            if (!string.IsNullOrWhiteSpace(Email) && Email != user.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, Email);
                if (!setEmailResult.Succeeded)
                {
                    ModelState.AddModelError("Email", "อีเมลนี้ไม่ถูกต้องหรือมีผู้ใช้งานแล้ว");
                    return View(user);
                }
            }

            if (!ModelState.IsValid)
                return View(user);

            await _userManager.UpdateAsync(user);
            await _signInManager.RefreshSignInAsync(user);
            ViewBag.Success = true;
            return View(user);
        }

        [HttpPost("changepassword")]
        public async Task<IActionResult> ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                ViewBag.PasswordError = "กรุณากรอกข้อมูลให้ครบถ้วน";
                return View("Index", user);
            }

            if (NewPassword != ConfirmPassword)
            {
                ViewBag.PasswordError = "รหัสผ่านใหม่และยืนยันรหัสผ่านไม่ตรงกัน";
                return View("Index", user);
            }

            var result = await _userManager.ChangePasswordAsync(user, CurrentPassword, NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                ViewBag.PasswordSuccess = true;
                return View("Index", user);
            }
            else
            {
                ViewBag.PasswordError = string.Join("<br>", result.Errors.Select(e => e.Description));
                return View("Index", user);
            }
        }

        [HttpPost("changephone")]
        public async Task<IActionResult> ChangePhone(string NewPhoneNumber)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (string.IsNullOrWhiteSpace(NewPhoneNumber))
            {
                ViewBag.PhoneError = "กรุณากรอกเบอร์โทรศัพท์ใหม่";
                return View("Index", user);
            }

            if (!System.Text.RegularExpressions.Regex.IsMatch(NewPhoneNumber, @"^\+?\d{9,15}$"))
            {
                ViewBag.PhoneError = "รูปแบบเบอร์โทรศัพท์ไม่ถูกต้อง";
                return View("Index", user);
            }

            user.PhoneNumber = NewPhoneNumber;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                ViewBag.PhoneSuccess = true;
                return View("Index", user);
            }
            else
            {
                ViewBag.PhoneError = string.Join("<br>", result.Errors.Select(e => e.Description));
                return View("Index", user);
            }
        }
    }
}