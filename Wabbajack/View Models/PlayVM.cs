using System.Reactive.Linq;
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

        public object BackCommand { get; set; }

        public PlayVM(MainWindowVM mainWindowVM)
        {
            this.ObservableForProperty(vm => vm.List)
                .Select(m => m.Value.Metadata?.Links.Readme)
                .Where(m => m != default)
                .SelectAsync(async url =>
                {
                    var client = new Client();
                    return await client.GetStringAsync(url);
                })
                .BindToStrict(this, x => x.Changelog);
            
            BackCommand = ReactiveCommand.Create(
                execute: () =>
                {
                    mainWindowVM.NavigateTo(mainWindowVM.ModeSelectionVM);
                });
        }
    }
}
