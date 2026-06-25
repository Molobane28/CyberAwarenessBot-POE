// Summary of comments:
// - using directives import required namespaces.
// - The `App` class derives from `Application` to configure startup behavior.
// - `OnStartup` is overridden to register global exception handlers for both UI and non-UI threads.
// - `AppDomain.CurrentDomain.UnhandledException` shows a critical error dialog and advises restart.
// - `DispatcherUnhandledException` shows a UI warning and marks the exception as handled so the app continues.

using System; // Basic system types and Exception
using System.Windows; // WPF application types like Application and MessageBox

namespace CyberAwarenessBot // Root namespace for the application
{
    public partial class App : Application // App class inherits from WPF Application
    {
        protected override void OnStartup(StartupEventArgs e) // Override to run custom startup logic
        {
            base.OnStartup(e); // Call base implementation to ensure normal startup behavior

            AppDomain.CurrentDomain.UnhandledException += (sender, args) => // Handle non-UI/unhandled exceptions
            {
                var exception = args.ExceptionObject as Exception; // Extract Exception from event args
                MessageBox.Show($"Critical Error: {exception?.Message}\n\nPlease restart the application.",
                    "CyberGuard AI - Error", MessageBoxButton.OK, MessageBoxImage.Error); // Show critical error dialog
            }; // End AppDomain unhandled exception handler

            DispatcherUnhandledException += (sender, args) => // Handle exceptions on the UI thread
            {
                MessageBox.Show($"UI Error: {args.Exception.Message}\n\nThe application will continue running.",
                    "CyberGuard AI", MessageBoxButton.OK, MessageBoxImage.Warning); // Inform user and warn the app will continue
                args.Handled = true; // Mark exception as handled to prevent application crash
            }; // End DispatcherUnhandledException handler
        } // End OnStartup override
    } // End App class
} // End namespace
