using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using ReactiveUI;

namespace Wabbajack.App.Controls
{
    public partial class GalleryItem : IActivatableView
    {
        public GalleryItem()
        {

            InitializeComponent();
            this.WhenActivated(dispose =>
            {
                this.WhenAny(x => x.ViewModel!.Title)
                    .BindToStrict(this, x => x.TitleText.Text)
                    .DisposeWith(dispose);

                this.WhenAny(x => x.ViewModel!.Image)
                    .Where(img => img != null)
                    .BindToStrict(this, x => x.Image.Source)
                    .DisposeWith(dispose);

                
                this.WhenAny(x => x.ViewModel!.Image)
                    .Select(img => img == null ? Visibility.Visible : Visibility.Collapsed)
                    .BindToStrict(this, x => x.ImageLoadingSpinner.Visibility)
                    .DisposeWith(dispose);


            });
        }
    }
}

