﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MailService.DTO
{
    public class SignInRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
