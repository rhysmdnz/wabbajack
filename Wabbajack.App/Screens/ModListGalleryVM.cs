using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Wabbajack.App.Controls;
using Wabbajack.Lib;
using Wabbajack.Lib.ModListRegistry;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using DynamicData.PLinq;
using Wabbajack.App.Services;
using Wabbajack.Common;

namespace Wabbajack.App.Screens
{
    public class ModListGalleryVM : ViewModel
    {
        private DownloadedModlistManager _downloadManager;
        
        [Reactive]
        public bool ShowNSFW { get; set; };

        [Reactive] 
        public ModlistMetadata[] ModLists { get; set; } = Array.Empty<ModlistMetadata>();

        [Reactive] public LoadingStatus Status { get; set; } = LoadingStatus.Loading;

        [Reactive] public ObservableCollectionExtended<GalleryItemVM> ModListVMs { get; set; } = new();

        [Reactive] public string SearchString { get; set; } = "";

        public ModListGalleryVM(DownloadedModlistManager downloadManager)
        {
            _downloadManager = downloadManager;
            var tsk = ReloadLists();

            var searchFilter = this.WhenAny(x => x.SearchString)
                .CombineLatest(this.WhenAny(x => x.ShowNSFW))
                .Select<string, Func<ModlistMetadata, bool>>(str =>
                    vm => string.IsNullOrEmpty(str) || vm.Title.Contains(str, StringComparison.InvariantCultureIgnoreCase));
            
            this.WhenAny(x => x.ModLists)
                .SelectMany(lists => lists)
                .ToObservableChangeSet(list => list.Links.MachineURL)
                .Filter(searchFilter)
                .Transform(list =>
                {
                    var vm = App.GetService<GalleryItemVM>();
                    vm.Title = list.ImageContainsTitle ? "" : list.Title;
                    vm.ImageUrl = list.Links.ImageUri;
                    vm.Description = list.Description;
                    vm.Commands = MakeCommands(list);
                    return vm;
                })
                .Bind(ModListVMs)
                .Subscribe()
                .DisposeWith(CompositeDisposable);

        }
        
        private GalleryItemCommandVM[] MakeCommands(ModlistMetadata list)
        {
            var haveList = _downloadManager.HaveModlist(list);
            return new[]
            {
                new GalleryItemCommandVM()
                {
                    Command = ReactiveCommand.Create(() =>
                    {
                        Utils.OpenWebsite(new Uri(list.Links.Download));
                    }),
                    Type = CommandType.Web
                },
                new GalleryItemCommandVM()
                {
                    Command = ReactiveCommand.CreateFromTask(async () =>
                    {
                        await _downloadManager.Download(list);
                    }, canExecute: haveList.Select(l => l == DownloadedModlistManager.Status.NotDownloaded)),
                    Type = CommandType.Download
                }
            };
        }

        public async Task ReloadLists()
        {
            Status = LoadingStatus.Loading;
            try
            {
                ModLists = (await ModlistMetadata.LoadFromGithub()).ToArray();
                Status = LoadingStatus.Ready;
            }
            catch(Exception)
            {
                Status = LoadingStatus.Errored;
            }
        }

        public enum LoadingStatus
        {
            Ready,
            Loading,
            Errored
        }
    }
}
