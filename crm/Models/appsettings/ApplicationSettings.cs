using crm.Models.storage;
using Newtonsoft.Json;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crm.Models.appsettings
{
    public class ApplicationSettings : IApplicationSettings
    {
        #region vars
        IStorage<ApplicationSettings> storage;
        Paths paths = Paths.getInstance();
        #endregion

        #region properties
        [JsonProperty("Login")]
        public string Login { get; set; } = "";
        [JsonProperty("Password")]
        public string Password { get; set; } = "";
        [JsonProperty("RememberMe")]
        public bool RememberMe { get; set; } = false;
        [JsonProperty("CreativesPerPage")]
        public int CreativesPerPage { get; set; } = 200;
        [JsonProperty("CreativesPerDragDrop")]
        public int CreativesPerDragDrop { get; set; } = 1;
        #endregion

        public ApplicationSettings()
        {
            string filename = Path.Combine(paths.AppDir, "settings.json");
            storage = new Storage<ApplicationSettings>(filename, this);
        }

        #region public
        public void Load()
        {
            var t = storage.load();
            Login = t.Login;
            Password = t.Password;
            RememberMe = t.RememberMe;
            CreativesPerPage = t.CreativesPerPage;
            CreativesPerDragDrop = t.CreativesPerDragDrop;
        }

        public void Save()
        {
            storage.save(this);
        }
        #endregion        
    }
}
