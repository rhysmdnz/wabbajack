using System.Reactive;
using ReactiveUI;
using Wabbajack.Lib;

namespace Wabbajack.App.Screens
{
    public class ModeSelectionVM : ViewModel
    {
        public ReactiveCommand<Unit, Unit> BrowseModListsCommand;

        public ModeSelectionVM()
        {
            BrowseModListsCommand = ReactiveCommand.Create(() =>
            {

            });
        }

    }
}
