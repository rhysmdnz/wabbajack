using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Wabbajack.Common;
using Wabbajack.Server.Services;

namespace Wabbajack.Server.DataLayer
{
    public partial class SqlService
    {
        public async Task<string> AddPinnedFile(Hash hash, string name, string nodeId)
        {

            await using var conn = await Open();
            await using var trans = await conn.BeginTransactionAsync();
            
            var res = (await conn.QueryAsync<string>("SELECT CID from IPFSPins WHERE Hash = @Hash", new {Hash = hash}, trans)).FirstOrDefault();
            if (res != null) return res;
            
            await conn.ExecuteAsync("INSERT INTO IPFSPins (Hash, Name, CID) VALUES (@Hash, @Name, @Cid)",
                new {
                    Hash = hash,
                    Name = name,
                    Cid = nodeId
                }, trans);

            await trans.CommitAsync();
            return nodeId;
        }

        public async Task<(string CID, string Name)> GetPinnedFile(Hash hash)
        {
            await using var conn = await Open();
            
            var res = (await conn.QueryAsync<(string, string)>("SELECT CID, Name from IPFSPins WHERE Hash = @Hash", new {Hash = hash})).FirstOrDefault();
            return res;
        }

        public async Task<(string CID, string Name)> GetPinnedFileByName(string name)
        {
            await using var conn = await Open();
            
            var res = (await conn.QueryAsync<(string, string)>("SELECT CID, Name from IPFSPins WHERE Name = @Name", new {Name = name})).FirstOrDefault();
            return res;
        }

        public async Task RemovePin(string cid)
        {
            await using var conn = await Open();
            await conn.ExecuteAsync("DELETE from IPFSPins WHERE CID = @CID", new {CID = cid});
        }

        public async Task<(Hash Hash, string Name)[]> GetPinnableFiles()
        {
            await using var conn = await Open();

            var authored = (await conn.QueryAsync<(string, string)>(
                "SELECT JSON_VALUE(CDNFileDefinition, '$.Hash'), JSON_VALUE(CDNFileDefinition, '$.MungedName') from AuthoredFiles"))
                .Select(f => (Hash.FromBase64(f.Item1), f.Item2))
                .ToArray();

            var existingPins = (await conn.QueryAsync<Hash>("SELECT Hash from IPFSPins")).ToHashSet();

            var toPin = authored.Where(f => !existingPins.Contains(f.Item1)).GroupBy(f => f.Item1)
                .Select(f => (f.Key, f.FirstOrDefault(f => f.Item2 != null).Item2));

            return toPin.ToArray();
        }
    }
}
