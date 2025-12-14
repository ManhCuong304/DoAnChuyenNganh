using DoAnCoSo.Models;
using DoAnCoSo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        private readonly ProfileService _profileService;

        public ProfileController(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            IConfiguration configuration,
            ProfileService profileService)
        {
            _userManager = userManager;
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _configuration = configuration;
            _profileService = profileService;
        }

        // ✅ Trang hồ sơ chính
        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            // Lấy thông tin user cùng follower và following
            var user = await _context.Users
                .Include(u => u.Followings)
                .Include(u => u.Followers)
                .FirstOrDefaultAsync(u => u.Id == currentUserId);

            if (user == null)
                return RedirectToAction("Login", "Account");

            ViewBag.Following = user.Followings?.Count ?? 0;
            ViewBag.Follower = user.Followers?.Count ?? 0;

            // LẤY BÀI VIẾT CỦA USER HIỆN TẠI
            var userPosts = await _context.Posts
                .Where(p => p.UserId == currentUserId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.UserPosts = userPosts;

            // Gợi ý follow
            var followingIds = await _context.Follow
                .Where(f => f.FollowerId == currentUserId)
                .Select(f => f.FollowingId)
                .ToListAsync();

            var suggestedUsers = await _context.Users
                .Where(u => u.Id != currentUserId && !followingIds.Contains(u.Id))
                .Take(5)
                .ToListAsync();

            ViewBag.SuggestedUsers = suggestedUsers;

            // 🔥 [ĐÃ SỬA] Logic mới: Lấy những người mình đang Follow để hiện lên Sidebar
            var followingList = await _context.Follow
                .Where(f => f.FollowerId == currentUserId)
                .Include(f => f.Following) // Include để lấy thông tin User
                .Select(f => f.Following)
                .ToListAsync();

            // Vẫn giữ tên biến là MutualFriends để không phải sửa bên View Layout
            ViewBag.MutualFriends = followingList ?? new List<ApplicationUser>();

            return View(user);
        }

        // ✅ Chỉnh sửa hồ sơ
        [HttpPost]
        public async Task<IActionResult> EditProfile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            user.FullName = model.FullName;
            user.UserName = model.Nickname;
            user.Description = model.Description;
            user.Major = model.Major;
            user.BirthDate = model.BirthDate;

            // ✅ Upload ảnh đại diện
            if (model.Avatar != null)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath ?? "wwwroot", "images", "avatars");
                Directory.CreateDirectory(uploadsFolder);
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.Avatar.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await model.Avatar.CopyToAsync(stream);
                }

                user.Image = "/images/avatars/" + fileName;
            }

            // ✅ Sinh mô tả tự động nếu chưa có
            user.Describe = string.IsNullOrWhiteSpace(model.Describe)
                ? await GenerateDescriptionAsync(user.Major, user.Description)
                : model.Describe;

            // ✅ Sinh vector embedding và lưu
            await _profileService.GenerateUserEmbeddingAsync(user);

            // ✅ Lưu thay đổi
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                Console.WriteLine("❌ Không lưu được user: " + string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            return RedirectToAction("Index");
        }

        // ✅ Gợi ý bạn bè (AI-based hoặc random fallback)
        [HttpGet]
        public async Task<IActionResult> GetSuggestedFriends()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Json(new List<object>());

            // Gợi ý theo vector (AI)
            var similarIds = await _profileService.GetSimilarUsersAsync(currentUser.Id);
            var users = await _context.Users
                .Where(u => similarIds.Contains(u.Id))
                .Select(u => new
                {
                    id = u.Id,
                    fullName = u.FullName,
                    userName = u.UserName,
                    image = string.IsNullOrEmpty(u.Image) ? "/images/default-avatar.png" : u.Image
                })
                .ToListAsync();

            // Nếu chưa có vector thì fallback random
            if (!users.Any())
            {
                users = await _context.Users
                    .Where(u => u.Id != currentUser.Id)
                    .Take(5)
                    .Select(u => new
                    {
                        id = u.Id,
                        fullName = u.FullName,
                        userName = u.UserName,
                        image = string.IsNullOrEmpty(u.Image) ? "/images/default-avatar.png" : u.Image
                    })
                    .ToListAsync();
            }

            return Json(users);
        }

        // ✅ Sinh mô tả bằng AI
        private async Task<string> GenerateDescriptionAsync(string major, string description)
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
                            content = $"Viết khoảng 20 đặc điểm băng tiếng việt , thế mạnh giỏi về công việc, cách nhau bằng dấu phẩy. Dựa trên chuyên ngành {major} và mô tả: {description}. Sử dụng động từ, tính từ, không viết thành câu."
                        }
                    }
                };

                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(url, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(responseBody);
                    return doc.RootElement
                        .GetProperty("choices")[0]
                        .GetProperty("message")
                        .GetProperty("content")
                        .GetString() ?? "Không thể sinh mô tả.";
                }

                return "Không thể tạo mô tả tự động.";
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Lỗi sinh mô tả: " + ex.Message);
                return "Đã xảy ra lỗi khi tạo mô tả.";
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> SearchProfile(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Json(new object[0]);

            var q = query.Trim().ToLower();

            var users = await _context.Users
                .Where(u => (u.UserName ?? "").ToLower().Contains(q)
                         || (u.FullName ?? "").ToLower().Contains(q))
                .Select(u => new {
                    id = u.Id,
                    fullName = u.FullName,
                    userName = u.UserName,
                    image = string.IsNullOrEmpty(u.Image) ? Url.Content("~/images/default-avatar.png") : u.Image
                })
                .Take(10)
                .ToListAsync();

            return Json(users);
        }

        // 🔥 [ĐÃ SỬA] Xem danh sách "Người đang theo dõi" (Thay vì bạn bè chung)
        [HttpGet]
        public async Task<IActionResult> MyFriends()
        {
            var currentUserId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(currentUserId))
            {
                return RedirectToAction("Login", "Account");
            }

            var myFollowings = await _context.Follow
                .Where(f => f.FollowerId == currentUserId)
                .Include(f => f.Following)
                .Select(f => f.Following)
                .ToListAsync();

            return View(myFollowings);
        }

        // 🔥 [THÊM MỚI] API để Javascript gọi và cập nhật Sidebar ngay khi bấm Follow
        [HttpGet]
        public async Task<IActionResult> GetFriendsJson()
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(currentUserId)) return Json(new List<object>());

            var followingUsers = await _context.Follow
                .Where(f => f.FollowerId == currentUserId) // Lấy danh sách mình đang follow
                .Select(f => new
                {
                    id = f.Following.Id,
                    fullName = f.Following.FullName,
                    image = f.Following.Image ?? "/images/default-avatar.png"
                })
                .ToListAsync();

            return Json(followingUsers);
        }

        [HttpGet]
        public async Task<IActionResult> ProfileDetail(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);

            // 1. Lấy thông tin user mục tiêu kèm Follower/Following
            var user = await _context.Users
                .Include(u => u.Followings)
                .Include(u => u.Followers)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return NotFound(); // Không tìm thấy user này
            }

            // 2. Đếm số lượng
            ViewBag.Following = user.Followings?.Count ?? 0;
            ViewBag.Follower = user.Followers?.Count ?? 0;

            // 3. Lấy danh sách bài viết của người này
            var userPosts = await _context.Posts
                .Where(p => p.UserId == id)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            ViewBag.UserPosts = userPosts;

            // 4. Kiểm tra xem mình đã Follow người này chưa
            if (currentUserId != null)
            {
                var isFollowing = await _context.Follow
                    .AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == id);
                ViewBag.IsFollowing = isFollowing;
            }

            // 5. Kiểm tra xem đây có phải là trang của chính mình không
            ViewBag.IsMe = (currentUserId == id);

            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleFollow(string id)
        {
            var currentUserId = _userManager.GetUserId(User);

            // 1. Kiểm tra đăng nhập
            if (string.IsNullOrEmpty(currentUserId))
                return Json(new { success = false, message = "Bạn cần đăng nhập để thực hiện chức năng này." });

            // 2. Không cho phép tự follow chính mình
            if (currentUserId == id)
                return Json(new { success = false, message = "Bạn không thể tự theo dõi chính mình." });

            // 3. Kiểm tra xem đã follow chưa
            var existingFollow = await _context.Follow
                .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == id);

            bool isFollowingNow;

            if (existingFollow != null)
            {
                // TRƯỜNG HỢP: ĐÃ FOLLOW -> HỦY FOLLOW (UNFOLLOW)
                _context.Follow.Remove(existingFollow);
                isFollowingNow = false;
            }
            else
            {
                // TRƯỜNG HỢP: CHƯA FOLLOW -> THÊM MỚI (FOLLOW)
                var newFollow = new Follow
                {
                    FollowerId = currentUserId,
                    FollowingId = id,
                    FollowAt = DateTime.Now
                };
                _context.Follow.Add(newFollow);
                isFollowingNow = true;
            }

            // 4. Lưu thay đổi vào Database
            await _context.SaveChangesAsync();

            // 5. Đếm lại tổng số người theo dõi
            var followerCount = await _context.Follow.CountAsync(f => f.FollowingId == id);

            // 6. Trả về kết quả cho JavaScript xử lý
            return Json(new { success = true, isFollowing = isFollowingNow, newFollowerCount = followerCount });
        }
    }
}