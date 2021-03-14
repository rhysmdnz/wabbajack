using System;
using System.IO;
using System.Net.Http;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ReactiveUI;
using Wabbajack.Common;

namespace Wabbajack
{
    public static class UIUtils
    {
        public static BitmapImage BitmapImageFromResource(string name) => BitmapImageFromStream(System.Windows.Application.GetResourceStream(new Uri("pack://application:,,,/Wabbajack;component/" + name)).Stream);

        public static BitmapImage BitmapImageFromStream(Stream stream)
        {
            var img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.StreamSource = stream;
            img.EndInit();
            img.Freeze();
            return img;
        }
        
        public static IObservable<BitmapImage?> DownloadBitmapImage(this IObservable<string> obs, Action<Exception> exceptionHandler)
        {
            return obs
                .ObserveOn(RxApp.TaskpoolScheduler)
                .SelectTask(async url =>
                {
                    try
                    {
                        var (found, mstream) = await FindCachedImage(url);
                        if (found) return mstream;
                        
                        var ret = new MemoryStream();
                        using (var client = new HttpClient())
                        await using (var stream = await client.GetStreamAsync(url))
                        {
                            await stream.CopyToAsync(ret);
                        }

                        ret.Seek(0, SeekOrigin.Begin);

                        await WriteCachedImage(url, ret.ToArray());
                        return ret;
                    }
                    catch (Exception ex)
                    {
                        exceptionHandler(ex);
                        return default;
                    }
                })
                .Select(memStream =>
                {
                    if (memStream == null) return default;
                    try
                    {
                        return BitmapImageFromStream(memStream);
                    }
                    catch (Exception ex)
                    {
                        exceptionHandler(ex);
                        return default;
                    }
                    finally
                    {
                        memStream.Dispose();
                    }
                })
                .ObserveOnGuiThread();
        }

        private static async Task WriteCachedImage(string url, byte[] data)
        {
            var folder = Consts.LocalAppDataPath.Combine("ModListImages");
            if (!folder.Exists) folder.CreateDirectory();
            
            var path = folder.Combine(Encoding.UTF8.GetBytes(url).xxHash().ToHex());
            await path.WriteAllBytesAsync(data);
        }

        private static async Task<(bool Found, MemoryStream? data)> FindCachedImage(string uri)
        {
            var folder = Consts.LocalAppDataPath.Combine("ModListImages");
            if (!folder.Exists) folder.CreateDirectory();
            
            var path = folder.Combine(Encoding.UTF8.GetBytes(uri).xxHash().ToHex());
            return path.Exists ? (true, new MemoryStream(await path.ReadAllBytesAsync())) : (false, default);
        }
    }
}
