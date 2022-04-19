
using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

using Util.MVVM;

namespace Thomsen.SoundProfiler2.Models {
    public class MixerApplicationModel : BaseModel {
        #region Public Constants
        public const string UNMAPPED_CATEGORY = "unmapped";
        #endregion Public Constants

        #region Private Fields
        private int _processId;
        private string _deviceName;
        private string _friendlyName;
        private string _processName;
        private Icon _applicationIcon;

        private string _category;

        private float _volumeLevel;
        #endregion Private Fields

        #region Public Properties
        public int ProcessId {
            get => _processId;
            set { _processId = value; OnPropertyChanged(); }
        }

        public string DeviceName {
            get => _deviceName;
            set { _deviceName = value; OnPropertyChanged(); }
        }

        public string ProcessName {
            get => _processName;
            set {
                _processName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UnifiedProcessName));
            }
        }

        public string UnifiedProcessName => ProcessName.ToLowerInvariant().Replace(" ", "");

        public string FriendlyName {
            get => _friendlyName;
            set { _friendlyName = value; OnPropertyChanged(); }
        }

        public Icon ApplicationIcon {
            get => _applicationIcon;
            set {
                _applicationIcon = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ApplicationIconBitmapSource));
            }
        }

        public BitmapSource ApplicationIconBitmapSource => Imaging.CreateBitmapSourceFromHIcon(ApplicationIcon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

        public string Category {
            get => _category;
            set {
                _category = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsVolumeEditable));
            }
        }

        public float VolumeLevel {
            get => _volumeLevel;
            set { _volumeLevel = value; OnPropertyChanged(); }
        }

        public bool IsVolumeEditable => Category == UNMAPPED_CATEGORY;
        #endregion Public Properties

        #region Base Overrides
        public override string ToString() {
            return $"{ProcessId} {DeviceName}:{ProcessName} - {base.ToString()}";
        }
        #endregion Base Overrides
    }
}
