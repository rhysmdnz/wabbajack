using System.Diagnostics;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Wpf;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Wabbajack.Lib;
using Wabbajack.Lib.Http;
using Wabbajack.Lib.ModListRegistry;

namespace Wabbajack
{
    public class PlayVM : ViewModel
    {
        [Reactive]
        public InstalledList List { get; set; }
        
        [Reactive]
        public string Changelog { get; set; }

        public ReactiveCommand<Unit, Unit> BackCommand { get; }
        public ReactiveCommand<Unit, Unit> PlayCommand { get; }
        
        public ReactiveCommand<Unit, Unit> BrowseLocalFilesCommand { get; set; }
        

        [Reactive]
        public string BrowserAddress { get; set; }

        private Subject<bool> _isIdle = new();

        public PlayVM(MainWindowVM mainWindowVM)
        {
            this.ObservableForProperty(vm => vm.List)
                .Select(m => m.Value.Metadata?.Links.MachineURL)
                .Where(m => m != default)
                .Select(x => $"https://www.wabbajack.org/#/modlists/info?machineURL={x}")
                .BindToStrict(this, x => x.BrowserAddress);

            BrowseLocalFilesCommand = ReactiveCommand.Create(() =>
            {
                Process.Start("explorer.exe", List.InstallLocation.ToString());
            });
            
            BackCommand = ReactiveCommand.Create(
                execute: () =>
                {
                    mainWindowVM.NavigateTo(mainWindowVM.ModeSelectionVM);
                });
            
            PlayCommand = ReactiveCommand.Create(
                execute: () =>
                {
                    Task.Run(async () =>
                    {
                        _isIdle.OnNext(false);
                        try
                        {
                            await List.Play();
                        }
                        finally
                        {
                            _isIdle.OnNext(true);
                        }
                    });
                },
                canExecute: _isIdle.ObserveOnGuiThread().StartWith(true));
        }
    }
}
