﻿namespace MailService.DTO
{
    public class SendMessageRequest
    {
        public string Address { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}
