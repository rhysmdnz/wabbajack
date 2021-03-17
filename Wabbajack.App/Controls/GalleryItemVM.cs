using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Windows.Media.Imaging;
using DynamicData.Alias;
using ReactiveUI.Fody.Helpers;
using Wabbajack.Lib;
using System.Reactive.Linq;
using DynamicData;
using DynamicData.Binding;
using MahApps.Metro.IconPacks;
using ReactiveUI;
using Wabbajack.Common;


namespace Wabbajack.App.Controls
{
    public class GalleryItemVM : ViewModel
    {
        [Reactive] public string Title { get; set; } = "";

        [Reactive] public BitmapImage? Image { get; set; } = null;
        [Reactive] public string? ImageUrl { get; set; } = null;
        [Reactive] public string Description { get; set; } = "";

        [Reactive]
        public ObservableCollectionExtended<GalleryItemCommandVM> Commands { get; set; } = new();

        public GalleryItemVM()
        {
            this.WhenAny(x => x.ImageUrl)
                .Where(url => url != default)
                .Select(url => url!)
                .DownloadBitmapImage(_ => { })
                .BindToStrict(this, x => x.Image)
                .DisposeWith(CompositeDisposable);
        }
    }

    public enum CommandType
    {
        Download,
        Web,
        Play,
        Cancel,
    }
}
