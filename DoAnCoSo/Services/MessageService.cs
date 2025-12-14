using DoAnCoSo.Models;
using Microsoft.EntityFrameworkCore;

namespace DoAnCoSo.Services
{
    public class MessageService
    {
        private readonly ApplicationDbContext _context;
        public MessageService(ApplicationDbContext context)
        {
            _context = context; 
        }

        public async Task<Message> SaveMessageAsync(string fromUserId, string toUserId, string content)
        {
            var message = new Message
            {
                FromUserId = fromUserId,
                ToUserId = toUserId,
                Content = content,
                Timestamp = DateTime.Now
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return message;
        }
        public async Task<List<Message>> GetMessagesAsync(string user1, string user2)
        {
            return await _context.Messages
                .Where(m =>
                    (m.FromUserId == user1 && m.ToUserId == user2) ||
                    (m.FromUserId == user2 && m.ToUserId == user1))
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }
    }
}
