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
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> cancelCmd { get; }
        public ReactiveCommand<Unit, Unit> acceptCmd { get; }
        #endregion

        public rolesDlgVM()
        {
            IsValidSelection = false;
            Tags = convetrer.GetAllTags();
        }

        public rolesDlgVM(ApplicationContext appcontext)
        {
            server = appcontext.ServerApi;
            token = appcontext.User.Token;

            Selection = new SelectionModel<tagsListItem>();
            Selection.SingleSelect = false;
            Selection.SelectionChanged += Selection_SelectionChanged;

            Tags = convetrer.GetAllTags();
            GetOfficeLocation();

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
                    newtoken = await server.GetNewUserToken(roles, token);

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
            bool isTeamLead = SelectedTags.Any(t => t.Name.Equals(Role.teamlead));
            bool isBuyer = SelectedTags.Any(t => t.Name.Equals(Role.buyer));

            bool isAnyOne = SelectedTags.Any(t =>
                !t.Name.Equals(Role.teamlead) &&
                !t.Name.Equals(Role.admin) &&
                !t.Name.Equals(Role.buyer) &&
                !t.Name.Equals(Role.creative) &&
                !t.Name.Equals(Role.financier));

            bool isCreative = SelectedTags.Any(t => t.Name.Equals(Role.creative));
            bool isFinancier = SelectedTags.Any(t => t.Name.Equals(Role.financier));

            IsValidSelection =
                (isAdmin && !isTeamLead && !isBuyer && !isAnyOne && !isCreative && !isFinancier) || //Адимн
                (isBuyer && !isTeamLead && isAnyOne) ||              //Байер чего-либо
                (isTeamLead && !isBuyer && isAnyOne) ||              //Тим-либ чего-либо
                (isCreative && !isAdmin && !isTeamLead && !isBuyer && !isAnyOne) ||
                (isFinancier && !isAdmin && !isTeamLead && !isBuyer && !isAnyOne);   

        }

        public async void GetOfficeLocation()
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
                Office = LocationsCollection[0];
        }
    }
}
