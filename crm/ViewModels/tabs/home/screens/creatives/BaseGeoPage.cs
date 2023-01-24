﻿using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace crm.ViewModels.tabs.home.screens.creatives
{
    public abstract class BaseGeoPage : ViewModelBase
    {
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
            set {
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
        #endregion

        public BaseGeoPage()
        {
        }

        #region public        
        public virtual void OnActivate() { }
        #endregion
    }
}
