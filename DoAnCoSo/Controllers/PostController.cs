using DoAnCoSo.Models;
using DoAnCoSo.Models.ViewModels;
using DoAnCoSo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Hosting;

namespace DoAnCoSo.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly PostService _postService;
        private readonly MyAiService _ai;
        private readonly ILogger<PostController> _logger;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;
        public PostController(
            ApplicationDbContext context,
            IWebHostEnvironment webHostEnvironment,
            IWebHostEnvironment env,
            PostService postService,
            MyAiService ai,
            ILogger<PostController> logger,
            UserManager<ApplicationUser> userManager) // ✔ Thêm vào
        {
            _context = context;
            _env = env;
            _postService = postService;
            _ai = ai;
            _logger = logger;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;// ✔ Gán đúng
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);


            var relevantPostIds = await _postService.GetRelevantPostIdsAsync(userId);

            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments).ThenInclude(c => c.User)
                .ToListAsync();

            posts = posts
                .OrderByDescending(p => relevantPostIds.Contains(p.Id)) 
                .ThenByDescending(p => p.CreatedAt)
                .ToList();

   
            var mutualFriends = await _context.Users
                .Where(u => u.Id != userId)
                .Take(5)
                .ToListAsync();

            var vm = new PostIndexViewModel
            {
                CurrentUserId = userId,
                Posts = posts,
                MutualFriends = mutualFriends,
                RelevantPostIds = new HashSet<int>(relevantPostIds)
            };

            ViewBag.RelevantPostIds = relevantPostIds;
            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> Index(PostIndexViewModel model, IFormFile imageFile)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrWhiteSpace(model.NewPost.Content) && imageFile == null)
            {
                ModelState.AddModelError("", "Bài viết không được để trống.");
                return RedirectToAction(nameof(Index));
            }

            var post = new Post
            {
                Content = model.NewPost.Content,
                UserId = userId,
                CreatedAt = DateTime.Now
            };

            if (imageFile != null)
            {
                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var filePath = Path.Combine(_env.WebRootPath, "uploads", fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }
                post.ImagePath = "/uploads/" + fileName;
            }

            _context.Posts.Add(post);
            await _context.SaveChangesAsync();

            // ✅ Sinh embedding cho bài viết mới
            await _postService.GeneratePostEmbeddingAsync(post);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(int postId, string content, int? parentCommentId)
        {
            if (string.IsNullOrWhiteSpace(content))
                return RedirectToAction(nameof(Index));

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var comment = new Comment
            {
                PostId = postId,
                Content = content,
                UserId = userId,
                CreatedAt = DateTime.Now,
                ParentCommentId = parentCommentId
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetFriends()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var friends = await _context.Follow
                .Where(f => f.FollowerId == userId || f.FollowingId == userId)
                .Select(f => f.FollowerId == userId ? f.Following : f.Follower)
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.UserName,
                    Image = string.IsNullOrEmpty(u.Image)
                                ? "/images/default-avatar.png"
                                : u.Image
                })
                .Distinct()
                .ToListAsync();

            return Json(friends);
        }

        [HttpGet]
        public async Task<IActionResult> GetSuggestedFriends()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var followingIds = await _context.Follow
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FollowingId)
                .ToListAsync();

            var suggested = await _context.Users
                .Where(u => u.Id != userId && !followingIds.Contains(u.Id))
                .Select(u => new
                {
                    u.Id,
                    u.FullName,
                    u.UserName,
                    Image = string.IsNullOrEmpty(u.Image)
                                ? "/images/default-avatar.png"
                                : u.Image
                })
                .Take(5)
                .ToListAsync();

            return Json(suggested);
        }
        [HttpGet]
        public async Task<IActionResult> CheckRecommendedPosts()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var relevantPostIds = await _postService.GetRelevantPostIdsAsync(userId);

            return Json(new
            {
                Success = true,
                Count = relevantPostIds?.Count ?? 0,
                RelevantPostIds = relevantPostIds
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetSortedPosts()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                // 1. Lấy danh sách ID bài viết gợi ý (đảm bảo không null)
                var relevantPostIds = await _postService.GetRelevantPostIdsAsync(userId) ?? new List<int>();

                // 2. Lấy dữ liệu từ DB (Sử dụng AsNoTracking để tối ưu tốc độ đọc)
                var posts = await _context.Posts
                    .Include(p => p.User)
                    .Include(p => p.Comments).ThenInclude(c => c.User)
                    .AsNoTracking()
                    .ToListAsync();

                // 3. Sắp xếp và Chuyển đổi dữ liệu (Projection) sang object ẩn danh
                // Việc này quan trọng để tránh lỗi "Reference Loop" (vòng lặp tham chiếu) khi trả về JSON
                var result = posts
                    .OrderByDescending(p => relevantPostIds.Contains(p.Id)) // Ưu tiên bài gợi ý
                    .ThenByDescending(p => p.CreatedAt)                     // Sau đó đến mới nhất
                    .Select(p => new
                    {
                        Id = p.Id,
                        Content = p.Content,
                        ImagePath = p.ImagePath,
                        CreatedAt = p.CreatedAt.ToString("HH:mm dd/MM/yyyy"), // Format ngày đẹp
                        IsRecommended = relevantPostIds.Contains(p.Id), // Cờ để UI biết đây là bài gợi ý
                        User = new
                        {
                            Id = p.User.Id,
                            FullName = p.User.FullName,
                            Avatar = string.IsNullOrEmpty(p.User.Image) ? "/images/default-avatar.png" : p.User.Image
                        },
                        CommentCount = p.Comments.Count,
                        // Lấy danh sách comment rút gọn (nếu cần hiển thị ngay)
                        Comments = p.Comments.OrderBy(c => c.CreatedAt).Select(c => new
                        {
                            c.Id,
                            c.Content,
                            UserFullName = c.User.FullName,
                            UserAvatar = string.IsNullOrEmpty(c.User.Image) ? "/images/default-avatar.png" : c.User.Image
                        }).ToList()
                    })
                    .ToList();

                return Json(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> GetChatHistory(string friendId)
        {
            try
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(currentUserId) || string.IsNullOrEmpty(friendId))
                {
                    return BadRequest("Thông tin không hợp lệ");
                }

                // Truy vấn tin nhắn 2 chiều giữa Mình và Bạn đó
                // Sắp xếp theo thời gian để tin nhắn cũ hiện trên, mới hiện dưới
                var messages = await _context.Messages
                    .Where(m => (m.FromUserId == currentUserId && m.ToUserId == friendId) ||
                                (m.FromUserId == friendId && m.ToUserId == currentUserId))
                    .OrderBy(m => m.Timestamp)
                    .Select(m => new {
                        fromId = m.FromUserId, // Để JS biết tin nhắn của ai (align trái/phải)
                        content = m.Content,
                        time = m.Timestamp.ToString("HH:mm dd/MM") // Format giờ đẹp
                    })
                    .ToListAsync();

                return Json(messages);
            }
            catch (Exception ex)
            {
                // Ghi log lỗi nếu cần thiết
                return StatusCode(500, "Lỗi server: " + ex.Message);
            }
        }
        [HttpPost]

        public async Task<IActionResult> SummarizePost([FromBody] SummarizeRequest req)
        {
            Console.WriteLine($"[SummarizePost] Nhận request tóm tắt cho PostId: {req?.PostId}");

            if (req == null || req.PostId <= 0)
            {
                Console.WriteLine("[SummarizePost] Request không hợp lệ.");
                return Json(new { success = false });
            }

            var post = await _context.Posts.FindAsync(req.PostId);
            if (post == null)
            {
                Console.WriteLine($"[SummarizePost] Không tìm thấy bài viết với PostId: {req.PostId}");
                return Json(new { success = false });
            }

            string summary = await _ai.Summarize(post.Content);
            Console.WriteLine($"[SummarizePost] Tóm tắt thành công cho PostId: {req.PostId}. Summary: {summary}");

            return Json(new { success = true, summary });
        }
        public class SummarizeRequest
        {
            public int PostId { get; set; }
        }
        [HttpPost]
        public IActionResult Like(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null)
                return Unauthorized();

            var post = _context.Posts.Find(id);
            if (post == null)
                return NotFound();

            // Kiểm tra user đã like chưa
            var existingLike = _context.PostLikes
                .FirstOrDefault(x => x.PostId == id && x.UserId == userId);

            bool isLiked = existingLike != null;

            if (isLiked)
            {
                // Đã like → bỏ like
                _context.PostLikes.Remove(existingLike);
                post.LikeCount = Math.Max(0, post.LikeCount - 1);
            }
            else
            {
                // Chưa like → thêm like
                _context.PostLikes.Add(new PostLike
                {
                    PostId = id,
                    UserId = userId
                });
                post.LikeCount += 1;
            }

            _context.SaveChanges();

            return Json(new
            {
                liked = !isLiked,
                likeCount = post.LikeCount
            });
        }
        [HttpPost]
        [ValidateAntiForgeryToken] // Bảo mật: Chống giả mạo request
        public async Task<IActionResult> Delete(int id)
        {
            // 1. Tìm bài viết cần xóa
            var post = await _context.Posts.FindAsync(id);

            if (post == null)
            {
                return NotFound(); // Lỗi 404 nếu không tìm thấy bài
            }

            // 2. KIỂM TRA QUYỀN (Rất quan trọng)
            // Phải đảm bảo người đang đăng nhập CÓ ĐÚNG LÀ chủ bài viết không
            var currentUserId = _userManager.GetUserId(User);
            if (post.UserId != currentUserId)
            {
                return Forbid(); // Hoặc Unauthorized() - Chặn nếu định xóa bài người khác
            }

            // 3. (Tùy chọn) Xóa ảnh trong thư mục wwwroot nếu có
            // Giúp dọn dẹp server cho nhẹ
            if (!string.IsNullOrEmpty(post.ImagePath))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, post.ImagePath.TrimStart('/'));
                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
            }

            // 4. Xóa khỏi Database
            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();

            // 5. Quay lại trang cũ (Trang bảng tin)
            return RedirectToAction("Index", "Post");
            // Mẹo: Nếu muốn quay lại đúng trang vừa đứng (ví dụ đang ở trang cá nhân), dùng dòng dưới:
            // return Redirect(Request.Headers["Referer"].ToString());
        }

    }
}
