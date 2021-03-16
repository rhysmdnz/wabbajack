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
using System.Windows.Shell;
using DynamicData;
using DynamicData.Binding;
using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using Wabbajack.Common;

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
                    .BindToStrict(this, x => x.WindowContent.Content)
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

                StatusUtils.StatusMessages.ToObservableChangeSet().ToCollection()
                    .Select(x => new Percent(x.Average(y => y.Percent.Value)))
                    .Select(x => x.Value)
                    .ObserveOnGuiThread()
                    .BindToStrict(this, x => x.TaskbarInfo.ProgressValue)
                    .DisposeWith(dispose);
                
                StatusUtils.StatusMessages.ToObservableChangeSet()
                    .ToCollection()
                    .Select(t => t.Count > 0 ? Visibility.Visible : Visibility.Collapsed)
                    .ObserveOnGuiThread()
                    .BindToStrict(this, x => x.StatusHeader.Visibility)
                    .DisposeWith(dispose);

                StatusUtils.OverallMessage
                    .Select(s => s.Percent.Value)
                    .ObserveOnGuiThread()
                    .BindToStrict(this, x => x.OverAllProgress.Value)
                    .DisposeWith(dispose);

                StatusUtils.StatusMessages.ToObservableChangeSet()
                    .ToCollection()
                    .Select(x => x.Where(x => x.Category.HasFlag(StatusCategory.Compute))
                        .Sum(x => x.BytesPerSecond))
                    .Select(x => x > 0 ? $"CPU: {x.ToFileSizeString()}/sec" : "CPU: Idle")
                    .ObserveOnGuiThread()
                    .Select(x => x)
                    .BindToStrict(this, x => x.CPUStatus.Text)
                    .DisposeWith(dispose);
                
                StatusUtils.StatusMessages.ToObservableChangeSet()
                    .ToCollection()
                    .Select(x => x.Where(x => x.Category.HasFlag(StatusCategory.Network))
                        .Sum(x => x.BytesPerSecond))
                    .Select(x => x > 0 ? $"Network: {x.ToFileSizeString()}/sec" : "Network: Idle")
                    .ObserveOnGuiThread()
                    .Select(x => x)
                    .BindToStrict(this, x => x.NetworkStatus.Text)
                    .DisposeWith(dispose);
                
                StatusUtils.StatusMessages.ToObservableChangeSet()
                    .ToCollection()
                    .Select(x => x.Where(x => x.Category.HasFlag(StatusCategory.Disk))
                        .Sum(x => x.BytesPerSecond))
                    .Select(x => x > 0 ? $"Disk: {x.ToFileSizeString()}/sec" : "Disk: Idle")
                    .ObserveOnGuiThread()
                    .Select(x => x)
                    .BindToStrict(this, x => x.DiskStatus.Text)
                    .DisposeWith(dispose);

                StatusUtils.OverallMessage
                    .Select(i => string.IsNullOrWhiteSpace(i.Message) ? $"Task: None" : $"{i.Message} - {i.Percent}")
                    .ObserveOnGuiThread()
                    .BindToStrict(this, x => x.OverAllAction.Text)
                    .DisposeWith(dispose);

                TaskbarInfo.ProgressState = TaskbarItemProgressState.Normal;
            });

        }
    }
}
