using crm.Models.api.server;
using crm.Models.appcontext;
using crm.ViewModels.tabs.home.screens.creatives;
using crm.ViewModels.tabs.home.screens.geo;
using crm.WS;
using ReactiveUI;
using System.Reactive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using crm.ViewModels.dialogs;
using System.Diagnostics;
using crm.Models.creatives;
using crm.Models.storage;
using crm.Models.uniq;
using crm.ViewModels.tabs.home.screens.location;
using crm.Models.location;
using System.Collections.ObjectModel;
using System.Globalization;

namespace crm.ViewModels.tabs.home.screens
{
    public class GEO : BaseScreen
    {
        #region vars
        public IServerApi server;
        public string token;
        public IWindowService ws = WindowService.getInstance();
        public string SortByCode = "+code";
        public string SortByType = "+type_id";
        #endregion
        #region properties
        protected GEOContent content;
        virtual public GEOContent Content
        {
            get => content;
            set
            {
                this.RaiseAndSetIfChanged(ref content, value);
                content?.OnActivate();
            }
        }

        bool isMassActionsVisible;
        public bool IsMassActionsVisible
        {
            get => isMassActionsVisible;
            set => this.RaiseAndSetIfChanged(ref isMassActionsVisible, value);
        }

        bool isMassActionOpen;
        public bool IsMassActionOpen
        {
            get => isMassActionOpen;
            set => this.RaiseAndSetIfChanged(ref isMassActionOpen, value);
        }
        bool isMassActionShowPopup;
        public bool IsMassActionShowPopup
        {
            get => isMassActionShowPopup;
            set => this.RaiseAndSetIfChanged(ref isMassActionShowPopup, value);
        }

        bool isOfficeLocationVisible;
        public bool IsOfficeLocationVisible
        {
            get => isOfficeLocationVisible;
            set => this.RaiseAndSetIfChanged(ref isOfficeLocationVisible, value);
        }

        bool isOfficeLocationEnabled;
        public bool IsOfficeLocationEnabled
        {
            get => isOfficeLocationEnabled;
            set => this.RaiseAndSetIfChanged(ref isOfficeLocationEnabled, value);
        }
        public ObservableCollection<LocationOffice> LocationsCollection { get; } = new();

        LocationOffice office;
        public LocationOffice Office
        {
            get => office;
            set
            {
                this.RaiseAndSetIfChanged(ref office, value);
                IsOfficeLocationVisible = false;
                if (Content != null)
                {
                    Content.OfficeId = Office.Id;
                    Content.OnActivate();
                }
            }
        }

        #endregion
        #region commands
        public ReactiveCommand<Unit, Unit> newGeoCmd { get; }
        public ReactiveCommand<Unit, Unit> massActionCmd { get; }
        public ReactiveCommand<Unit, Unit> deselectAllCmd { get; }
        public ReactiveCommand<Unit, Unit> synchronizeAllCmd { get; }

        public ReactiveCommand<bool, Unit> setEnableCmd { get; }
        public ReactiveCommand<string?, Unit>? sortParameterCmd { get; }
        #endregion
        public GEO() : base()
        {
            server = AppContext.ServerApi;
            token = AppContext.User.Token;
            Office = AppContext.User.Location;
            IsOfficeLocationEnabled = AppContext.User.Roles.Any(x => x.Type == Models.user.RoleType.superadmin);


            #region commands
            newGeoCmd = ReactiveCommand.CreateFromTask(async () =>
            {

                
            });
            massActionCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                IsMassActionShowPopup = true;
            });

            deselectAllCmd = ReactiveCommand.Create(() =>
            {
                Content.IsAllChecked = false;
            });

            synchronizeAllCmd = ReactiveCommand.Create(() => {

                Content.OnActivate();
            });

            setEnableCmd = ReactiveCommand.CreateFromTask<bool>(async (enable) =>
            {
                Content.SetMassGeoEnable(enable);
                IsMassActionShowPopup = false;
            });
            sortParameterCmd = ReactiveCommand.Create<string?>((o) => {
                Content.SortKey = "+" + o.Replace("_",".");
                Content.OnActivate();

            });
            #endregion
        }
        public async void GetOfficeLocation()
        {
            try
            {
                List<LocationOfficeServer> locations = await server.GetLocationOfficeServer(token);
                foreach (var location in locations)
                {
                    bool found = LocationsCollection.Any(o => o.Key.Equals(location.key));
                    if (found)
                        continue;

                    var gp = new LocationOffice(location);
                    LocationsCollection.Add(gp);
                }

                if (Office == null)
                {
                    if (LocationsCollection.Count != 0)
                    {
                        Office = LocationsCollection.FirstOrDefault(t => t.Title.Equals(AppContext.User.OfficeTitle));
                    }
                    else
                    {
                        Office = AppContext.User.Location;
                    }
                }
            }
            catch (Exception ex)
            {
                IsOfficeLocationVisible = false;
                ws.ShowDialog(new errMsgVM(ex.Message));
            }
        }
        #region callbacks
        protected void GeoContent_GeoSelectionChangedEvent(int number)
        {
            updateMassActions(number);
        }
        #endregion
        #region helpers
        void updateMassActions(int checkedNumber)
        {
            IsMassActionsVisible = checkedNumber > 0;
        }
        #endregion
        #region override
        public override async void OnActivate()
        {
            base.OnActivate();
#if ONLINE
            GetOfficeLocation();
            try
            {
                Content = new GEOContent(Office.Id);
                Content.GeoSelectionChangedEvent += GeoContent_GeoSelectionChangedEvent;
            }
            catch (Exception ex)
            {
                ws.ShowDialog(new errMsgVM(ex.Message));
            }
#else           
#endif

        }
        #endregion
        public override string Title => "Геотеги";
    }
}
