using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Wabbajack.BuildServer;
using Wabbajack.Server.DataLayer;
using Wabbajack.Server.DTOs;

namespace Wabbajack.Server.Services
{
    public class PinningService : AbstractService<PinningService, int>
    {
        private SqlService _sql;
        private IPFSService _ipfs;
        private ArchiveMaintainer _archiver;
        private DiscordWebHook _discord;

        public PinningService(ILogger<PinningService> logger, AppSettings settings, QuickSync quickSync, SqlService sql, IPFSService ipfs, ArchiveMaintainer archiver, DiscordWebHook discord) 
            : base(logger, settings, quickSync, TimeSpan.FromMinutes(1))
        {
            _sql = sql;
            _ipfs = ipfs;
            _archiver = archiver;
            _discord = discord;
        }

        public override async Task<int> Execute()
        {
            var toPin = (await _sql.GetPinnableFiles()).Where(p => _archiver.HaveArchive(p.Hash)).ToArray();

            foreach (var archive in toPin.Select((archive, idx) => (archive.Hash, archive.Name, idx)))
            {
                await _discord.Send(Channel.Spam, new DiscordMessage {Content = $"({archive.idx}/{toPin.Length}) Pinning {archive.Hash} - {archive.Name}"});
                await _ipfs.PinArchiveFile(archive.Hash, archive.Name);
            }

            return toPin.Length;
        }
    }
}
