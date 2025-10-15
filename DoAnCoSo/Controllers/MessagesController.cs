using DoAnCoSo.Services;
using Microsoft.AspNetCore.Mvc;

namespace DoAnCoSo.Controllers
{
    [Route("messages")]
    public class MessagesController : Controller
    {
        private readonly MessageService _messageService;

        public MessagesController(MessageService messageService)
        {
            _messageService = messageService;   
        }

        [HttpGet("GetMessages/{user1}/{user2}")]
        public async Task<IActionResult>GetMessages(string user1, string user2)
        {
            var messages = await _messageService.GetMessagesAsync(user1, user2);
            return Json(messages);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
