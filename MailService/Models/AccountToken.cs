﻿using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MailService.Models
{
    public class AccountToken
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string RefreshToken { get; set; }
        [Required]
        public DateTime RefreshExpires { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
    }
}
