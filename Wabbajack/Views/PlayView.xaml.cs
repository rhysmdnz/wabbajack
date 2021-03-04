using System.Net;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using CefSharp.Wpf;
using OMODFramework;
using ReactiveUI;
using Wabbajack.Lib.Http;
using Utils = Wabbajack.Common.Utils;

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
                
                this.WhenAny(x => x.ViewModel.BackCommand)
                    .BindToStrict(this, x => x.BackButton.Command)
                    .DisposeWith(dispose);
                
                this.WhenAny(x => x.ViewModel.List)
                    .Select(x => x.Metadata?.Links.ImageUri ?? "")
                    .Where(uri => uri != default)
                    .DownloadBitmapImage(e => {})
                    .BindToStrict(this, x => x.DetailImage.Image)
                    .DisposeWith(dispose);

                /*
                this.WhenAny(x => x.ViewModel.ReadmeBrowserAddress)
                    .BindToStrict(this, x => x.Browser.Address)
                    .DisposeWith(dispose);
                

                


                
                this.WhenAny(x => x.ViewModel.BrowseLocalFilesCommand)
                    .BindToStrict(this, x => x.BrowseLocalFiles.Command)
                    .DisposeWith(dispose);
                
                this.WhenAny(x => x.ViewModel.PlayCommand)
                    .BindToStrict(this, x => x.PlayButton.Command)
                    .DisposeWith(dispose);
                
                this.WhenAny(x => x.ViewModel.CopyFilesCommand)
                    .BindToStrict(this, x => x.CopyGameFilesButton.Command)
                    .DisposeWith(dispose);
                    */
            });
        }
    }
}

