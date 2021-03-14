using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using DynamicData;
using Microsoft.Extensions.DependencyInjection;
using Wabbajack.App.Screens;
using Wabbajack.App.Services;

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
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow!.Show();
        }
    }
}
