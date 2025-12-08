namespace Backend.Models
{
    public class Message(List<string> to, string subject, string content)
    {
        public List<string> To { get; set; } = to;
        public string Subject { get; set; } = subject;
        public string Content { get; set; } = content;
        
    }
}
