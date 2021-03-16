using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Wabbajack.Common;
using Wabbajack.Lib;
using Wabbajack.Lib.Downloaders;
using Wabbajack.Lib.ModListRegistry;

namespace Wabbajack.App.Services
{
    public class DownloadedModlistManager : IDisposable
    {
        private GlobalInformation _globalInformation;
        private FileSystemWatcher _watcher;
        private Dictionary<AbsolutePath, (Subject<Status> HaveList, ModlistMetadata MetaData)> _modlistWatchers = new();

        public DownloadedModlistManager(GlobalInformation globalInformation)
        {
            _globalInformation = globalInformation;
            _watcher = new FileSystemWatcher(_globalInformation.ModlistFolder.ToString());
            _watcher.Renamed += HandleChange;
            _watcher.Changed += HandleChange;
            _watcher.Deleted += HandleChange;
            _watcher.Created += HandleChange;
        }

        private void HandleChange(object sender, FileSystemEventArgs e)
        {
            lock (this)
            {
                var path = (AbsolutePath)e.FullPath;
                if (_modlistWatchers.TryGetValue((AbsolutePath)e.FullPath, out var subject))
                {
                    if (path.Exists && path.Size == subject.MetaData.DownloadMetadata!.Size)
                    {
                        subject.HaveList.OnNext(Status.Downloaded);
                    }
                    else
                    {
                        subject.HaveList.OnNext(Status.NotDownloaded);
                    }
                }
            }
        }

        public IObservable<Status> HaveModlist(ModlistMetadata list)
        {
            lock (this)
            {
                var path = GetPath(list);
                if (_modlistWatchers.TryGetValue(path, out var subject))
                {
                    return subject.HaveList.StartWith(HaveModListInternal(list) ? Status.Downloaded : Status.NotDownloaded);
                }
                else
                {
                    _modlistWatchers.Add(path, (new Subject<Status>(), list));
                    return _modlistWatchers[path].HaveList.StartWith(HaveModListInternal(list) ? Status.Downloaded : Status.NotDownloaded);
                }
            }
        }

        public async Task Download(ModlistMetadata list)
        {
            var path = GetPath(list);
            var tmpPath = path.WithExtension(Consts.TempExtension);

            var resolved = DownloadDispatcher.ResolveArchive(list.Links.Download);
            if (resolved == null)
                Utils.Error($"Can't resolve archive {list.Links.Download}");
            
            lock (this)
            {
                foreach (var s in _modlistWatchers.Values)
                {
                    SetStatus(s.MetaData, Status.Downloading);
                }
            }


            try
            {
                await resolved!.Download(
                    new Archive(resolved!)
                    {
                        Size = list.DownloadMetadata!.Size,
                        Name = path.FileName.ToString(),
                        Hash = list.DownloadMetadata.Hash
                    }, tmpPath);
                await path.DeleteAsync();
                await tmpPath.MoveToAsync(path);

                await list.ToJsonAsync(path.WithExtension(Consts.JsonExtension));
            }
            finally
            {
                lock (this)
                {
                    foreach (var s in _modlistWatchers.Values)
                    {
                        SetStatus(s.MetaData, HaveModListInternal(s.MetaData) ? Status.Downloaded : Status.NotDownloaded);
                    }
                }
            }
        }

        public void SetStatus(ModlistMetadata list, Status status)
        {
            lock (this)
            {
                var path = GetPath(list);
                if (_modlistWatchers.TryGetValue(path, out var subject))
                {
                    subject.HaveList.OnNext(status);
                }
                else
                {
                    _modlistWatchers.Add(path, (new Subject<Status>(), list));
                }
            }
        }

        public bool HaveModListInternal(ModlistMetadata list)
        {
            var path = GetPath(list);
            return path.Exists && path.Size == list.DownloadMetadata!.Size;
        }

        private AbsolutePath GetPath(ModlistMetadata list)
        {
            return _globalInformation.ModlistFolder.Combine(
                $"{list.Links.MachineURL}_{list.DownloadMetadata!.Hash.ToHex()}.wabbajack");
        }

        public void Dispose()
        {
            _watcher.Dispose();
        }
        
        public enum Status
        {
            NotDownloaded,
            Downloading,
            Downloaded
        }
    }
}
