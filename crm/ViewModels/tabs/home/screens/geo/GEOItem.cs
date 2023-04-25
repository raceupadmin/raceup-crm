using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using crm.Models.geoservice;
using crm.ViewModels.tabs.home.screens.creatives;
using ReactiveUI;
using System.Reactive;
using crm.Models.api.server;
using crm.ViewModels.dialogs;
using Fizzler;
using crm.Models.creatives;
using crm.Models.storage;
using crm.Models.uniq;
using crm.WS;
using ExCSS;

namespace crm.ViewModels.tabs.home.screens.geo
{
    public class GEOItem : ViewModelBase
    {
        #region vars
        IServerApi serverApi;
        IWindowService ws = WindowService.getInstance();
        string token;
        #endregion
        #region properties
        bool isChecked;
        public bool IsChecked
        {
            get => isChecked;
            set
            {
                if (!IsReadOnly)
                {
                    this.RaiseAndSetIfChanged(ref isChecked, value);
                    CheckedEvent?.Invoke(this, value);
                }
            }
        }
        int id;
        public int Id
        {
            get => id;
            set => this.RaiseAndSetIfChanged(ref id, value);
        }

        string code;
        public string Code
        {
            get => code;
            set => this.RaiseAndSetIfChanged(ref code, value);
        }

        int type_id;
        public int TypeId
        {
            get => type_id;
            set => this.RaiseAndSetIfChanged(ref type_id, value);
        }

        string flow_type;
        public string FlowType
        {
            get => flow_type;
            set => this.RaiseAndSetIfChanged(ref flow_type, value);
        }

        string enabled_from;
        public string EnableFrom
        {
            get => enabled_from;
            set => this.RaiseAndSetIfChanged(ref enabled_from, value);
        }

        string enabled_to;
        public string EnableTo
        {
            get => enabled_to;
            set => this.RaiseAndSetIfChanged(ref enabled_to, value);
        }

        bool enabled;
        public bool Enabled
        {
            get => enabled;
            set
            {
                if (!IsReadOnly)
                {
                    this.RaiseAndSetIfChanged(ref enabled, value);
                }
            }
        }

        bool is_read_only;
        public bool IsReadOnly
        {
            get => is_read_only;
            set
            {
                this.RaiseAndSetIfChanged(ref is_read_only, value);
            }
        }
        #endregion
        #region commands
        public ReactiveCommand<Unit, Unit> setEnableCmd { get; }
        #endregion
        #region public
        public GEOItem(GEODTO geo) : base() 
        {
            token = AppContext.User.Token;
            serverApi = AppContext.ServerApi;

            Id = geo.Id;
            Code = geo.Code;
            TypeId = geo.TypeId;
            FlowType = geo.FlowType;
            enabled = geo.Enabled;
            EnableFrom = geo.EnableFrom;
            EnableTo = geo.EnableTo;

            IsReadOnly = !AppContext.User.Roles.Any(x => (x.Type == Models.user.RoleType.superadmin)
                                                         || (x.Type == Models.user.RoleType.admin));

            #region command
            setEnableCmd = ReactiveCommand.Create(() =>
            {

                Task.Run(async () =>
                {
                    try
                    {
                        await serverApi.SetEnableGeo(token, Id, Enabled);
                    }
                    catch (Exception ex)
                    {
                        Enabled = !Enabled;
                        ws.ShowDialog(new errMsgVM(ex.Message));
                    }
                });

            });
            #endregion

        }
        public void SetEnable(bool enabled)
        {
            Enabled = enabled;
            Task.Run(async () =>
            {
                try
                {
                    await serverApi.SetEnableGeo(token, Id, Enabled);
                }
                catch (Exception ex)
                {
                }
            });
        }
        #endregion
        #region callbacks
        public event Action<GEOItem, bool> CheckedEvent;
        #endregion
    }
}
