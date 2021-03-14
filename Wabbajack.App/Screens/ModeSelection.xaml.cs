using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI;

namespace Wabbajack.App.Screens
{
    public partial class ModeSelection 
    {
        public ModeSelection(ModeSelectionVM vm) : base(vm)
        {
            InitializeComponent();
            this.WhenActivated(dispose =>
            {
                this.WhenAny(x => x.ViewModel)
                    .Where(x => x != default)
                    .Select(x => x!.BrowseButtonCommand)
                    .BindToStrict(this, x => x.BrowseButton.Command)
                    .DisposeWith(dispose);

            });
        }
    }
}

