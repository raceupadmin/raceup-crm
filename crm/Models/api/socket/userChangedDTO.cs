﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace crm.Models.api.socket
{
    public class userChangedDTO
    {
        [JsonPropertyName("user_id")]
        public string user_id { get; set; }
    }
}
