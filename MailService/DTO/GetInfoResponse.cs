namespace MailService.DTO
{
    public class GetInfoResponse
    {
        public int Sent_total { get; set; }
        public int Delivered_total { get; set; }
        public int Sent_today { get; set; }
        public int Delivered_today { get; set; }
        public int Left_today { get; set; }
    }
}
