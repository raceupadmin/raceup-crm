using Avalonia.Controls.Selection;
using crm.Models.user;
using crm.ViewModels.Helpers;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace crm.ViewModels.dialogs
{    
    public class tagsDlgVM : BaseDialog
    {
        #region vars
        TagsAndRolesConvetrer convetrer = new();
        #endregion

        #region properties        
        public ObservableCollection<tagsListItem> Tags { get; } = new ObservableCollection<tagsListItem>();
        public List<tagsListItem> SelectedTags { get; }
        public SelectionModel<tagsListItem> Selection { get; set; }
        #endregion

        public tagsDlgVM()
        {
            Tags = convetrer.GetAllTags();
            Selection = new SelectionModel<tagsListItem>();
            Selection.SingleSelect = false;
        }

        public tagsDlgVM(List<Role> roles)
        {
            Tags = convetrer.GetAllTags();
            Selection = new SelectionModel<tagsListItem>();
            Selection.SingleSelect = false;
            SelectedTags = convetrer.RolesToTags(roles);
            foreach (var item in SelectedTags)
            {
                int index = Tags.IndexOf(Tags.FirstOrDefault(t => t.Name.Equals(item.Name)));
                Selection.Select(index);
            }
        }
    }
}
