using System.Reactive.Disposables;
using System.Windows.Controls;
using ReactiveUI;

namespace Wabbajack.Views
{
    public partial class InstalledListSelectionView : ReactiveUserControl<InstalledListSelectionVM>
    {
        public InstalledListSelectionView()
        {
            InitializeComponent();

            this.WhenActivated(dispose =>
            {
                this.WhenAny(x => x.ViewModel.Lists)
                    .BindToStrict(this, x => x.Lists.ItemsSource)
                    .DisposeWith(dispose);
                
                this.WhenAny(x => x.ViewModel.BackCommand)
                    .BindToStrict(this, x => x.BackButton.Command)
                    .DisposeWith(dispose);

            });
        }
    }
}

