using DoAnCoSo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace DoAnCoSo.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> GetFriends()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return RedirectToAction("Login", "Account");

            var currentUserId = currentUser.Id;

            var followingIds = await _context.Follow
                .Where(f => f.FollowerId == currentUserId)
                .Select(f => f.FollowingId)
                .ToListAsync();

            var followerIds = await _context.Follow
                .Where(f => f.FollowingId == currentUserId)
                .Select(f => f.FollowerId)
                .ToListAsync();

            var friendIds = followingIds.Intersect(followerIds).ToList();

            var friends = await _context.Users
                .Where(u => friendIds.Contains(u.Id))
                .ToListAsync();
            return Json(friends);
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var currentUserId = currentUser?.Id;
            ViewBag.userId = currentUserId;
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Profile()
        {
            var model = new ProfileViewModel
            {
                FullName = "Nguyễn Văn A",
                Nickname = "NVA",
                PhoneNumber = "0123456789",
                BirthDate = new DateTime(2000, 1, 1)
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Profile(ProfileViewModel model)
        {
            if (ModelState.IsValid)
            {
                ViewBag.Message = "Cập nhật thông tin thành công!";
            }

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
