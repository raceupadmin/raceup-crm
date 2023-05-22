using Avalonia.X11;
using bootstarter.Models.bootstarterpath;
using bootstarter.Models.consoles;
using bootstarter.Models.local;
using bootstarter.Models.remote;
using bootstarter.Models.version;
using crm.Models.storage;
using crm.ViewModels.dialogs;
using crm.WS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace crm.Models.bootstarter.prelaunch
{
    public class Prelaunch
    {
        #region vars
        IPaths paths;
        IRemoteManager remoteManager;
        ILocalManager localManager;
        IConsole console;
        WindowService ws = WindowService.getInstance();
        #endregion

        public Prelaunch()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                paths = Paths.getInstance();
                console = new bash();
            }
            else
            {
                paths = Paths.getInstance();
                console = new cmd();
            }
            remoteManager = new RemoteManager(paths);
            localManager = new LocalManager(paths);

        }

        public async Task OnStart()
        {
            string bootstarter_path = "";
            try
            {
                VersionFile remoteVersion = await remoteManager.GetVersion();
                string localVersion = localManager.GetVersion();

                if (!remoteVersion.Version.Equals(localVersion))
                {
                    BootStarterFileConfig bs_config = new BootStarterFileConfig(paths.AppDir);
                    bs_config.Load();
                    bootstarter_path = bs_config.BootStarterPath;
                    if (string.IsNullOrEmpty(bootstarter_path))
                    {
                        throw new Exception("Не удалось найти загрузчик");
                    }
                    try
                    {
                        ws.ShowDialog(new msgDlgVM("Запуск загрузчика"));
                        await Task.Run(() =>
                        {
                            Thread.Sleep(2000);
                        });
                        console.Startup(bootstarter_path);
                        Process.GetCurrentProcess().Kill();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Не удалось запустить загрузчик");
                    }
                }
            }
            catch (Exception ex)
            {
                ws.ShowDialog(new errMsgVM(ex.Message));
            }
        }
    }
}
