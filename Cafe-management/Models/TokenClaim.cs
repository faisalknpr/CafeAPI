using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Cafe_management.Models
{
    public class TokenClaim
    {
        public string Email { get; set; }
        public string Role { get; set; }
    }
}