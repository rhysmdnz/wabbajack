using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI.Fody.Helpers;
using Wabbajack.App.Screens;
using Wabbajack.Lib;

namespace Wabbajack.App
{
    public class MainWindowVM : ViewModel
    {
        private Dictionary<Type, IScreen> _screens;

        [Reactive]
        public IScreen CurrentScreen { get; set; }

        public MainWindowVM(IEnumerable<IScreen> screens)
        {
            _screens = screens.ToDictionary(screen => screen.GetType());
            CurrentScreen = _screens.First().Value;
        }
        
    }
}
