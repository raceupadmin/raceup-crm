using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Mixins;
using Avalonia.Markup.Xaml;
using bootstarter.Models.consoles;
using bootstarter.Models.local;
using bootstarter.Models.remote;
using bootstarter.Models.version;
using crm.Models.storage;
using crm.ViewModels;
using crm.Views;
using crm.WS;
using System.Runtime.InteropServices;
using System;
using crm.Models.bootstarter.prelaunch;

namespace crm
{
    public partial class App : Application
    {

        WindowService ws = WindowService.getInstance();

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {

                #region prelaunch
                Prelaunch pre_start = new Prelaunch();
                pre_start.OnStart();
                #endregion

                mainVM main = new mainVM();
                ws.ShowWindow(main);

                //desktop.MainWindow = new MainWindow
                //{
                //    DataContext = new mainVM(),
                //};
            }

            base.OnFrameworkInitializationCompleted();
        }
    }


}
