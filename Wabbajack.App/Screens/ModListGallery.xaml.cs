using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls;
using DynamicData;
using ReactiveUI;
using Wabbajack.App.Controls;

namespace Wabbajack.App.Screens
{
    public partial class ModListGallery
    {
        public ModListGallery(ModListGalleryVM vm) : base(vm)
        {
            InitializeComponent();
            this.WhenActivated(dispose =>
            {
                this.WhenAny(x => x.ViewModel!.Status)
                    .Select(x =>
                    {
                        return x switch
                        {
                            ModListGalleryVM.LoadingStatus.Ready => Gallery.LoadingStatus.Ready,
                            ModListGalleryVM.LoadingStatus.Loading => Gallery.LoadingStatus.Loading,
                            ModListGalleryVM.LoadingStatus.Errored => Gallery.LoadingStatus.Errored,
                            _ => throw new ArgumentOutOfRangeException(nameof(x), x, null)
                        };
                    })
                    .BindToStrict(this, x => x.Gallery.Status)
                    .DisposeWith(dispose);

                this.WhenAny(x => x.ViewModel!.ModLists)

                    .Select(lists => lists.Select(list => 
                    new GalleryItemVM
                    {
                        Title = list.Title,
                        ImageUrl = list.Links.ImageUri
                    }).ToArray())
                    .BindToStrict(this, x => x.Gallery.Items)
                    .DisposeWith(dispose);

            });
        }
    }
}

