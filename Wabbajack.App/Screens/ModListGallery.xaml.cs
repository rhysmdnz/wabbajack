using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls;
using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Wabbajack.App.Controls;
using Wabbajack.App.Services;

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
                    {
                        var vm = App.GetService<GalleryItemVM>();
                        vm.Title = list.Title;
                        vm.ImageUrl = list.Links.ImageUri;
                        return vm;
                    }).ToArray())
                    .BindToStrict(this, x => x.Gallery.Items)
                    .DisposeWith(dispose);

            });
        }
    }
}

