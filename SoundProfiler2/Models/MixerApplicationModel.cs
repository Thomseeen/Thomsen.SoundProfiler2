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
        private string deviceName;
        private string applicationName;
        private Icon applicationIcon;

        private int volumeLevel;
        #endregion Private Fields

        #region Public Properties
        public string DeviceName {
            get => deviceName;
            set { deviceName = value; OnPropertyChanged(); }
        }

        public string ApplicationName {
            get => applicationName;
            set { applicationName = value; OnPropertyChanged(); }
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

        public int VolumeLevel {
            get => volumeLevel;
            set { volumeLevel = value; OnPropertyChanged(); }
        }
        #endregion Public Properties

        #region Base Overrides
        public override int GetHashCode() {
            return deviceName.GetHashCode() + applicationName.GetHashCode();
        }

        public override bool Equals(object obj) {
            return obj is MixerApplicationModel model &&
                   DeviceName == model.DeviceName &&
                   ApplicationName == model.ApplicationName;
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
