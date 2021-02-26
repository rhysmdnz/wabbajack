using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using DynamicData;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Wabbajack.Lib;
using Wabbajack.Lib.ModListRegistry;

namespace Wabbajack
{
    public class InstalledListSelectionVM : ViewModel
    {
        [Reactive]
        public InstalledListsData ListSources { get; set; }
        
        [Reactive]
        public List<InstalledListVM> Lists { get; set; }
        public ReactiveCommand<Unit, Unit> BackCommand { get; set; }
        
        public InstalledListSelectionVM(MainWindowVM mainWindowVM)
        {
            this.WhenAny(x => x.ListSources)
                .Where(x => x != default)
                .Select(x => x.Lists.Select(lst => new InstalledListVM(lst)).ToList())
                .BindToStrict(this, x => x.Lists)
                .DisposeWith(this.CompositeDisposable);
            
            BackCommand = ReactiveCommand.Create(
                execute: () =>
                {
                    mainWindowVM.NavigateTo(mainWindowVM.ModeSelectionVM);
                });

        }

    }

    public class InstalledListVM : ViewModel
    {
        [Reactive]
        private InstalledList List { get; set; }

        public string Title => List.Metadata!.Title;
        public string Version => List.InstalledVersion.ToString();
        public string InstallPath => List.InstallLocation.ToString();
        public string DownloadsPath => List.DownloadLocation.ToString();
        
        [Reactive]
        public BitmapImage Image { get; set; }
        public InstalledListVM(InstalledList lst)
        {
            List = lst;
            this.WhenAny(x => x.List)
                .Select(x => x.Metadata!.Links.ImageUrlFast)
                .DownloadBitmapImage(_ => { })
                .BindToStrict(this, x => x.Image)
                .DisposeWith(CompositeDisposable);
        }
    }
}
