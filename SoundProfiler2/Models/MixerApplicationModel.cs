using System.Drawing;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

using Util.MVVM;

namespace SoundProfiler2.Models {
    public class MixerApplicationModel : BaseModel {
        #region Private Fields
        private int processId;
        private string deviceName;
        private string friendlyName;
        private string processName;
        private Icon applicationIcon;

        private string category;

        private float volumeLevel;
        #endregion Private Fields

        #region Public Properties
        public int ProcessId {
            get => processId;
            set { processId = value; OnPropertyChanged(); }
        }

        public string DeviceName {
            get => deviceName;
            set { deviceName = value; OnPropertyChanged(); }
        }

        public string ProcessName {
            get => processName;
            set {
                processName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(UnifiedProcessName));
            }
        }

        public string UnifiedProcessName => ProcessName.ToLowerInvariant().Replace(" ", "");

        public string FriendlyName {
            get => friendlyName;
            set { friendlyName = value; OnPropertyChanged(); }
        }

        public Icon ApplicationIcon {
            get => applicationIcon;
            set {
                applicationIcon = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ApplicationIconBitmapSource));
            }
        }

        public BitmapSource ApplicationIconBitmapSource => Imaging.CreateBitmapSourceFromHIcon(ApplicationIcon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());

        public string Category {
            get => category;
            set { category = value; OnPropertyChanged(); }
        }

        public float VolumeLevel {
            get => volumeLevel;
            set { volumeLevel = value; OnPropertyChanged(); }
        }
        #endregion Public Properties

        #region Base Overrides
        public override string ToString() {
            return $"{ProcessId} {DeviceName}:{ProcessName} - {base.ToString()}";
        }
        #endregion Base Overrides
    }
}
