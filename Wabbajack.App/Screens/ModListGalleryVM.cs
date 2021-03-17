using System;
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
using Wabbajack.App.Services;
using Wabbajack.Common;

namespace Wabbajack.App.Screens
{
    public class ModListGalleryVM : ViewModel
    {
        private DownloadedModlistManager _downloadManager;
        private EventRouter _router;

        [Reactive]
        public bool ShowNSFW { get; set; }

        [Reactive] 
        public ModlistMetadata[] ModLists { get; set; } = Array.Empty<ModlistMetadata>();

        [Reactive] public LoadingStatus Status { get; set; } = LoadingStatus.Loading;

        [Reactive] public ObservableCollectionExtended<GalleryItemVM> ModListVMs { get; set; } = new();

        [Reactive] public string SearchString { get; set; } = "";

        [Reactive] public Game? GameFilter { get; set; } = null;
        [Reactive] public bool OnlyInstalled { get; set; }
        [Reactive] public bool OnlyUtilityLists { get; set; }

        public ModListGalleryVM(DownloadedModlistManager downloadManager, EventRouter router)
        {
            _downloadManager = downloadManager;
            _router = router;
            
            var tsk = ReloadLists();

            var searchFilter = this.WhenAny(x => x.SearchString)
                .CombineLatest(this.WhenAny(x => x.ShowNSFW), 
                    this.WhenAny(x => x.GameFilter),
                    this.WhenAny( x => x.OnlyInstalled),
                    this.WhenAny( x => x.OnlyUtilityLists))
                .Select< (string SearchString, bool ShowNSFW, Game? Game, bool OnlyInstalled, bool OnlyUtilityLists), Func<ModlistMetadata, bool>>(
                    d =>
                      vm => (string.IsNullOrEmpty(d.SearchString) || vm.Title.Contains(d.SearchString, StringComparison.InvariantCultureIgnoreCase))
                    && ((vm.NSFW && d.ShowNSFW) || !vm.NSFW)
                    && ((d.Game != null && vm.Game == d.Game) || d.Game == null)
                    && ((!d.OnlyInstalled || d.OnlyInstalled && vm.Game.MetaData().IsInstalled))
                    && ((!d.OnlyUtilityLists && !vm.UtilityList || d.OnlyUtilityLists && vm.UtilityList)));
            
            this.WhenAny(x => x.ModLists)
                .SelectMany(lists => lists)
                .ToObservableChangeSet(list => list.Links.MachineURL)
                .Filter(searchFilter)
                .Transform(list =>
                {
                    var vm = App.GetService<GalleryItemVM>()!;
                    MakeCommands(list).Bind(vm.Commands).Subscribe().DisposeWith(CompositeDisposable);
                    vm.Title = list.ImageContainsTitle ? "" : list.Title;
                    vm.ImageUrl = list.Links.ImageUri;
                    vm.Description = list.Description;
                    return vm;
                })
                .Bind(ModListVMs)
                .Subscribe()
                .DisposeWith(CompositeDisposable);

        }
        
        private IObservable<IChangeSet<GalleryItemCommandVM, CommandType>> MakeCommands(ModlistMetadata list)
        {
            var haveList = _downloadManager.HaveModlist(list);
            var buttons = new[]
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
                    },canExecute: haveList.Select(l => l != DownloadedModlistManager.Status.Downloading)),
                    Type = CommandType.Download
                },
                new GalleryItemCommandVM()
                {
                    Command = ReactiveCommand.Create(() =>
                    {
                        _router.NavigateTo<ModListManager>();
                    },canExecute: haveList.Select(l => l != DownloadedModlistManager.Status.Downloading)),
                    Type = CommandType.Play
                },
            };
            return buttons.AsObservableChangeSet(x => x.Type)
                .Filter(_downloadManager.HaveModlist(list).Select<DownloadedModlistManager.Status, Func<GalleryItemCommandVM, bool>>(listState =>
                {
                    if (listState == DownloadedModlistManager.Status.Downloaded)
                    {
                        return vm => vm.Type != CommandType.Download;
                    }

                    return vm => vm.Type != CommandType.Play;
                }));
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
