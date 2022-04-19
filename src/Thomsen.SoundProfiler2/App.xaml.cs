using System;
using System.IO;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using Thomsen.SoundProfiler2.ViewModels;

using Util.MVVM;

[assembly: SupportedOSPlatform("windows")]
namespace SoundProfiler2 {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        #region Private Fields
        private BaseViewModel viewModel;
        #endregion Private Fields

        #region Application Overrides
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException += App_UnhandledException;
            DispatcherUnhandledException += Dispatcher_UnhandledException;
            TaskScheduler.UnobservedTaskException += Task_UnhandledException;

            viewModel = new MainViewModel();
            viewModel.Show();
        }

        protected override void OnExit(ExitEventArgs e) {
            base.OnExit(e);

            viewModel.Dispose();
        }
        #endregion Application Overrides

        #region Crash Handling
        private void App_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            WriteCrashDump("App", (Exception)e.ExceptionObject);
        }

        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            WriteCrashDump("Dispatcher", e.Exception);
        }

        private void Task_UnhandledException(object sender, UnobservedTaskExceptionEventArgs e) {
            WriteCrashDump("Task", e.Exception);
        }

        private static void WriteCrashDump(string facility, Exception ex) {
            string path = $"CrashDump_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.log";
            using StreamWriter writer = File.CreateText(path);

            writer.WriteLine($"--- Fatal error in {facility} ---");
            writer.WriteLine($"Time local: {DateTime.Now}");
            writer.WriteLine($"Time UTC: {DateTime.UtcNow}\r\n");
            writer.WriteLine($"Message: {ex.Message}");
            writer.WriteLine($"Source: {ex.Source}");
            writer.WriteLine($"TargetSite: {ex.TargetSite}");
            writer.WriteLine($"Stack Trace:");
            writer.WriteLine($"---");
            writer.Write($"{ex.StackTrace}");
            writer.WriteLine($"---\r\n");
            writer.WriteLine($"Full Exception:");
            writer.WriteLine($"---");
            writer.WriteLine($"{ex}");
            writer.WriteLine($"---");
        }
        #endregion Crash Handling
    }
}
