using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crm.Models.location
{
    public class LocationOfficeServer
    {
        int id_;
        [JsonProperty("id")]
        public int id { get; set; }
        string? name_;
        [JsonProperty("name")]
        public string name 
        { 
            get => name_;
            set
            {
                name_ = value;
                key = value;
            } 
        }
        string? key_;
        public string key 
        {
            get => key_;
            set => key_ = value;
        }

    }
}
