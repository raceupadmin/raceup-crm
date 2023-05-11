using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using crm.ViewModels.tabs.home.screens.creatives;
using ReactiveUI;
using crm.Models.api.server;
using crm.ViewModels.dialogs;
using System.Globalization;
using crm.WS;
using System.Diagnostics;
using Avalonia.Threading;
using crm.Models.creatives;
using crm.Models.geoservice;
using System.Reflection.Metadata;
using TextCopy;

namespace crm.ViewModels.tabs.home.screens.geo
{
    public class GEOContent : ViewModelBase
    {
        #region var
        IWindowService ws = WindowService.getInstance();
        public List<GEOItem> CheckedGeos = new();
        string token;
        IServerApi server;
        public string SortKey = "+code";
        #endregion
        #region properties
        string title;
        public string Title
        {
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }
        #endregion

        #region pagination
        const int displayed_lines_num1 = 20;

        int page = 1;
        public int SelectedPage
        {
            get => page;
            set
            {
                IsPrevActive = (value > 1);
                IsNextActive = (value < TotalPages);
                this.RaiseAndSetIfChanged(ref page, value);
            }
        }

        int totalPages = 1;
        public int TotalPages
        {
            get => totalPages;
            set
            {
                IsPrevActive = (SelectedPage > 1);
                IsNextActive = (SelectedPage < value || TotalPages == 0);
                this.RaiseAndSetIfChanged(ref totalPages, value);
            }
        }

        int pageSize;
        public int PageSize
        {
            get => AppContext.Settings.CreativesPerPage;
            //set => this.RaiseAndSetIfChanged(ref pageSize, value);
        }

        bool isNextActive = true;
        public bool IsNextActive
        {
            get => isNextActive;
            set
            {
                if (value && SelectedPage == TotalPages)
                    return;
                this.RaiseAndSetIfChanged(ref isNextActive, value);
            }
        }

        bool isPrevActive = true;
        public bool IsPrevActive
        {
            get => isPrevActive;
            set
            {
                if (value && SelectedPage == 1)
                    return;
                this.RaiseAndSetIfChanged(ref isPrevActive, value);
            }
        }

        string pageInfo;
        public string PageInfo
        {
            get => pageInfo;
            set => this.RaiseAndSetIfChanged(ref pageInfo, value);
        }

        public string getPageInfo(int page, int items_count, int total_users)
        {
            int displayed_lines_num = AppContext.Settings.CreativesPerPage;
            int p = (items_count < displayed_lines_num) ? (page - 1) * displayed_lines_num + items_count : page * displayed_lines_num;
            return $"{(page - 1) * displayed_lines_num + 1}-{p} из {total_users}";
        }
        int officeId;
        public int OfficeId
        {
            get => officeId;
            set => this.RaiseAndSetIfChanged(ref officeId, value);
        }

        public ObservableCollection<GEOItem> GeoList { get; set; } = new();

        bool needInvokeAllCheck { get; set; } = true;
        bool isAllChecked;
        public bool IsAllChecked
        {
            get => isAllChecked;
            set
            {
                if (needInvokeAllCheck)
                {
                    foreach (var item in GeoList)
                        item.IsChecked = value;

                    if (!value)
                    {
                        CheckedGeos.Clear();
                        //CreativesSelectionChangedEvent?.Invoke(0);
                    }
                }
                this.RaiseAndSetIfChanged(ref isAllChecked, value);
            }
        }

        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> nextPageCmd { get; }
        public ReactiveCommand<Unit, Unit> prevPageCmd { get; }
        #endregion
        #region events
        public event Action<int> GeoSelectionChangedEvent;
        #endregion

        #region public
        public GEOContent(int office_id) : base()
        {
            server = AppContext.ServerApi;
            token = AppContext.User.Token;

            OfficeId= office_id;

            SelectedPage = 1;
            #region commands
            prevPageCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                SelectedPage--;
                try
                {
                    //Users.Clear();
                    await updatePageInfo(SelectedPage, PageSize, SortKey);
                }
                catch (Exception ex)
                {
                    ws.ShowDialog(new errMsgVM(ex.Message));
                }
            });

            nextPageCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                SelectedPage++;
                try
                {
                    //Users.Clear();
                    await updatePageInfo(SelectedPage, PageSize, SortKey);
                }
                catch (Exception ex)
                {
                    ws.ShowDialog(new errMsgVM(ex.Message));
                }
            });

            #endregion
        }
        public async void OnActivate()
        {
            try
            {
                await updatePageInfo(SelectedPage, PageSize, SortKey);
            }
            catch (Exception ex)
            {
                ws.ShowDialog(new errMsgVM(ex.Message));
            }
        }
        public void SetMassGeoEnable(bool enable)
        {
            var geos = GeoList;
            Task.Run(async () =>
            {
                foreach (var geo in geos)
                {
                    if (geo.IsChecked)
                    {
                        try
                        {
                            geo.SetEnable(enable);
                            geo.IsChecked = false;
                        }
                        catch (Exception ex)
                        {
                            break;
                        }
                    }
                }
            });
        }
        
        #endregion
        #region helpers
        async Task updatePageInfo(int page, int pagesize, string sortkey)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                GeoList.Clear();
            });

#if ONLINE
            int total_pages = 0;
            int total_creatives = 0;
            List<GEODTO> crdtos;

            (crdtos, TotalPages, total_creatives) = AppContext.ServerApi.GetGeos(token, page - 1, pagesize, OfficeId, sortkey);

            PageInfo = getPageInfo(SelectedPage, crdtos.Count, total_creatives);

            IsPrevActive = false;
            IsNextActive = false;

            foreach (var cdt in crdtos)
            {
                GEOItem geo = new GEOItem(cdt);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    geo.CheckedEvent -= Geo_CheckedEvent;
                    geo.CheckedEvent += Geo_CheckedEvent;
                    geo.IsChecked = CheckedGeos.Any(u => u.Id.Equals(geo.Id)) || IsAllChecked;
                    GeoList.Add(geo);

                });
            }

            IsPrevActive = true;
            IsNextActive = true;
#else
#endif
        }
        #endregion
            #region private
        private void Geo_CheckedEvent(GEOItem geo, bool ischecked)
        {

            if (!ischecked && IsAllChecked)
            {
                needInvokeAllCheck = false;
                IsAllChecked = false;
                needInvokeAllCheck = true;
            }

            var found = CheckedGeos.FirstOrDefault(o => o.Id.Equals(geo.Id));

            if (ischecked)
            {
                if (found == null)
                    CheckedGeos.Add(geo);
            }
            else
            {
                if (found != null)
                    CheckedGeos.Remove(found);
            }

            GeoSelectionChangedEvent?.Invoke(CheckedGeos.Count);
        }
        #endregion
    }
}
