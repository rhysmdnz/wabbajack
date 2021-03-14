using System.Reactive;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Wabbajack.App.Services;
using Wabbajack.Lib;

namespace Wabbajack.App.Screens
{
    public class ModeSelectionVM : ViewModel
    {
        [Reactive]
        public ReactiveCommand<Unit, Unit> BrowseButtonCommand { get; private set; }
        
        private EventRouter _router;

        public ModeSelectionVM(EventRouter router)
        {
            _router = router;
            BrowseButtonCommand = ReactiveCommand.Create(() =>
            {
                _router.NavigateTo<ModListGallery>();
            });
        }

    }
}
