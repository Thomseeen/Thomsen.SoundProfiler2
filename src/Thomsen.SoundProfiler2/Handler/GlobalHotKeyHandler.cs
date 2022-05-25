
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace Thomsen.SoundProfiler2.Handler {
    public class GlobalHotKeyHandler : IDisposable {
        #region WinAPI
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, int mod, int key);
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        #endregion WinAPI

        #region Private Constants
        private const int WM_HOTKEY = 0x0312;
        #endregion Private Constants

        #region Private Fields
        private readonly ModifierKeys _modifier;
        private readonly Key _key;

        private Window? _view;

        private ICommand? _command;
        private object? _commandParameter;

        private bool isDisposed;
        #endregion Private Fields

        #region Constructor
        public GlobalHotKeyHandler(ModifierKeys modifier, Key key) {
            _modifier = modifier;
            _key = key;
        }
        #endregion Constructor

        #region Public Methods
        public void Register(Window view, ICommand command, object? commandParameter = null) {
            _command = command;
            _commandParameter = commandParameter;
            _view = view;

            if (PresentationSource.FromVisual(view) is not HwndSource source) {
                throw new InvalidOperationException("No source could be created from the view");
            }

            source.AddHook(WndProc);

            if (!RegisterHotKey(new WindowInteropHelper(view).Handle, GetHashCode(), (int)_modifier, KeyInterop.VirtualKeyFromKey(_key))) {
                throw new InvalidOperationException("HotKey could not be registed");
            }
        }

        public void Unregister() {
            if (!UnregisterHotKey(new WindowInteropHelper(_view).Handle, GetHashCode())) {
                /* We don't care about failed unregistering */
            }
        }
        #endregion Public Methods

        #region Private Methods
        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) {
            if (_command is null) {
                return IntPtr.Zero;
            }

            if (msg == WM_HOTKEY) {
                int iparam = lParam.ToInt32();

                int rawMod = iparam & 0x0000FFFF;
                int rawKey = (int)(iparam & 0xFFFF0000) >> 16;

                ModifierKeys modifier = (ModifierKeys)rawMod;
                Key key = KeyInterop.KeyFromVirtualKey(rawKey);

                if (modifier == _modifier && key == _key) {
                    _command.Execute(_commandParameter);
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
