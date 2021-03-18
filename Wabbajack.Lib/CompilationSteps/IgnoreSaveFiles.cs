﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wabbajack.Common;

namespace Wabbajack.Lib.CompilationSteps
{
    public class IgnoreSaveFiles : MO2CompilationStep
    {
        private AbsolutePath[] _profilePaths;
        private readonly bool _includeSaves;
        private readonly string _tag;
        private readonly AbsolutePath _sourcePath;

        public IgnoreSaveFiles(ACompiler compiler) : base(compiler)
        {
            _tag = Consts.WABBAJACK_INCLUDE_SAVES;
            _sourcePath = compiler.SourcePath;
            string rootDirectory = (string)_sourcePath;

            try
            {
                _includeSaves = File.Exists(((String)Directory.EnumerateFiles(rootDirectory, _tag).ToList().First())) ? true : false;
            }
            catch // Cant get a .First() if the list is empty, which it is when the files doesn't exist.
            {
                _includeSaves = false;
            }
             

            _profilePaths =
                MO2Compiler.SelectedProfiles.Select(p => MO2Compiler.SourcePath.Combine("profiles", p, "saves")).ToArray();
        }

        public override async ValueTask<Directive?> Run(RawSourceFile source)
        {
            if (_includeSaves)
            {
                foreach (var folderpath in _profilePaths)
                {
                    if (!source.AbsolutePath.InFolder(folderpath)) continue;
                    var result = source.EvolveTo<InlineFile>();
                    result.SourceDataID = await _compiler.IncludeFile(source.AbsolutePath);
                    return result;
                }
            }
            else
            {
                if (!_profilePaths.Any(p => source.File.AbsoluteName.InFolder(p)))
                    return null;

                var result = source.EvolveTo<IgnoredDirectly>();
                result.Reason = "Ignore Save files";
                return result;
            }
            return null;

        }
    }
}
