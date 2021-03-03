using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
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

        public async Task Play()
        {
            if (Process.GetProcessesByName("ModOrganizer.exe").Any())
            {
                MessageBox.Show("ModOrganizer.exe is already running please exit it now", "Can't Start Game");
                return;
            }

            var ini = InstallLocation.Combine("ModOrganizer.ini").LoadIniFile();
            var exe1 = (string)ini.customExecutables["1\\title"];

            await using var tmpFile = new TempFile(new Extension(".bat"));
            await tmpFile.Path.WriteAllLinesAsync(string.Join(" ", new []
            {
                "start",
                "\"\"",
                "\"" + InstallLocation.Combine("ModOrganizer.exe") + "\"",
                exe1
            }));

            var process = new ProcessHelper
            {
                Path = AbsolutePath.WindowsFolder.Combine("explorer.exe"), 
                Arguments = new object[] {tmpFile.Path},
                WorkingDirectory = InstallLocation,
                WorkaroundMode = false
            };
            Utils.Log(process.Path.ToString());
            Utils.Log(string.Join(", ", process.Arguments.Select(a => a.ToString())));
            process.Output.Subscribe(p => Utils.Log(p.Line));
            await process.Start();

            Process? proc = default;
            while (proc == default)
            {
                proc = Process.GetProcessesByName("ModOrganizer.exe").FirstOrDefault();
                await Task.Delay(500);
            }

            await proc!.WaitForExitAsync();
        }
        
    }
}
