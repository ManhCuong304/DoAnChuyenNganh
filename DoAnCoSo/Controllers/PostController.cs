// Controllers/PostController.cs
using Azure.Messaging;
using DoAnCoSo.Models;
using DoAnCoSo.Models.ViewModels;
using DoAnCoSo.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DoAnCoSo.Controllers
{
    [Authorize]
    public class PostController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly PostService _postService;

        public PostController(ApplicationDbContext context, IWebHostEnvironment env, PostService postService)
        {
            _context = context;
            _env = env;
            _postService = postService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var relevantPostIds = await _postService.GetRelevantPostIdsAsync(userId);
            var posts = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.Comments).ThenInclude(c => c.User)
                .OrderBy(p => !relevantPostIds.Contains(p.Id))
                .ThenByDescending(p => p.CreatedAt)           
                .ToListAsync();


            var mutualFriends = await _context.Users
                .Where(u => u.Id != userId) 
                .Take(5)
                .ToListAsync();

            

            var vm = new PostIndexViewModel
            {
                CurrentUserId = userId,
                Posts = posts,
                MutualFriends = mutualFriends,
                RelevantPostIds = relevantPostIds
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
        public async Task<IActionResult> GetSuggestedFriends()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var suggested = await _context.Users
                .Where(u => u.Id != userId)
                .Select(u => new {
                    u.Id,
                    u.FullName,
                    u.UserName,
                    u.Image
                })
                .Take(5)
                .ToListAsync();

            return Json(suggested);
        }
    }
}
