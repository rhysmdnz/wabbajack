using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Wabbajack.Common;

namespace Wabbajack.App.Services
{
    public class GlobalInformation 
    {
        public Version Version { get; }
        
        public GlobalInformation()
        {
            var assembly = typeof(GlobalInformation).Assembly;
            var fvi = FileVersionInfo.GetVersionInfo(assembly.Location);
            Consts.CurrentMinimumWabbajackVersion = Version.Parse(fvi.FileVersion!);
            Version = Consts.CurrentMinimumWabbajackVersion;
            
            Utils.Log($"Wabbajack Version: {Version}");
        }

        private static bool? _inLauncherFolder = null;
        /// <summary>
        /// Returns true if the app is inside a launcher folder. That is if we're in a folder with a version
        /// name, and the folder above contains `Wabbajack.exe`
        /// </summary>
        public static bool InLauncherFolder
        {
            get
            {
                if (_inLauncherFolder != null) return _inLauncherFolder.Value;
                
                var entryPoint = AbsolutePath.EntryPoint;
                // If we're not in a folder that looks like a version, abort
                if (!Version.TryParse(entryPoint.FileName.ToString(), out var version))
                {
                    _inLauncherFolder = false;
                    return false;
                }

                // If we're not in a folder that has Wabbajack.exe in the parent folder, abort
                if (!entryPoint.Parent.Parent.Combine(Consts.AppName).WithExtension(new Extension(".exe")).IsFile)
                {
                    _inLauncherFolder = false;
                    return false;
                }

                _inLauncherFolder = true;
                return true;
            }
        }

        public AbsolutePath ModlistFolder
        {
            get
            {
                AbsolutePath folder;
                folder = !InLauncherFolder ? Consts.ModListDownloadFolder : AbsolutePath.EntryPoint.Parent.Combine("downloaded_mod_lists");
                folder.CreateDirectory();
                return folder;
            }
        }
    }
}
