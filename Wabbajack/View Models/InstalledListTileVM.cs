using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Alphaleonis.Win32.Filesystem;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Wabbajack.Common;
using Wabbajack.Lib;
using Wabbajack.Lib.Downloaders;
using Wabbajack.Lib.ModListRegistry;

namespace Wabbajack
{
    
    public class InstalledListTileVM : ViewModel
    {
        public ModlistMetadata Metadata { get; }
        private InstalledListGalleryVM _parent;

        [Reactive]
        public InstalledList Data { get; set; }
        public ICommand OpenWebsiteCommand { get; }
        public ICommand ExecuteCommand { get; }

        private readonly ObservableAsPropertyHelper<bool> _Exists;
        public bool Exists => true;

        public AbsolutePath Location { get; }

        [Reactive]
        public List<ModListTag> ModListTagList { get; private set; }

        [Reactive]
        public Percent ProgressPercent { get; private set; }

        [Reactive]
        public bool IsBroken { get; private set; }
        
        [Reactive]
        public bool IsDownloading { get; private set; }

        [Reactive]
        public string DownloadSizeText { get; private set; }

        [Reactive]
        public string InstallSizeText { get; private set; }

        [Reactive]
        public IErrorResponse Error { get; private set; }


        [Reactive] 
        public BitmapImage Image { get; set; }
        
        [Reactive]
        public string Title { get; set; }

        [Reactive]
        public string Description { get; set; }

        public InstalledListTileVM(InstalledListGalleryVM parent, InstalledList data)
        {            
            _parent = parent;
            Metadata = data.Metadata;
            Data = data;

            this.WhenAny(x => x.Data)
                .Select(x => x.Metadata?.Links.ImageUri ?? default)
                .Where(x => x != default)
                .DownloadBitmapImage(_ => { })
                .BindToStrict(this, x => x.Image)
                .DisposeWith(CompositeDisposable);

            this.WhenAny(x => x.Data)
                .Select(x => x.Metadata?.Title)
                .BindToStrict(this, x => x.Title)
                .DisposeWith(CompositeDisposable);
            
            this.WhenAny(x => x.Data)
                .Select(x => $"Install Location: {x.InstallLocation}\nVersion: {x.InstalledVersion}")
                .BindToStrict(this, x => x.Description)
                .DisposeWith(CompositeDisposable);

            ExecuteCommand = ReactiveCommand.Create(async () =>
            {
                await _parent.MWVM.Play.Value.SetList(Data);
                _parent.MWVM.NavigateTo(_parent.MWVM.Play.Value);

            });



            Location = Consts.ModListDownloadFolder.Combine(Metadata.Links.MachineURL + (string)Consts.ModListExtension);
            ModListTagList = new List<ModListTag>();
            Metadata.tags.ForEach(tag =>
            {
                ModListTagList.Add(new ModListTag(tag));
            });
            DownloadSizeText = "Download size : " + UIUtils.FormatBytes(Metadata.DownloadMetadata.SizeOfArchives);
            InstallSizeText = "Installation size : " + UIUtils.FormatBytes(Metadata.DownloadMetadata.SizeOfInstalledFiles);
            //https://www.wabbajack.org/#/modlists/info?machineURL=eldersouls
            OpenWebsiteCommand = ReactiveCommand.Create(() => Utils.OpenWebsite(new Uri($"https://www.wabbajack.org/#/modlists/info?machineURL={Metadata.Links.MachineURL}")));
        }



        private async Task<bool> Download()
        {
            ProgressPercent = Percent.Zero;
            using (var queue = new WorkQueue(1))
            using (queue.Status.Select(i => i.ProgressPercent)
                .ObserveOnGuiThread()
                .Subscribe(percent => ProgressPercent = percent))
            {
                var tcs = new TaskCompletionSource<bool>();
                queue.QueueTask(async () =>
                {
                    try
                    {
                        IsDownloading = true;
                        Utils.Log($"Starting Download of {Metadata.Links.MachineURL}");
                        var downloader = DownloadDispatcher.ResolveArchive(Metadata.Links.Download);
                        var result = await downloader.Download(
                            new Archive(state: null!)
                            {
                                Name = Metadata.Title, Size = Metadata.DownloadMetadata?.Size ?? 0
                            }, Location);
                        Utils.Log($"Done downloading {Metadata.Links.MachineURL}");

                        await Metadata.ToJsonAsync(Location.WithExtension(Consts.MetaDataExtension));

                        // Want to rehash to current file, even if failed?
                        await Location.FileHashCachedAsync();
                        Utils.Log($"Done hashing {Metadata.Links.MachineURL}");
                        tcs.SetResult(result);
                    }
                    catch (Exception ex)
                    {
                        Utils.Error(ex, $"Error Downloading of {Metadata.Links.MachineURL}");
                        tcs.SetException(ex);
                    }
                    finally
                    {
                        IsDownloading = false;
                    }
                });


                Task.Run(async () => await Metrics.Send(Metrics.Downloading, Metadata.Title))
                    .FireAndForget(ex => Utils.Error(ex, "Error sending download metric"));

                return await tcs.Task;
            }
        }
    }
}
