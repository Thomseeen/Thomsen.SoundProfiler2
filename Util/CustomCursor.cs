using System;
using System.Windows.Input;

namespace Util {
    /// <summary>
    /// To be used with the using-pattern to change the cursor appearance
    /// </summary>
    public class CustomCursor : IDisposable {
        #region Private Fields
        private readonly Cursor savedCursor;
        #endregion Private Fields

        #region Constructor
        public CustomCursor(Cursor newCursor) {
            savedCursor = Mouse.OverrideCursor;
            Mouse.OverrideCursor = newCursor;
        }
        #endregion Constructor

        #region IDisposable
        public void Dispose() {
            Mouse.OverrideCursor = savedCursor;
            GC.SuppressFinalize(this);
        }
        #endregion IDisposable
    }

    /// <summary>
    /// To be used with the using-pattern to change the cursor appearance to Wait
    /// </summary>
    public class WaitCursor : CustomCursor {
        public WaitCursor() : base(Cursors.Wait) { }
    }
}