using System;
using System.Diagnostics;
using System.Reflection;
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
        
    }
}
