using SoundProfiler2.ViewModels;
using SoundProfiler2.Views;

using System.Runtime.Versioning;
using System.Windows;
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

            viewModel = new MainViewModel();
            viewModel.Show();
        }

        protected override void OnExit(ExitEventArgs e) {
            base.OnExit(e);

            viewModel.Dispose();
        }
        #endregion Application Overrides
    }
}
