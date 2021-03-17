using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Controls;
using ReactiveUI;

namespace Wabbajack.App.Screens
{
    public partial class ModListManager : IActivatableView
    {
        public ModListManager(ModListManagerVM vm) : base(vm)
        {
            InitializeComponent();

            this.WhenActivated(dispose =>
            {
                this.WhenAny(x => x.ReadmeBrowser.Address)
                    .BindToStrict(this, x => x.BrowserAddress.Text)
                    .DisposeWith(dispose);

                this.WhenAny(x => x.ViewModel!.ReadmeAddress)
                    .BindToStrict(this, x => x.ReadmeBrowser.Address)
                    .DisposeWith(dispose);

                BrowserBackButton.Command = ReadmeBrowser.BackCommand;
                BrowserHomeButton.Command = ReactiveCommand.Create(() =>
                {
                    ReadmeBrowser.Address = this.ViewModel!.ReadmeAddress;
                });
            });

        }
    }
}

