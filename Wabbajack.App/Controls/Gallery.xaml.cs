using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Wabbajack.App.Controls
{
    public partial class Gallery : IActivatableView
    {
        public enum LoadingStatus
        {
            Ready,
            Loading,
            Errored
        }
        
        [Reactive] public GalleryItemVM[] Items { get; set; } = Array.Empty<GalleryItemVM>();

        [Reactive] public LoadingStatus Status { get; set; } = LoadingStatus.Loading;

        [Reactive] public string StatusText { get; set; } = "Loading";
        
        public Gallery()
        {
            InitializeComponent();

            this.WhenActivated(dispose =>
            {
                this.WhenAny(x => x.Status)
                    .CombineLatest(this.WhenAny(x => x.Items))
                    .Select(s =>
                        s.First != LoadingStatus.Ready || s.Second.Length == 0 ? Visibility.Visible : Visibility.Hidden)
                    .BindToStrict(this, x => x.Overlay.Visibility)
                    .DisposeWith(dispose);
                
                this.WhenAny(x => x.Items)
                    .CombineLatest(this.WhenAny(x => x.Status))
                    .Select(x =>
                        x.First.Length == 0 && x.Second == LoadingStatus.Ready
                            ? Visibility.Visible
                            : Visibility.Collapsed)
                    .BindToStrict(this, x => x.NotFoundIcon.Visibility)
                    .DisposeWith(dispose);

                this.WhenAny(x => x.Status)
                    .Select(status => status == LoadingStatus.Loading ? Visibility.Visible : Visibility.Collapsed)
                    .BindToStrict(this, x => x.LoadingRing.Visibility)
                    .DisposeWith(dispose);
                
                this.WhenAny(x => x.Status)
                    .Select(status => status == LoadingStatus.Errored ? Visibility.Visible : Visibility.Collapsed)
                    .BindToStrict(this, x => x.ErrorIcon.Visibility)
                    .DisposeWith(dispose);

                this.WhenAny(x => x.StatusText)
                    .BindToStrict(this, x => x.OverlayText.Text)
                    .DisposeWith(dispose);

                this.WhenAny(x => x.Items)
                    .BindToStrict(this, x => x.GalleryItems.ItemsSource)
                    .DisposeWith(dispose);

            });

        }
    }
}

