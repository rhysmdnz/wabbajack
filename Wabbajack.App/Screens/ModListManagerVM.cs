using ReactiveUI.Fody.Helpers;
using Wabbajack.Lib;

namespace Wabbajack.App.Screens
{
    public class ModListManagerVM : ViewModel
    {
        [Reactive]
        public string ReadmeAddress { get; set; }

        public ModListManagerVM()
        {
            ReadmeAddress = "https://www.google.com";
        }
    }
}
