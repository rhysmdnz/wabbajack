using System;
using System.Threading.Tasks;
using ReactiveUI.Fody.Helpers;
using Wabbajack.Lib;
using Wabbajack.Lib.ModListRegistry;

namespace Wabbajack.App.Screens
{
    public class ModListGalleryVM : ViewModel
    {
        [Reactive] 
        public ModlistMetadata[] ModLists { get; set; } = Array.Empty<ModlistMetadata>();

        [Reactive] public LoadingStatus Status { get; set; } = LoadingStatus.Loading;
        public ModListGalleryVM()
        {
            var tsk = ReloadLists();
        }

        public async Task ReloadLists()
        {
            Status = LoadingStatus.Loading;
            try
            {
                ModLists = (await ModlistMetadata.LoadFromGithub()).ToArray();
                Status = LoadingStatus.Ready;
            }
            catch(Exception ex)
            {
                Status = LoadingStatus.Errored;
            }
        }

        public enum LoadingStatus
        {
            Ready,
            Loading,
            Errored
        }
    }
}
