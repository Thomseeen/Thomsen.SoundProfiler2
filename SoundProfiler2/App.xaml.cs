using System.Runtime.Versioning;
using System.Windows;

[assembly: SupportedOSPlatform("windows")]
namespace SoundProfiler2 {
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application {
        #region Application Overrides
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);

            MainView view = new();
            MainViewModel viewModel = new();

            view.DataContext = viewModel;
            view.Show();
        }
        #endregion Application Overrides
    }
}
