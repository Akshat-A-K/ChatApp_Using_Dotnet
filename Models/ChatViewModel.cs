namespace ChatApp.Models
{
    public class ChatViewModel
    {
        public string ReceiverUsername { get; set; }
        public string SenderId { get; set; }
        public string ReceiverId { get; set; }
        public List<Message> Messages { get; set; }
    }

}
