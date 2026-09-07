namespace ERP.Models
{
    public class Mail
    {
        public int MailId { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public DateTime SentDate { get; set; }

        public User? Sender { get; set; }
        public User? Receiver { get; set; }
    }
}
