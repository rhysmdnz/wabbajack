using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;

namespace Wabbajack.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow, IActivatableView
    {
        public MainWindow()
        {
            InitializeComponent();

            this.WhenActivated(dispose =>
            {
                var vm = ((App)Application.Current).ServiceProvider.GetService<MainWindowVM>()!;
                vm.WhenAny(x => x.CurrentScreen)
                    .Select(x => (ContentControl)x)
                    .BindToStrict(this, x => x.Content.Content)
                    .DisposeWith(dispose);

                vm.WhenAny(x => x.VersionString)
                    .Select(v => $"v{v}")
                    .BindToStrict(this, x => x.VersionButton.Content)
                    .DisposeWith(dispose);

                vm.WhenAny(x => x.VersionString)
                    .Select(v => ReactiveCommand.Create(() =>
                    {
                        Clipboard.SetText(v);
                    }))
                    .BindToStrict(this, x => x.VersionButton.Command)
                    .DisposeWith(dispose);
            });

        }
    }
}
