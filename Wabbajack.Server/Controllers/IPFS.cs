using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Wabbajack.Common;
using Wabbajack.Server;
using Wabbajack.Server.DataLayer;
using Wabbajack.Server.Services;

namespace Wabbajack.BuildServer.Controllers
{
    [ApiController]
    [Route("/ipfs")]
    public class IPFS : ControllerBase
    {
        private ILogger<IPFS> _logger;
        private IPFSService _ipfs;

        public IPFS(ILogger<IPFS> logger, IPFSService ipfs)
        {
            _logger = logger;
            _ipfs = ipfs;
        }
        
        [HttpGet("by_hash/{hash}")]
        public async Task<IActionResult> GetByHash(string hash)
        {
            var url = await _ipfs.GetDownloadUrl(Hash.Interpret(hash));
            if (url == default)
                return NotFound();
            return Redirect(url.ToString());

        }
        
        [HttpGet("by_name/{name}")]
        public async Task<IActionResult> GetByName(string name)
        {
            var url = await _ipfs.GetDownloadUrlForName(name);
            if (url == default)
                return NotFound();
            return Redirect(url.ToString());
        }
        
    }
}
