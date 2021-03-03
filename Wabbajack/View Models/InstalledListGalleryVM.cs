using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Wabbajack.Common;
using Wabbajack.Lib;
using Wabbajack.Lib.ModListRegistry;

namespace Wabbajack
{
    public class InstalledListGalleryVM : BackNavigatingVM
    {
        public MainWindowVM MWVM { get; }

        [Reactive]
        public InstalledListTileVM[] ModLists { get; set; }

        private const string ALL_GAME_TYPE = "All";

        [Reactive]
        public IErrorResponse Error { get; set; }

        [Reactive]
        public string Search { get; set; }

        [Reactive]
        public bool OnlyInstalled { get; set; }

        [Reactive]
        public bool ShowNSFW { get; set; }
        
        [Reactive]
        public bool ShowUtilityLists { get; set; }

        [Reactive]
        public string GameType { get; set; }

        public List<string> GameTypeEntries { get { return GetGameTypeEntries(); } }

        private readonly ObservableAsPropertyHelper<bool> _Loaded;

        private FiltersSettings settings => MWVM.Settings.Filters;

        public bool Loaded => _Loaded.Value;

        public ICommand ClearFiltersCommand { get; }
        
        [Reactive]
        public InstalledListsData ListSources { get; set; }

        public InstalledListGalleryVM(MainWindowVM mainWindowVM)
            : base(mainWindowVM)
        {
            MWVM = mainWindowVM;

            this.WhenAny(x => x.ListSources)
                .Where(lists => lists != default)
                .Select(x => x.Lists.Select(lst => new InstalledListTileVM(this, lst)).ToArray())
                .BindToStrict(this, x => x.ModLists)
                .DisposeWith(this.CompositeDisposable);

        }

        public override void Unload()
        {
            Error = null;
        }

        private List<string> GetGameTypeEntries()
        {
            List<string> gameEntries = new List<string> { ALL_GAME_TYPE };
            gameEntries.AddRange(EnumExtensions.GetAllItems<Game>().Select(gameType => gameType.GetDescription<Game>()));
            return gameEntries;
        }

        private void UpdateFiltersSettings()
        {
            settings.Game = GameType;
            settings.Search = Search;
            settings.ShowNSFW = ShowNSFW;
            settings.ShowUtilityLists = ShowUtilityLists;
            settings.OnlyInstalled = OnlyInstalled;
        }
    }
}
