using System;
using System.Windows;

namespace CyberAwarenessBot
{
    /// <summary>
    /// Main application entry point with global exception handling
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Override OnStartup to register global exception handlers for both UI and non-UI threads
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Handle non-UI/unhandled exceptions (e.g., background threads)
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var exception = args.ExceptionObject as Exception;
                MessageBox.Show($"Critical Error: {exception?.Message}\n\nPlease restart the application.",
                    "CyberGuard AI - Error", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            // Handle exceptions on the UI thread (Dispatcher)
            DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show($"UI Error: {args.Exception.Message}\n\nThe application will continue running.",
                    "CyberGuard AI", MessageBoxButton.OK, MessageBoxImage.Warning);
                args.Handled = true; // Mark as handled to prevent crash
            };
        }
    }
}