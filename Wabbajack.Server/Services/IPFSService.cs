using System;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Logging;
using Wabbajack.BuildServer;
using Wabbajack.Server.DataLayer;
using Wabbajack.Common;

namespace Wabbajack.Server.Services
{
    public class IPFSService : IStartable
    {
        private ILogger<IPFSService> _logger;
        private SqlService _sql;
        private ArchiveMaintainer _archives;
        private DiscordWebHook _discord;
        private AppSettings _settings;

        public IPFSService(ILogger<IPFSService> logger, SqlService sql, ArchiveMaintainer archives, DiscordWebHook discordWebHook, AppSettings settings)
        {
            _logger = logger;
            _sql = sql;
            _archives = archives;
            _discord = discordWebHook;
            _settings = settings;
        }

        public async Task<string> PinArchiveFile(Hash hash, string name = null)
        {
            if (!_archives.TryGetPath(hash, out var path))
            {
                throw new Exception($"File {hash} not in archiver");
            }

            var helper = new ProcessHelper() {Path = _settings.IPFSExe, Arguments = new object[] {"add", path, "-q"}};
            string line = "";
            var result = helper.Output.Subscribe(f =>
            {
                if (f.Type == ProcessHelper.StreamType.Output)
                    line = f.Line;
                else 
                    _logger.LogError(f.Line);
            });

            if (await helper.Start() != 0)
                throw new Exception("ipfs error");
            result.Dispose();

            await _sql.AddPinnedFile(hash, name, line);
            return line;

        }
        
        public void Start()
        {
        }

        public async Task<Uri> GetDownloadUrl(Hash hash)
        {
            var (cid, name) = await _sql.GetPinnedFile(hash);
            if (cid == default) return null;
            
            if (string.IsNullOrEmpty(name))
            {
                return new Uri($"https://ipfs.io/ipfs/{cid}");
            }
            return new Uri($"https://ipfs.io/ipfs/{cid}?filename={HttpUtility.UrlEncode(name)}");
        }
        public async Task<Uri> GetDownloadUrlForName(string name)
        {
            var (cid, _) = await _sql.GetPinnedFileByName(name);
            if (cid == default) return null;
            
            if (string.IsNullOrEmpty(name))
            {
                return new Uri($"https://ipfs.io/ipfs/{cid}");
            }
            return new Uri($"https://ipfs.io/ipfs/{cid}?filename={HttpUtility.UrlEncode(name)}");
        }

        public async Task Unpin(string cid)
        {
            var helper = new ProcessHelper {Path = _settings.IPFSExe, Arguments = new object[] {"pin", "rm", cid}};
            var result = helper.Output.Subscribe(f =>
            {
                if (f.Type == ProcessHelper.StreamType.Error)
                    _logger.LogError(f.Line);
            });

            if (await helper.Start() != 0)
                throw new Exception("ipfs error");
            result.Dispose();

            await _sql.RemovePin(cid);
        }
    }
}
