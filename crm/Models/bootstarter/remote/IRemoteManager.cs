using bootstarter.Models.version;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bootstarter.Models.remote
{
    public interface IRemoteManager
    {
        public Task<VersionFile> GetVersion();

        public event Action<double, double> ProgressChangedEvent;
    }
}
