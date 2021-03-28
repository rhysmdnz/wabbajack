using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using CommandLine;
using Wabbajack.Common;
using Wabbajack.Lib;
using Wabbajack.Lib.Downloaders;
using Wabbajack.Lib.ModListRegistry;

namespace Wabbajack.CLI.Verbs
{
    [Verb("download-modlist", HelpText = "Download a modlist")]
    public class DownloadModlist : AVerb
    {

        [Option('n', "name", Required = true, HelpText = @"Modlist name")]
        public string Name { get; set; } = "";
        protected override async Task<ExitCode> Run()
        {

            Utils.LogMessages
                .OfType<IUserIntervention>()
                .ObserveOnGuiThread()
                .SelectTask(async msg =>
                {
                    switch (msg)
                    {
                        case ConfirmUpdateOfExistingInstall c:
                            c.Confirm();
                            break;
                        default:
                            CLIUtils.Log("Can't handle user intervention on the CLI");
                            msg.Cancel();
                            break;
                    }
                }
                )
                .Subscribe();


            var list = await ModlistMetadata.LoadFromGithub();
            foreach (var modlist in list)
            {
                if (modlist.Title == Name)
                {
                    var Location = LauncherUpdater.CommonFolder.Value.Combine("downloaded_mod_lists", modlist.Links.MachineURL + (string)Consts.ModListExtension);
                    CLIUtils.Log($"Starting Download of {modlist.Links.MachineURL}");
                    var downloader = DownloadDispatcher.ResolveArchive(modlist.Links.Download);
                    if (downloader != null)
                    {
                        var result = await downloader.Download(
                            new Archive(state: null!)
                            {
                                Name = modlist.Title,
                                Size = modlist.DownloadMetadata?.Size ?? 0
                            }, Location);
                        CLIUtils.Log($"Done downloading {modlist.Links.MachineURL}");

                        // Want to rehash to current file, even if failed?
                        await Location.FileHashCachedAsync();
                        Utils.Log($"Done hashing {modlist.Links.MachineURL}");

                        await modlist.ToJsonAsync(Location.WithExtension(Consts.ModlistMetadataExtension));

                        ModlistMetadata? sourceModListMetadata = null;


                        var metadataPath = Location.WithExtension(Consts.ModlistMetadataExtension);
                        if (metadataPath.Exists)
                        {
                            try
                            {
                                sourceModListMetadata = metadataPath.FromJson<ModlistMetadata>();
                            }
                            catch (Exception)
                            {
                                sourceModListMetadata = null;
                            }
                        }

                        var parameters = new SystemParameters();
                        parameters.ScreenHeight = 1440;
                        parameters.ScreenWidth = 2560;
                        parameters.SystemMemorySize = 16000000000;
                        parameters.SystemPageSize = 16000000000;
                        parameters.VideoMemorySize = 16000000000 / 2;

                        using (var installer = new MO2Installer(
                                archive: Location,
                                modList: AInstaller.LoadFromFile(Location),
                                outputFolder: new AbsolutePath("/home/rhys/Documents/SecretLivingSkyrimLocation/Game"),
                                downloadFolder: new AbsolutePath("/home/rhys/Documents/SecretLivingSkyrimLocation/Download"),
                        // parameters: SystemParametersConstructor.Create()))
                        parameters: parameters))
                        {
                            installer.Metadata = sourceModListMetadata;
                            installer.UseCompression = false;
                            //Parent.MWVM.Settings.Performance.SetProcessorSettings(installer);

                            await Task.Run(async () =>
                            {
                                try
                                {
                                    var workTask = installer.Begin();
                                    //ActiveInstallation = installer;
                                    await workTask;
                                }
                                finally
                                {
                                    //ActiveInstallation = null;
                                }
                            });
                        }
                    }
                }
            }
            return 0;
        }
    }
}
