using System;
using System.Threading.Tasks;
using System.Windows;

namespace Thomsen.WpfTools.Util {
    public static class SafeDispatcher {
        public static void SafeInvoke(Action callback) {
            try {
                Application.Current?.Dispatcher.Invoke(() => {
                    callback.Invoke();
                });
            } catch (TaskCanceledException) { }
        }
    }
}
