using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EPG.Samples.API.Models
{
    public class MessageBusSend
    {
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
