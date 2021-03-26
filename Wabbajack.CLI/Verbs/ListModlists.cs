using System.Threading.Tasks;
using CommandLine;
using Wabbajack.Lib.FileUploader;
using Wabbajack.Lib.ModListRegistry;

namespace Wabbajack.CLI.Verbs
{
    [Verb("list-modlists", HelpText = "List modlists")]
    public class ListModlists : AVerb
    {
        protected override async Task<ExitCode> Run()
        {
            var list = await ModlistMetadata.LoadFromGithub();
            foreach (var modlist in list) {
                CLIUtils.Log(modlist.Title);
            }
            return 0;
        }
    }
}
