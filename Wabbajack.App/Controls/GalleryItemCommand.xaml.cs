using System.Reactive.Disposables;
using System.Windows.Controls;
using ReactiveUI;

namespace Wabbajack.App.Controls
{
    public partial class GalleryItemCommand : IActivatableView
    {
        public GalleryItemCommand()
        {
            InitializeComponent();
            this.WhenActivated(dispose =>
            {
                this.WhenAny(x => x.ViewModel!.Command)
                    .BindToStrict(this, x => x.Button.Command)
                    .DisposeWith(dispose);

                this.WhenAny(x => x.ViewModel!.AwesomeKind)
                    .BindToStrict(this, x => x.Icon.Kind)
                    .DisposeWith(dispose);
            });
        }
    }
}

