using System.IO;
using Wabbajack.Common;

namespace Wabbajack.App.Services
{
    public class Settings
    {
        private FileSystemWatcher _fileWatcher;

        public Settings()
        {
            _fileWatcher = new FileSystemWatcher(Consts.LocalAppDataPath.ToString());
        }
    }
}
