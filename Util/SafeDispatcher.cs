using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Util {
    public static class SafeDispatcher {
        public static void Invoke(Action callback) {
            try {
                Application.Current?.Dispatcher.Invoke(() => {
                    callback.Invoke();
                });
            } catch (TaskCanceledException) { }
        }
    }
}
