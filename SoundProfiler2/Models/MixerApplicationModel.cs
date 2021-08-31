using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace SoundProfiler2.Models {
    public class MixerApplicationModel : INotifyPropertyChanged {
        #region Private Fields
        private int processId;
        private string deviceName;
        private string friendlyName;
        private string processName;
        private Icon applicationIcon;

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
            set { processName = value; OnPropertyChanged(); }
        }

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

        public BitmapSource ApplicationIconBitmapSource {
            get => Imaging.CreateBitmapSourceFromHIcon(ApplicationIcon.Handle, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
        }

        public float VolumeLevel {
            get => volumeLevel;
            set { volumeLevel = value; OnPropertyChanged(); }
        }
        #endregion Public Properties

        #region Base Overrides
        public override int GetHashCode() {
            return ProcessId;
        }

        public override bool Equals(object obj) {
            return obj is MixerApplicationModel model &&
                   DeviceName == model.DeviceName &&
                   ProcessId == model.ProcessId &&
                   ProcessName == model.ProcessName;
        }

        public override string ToString() {
            return $"{ProcessId} {DeviceName}:{ProcessName} - {base.ToString()}";
        }
        #endregion Base Overrides

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = "") {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion INotifyPropertyChanged
    }
}
