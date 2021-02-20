using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wabbajack.Common;
using Wabbajack.Common.Serialization.Json;

namespace Wabbajack.Lib.ModListRegistry
{
    [JsonName("InstalledListsData")]
    public class InstalledListsData
    {
        public List<InstalledList> Lists { get; set; } = new();

        private static AbsolutePath Location => Consts.LocalAppDataPath.Combine("installed_list_registry.json");
        public static async Task<InstalledListsData> Load()
        {
            InstalledListsData data = new();
            if (Location.Exists)
            {
                try
                {
                    data = Location.FromJson<InstalledListsData>();
                }
                catch(Exception)
                {}
            }
            return data;
        }

        public static async Task Add(InstalledList newEntry)
        {
            var data = await Load();
            data.Lists = data.Lists.Where(f => f.InstallLocation != newEntry.InstallLocation).Cons(newEntry).ToList();
            await data.ToJsonAsync(Location, prettyPrint: true);
        }
    }

    [JsonName("InstalledList")]
    public class InstalledList
    {
        public Version InstalledVersion { get; set; } = new();
        public Hash InstalledHash { get; set; } = new();
        public AbsolutePath InstallLocation { get; set; }
        public AbsolutePath DownloadLocation { get; set; }
        public AbsolutePath WabbajackFileLocation { get; set; }
        public ModlistMetadata? Metadata { get; set; } = new();
    }
}
