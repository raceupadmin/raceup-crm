using crm.Models.api.server;
using crm.Models.location;
using crm.WS;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crm.ViewModels.tabs.home.screens.location
{
    public class LocationOffice : LocationBase
    {
        #region vars

        #endregion
        #region properties
        LocationOfficeServer location;
        public LocationOfficeServer Location
        {
            get => location;
            set => this.RaiseAndSetIfChanged(ref location, value);
        }
        int id;
        public int Id
        {
            get => id;
            set => this.RaiseAndSetIfChanged(ref id, value);
        }
        string key;
        public string Key
        {
            get => key;
            set => this.RaiseAndSetIfChanged(ref key, value);
        }
        #endregion
        public LocationOffice(LocationOfficeServer location) : base() 
        {
            Location = location;
            Title = location.name;
            Id = location.id;
            Key = location.key;
        }
        #region override
        public override async void OnActivate()
        {
            base.OnActivate();
        }

        #endregion
    }
}
