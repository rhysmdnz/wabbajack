using System;
using System.Reactive;
using System.Reactive.Disposables;
using System.Windows.Media.Imaging;
using DynamicData.Alias;
using ReactiveUI.Fody.Helpers;
using Wabbajack.Lib;
using System.Reactive.Linq;
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
        public GalleryItemCommandVM[] Commands { get; set; } = Array.Empty<GalleryItemCommandVM>();

        [Reactive] public string Key { get; set; } = "";

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
        Cancel,
    }
}
