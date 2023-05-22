using crm.Models.storage;
using bootstarter.Models.version;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace bootstarter.Models.remote
{
    public class RemoteManager : IRemoteManager
    {
        #region vars
        IPaths paths;        
        WebClient webClient;
        #endregion
        public RemoteManager(IPaths paths)
        {
            this.paths = paths;            
            webClient = new WebClient();
            webClient.DownloadProgressChanged += (sender, arg) => {

                double total = arg.TotalBytesToReceive;
                double received = arg.BytesReceived;

                ProgressChangedEvent?.Invoke(received, total);
            };
        }

        #region public
        public async Task<VersionFile> GetVersion()
        {

            VersionFile res = new VersionFile();

            string tmpVerPath = Path.Combine(paths.TmpDir, "version.json");
            if (File.Exists(tmpVerPath))
                File.Delete(tmpVerPath);
            await webClient.DownloadFileTaskAsync(new System.Uri(paths.VerURL), tmpVerPath);
            var str = File.ReadAllText(tmpVerPath);
            JObject json = JObject.Parse(str);
            string? version = json["version"]?.ToString();
            if (version == null)
                throw new Exception("Не удалось получить версию обновления");

            res.Version = version;
            res.File = str;

            return res;
        }
        #endregion

        #region callbacks
        public event Action<double, double> ProgressChangedEvent;
        #endregion

    }

}
