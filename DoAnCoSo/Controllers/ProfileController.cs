using DoAnCoSo.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace DoAnCoSo.Controllers
{
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _hostEnvironment;
        public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, 
            IWebHostEnvironment webHostEnvironment, IConfiguration configuration, IWebHostEnvironment hostEnvironment)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _hostEnvironment = hostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _context.Users
                .Include(u => u.Followings)
                .Include(u => u.Followers)
                .FirstOrDefaultAsync(u => u.Id == _userManager.GetUserId(User));

            ViewBag.Following = user.Followings?.Count ?? 0;
            ViewBag.Follower = user.Followers?.Count ?? 0;

            var currentUserId = _userManager.GetUserId(User);
            var suggestedUsers = await _context.Users
                .Where(u => u.Id != currentUserId &&
                            !_context.Follow.Any(f => f.FollowerId == currentUserId && f.FollowingId == u.Id))
                .Take(5)
                .ToListAsync();

            ViewBag.SuggestedUsers = suggestedUsers;

            return View(user);
        }

        public async Task<IActionResult> SearchProfile(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new List<object>());

            var profiles = await _context.Users
                .Where(u => u.FullName.Contains(query))
                .Select(u => new
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Image = string.IsNullOrEmpty(u.Image) ? "/images/default-avatar.png" : u.Image
                })
                .Take(10)
                .ToListAsync();

            return Json(profiles);
        }

        public async Task<IActionResult> ProfileDetail(string Id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var profiledetail = await _context.Users
                .Include(u => u.Followers)
                .Include(u => u.Followings)
                .FirstOrDefaultAsync(p => p.Id == Id);

            if (profiledetail == null) return NotFound();

            ViewBag.Follower = profiledetail.Followers?.Count ?? 0;
            ViewBag.Following = profiledetail.Followings?.Count ?? 0;

            var isFollowing = await _context.Follow
                .AnyAsync(f => f.FollowerId == currentUser.Id && f.FollowingId == Id);
            var isFollowedBack = await _context.Follow
                .AnyAsync(f => f.FollowerId == Id && f.FollowingId == currentUser.Id);

            ViewBag.IsFollowing = isFollowing;
            ViewBag.IsMutualFollow = isFollowing && isFollowedBack;

            return View(profiledetail);
        }

        [HttpPost]
        public async Task<IActionResult> Follow(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.Id == id) return BadRequest();

            var userToFollow = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (userToFollow == null) return NotFound();

            var existingFollow = await _context.Follow.FirstOrDefaultAsync(f => f.FollowerId == currentUser.Id && f.FollowingId == id);

            if (existingFollow != null)
                _context.Follow.Remove(existingFollow);
            else
                _context.Follow.Add(new Follow
                {
                    FollowerId = currentUser.Id,
                    FollowingId = id,
                    FollowAt = DateTime.Now
                });

            await _context.SaveChangesAsync();
            return Json("Thành Công");
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);

            user.FullName = model.FullName;
            user.UserName = model.Nickname;
            user.Description = model.Description;
            user.Major = model.Major;
            user.BirthDate = model.BirthDate;

            if (model.Avatar != null)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath ?? "wwwroot", "images", "avatars");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Avatar.FileName);
               var filePath = Path.Combine(uploadsFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Avatar.CopyToAsync(fileStream);
                }

               user.Image = "/images/avatars/" + fileName;
           }

           if (string.IsNullOrWhiteSpace(model.Describe))
           {
               user.Describe = await GenerateDescriptionAsync(user.Major, user.Description);
           }
            else
            {
                user.Describe = model.Describe;
           }

            await _userManager.UpdateAsync(user);
           return RedirectToAction("Index");
        }

        public async Task<IActionResult> MyFriends()
        {
            var currentUserId = _userManager.GetUserId(User);

            var friends = await _context.Follow
                .Where(f => f.FollowerId == currentUserId)
                .Select(f => f.Following)
                .ToListAsync();

            return View(friends);
        }

        private async Task<string> GenerateDescriptionAsync(string major, string Description)
        {
            try
            {
                var apiKey = _configuration["OpenRouter:ApiKey"];
                var url = "https://openrouter.ai/api/v1/chat/completions";

                using var httpClient = new HttpClient();

                httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");
                httpClient.DefaultRequestHeaders.Add("HTTP-Referer", "https://localhost:5001");
                httpClient.DefaultRequestHeaders.Add("X-Title", "Profile Description");

                var requestBody = new
                {
                    model = "openai/gpt-3.5-turbo",
                    messages = new[]
                    {
                new {
                    role = "user",
                    content = $"Viết đặc điểm, thế mạnh giỏi về công việc gì một cách ngắn gọn,cách nhau bằng dấu phẩy{Description}, chuyên ngành {major}.Sử dụng các động từ,tính từ,không viết thành câu, "
                }
            }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(url, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(responseContent);
                    var text = doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString();

                    return text ?? "Không thể sinh mô tả.";
                }
                else
                {
                    Console.WriteLine($"OpenRouter Error: {response.StatusCode}, {responseContent}");
                    return "Không thể tạo mô tả tự động.";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi tạo mô tả: {ex.Message}");
                return "Đã xảy ra lỗi khi tạo mô tả.";
            }
        }
        [HttpGet]
        public async Task<IActionResult> TestGemini()
        {
            var text = await GenerateDescriptionAsync("Công nghệ thông tin", "Phát triển phần mềm");
            return Content(text);
        }

    }
}
