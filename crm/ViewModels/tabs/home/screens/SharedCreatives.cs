using crm.Models.appcontext;
using crm.Models.location;
using crm.ViewModels.dialogs;
using crm.ViewModels.tabs.home.screens.location;
using Fizzler;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using crm.Models.creatives;
using crm.Models.storage;
using crm.Models.uniq;
using crm.ViewModels.tabs.home.screens.creatives;
using System.Globalization;
using crm.Models.user;
using Avalonia.Threading;
using crm.ViewModels.tabs.home.menu.items;
using System.Reflection.Metadata;
using System.Diagnostics;

namespace crm.ViewModels.tabs.home.screens
{
    public class SharedCreatives : Creatives
    {
        #region properties
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

        bool isSharedCreativesVisible;
        public bool IsSharedCreativesVisible
        {
            get => isSharedCreativesVisible;
            set => this.RaiseAndSetIfChanged(ref isSharedCreativesVisible, value);
        }

        bool isSharedCreativesEnabled;
        public bool IsSharedCreativesEnabled
        {
            get => isSharedCreativesEnabled;
            set => this.RaiseAndSetIfChanged(ref isSharedCreativesEnabled, value);
        }

        public override GeoPage Content
        {
            get => content;
            set
            {
                this.RaiseAndSetIfChanged(ref content, value);
                IsServerDirectoriesVisible = false;
                ChangeParametr();
            }
        }

        LocationOffice office;
        public LocationOffice Office
        {
            get => office;
            set
            {
                this.RaiseAndSetIfChanged(ref office, value);
                IsOfficeLocationVisible = false;
                if(Content != null)
                {
                    Content.OfficeId = Office.Id;
                    CreateUsersCreativeList();
                }
            }
        }
        public ObservableCollection<LocationOffice> LocationsCollection { get; } = new();

        UsersCreativesInfo userCreatives;
        public UsersCreativesInfo UserCreatives
        {
            get => userCreatives;
            set
            {
                this.RaiseAndSetIfChanged(ref userCreatives, value);
                IsSharedCreativesVisible = false;
                ChangeParametr();
            }
        }
        public ObservableCollection<UsersCreativesInfo> UserCreativesCollection { get; } = new();

        bool is_private = false;
        public bool IsPrivate { get => is_private; set => is_private = value; }
        
        string SortKey = "-enabled,-is_connected";
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> newCreativeCmd { get; }
        public ReactiveCommand<Unit, Unit> setInvisibleDirCmd { get; }
        #endregion

        #region private
        private void ChangeParametr()
        {
            if(content != null)
            {
                if(Office != null)
                    content.OfficeId = Office.Id;
                if(UserCreatives != null)
                {
                    IsPrivate = !UserCreatives.IsCommon;
                    content.UserId = UserCreatives.UserId;
                    content.IsPrivate = IsPrivate;
                    content.LetterId = UserCreatives.LetterId;
                }
                content.OnActivate();
            }
        }

        private void CreateUsersCreativeList()
        {
            UserCreativesCollection.Clear();
            UserCreativesCollection.Add(new UsersCreativesInfo());
            UserCreatives = UserCreativesCollection[0];
            if(userInfoCollection.Count > 0)
            {
                foreach(var user in userInfoCollection)
                {
                    if (user.OfficeId == Office.Id)
                    {
                        UserCreativesCollection.Add(user);
                    }
                }
            }
        }

        private ObservableCollection<UsersCreativesInfo> userInfoCollection { get; } = new();
        #endregion

        public SharedCreatives() : base()
        {
            Title = "Общие";
            IsOfficeLocationEnabled = AppContext.User.Roles.Any(x => x.Type == Models.user.RoleType.superadmin);
            IsSharedCreativesEnabled = AppContext.User.Roles.Any(x => (x.Type == Models.user.RoleType.superadmin)
                                                                      ||(x.Type == Models.user.RoleType.admin));

            IsPrivate = AppContext.User.Roles.Any(x => x.Type == Models.user.RoleType.superadmin);
            IsPrivate = false;

            Office = AppContext.User.Location;
            UserCreatives = new();
            #region commands
            newCreativeCmd = ReactiveCommand.CreateFromTask(async () =>
            {

                string[] files = await ws.ShowFileDialog("Выберите креатив");
                if (files != null && files.Length > 0)
                {
                    var dlg = new creativeUploadDlgVM()
                    {
                        Files = files,
                        CreativeServerDirectory = Content.CreativeServerDirectory,
                        OfficeId = Office.Id,
                        IsPrivate = IsPrivate
                    };

                    ws.ShowModalWindow(dlg);

                    try
                    {
                        Content.ToogleUpdate(false);
                        await dlg.RunFilesUploadAsync();
                        Content.ToogleUpdate(true);

                    }
                    catch (Exception ex)
                    {
                        ws.ShowDialog(new errMsgVM(ex.Message));
                    }
                }
            });
            setInvisibleDirCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                Debug.WriteLine("Set Invisible");
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

        public async void GetUsers()
        {
            List<User> users;
            int total_users;
            int total_pages;
            (users, total_pages, total_users) = await server.GetUsers(0, 100, token, SortKey);
            userInfoCollection.Clear();
            if (total_users > 0)
            {
                foreach (var user in users)
                {
                    if (user.Roles.Any(x => x.Type == Models.user.RoleType.buyer))
                    {
                        var tmp = new UsersCreativesInfo(user);
                        userInfoCollection.Add(tmp);
                    }
                }
            }
        }
        

        #region override
        public override async void OnActivate()
        {
            base.OnActivate();
            //var dlg = new progressDlgVM();
            //ws.ShowModalWindow(dlg);

            await Uniqalizer.Init(Paths.getInstance().CodecBinariesPath, (progress) =>
            {
                //dlg.Progress = progress;
            });

#if ONLINE
            GetOfficeLocation();
            GetUsers();
            CreateUsersCreativeList();
            try
            {
                List<CreativeServerDirectory> dirs = await server.GetCreativeServerDirectories(token);

                foreach (var dir in dirs)
                {
                    bool found = GeoPages.Any(o => o.Title.Equals(dir.dir));
                    if (found)
                        continue;

                    var gp = new GeoPage(dir, IsPrivate, Office.Id, UserCreatives.UserId);
                    gp.CreativesSelectionChangedEvent += GeoPage_CreativesSelectionChangedEvent;
                    GeoPages.Add(gp);
                }

                if (Content == null)
                    Content = GeoPages[0];
                else
                    Content.OnActivate();

            }
            catch (Exception ex)
            {
                ws.ShowDialog(new errMsgVM(ex.Message));
            }
#else           
#endif

        }
        #endregion
    }
}
