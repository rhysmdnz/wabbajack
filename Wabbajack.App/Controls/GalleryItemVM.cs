using System;
using System.Reactive.Disposables;
using System.Windows.Media.Imaging;
using DynamicData.Alias;
using ReactiveUI.Fody.Helpers;
using Wabbajack.Lib;
using System.Reactive.Linq;


namespace Wabbajack.App.Controls
{
    public class GalleryItemVM : ViewModel
    {
        [Reactive] public string Title { get; set; } = "";

        [Reactive] public BitmapImage? Image { get; set; } = null;
        [Reactive] public string? ImageUrl { get; set; } = null;

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
}
