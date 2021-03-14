using System.Reactive;
using System.Reactive.Disposables;
using System.Windows.Controls;
using MahApps.Metro.IconPacks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Wabbajack.App.Controls
{
    public partial class GlowingIconButton : UserControl, IActivatableView
    {

        [Reactive] public PackIconFontAwesomeKind Icon { get; set; } = PackIconFontAwesomeKind.None;

        [Reactive] public string Text { get; set; } = "";

        [Reactive] public ReactiveCommand<Unit, Unit> Command { get; set; } = ReactiveCommand.Create(() => { });

        public GlowingIconButton()
        {
            InitializeComponent();
            this.WhenActivated(dispose =>
            {
                this.WhenAny(x => x.Text)
                    .BindToStrict(this, x => x.TextBlock.Text)
                    .DisposeWith(dispose);
                
                this.WhenAny(x => x.Icon)
                    .BindToStrict(this, x => x.IconBase.Kind)
                    .DisposeWith(dispose);
                
                this.WhenAny(x => x.Icon)
                    .BindToStrict(this, x => x.IconGlow.Kind)
                    .DisposeWith(dispose);
                
                this.WhenAny(x => x.Icon)
                    .BindToStrict(this, x => x.IconGlow2.Kind)
                    .DisposeWith(dispose);
                
                this.WhenAny(x => x.Command)
                    .BindToStrict(this, x => x.WrapperButton.Command)
                    .DisposeWith(dispose);
            });
        }
    }
}

