using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace SoundProfiler2.Handler {
    public class GlobalHotKeyHandler : IDisposable {
        #region WinAPI
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int mod, int key);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        #endregion WinAPI

        #region Private Constants
        private const int WM_HOTKEY = 0x0312;
        #endregion Private Constants

        #region Private Fields
        private ModifierKeys modifier;
        private Key key;

        private Window view;

        private ICommand command;
        private object commandParameter;

        private bool isDisposed;
        #endregion Private Fields

        #region Constructor
        public GlobalHotKeyHandler(ModifierKeys modifier, Key key) {
            this.modifier = modifier;
            this.key = key;
        }
        #endregion Constructor

        #region Public Methods
        public void Register(Window view, ICommand command, object commandParameter = null) {
            this.command = command;
            this.commandParameter = commandParameter;
            this.view = view;

            if (PresentationSource.FromVisual(view) is not HwndSource source) {
                throw new InvalidOperationException("No source could be created from the view");
            }

            source.AddHook(WndProc);

            if (!RegisterHotKey(new WindowInteropHelper(view).Handle, GetHashCode(), (int)modifier, KeyInterop.VirtualKeyFromKey(key))) {
                throw new InvalidOperationException("HotKey could not be registed");
            }
        }

        public void Unregister() {
            if (!UnregisterHotKey(new WindowInteropHelper(view).Handle, GetHashCode())) {
                throw new InvalidOperationException("HotKey could not be unregisted");
            }
        }
        #endregion Public Methods

        #region Private Methods
        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            if (msg == WM_HOTKEY) {
                int iparam = lParam.ToInt32();

                int rawMod = iparam & 0x0000FFFF;
                int rawKey = (int)(iparam & 0xFFFF0000) >> 16;

                ModifierKeys modifier = (ModifierKeys)rawMod;
                Key key = KeyInterop.KeyFromVirtualKey(rawKey);

                if (modifier == this.modifier && key == this.key) {
                    command.Execute(commandParameter);
                }
            }

            return IntPtr.Zero;
        }
        #endregion Private Methods

        #region IDisposable
        protected virtual void Dispose(bool disposing) {
            if (!isDisposed) {
                if (disposing) {
                    Unregister();
                }

                isDisposed = true;
            }
        }

        public void Dispose() {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
        #endregion IDisposable
    }
}
