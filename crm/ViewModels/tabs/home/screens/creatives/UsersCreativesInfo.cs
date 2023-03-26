using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using crm.Models.user;

namespace crm.ViewModels.tabs.home.screens.creatives
{

    public class UsersCreativesInfo : ViewModelBase
    {
        #region properties
        string title;
        public string Title
        {
            get => title; 
            set => this.RaiseAndSetIfChanged(ref title, value);
        }
        string letterId;
        public string LetterId
        {
            get => letterId;
            set => this.RaiseAndSetIfChanged(ref letterId, value);
        }
        string firstName;
        public string FirstName
        {
            get => firstName;
            set => this.RaiseAndSetIfChanged(ref firstName, value);
        }
        string lastName;
        public string LastName
        {
            get => lastName;
            set => this.RaiseAndSetIfChanged(ref lastName, value);
        }
        int officeId;
        public int OfficeId
        {
            get => officeId; 
            set => this.RaiseAndSetIfChanged(ref officeId, value);
        }
        string userId;
        public string UserId
        {
            get => userId; 
            set => this.RaiseAndSetIfChanged(ref userId, value);
        }
        bool isCommon = false;
        public bool IsCommon
        {
            get => isCommon;
            set => this.RaiseAndSetIfChanged(ref isCommon, value);
        }


        #endregion
        #region public
        public UsersCreativesInfo(User user) 
        {
            UserId = user.Id;
            LetterId = user.Litera;
            FirstName = user.FirstName;
            LastName = user.LastName;
            Title = LetterId + " - " + FirstName + " " + LastName;
            OfficeId = user.OfficeId;
        }

        public UsersCreativesInfo()
        {
            Title = "Общие";
            IsCommon = true;
            UserId = "";
        }
        #endregion
    }
}
