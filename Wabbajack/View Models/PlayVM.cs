using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using CefSharp;
using CefSharp.Wpf;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Wabbajack.Common;
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
        public ReactiveCommand<Unit, Unit> CopyFilesCommand { get; set; }
        

        [Reactive]
        public string BrowserAddress { get; set; }

        private Subject<bool> _isIdle = new();

        [Reactive]
        private bool NeedsGameFileFolderCopy { get; set; }

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
                    _isIdle.OnNext(false);
                    Task.Run(async () =>
                    {
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
                canExecute: _isIdle.StartWith(true)
                    .CombineLatest(this.WhenAny(x => x.NeedsGameFileFolderCopy))
                    .Select(state => state.First && !state.Second)
                    .ObserveOnGuiThread());

            CopyFilesCommand = ReactiveCommand.Create(
                execute: () =>
                {
                    Task.Run(async () => await CopyGameFolderFiles());
                },
                canExecute: _isIdle.StartWith(true)
                    .CombineLatest(this.WhenAny(x => x.NeedsGameFileFolderCopy))
                    .Select(state => state.First && state.Second)
                    .ObserveOnGuiThread());


            this.WhenAny(x => x.List)
                .Merge(_isIdle.Select(_ => this.List))
                .Where(lst => lst != default)
                .Select(lst => GameFolderFiles())
                .Select(NeedsGameFileUpdate)
                .BindToStrict(this, x => x.NeedsGameFileFolderCopy);
        }




        private HashSet<Extension> _nonCriticalGameFiles = new() {new Extension(".json"), new Extension(".txt"), new Extension(".ini")};
        private bool NeedsGameFileUpdate(List<(RelativePath Path, long Size)> lst)
        {
            var gamePath = List.Metadata!.Game.MetaData().GameLocation();
            foreach (var (relativePath, size) in lst)
            {
                var path = gamePath.Combine(relativePath);
                if (_nonCriticalGameFiles.Contains(path.Extension))
                {
                    if (!path.Exists)
                        return true;
                }
                else
                {
                    if (!path.Exists || path.Size != size)
                        return true;
                }
            }

            return false;
        }

        public List<(RelativePath, long)> GameFolderFiles()
        {
            var gff = List.InstallLocation.Combine(Consts.GameFolderFilesDir);
            if (!gff.IsDirectory)
                return new List<(RelativePath, long)>();

            return gff.EnumerateFiles()
                .Select(f => (f.RelativeTo(gff), f.Size))
                .ToList();
        }

        public async Task CopyGameFolderFiles()
        {
            try
            {
                _isIdle.OnNext(false);
                var files = GameFolderFiles();
                var gff = List.InstallLocation.Combine(Consts.GameFolderFilesDir);
                var gameFolder = List.Metadata!.Game.MetaData().GameLocation();

                await gff.Combine("enbseries").DeleteDirectory();
                await gff.Combine("enbcache").DeleteDirectory();
                foreach (var (relativePath, _) in files)
                {
                    var src = relativePath.RelativeTo(List.InstallLocation.Combine(Consts.GameFolderFilesDir));
                    var dst = relativePath.RelativeTo(gameFolder);
                    await src.CopyToAsync(dst);
                }
            }
            finally
            {
                _isIdle.OnNext(true);
            }
        }
        
        
    }
}
