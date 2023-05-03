using Avalonia.Controls.Selection;
using crm.Models.api.server;
using crm.Models.appcontext;
using crm.Models.location;
using crm.Models.user;
using crm.ViewModels.Helpers;
using crm.ViewModels.tabs.home.screens.creatives;
using crm.ViewModels.tabs.home.screens.location;
using crm.WS;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using TextCopy;

namespace crm.ViewModels.dialogs
{
    public class rolesDlgVM : BaseDialog
    {
        #region vars
        IWindowService ws = WindowService.getInstance();
        IServerApi server;
        string token;
        TagsAndRolesConvetrer convetrer = new();
        #endregion

        #region properties
        bool isValidSelection = false;
        public bool IsValidSelection
        {
            get => isValidSelection;
            set => this.RaiseAndSetIfChanged(ref isValidSelection, value);
        }
        public ObservableCollection<tagsListItem> Tags { get; } = new();
        public List<tagsListItem> SelectedTags { get; } = new();
        public SelectionModel<tagsListItem> Selection { get; }
        LocationOffice office;
        public LocationOffice Office
        {
            get => office;
            set
            {
                this.RaiseAndSetIfChanged(ref office, value);
                IsLocationOfficeVisible = false;
            }
        }
        public ObservableCollection<LocationOffice> LocationsCollection { get; } = new();
        bool isLocationOfficeVisible;
        public bool IsLocationOfficeVisible
        {
            get => isLocationOfficeVisible;
            set => this.RaiseAndSetIfChanged(ref isLocationOfficeVisible, value);
        }
        bool isLocationButtonVisible;
        public bool IsLocationButtonVisible
        {
            get => isLocationButtonVisible;
            set => this.RaiseAndSetIfChanged(ref isLocationButtonVisible, value);
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> cancelCmd { get; }
        public ReactiveCommand<Unit, Unit> acceptCmd { get; }
        #endregion

        public rolesDlgVM()
        {
            IsValidSelection = false;
            Tags = convetrer.GetAllTags(false);
        }

        public rolesDlgVM(ApplicationContext appcontext)
        {
            server = appcontext.ServerApi;
            token = appcontext.User.Token;

            Selection = new SelectionModel<tagsListItem>();
            Selection.SingleSelect = false;
            Selection.SelectionChanged += Selection_SelectionChanged;

            Tags = convetrer.GetAllTags(false);

            IsLocationButtonVisible = appcontext.User.Roles.Any(x => x.Type == Models.user.RoleType.superadmin) ? true : false;
            GetOfficeLocation(appcontext);

            #region commands    
            cancelCmd = ReactiveCommand.Create(() =>
            {
                OnCloseRequest();
            });

            acceptCmd = ReactiveCommand.CreateFromTask(async () =>
            {

                var roles = convetrer.TagsToRoles(SelectedTags);

                string newtoken = "";
                try
                {
                    newtoken = await server.GetNewUserToken(roles, Office.Id, token);

                    OnCloseRequest();

                }
                catch (Exception ex)
                {
                    ws.ShowDialog(new errMsgVM(ex.Message));
                }

                Clipboard clipboard = new Clipboard();
                clipboard.SetText(newtoken);
                appcontext.BottomPopup.Show("Токен скопирован");

            });
            #endregion
        }

        private void Selection_SelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs<tagsListItem> e)
        {

            foreach (var item in e.SelectedItems)
            {
                SelectedTags.Add(item);
            }

            foreach (var item in e.DeselectedItems)
            {
                SelectedTags.Remove(item);
            }

            bool isAdmin = SelectedTags.Any(t => t.Name.Equals(Role.admin));

            bool isAnyOne = SelectedTags.Any(t =>
                !t.Name.Equals(Role.admin));
            bool isOneSelected = SelectedTags.Count() == 1;

            IsValidSelection =
                (isAdmin) || (isAdmin && isAnyOne) || isOneSelected;   

        }

        public async void GetOfficeLocation(ApplicationContext appcontext)
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
                    Office = LocationsCollection.FirstOrDefault(t => t.Title.Equals(appcontext.User.OfficeTitle));
                }
            }
            catch (Exception ex)
            {
                IsLocationButtonVisible = false;
                ws.ShowDialog(new errMsgVM(ex.Message));
            }
        }
    }
}
