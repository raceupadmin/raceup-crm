using crm.Models.storage;
using bootstarter.Models.version;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bootstarter.Models.local
{
    public class LocalManager : ILocalManager
    {
        #region vars
        IPaths paths;
        #endregion
        public LocalManager(IPaths paths)
        {
            this.paths = paths;
        }

        #region public
        public string GetVersion()
        {
            string res = "0.0";             

            if (File.Exists(paths.VerPath)) {
                var str = File.ReadAllText(paths.VerPath);
                JObject json = JObject.Parse(str);
                string? version = json["version"]?.ToString();
                if (version != null)
                    res = version;
            }

            return res;
        }
        #endregion
    }
}
