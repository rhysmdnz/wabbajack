using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI.Fody.Helpers;
using Wabbajack.App.Screens;
using Wabbajack.App.Services;
using Wabbajack.Lib;

namespace Wabbajack.App
{
    public class MainWindowVM : ViewModel
    {
        private Dictionary<Type, IScreen> _screens;
        private GlobalInformation _globalInformation;

        [Reactive]
        public IScreen CurrentScreen { get; set; }
        
        [Reactive]
        public string VersionString { get; set; }

        public MainWindowVM(IEnumerable<IScreen> screens, GlobalInformation globalInformation)
        {
            _screens = screens.ToDictionary(screen => screen.GetType());
            _globalInformation = globalInformation;
            VersionString = _globalInformation.Version.ToString();
            CurrentScreen = _screens.First().Value;
        }


    }
}
