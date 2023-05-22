using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using crm.Models.storage;
using Newtonsoft.Json;
using System.IO;

namespace bootstarter.Models.bootstarterpath
{
    public class BootStarterFileConfig
    {
        #region vars
        IStorage<BootStarterFileConfig> storage;
        string combine_filename;
        #endregion
        #region properties
        [JsonProperty("BootStarterPath")]
        public string BootStarterPath { get; set; } = "";
        #endregion
        public BootStarterFileConfig() {
            storage = null;
            BootStarterPath = "";
        }
        public BootStarterFileConfig(string app_path)
        {

            combine_filename = Path.Combine(app_path, "Bootstarter.json");
            if (File.Exists(combine_filename))
            {
                storage = new Storage<BootStarterFileConfig>(combine_filename, this);
            }
        }
        #region public
        public void Load()
        {
            if (storage != null)
            {
                var t = storage.load();
                BootStarterPath = t.BootStarterPath;
            }else
            {
                BootStarterPath = "";
            }
        }

        public void Save()
        {
            if (storage != null)
            {
                storage.save(this);
            }
        }
        #endregion
    }
}
