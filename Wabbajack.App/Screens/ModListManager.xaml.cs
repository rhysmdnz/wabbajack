using System.Windows.Controls;
using ReactiveUI;

namespace Wabbajack.App.Screens
{
    public partial class ModListManager : IActivatableView
    {
        public ModListManager(ModListManagerVM vm) : base(vm)
        {
            InitializeComponent();
            
        }
    }
}

