using ReactiveUI.Fody.Helpers;
using Wabbajack.Lib;

namespace Wabbajack.App.Controls
{
    public class GalleryItemVM : ViewModel
    {
        [Reactive] public string Title { get; set; } = "";

    }
}
