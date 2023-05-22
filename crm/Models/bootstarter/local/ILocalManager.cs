using bootstarter.Models.version;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bootstarter.Models.local
{
    public interface ILocalManager
    {
        string GetVersion();
    }
}
