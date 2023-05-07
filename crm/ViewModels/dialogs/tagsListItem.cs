using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crm.ViewModels.dialogs
{
    public class tagsListItem : ViewModelBase
    {
        string name;
        public string Name
        {
            get => name;
            set => this.RaiseAndSetIfChanged(ref name, value);
        }
        bool changeable;
        public bool Changeable
        {
            get => changeable;
            set => this.RaiseAndSetIfChanged(ref changeable, value);
        }

        public tagsListItem(string name)
        {
            Name = name;
            Changeable= true;
        }

        public tagsListItem(string name, bool changeable)
        {
            Name = name;
            Changeable= changeable;
        }

    }
}
