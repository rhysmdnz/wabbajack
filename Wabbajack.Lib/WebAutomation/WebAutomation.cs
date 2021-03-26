﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using HtmlAgilityPack;
using Wabbajack.Common;
using Wabbajack.Lib.LibCefHelpers;

namespace Wabbajack.Lib.WebAutomation
{
    public class Driver : IDisposable
    {
        // private IWebBrowser _browser;
        // private CefSharpWrapper _driver;

        public Driver()
        {

            // _browser = new ChromiumWebBrowser();

            // _driver = new CefSharpWrapper(_browser);
        }
        public static async Task<Driver> Create()
        {
            var driver = new Driver();
            // await driver._driver.WaitForInitialized();
            return driver;
        }

        public async Task<Uri?> NavigateTo(Uri uri, CancellationToken? token = null)
        {
            try
            {
                // await _driver.NavigateTo(uri, token);
                return await GetLocation();
            }
            catch (TaskCanceledException ex)
            {
                await DumpState(uri, ex);
                throw;
            }
        }

        private async Task DumpState(Uri uri, Exception ex)
        {
            var file = AbsolutePath.EntryPoint.Combine("CEFStates", DateTime.UtcNow.ToFileTimeUtc().ToString())
                .WithExtension(new Extension(".html"));
            file.Parent.CreateDirectory();
            var source = await GetSourceAsync();
            var cookies = await Helpers.GetCookies();
            var cookiesString = string.Join('\n', cookies.Select(c => c.Name + " - " + c.Value));
            await file.WriteAllTextAsync(uri + "\n " + source + "\n" + ex + "\n" + cookiesString);
        }

        public async Task<long> NavigateToAndDownload(Uri uri, AbsolutePath absolutePath, bool quickMode = false, CancellationToken? token = null)
        {
            try
            {
                // return await _driver.NavigateToAndDownload(uri, absolutePath, quickMode: quickMode, token: token);
                return 0;
            }
            catch (TaskCanceledException ex) {
                await DumpState(uri, ex);
                throw;
            }
        }

        public async ValueTask<Uri?> GetLocation()
        {
            try
            {
                return new Uri("https://google.com");
                // return new Uri(_browser.Address);

            }
            catch (UriFormatException)
            {
                return null;
            }
        }

        public async ValueTask<string> GetSourceAsync()
        {
            // return await _browser.GetSourceAsync();
            return "";
        }
        
        public async ValueTask<HtmlDocument> GetHtmlAsync()
        {
            var body = await GetSourceAsync();
            var doc = new HtmlDocument();
            doc.LoadHtml(body);
            return doc;
        }

        // public Action<Uri?> DownloadHandler { 
        //     set => _driver.DownloadHandler = value;
        // }

        public string hi() { return ""; }

        public Task<string> GetAttr(string selector, string attr)
        {
            // return _driver.EvaluateJavaScript($"document.querySelector(\"{selector}\").{attr}");
            return new Task<string>(new Func<string>(hi));
        }

        public Task<string> EvalJavascript(string js)
        {
            // return _driver.EvaluateJavaScript(js);
            return new Task<string>(new Func<string>(hi));
        }

        public void Dispose()
        {
            // _browser.Dispose();
        }

        public static void ClearCache()
        {
            Helpers.ClearCookies();
        }

        public async Task DeleteCookiesWhere(Func<Helpers.Cookie, bool> filter)
        {
            await Helpers.DeleteCookiesWhere(filter);
        }
    }
}
