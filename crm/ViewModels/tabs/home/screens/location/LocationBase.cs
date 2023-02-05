using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crm.ViewModels.tabs.home.screens.location
{
    public abstract class LocationBase : ViewModelBase
    {
        #region properties
        string title;
        public string Title 
        { 
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }
        #endregion
        #region public
        public LocationBase() { }

        public virtual void OnActivate() { }
        #endregion
    }
}
