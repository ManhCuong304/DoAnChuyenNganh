using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using DoAnCoSo.Models;
using DoAnCoSo.Services;

namespace DoAnCoSo.Hubs
{
    public class ChatHub : Hub
    {
        private readonly MessageService _messageService;

        public ChatHub(MessageService messageService)
        {
            _messageService = messageService;
        }

        public async Task SendMessage(string fromUserId, string toUserId, string content)
        {
            var message = await _messageService.SaveMessageAsync(fromUserId, toUserId, content);

            await Clients.User(toUserId).SendAsync("ReceiveMessage", fromUserId, content, message.Timestamp);

            await Clients.User(fromUserId).SendAsync("ReceiveMessage", fromUserId, content, message.Timestamp);
        }
    }
}
