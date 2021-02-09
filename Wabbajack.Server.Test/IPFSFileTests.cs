using System.Threading.Tasks;
using Wabbajack.BuildServer.Test;
using Wabbajack.Common;
using Wabbajack.Lib.Http;
using Wabbajack.Server.Services;
using Xunit;
using Xunit.Abstractions;

namespace Wabbajack.Server.Test
{
    public class IPFSFileTests : ABuildServerSystemTest
    {
        public IPFSFileTests(ITestOutputHelper output, SingletonAdaptor<BuildServerFixture> fixture) : base(output, fixture)
        {
        }

        [Fact]
        public async Task CanPinArchivedFiles()
        {
            var ipfs = Fixture.GetService<IPFSService>();
            
            var archiver = Fixture.GetService<ArchiveMaintainer>();

            await using var file = new TempFile();
            await using var backup = new TempFile();

            await file.Path.WriteAllBytesAsync(RandomData(1024));
            await file.Path.CopyToAsync(backup.Path);
            
            var hash = await file.Path.FileHashAsync();
            await archiver.Ingest(file.Path);
            
            var pin = await ipfs.PinArchiveFile(hash.Value, file.Path.FileName.ToString());

            var url = await ipfs.GetDownloadUrl(hash.Value);
            Utils.Log($"Finding file from {url}");
            Assert.NotNull(url);

            var client = new Client();

            var data = await client.GetAsync(url);
            
            Assert.True(data.IsSuccessStatusCode);
            await using var response = await data.Content.ReadAsStreamAsync();
            Assert.Equal(await backup.Path.ReadAllBytesAsync(), await response.ReadAllAsync());
            
            var data2 = await client.GetAsync(MakeURL($"ipfs/by_hash/{hash.Value.ToHex()}"));
            Assert.True(data.IsSuccessStatusCode);
            await using var response2 = await data2.Content.ReadAsStreamAsync();
            Assert.Equal(await backup.Path.ReadAllBytesAsync(), await response2.ReadAllAsync());

            await ipfs.Unpin(pin);
            

        }
    }
}
