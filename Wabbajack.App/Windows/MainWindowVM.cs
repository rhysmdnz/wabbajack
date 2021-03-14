using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using ReactiveUI.Fody.Helpers;
using Wabbajack.App.Screens;
using Wabbajack.App.Services;
using Wabbajack.Common;
using Wabbajack.Lib;

namespace Wabbajack.App
{
    public class MainWindowVM : ViewModel
    {
        private Dictionary<Type, IScreen> _screens;
        private GlobalInformation _globalInformation;
        private EventRouter _router;

        [Reactive]
        public IScreen CurrentScreen { get; set; }
        
        [Reactive]
        public string VersionString { get; set; }

        public MainWindowVM(IEnumerable<IScreen> screens, GlobalInformation globalInformation, EventRouter router)
        {
            _screens = screens.ToDictionary(screen => screen.GetType());
            _globalInformation = globalInformation;
            _router = router;
            VersionString = _globalInformation.Version.ToString();
            CurrentScreen = _screens.First().Value;

            // Bind NavigateTo events to CurrentScreen
            _router.Events.OfType<NavigateToEvent>()
                .Select(e =>
                {
                    var found = _screens.TryGetValue(e.Screen, out var screen);
                    if (!found)
                        Utils.Log($"Got NavigateTo for {e.Screen} but that screen isn't registered");
                    return (found, screen, e);
                })
                .Where(e => e.found)
                .Select(e => e.screen)
                .BindToStrict(this, x => x.CurrentScreen)
                .DisposeWith(CompositeDisposable);
        }


    }
}
