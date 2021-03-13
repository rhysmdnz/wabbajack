using ReactiveUI;

namespace Wabbajack.App.Screens
{
    public abstract class ScreenBase<T> : ReactiveUserControl<T>, IScreen where T : class 
    {
        protected ScreenBase(T vm)
        {
            DataContext = vm;
        }
        
    }
}
