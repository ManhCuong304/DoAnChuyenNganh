namespace DoAnCoSo.Models
{
    public class ChatMessagePayload
    {
        public string FromUserId { get; set; }
        public string ToUserId { get; set; }
        public string Content { get; set; }
    }
}
