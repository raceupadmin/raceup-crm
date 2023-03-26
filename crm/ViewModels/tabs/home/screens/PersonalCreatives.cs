using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive;
using crm.ViewModels.dialogs;
using crm.ViewModels.tabs.home.screens.location;
using crm.Models.creatives;
using crm.Models.storage;
using crm.Models.uniq;
using crm.ViewModels.tabs.home.screens.creatives;

namespace crm.ViewModels.tabs.home.screens
{
    public class PersonalCreatives : Creatives
    {
        #region properties
        LocationOffice office;
        public LocationOffice Office
        {
            get => office;
            set
            {
                this.RaiseAndSetIfChanged(ref office, value);
            }
        }

        bool is_private = true;
        public bool IsPrivate { get => is_private; set => is_private = value; }

        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> newCreativeCmd { get; }
        #endregion
        public PersonalCreatives() : base()
        {
            Title = "Мои";
            Office = AppContext.User.Location;

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
            #endregion
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

            try
            {
                List<CreativeServerDirectory> dirs = await server.GetCreativeServerDirectories(token);

                foreach (var dir in dirs)
                {
                    bool found = GeoPages.Any(o => o.Title.Equals(dir.dir));
                    if (found)
                        continue;

                    var gp = new GeoPage(dir,IsPrivate, -1);
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
