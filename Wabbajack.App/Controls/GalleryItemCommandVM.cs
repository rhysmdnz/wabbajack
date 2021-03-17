using System.Reactive;
using MahApps.Metro.IconPacks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Wabbajack.Lib;

namespace Wabbajack.App.Controls
{
    public class GalleryItemCommandVM : ViewModel
    {
        [Reactive] public ReactiveCommand<Unit, Unit> Command { get; set; } = ReactiveCommand.Create(() => {});
        [Reactive] public CommandType Type { get; set; } = CommandType.Download;
        
        public PackIconFontAwesomeKind AwesomeKind => Type switch
        {
            CommandType.Download => PackIconFontAwesomeKind.DownloadSolid,
            CommandType.Web => PackIconFontAwesomeKind.InfoSolid,
            CommandType.Play => PackIconFontAwesomeKind.PlaySolid
        };
        public GalleryItemCommandVM()
        {

        }
        
    }
}
