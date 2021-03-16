using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls;
using DynamicData;
using MahApps.Metro.IconPacks;
using MahApps.Metro.IconPacks.Converter;
using Microsoft.Extensions.DependencyInjection;
using OMODFramework;
using ReactiveUI;
using Wabbajack.App.Controls;
using Wabbajack.App.Services;
using Wabbajack.Common;
using Wabbajack.Lib.ModListRegistry;

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

                this.WhenAny(x => x.SearchBox.Text)
                    .Debounce(TimeSpan.FromMilliseconds(250))
                    .BindToStrict(this.ViewModel!, x => x.SearchString)
                    .DisposeWith(dispose);

                this.WhenAny(x => x.ShowNSFW.IsChecked)
                    .Select(x => x ?? false)
                    .BindToStrict(this.ViewModel!, x => x.ShowNSFW)
                    .DisposeWith(dispose);

                this.WhenAny(x => x.ViewModel!.ModListVMs)
                    .BindToStrict(this, x => x.Gallery.Items)
                    .DisposeWith(dispose);


                var allItm = new ComboBoxItem() {Content = "All Games"};
                allItm.WhenAny(x => x.IsSelected)
                    .Where(x => x)
                    .Select(_ => (Game?)null)
                    .BindToStrict(this.ViewModel!, x => x!.GameFilter)
                    .DisposeWith(dispose);
                GameSelectorComboBox.Items.Add(allItm);
                
                foreach (var game in GameRegistry.Games.Values.OrderBy(x => x.HumanFriendlyGameName))
                {
                    var itm = new ComboBoxItem() {Content = game.HumanFriendlyGameName};
                    itm.WhenAny(x => x.IsSelected)
                        .Where(x => x)
                        .Select(_ => (Game?)game.Game)
                        .BindToStrict(this.ViewModel!, x => x!.GameFilter)
                        .DisposeWith(dispose);
                    GameSelectorComboBox.Items.Add(itm);
                }
            });
        }
    }
}

