using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crm.Models.user
{
    public enum RoleType : int
    {
        admin = 1,
        user,
        developer,
        buyer = 8,
        link = 9,
        сloser = 10,
        creative = 12,
        superadmin = 14
    }

    public class Role
    {
        public const string admin = "Админ";
        public const string creative = "Креативщик";
        public const string buyer = "Баер";
        public const string link = "Связка";
        public const string сloser = "Обработчик";
        public const string developer = "Разработчик";
        public const string superadmin = "Супер-Админ";

        
        int id;
        [JsonProperty("id")]
        public int Id { 
            get => id;
            set
            {
                id = value;
                Type = (RoleType)id;
            }
        }
        //[JsonProperty("name")]
        [JsonIgnore]
        public string Name { get; set; }
        
        RoleType type;
        [JsonIgnore]
        public RoleType Type {
            get => type;
            set
            {
                type = value;
                //Id = (int)type;
            }
        }

        public Role(RoleType type)
        {
            Type = type;
            Id = (int)type;
            Name = type.ToString();
        }

        public Role(int id)
        {
            Type = (RoleType)id;
            Id = id;
            Name = Type.ToString();
        }

        public Role()
        {
        }    
    }

}
