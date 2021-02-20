using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using ReactiveUI;
using Wabbajack.Lib.Http;

namespace Wabbajack.Views
{
    public partial class PlayView : ReactiveUserControl<PlayVM>
    {
        public PlayView()
        {
            InitializeComponent();
            this.WhenActivated(dispose =>
            {

                this.WhenAny(x => x.ViewModel.List)
                    .Select(x => x.Metadata?.Title ?? "")
                    .BindToStrict(this, x => x.TopProgressBar.Title)
                    .DisposeWith(dispose);
                
                this.WhenAny(x => x.ViewModel.List)
                    .Select(x => x.Metadata?.Links.ImageUrlFast ?? "")
                    .Where(uri => uri != default)
                    .DownloadBitmapImage(e => {})
                    .BindToStrict(this, x => x.Image.Source)
                    .DisposeWith(dispose);
                
                this.WhenAny(x => x.ViewModel.BackCommand)
                    .BindToStrict(this, x => x.BackButton.Command)
                    .DisposeWith(dispose);
                
                this.WhenAny(x => x.ViewModel.Changelog)
                    .BindToStrict(this, x => x.Changelog.Markdown)
                    .DisposeWith(dispose);
            });
        }
    }
}

