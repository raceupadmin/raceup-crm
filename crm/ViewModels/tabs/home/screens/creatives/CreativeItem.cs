﻿using Avalonia.Media.Imaging;
using crm.Models.api.server;
using crm.Models.creatives;
using crm.Models.storage;
using crm.Models.uniq;
using crm.ViewModels.dialogs;
using crm.WS;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using geo = crm.Models.geoservice;

namespace crm.ViewModels.tabs.home.screens.creatives
{
    public class CreativeItem : ViewModelBase, ICreative
    {
        #region vars
        ICreativesRemoteManager remoteManager;
        ICreativesLocalManager localManager;
        IUniqalizer uniqalizer;
        ICreativePreviewer previewer;
        IPaths paths = Paths.getInstance();
        bool isSynchronizing = false;
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
                this.RaiseAndSetIfChanged(ref isChecked, value);
                CheckedEvent?.Invoke(this, value);
            }
        }

        int uniques = 0;
        public int Uniques
        {
            get => uniques;
            set => this.RaiseAndSetIfChanged(ref uniques, value);
        }

        int progress;
        public int Progress
        {
            get => progress;
            set => this.RaiseAndSetIfChanged(ref progress, value);
        }

        bool isSynchronized;
        public bool IsSynchronized
        {
            get => isSynchronized;
            set => this.RaiseAndSetIfChanged(ref isSynchronized, value);
        }
        public CreativeType Type { get; set; }
        public int Id { get; set; }

        bool isVisible;
        public bool IsVisible
        {
            get => isVisible;
            set => this.RaiseAndSetIfChanged(ref isVisible, value);
        }

        Bitmap preview;
        public Bitmap Preview
        {
            get => preview;
            set => this.RaiseAndSetIfChanged(ref preview, value);
        }

        Bitmap fastView;
        public Bitmap FastView
        {
            get => fastView;
            set => this.RaiseAndSetIfChanged(ref fastView, value);
        }

        public CreativeServerDirectory ServerDirectory { get; set; }
        public string Name { get; set; }
        public string FileName { get; set; }
        public string LocalPath { get; set; }
        public string ThumbNail { get; set; }
        public string UrlPath { get; set; }
        public bool IsUploaded { get; set; }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> previewCmd { get; }
        public ReactiveCommand<Unit, Unit> setVisibilityCmd { get; }
        #endregion

        public CreativeItem(CreativeDTO dto, CreativeServerDirectory dir)
        {

            token = AppContext.User.Token;
            serverApi = AppContext.ServerApi;

            remoteManager = new CreativesRemoteManager();
            remoteManager.DownloadProgessUpdateEvent += RemoteManager_DownloadProgessUpdateEvent;
            remoteManager.DownloadCompleted += RemoteManager_DownloadCompleted;
            localManager = new CreativesLocalManager();
            uniqalizer = new Uniqalizer();
            uniqalizer.UniqalizeProgessUpdateEvent += Uniqalizer_UniqalizeProgessUpdateEvent;
            previewer = new CreativePreviewer();

            Id = dto.id;
            Name = dto.name;
            //FileName = $"{dto.filename}.{dto.file_extension}";

            ServerDirectory = new CreativeServerDirectory()
            {
                id = dir.id,
                dir = dir.dir
            };

            Type = (dto.file_type.Equals("video")) ? CreativeType.video : CreativeType.picture;

            string stype = "";
            switch (Type)
            {
                case CreativeType.video:
                    stype = "videos";
                    break;
                case CreativeType.picture:
                    stype = "picures";
                    break;
                default:
                    break;
            }


            UrlPath = $"{paths.CreativesRootURL}/{ServerDirectory.dir}/{stype}/{dto.name}.{dto.file_extension}";

            string geopath = Path.Combine(paths.CreativesRootPath, ServerDirectory.dir);
            if (!Directory.Exists(geopath))
                Directory.CreateDirectory(geopath);

            LocalPath = Path.Combine(paths.CreativesRootPath, ServerDirectory.dir, $"{dto.name}.{dto.file_extension}");

            ThumbNail = Path.Combine(paths.CreativesRootPath, ServerDirectory.dir, $"{dto.name}.png");

            IsVisible = dto.visibility;
            IsUploaded = dto.uploaded;

            #region commands
            previewCmd = ReactiveCommand.Create(() =>
            {
                previewer.Preview(this);
            });

            setVisibilityCmd = ReactiveCommand.Create(() =>
            {

                Task.Run(async () =>
                {
                    try
                    {
                        //await serverApi.SetVisibility(token, Id, IsVisible);
                        await serverApi.SetCreativeStatus(token, Id, IsUploaded, IsVisible);

                    } catch (Exception ex)
                    {
                        IsVisible = !IsVisible;
                        ws.ShowDialog(new errMsgVM(ex.Message));
                    }
                });

            });
            #endregion
        }

        private void Uniqalizer_UniqalizeProgessUpdateEvent(int progress)
        {
            //Progress = (progress < 100) ? progress : 0;
            
            Progress = progress;
            //Debug.WriteLine(Progress);
        }

        private void RemoteManager_DownloadCompleted()
        {
            IsSynchronized = true;
            Progress = 0;
        }

        private void RemoteManager_DownloadProgessUpdateEvent(int progress)
        {
            Progress = progress;
        }

        #region public
        public async Task SynchronizeAsync()
        {
            var remote_size = await remoteManager.GetFileSize(this);

            if (localManager.CheckCreativeDownloaded(this, remote_size))
            {
                IsSynchronized = true;
            } else
            {
                await remoteManager.Download(this);
                await localManager.GetThumbNail(this);
            }

            if (!File.Exists(ThumbNail))
                await localManager.GetThumbNail(this);

            var ms = File.OpenRead(ThumbNail);
            Preview = await Task.Run(() => Bitmap.DecodeToWidth(ms, 40));//new Bitmap(ThumbNail);
            ms = File.OpenRead(ThumbNail);
            FastView = await Task.Run(() => Bitmap.DecodeToHeight(ms, 500));           
        }

        public async Task Uniqalize()
        {
            if (!IsChecked)
                return;
            await uniqalizer.Uniqalize(this, Uniques, paths.CreativesOutputRootPath);
            
        }

        public async Task Uniqalize(int uniques)
        {
            if (!IsChecked)
                return;
            await uniqalizer.Uniqalize(this, uniques, paths.CreativesOutputRootPath);
            
        }

        public void StopUniqalization()
        {
            uniqalizer.Cancel();
        }
        #endregion

        #region callbacks
        public event Action<CreativeItem, bool> CheckedEvent;
        #endregion
    }
}
