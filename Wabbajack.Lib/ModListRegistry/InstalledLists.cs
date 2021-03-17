using Wabbajack.Common;

namespace Wabbajack.Lib.ModListRegistry
{
    public class InstalledLists
    {
        public AbsolutePath InstallLocation { get; set; }
        public AbsolutePath DownloadsLocation { get; set; }
        public AbsolutePath WabbajackLoaction { get; set; }
        public ModlistMetadata Metadata { get; set; } = new();
    }
}
