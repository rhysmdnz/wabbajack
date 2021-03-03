using System;
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
using Wabbajack.Lib.Downloaders;
using Wabbajack.Lib.Http;
using Wabbajack.Lib.ModListRegistry;

namespace Wabbajack
{
    public class PlayVM : ViewModel
    {
        [Reactive]
        public InstalledList List { get; protected set; }
        
        [Reactive]
        public string Changelog { get; set; }

        public ReactiveCommand<Unit, Unit> BackCommand { get; }
        public ReactiveCommand<Unit, Unit> PlayCommand { get; }
        
        public ReactiveCommand<Unit, Unit> BrowseLocalFilesCommand { get; set; }
        public ReactiveCommand<Unit, Unit> CopyFilesCommand { get; set; }
        

        [Reactive]
        public string ReadmeBrowserAddress { get; set; }
        
        [Reactive]
        public string ChangelogBrowserAddress { get; set; }
        
        [Reactive]
        public string NewsBrowserAddress { get; set; }

        private Subject<bool> _isIdle = new();

        [Reactive]
        private bool NeedsGameFileFolderCopy { get; set; }

        [Reactive] private HashSet<RelativePath> GameFiles { get; set; } = new();

        public PlayVM(MainWindowVM mainWindowVM)
        {
            this.ObservableForProperty(vm => vm.List)
                .Select(m => m.Value.Metadata!)
                .Where(m => m != default)
                .Select(m => m.Links.Readme.EndsWith(".md", StringComparison.InvariantCultureIgnoreCase) 
                    ? $"https://www.wabbajack.org/#/modlists/info?machineURL={m.Links.MachineURL}" 
                    : m.Links.Readme)
                .BindToStrict(this, x => x.ReadmeBrowserAddress)
                .DisposeWith(CompositeDisposable);

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
                .SelectAsync(NeedsGameFileUpdate)
                .BindToStrict(this, x => x.NeedsGameFileFolderCopy)
                .DisposeWith(CompositeDisposable);
        }



        public async Task SetList(InstalledList list)
        {
            var data = await ClientAPI.GetGameFilesFromGithub(list.Metadata!.Game,
                list.Metadata!.Game.MetaData().InstalledVersion);
            GameFiles = data.Select(gf => ((GameFileSourceDownloader.State)gf.State).GameFile).ToHashSet();
            List = list;
        }

        private HashSet<Extension> _nonCriticalGameFiles = new() {new Extension(".json"), new Extension(".txt"), new Extension(".ini")};
        private HashSet<Extension> _criticalGameFiles = new() {new Extension(".exe"), new Extension(".dll")};
        private async Task<bool> NeedsGameFileUpdate(List<(RelativePath Path, long Size)> lst)
        {
            var gff = List.InstallLocation.Combine(Consts.GameFolderFilesDir);
            if (!gff.Exists) return false;
            
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
            
            var gameFolder = List.Metadata!.Game.MetaData().GameLocation();

            foreach (var file in gameFolder.EnumerateFiles())
            {
                var relPath = file.RelativeTo(gameFolder);
                if (_criticalGameFiles.Contains(file.Extension) && !GameFiles.Contains(relPath) && !relPath.RelativeTo(gff).Exists)
                {
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
                await gff.Combine("reshade").DeleteDirectory();
                
                foreach (var file in gameFolder.EnumerateFiles())
                {
                    if (_criticalGameFiles.Contains(file.Extension) && !GameFiles.Contains(file.RelativeTo(gameFolder)))
                    {
                        await file.DeleteAsync();
                    }
                }
                
                foreach (var (relativePath, _) in files)
                {
                    var src = relativePath.RelativeTo(List.InstallLocation.Combine(Consts.GameFolderFilesDir));
                    var dst = relativePath.RelativeTo(gameFolder);
                    dst.Parent.CreateDirectory();
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
