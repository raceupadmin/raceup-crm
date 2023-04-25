using crm.ViewModels;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace crm.Models.geoservice
{
    public class GEODTO : ViewModelBase
    {
        #region properties             
        int id;
        [JsonProperty("id")]
        public int Id
        {
            get => id;
            set => this.RaiseAndSetIfChanged(ref id, value);
        }

        string code;
        [JsonProperty("code")]
        public string Code
        {
            get => code;
            set => this.RaiseAndSetIfChanged(ref code, value); 
        }

        int type_id;
        [JsonProperty("type_id")]
        public int TypeId
        {
            get => type_id;
            set => this.RaiseAndSetIfChanged(ref type_id, value);
        }

        string flow_type;
        [JsonProperty("flow_type")]
        public string FlowType
        {
            get => flow_type;
            set => this.RaiseAndSetIfChanged(ref flow_type, value);
        }

        string enabled_from;
        [JsonProperty("enabled_from")]
        public string EnableFrom
        {
            get => enabled_from;
            set => this.RaiseAndSetIfChanged(ref enabled_from, value);
        }

        string enabled_to;
        [JsonProperty("enabled_to")]
        public string EnableTo
        {
            get => enabled_to;
            set => this.RaiseAndSetIfChanged(ref enabled_to, value);
        }

        bool enabled;
        [JsonProperty("enabled")]
        public bool Enabled
        {
            get => enabled;
            set => this.RaiseAndSetIfChanged(ref enabled, value);
        }
        #endregion

        public GEODTO()
        {            
        }
    }
}
