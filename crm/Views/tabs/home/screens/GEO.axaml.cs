using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using vm = crm.ViewModels.tabs.home.screens;

namespace crm.Views.tabs.home.screens
{
    public partial class GEO : UserControl
    {
        public GEO()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
