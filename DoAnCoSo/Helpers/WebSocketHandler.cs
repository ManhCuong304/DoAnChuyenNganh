using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using DoAnCoSo.Services;
using Microsoft.AspNetCore.Http;

namespace DoAnCoSo.Helpers
{
    public class ChatMessagePayload
    {
        public string ToId { get; set; }
        public string Message { get; set; }
    }

    public static class WebSocketHandler
    {
        private static readonly Dictionary<string, WebSocket> _userSockets = new();

        public static async Task Handle(HttpContext context, WebSocket socket, MessageService messageService)
        {
            var userId = context.Request.Query["userId"].ToString();
            if (string.IsNullOrEmpty(userId))
            {
                Console.WriteLine("❌ userId bị thiếu.");
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Thiếu userId", CancellationToken.None);
                return;
            }

            Console.WriteLine($"🟢 {userId} đã kết nối WebSocket");
            _userSockets[userId] = socket;

            var buffer = new byte[1024 * 4];

            try
            {
                while (true)
                {
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.CloseStatus.HasValue)
                        break;

                    var messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Console.WriteLine($"📥 Dữ liệu JSON nhận được: {messageJson}");

                    try
                    {
                        var messageData = JsonSerializer.Deserialize<ChatMessagePayload>(messageJson, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (messageData == null || string.IsNullOrEmpty(messageData.ToId) || string.IsNullOrEmpty(messageData.Message))
                        {
                            Console.WriteLine($"⚠️ Dữ liệu không hợp lệ: ToId={messageData?.ToId}, Message={messageData?.Message}");
                            continue;
                        }

                        Console.WriteLine($"📨 {userId} gửi đến {messageData.ToId}: {messageData.Message}");

                        // ✅ Lưu tin nhắn vào DB nhưng không chặn việc gửi WebSocket
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                await messageService.SaveMessageAsync(userId, messageData.ToId, messageData.Message);
                            }
                            catch (Exception saveEx)
                            {
                                Console.WriteLine($"❌ Lỗi khi lưu tin nhắn vào DB: {saveEx.Message}");
                            }
                        });

                        var formattedMessage = JsonSerializer.Serialize(new
                        {
                            fromId = userId,
                            message = messageData.Message
                        });

                        if (_userSockets.TryGetValue(messageData.ToId, out var receiverSocket) && receiverSocket.State == WebSocketState.Open)
                        {
                            var sendBuffer = Encoding.UTF8.GetBytes(formattedMessage);
                            await receiverSocket.SendAsync(
                                new ArraySegment<byte>(sendBuffer),
                                WebSocketMessageType.Text,
                                true,
                                CancellationToken.None
                            );
                            Console.WriteLine($"✅ Tin nhắn đã gửi đến {messageData.ToId}");
                        }
                        else
                        {
                            Console.WriteLine($"⚠️ Không tìm thấy hoặc socket đã đóng của {messageData.ToId}");
                            Console.WriteLine("🧾 Danh sách user đang kết nối:");
                            foreach (var kv in _userSockets)
                                Console.WriteLine($"- {kv.Key} : {kv.Value.State}");
                        }
                    }
                    catch (JsonException ex)
                    {
                        Console.WriteLine($"❌ Lỗi giải mã JSON: {ex.Message}. JSON gốc: {messageJson}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Lỗi xử lý tin nhắn: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi kết nối với {userId}: {ex.Message}\nStackTrace: {ex.StackTrace}");
            }

            Console.WriteLine($"🔴 Ngắt kết nối: {userId}");
            _userSockets.Remove(userId);
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Ngắt kết nối", CancellationToken.None);
        }
    }
}
