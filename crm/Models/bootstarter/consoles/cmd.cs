using crm.Models.storage;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace bootstarter.Models.consoles
{
    public class cmd : IConsole
    {
        #region vars
        #endregion
        public cmd()
        {
        }
        #region public
        public void Startup(string filename)
        {
            Process.Start(filename);
        }
        #endregion
    }
}
