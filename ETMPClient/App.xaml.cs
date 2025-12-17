using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ETMPClient.Extensions;
using ETMPClient.Services;
using ETMPClient.ViewModels;
using ETMPData.Data;
using ETMPData.DataEntities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ETMPClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IServiceProvider? _serviceProvider;
        
        public App()
        {
            // Handle unhandled exceptions
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            string message = $"UI Thread Exception: {e.Exception.Message}\n\nStack Trace:\n{e.Exception.StackTrace}\n\nInner Exception: {e.Exception.InnerException?.Message}\n{e.Exception.InnerException?.StackTrace}";
            File.WriteAllText("ui_crash.txt", message);
            MessageBox.Show(message, "Application Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = false; // Let it crash so we can see it
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var ex = e.ExceptionObject as Exception;
            string message = $"Unhandled Exception: {ex?.Message}\n\nStack Trace:\n{ex?.StackTrace}\n\nInner Exception: {ex?.InnerException?.Message}\n{ex?.InnerException?.StackTrace}";
            File.WriteAllText("unhandled_crash.txt", message);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                IServiceCollection services = new ServiceCollection();

                _serviceProvider = services.AddViewModels()
                                           .AddNavigation()
                                           .AddDbContextFactory()
                                           .AddStores()
                                           .AddServices()
                                           .BuildServiceProvider();

                IDbContextFactory<DataContext> dbFactory = _serviceProvider.GetRequiredService<IDbContextFactory<DataContext>>();

                Directory.CreateDirectory("data");
                Directory.CreateDirectory("downloads");
                Directory.CreateDirectory("banners");
                Directory.CreateDirectory("songs");

                using (var dbContext = dbFactory.CreateDbContext())
                {
                    dbContext.Database.Migrate();
                }

                // Show splash screen immediately
                var splash = new SplashScreen();
                splash.Show();

                // Prepare main window in background
                var mainWindow = new MainWindow()
                {
                    DataContext = _serviceProvider.GetRequiredService<MainViewModel>()
                };

                // When splash closes, show main window
                splash.Closed += (s, args) =>
                {
                    MainWindow = mainWindow;
                    MainWindow.Show();
                };

                base.OnStartup(e);
            }
            catch (Exception ex)
            {
                string message = $"Startup Error: {ex.Message}\nStack Trace: {ex.StackTrace}";
                if (ex.InnerException != null)
                {
                    message += $"\nInner Exception: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}";
                }
                
                File.WriteAllText("startup_error.txt", message);
                MessageBox.Show(message, "Startup Error", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}
