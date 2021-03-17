using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Wabbajack.App.Screens;
using Wabbajack.App.Services;
using Wabbajack.Common;
using Wabbajack.Lib.LibCefHelpers;

namespace Wabbajack.App
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public ServiceProvider ServiceProvider { get; }

        public App()
        {
            LoggingSettings.LogToFile = true;
            Helpers.Init();
            
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }
        
        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowVM>();
            services.RegisterAllTypes<IScreen>(typeof(App).Assembly);
            services.RegisterAllVMs(typeof(App).Assembly);
            services.AddSingleton<GlobalInformation>();
            services.AddSingleton<Settings>();
            services.AddSingleton<EventRouter>();
            services.AddSingleton<DownloadedModlistManager>();
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow!.Show();
        }
        
        public static T GetService<T>() where T : notnull
        {
            return ((App)Current).ServiceProvider.GetRequiredService<T>();
        }
    }
}
