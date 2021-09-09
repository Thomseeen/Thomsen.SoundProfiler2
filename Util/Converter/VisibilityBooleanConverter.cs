using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;

namespace Util.Converter {
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class VisibilityBooleanConverter : IValueConverter {
        #region Private Fields
        private bool not = false;
        private bool inverted = false;
        #endregion Private Fields

        #region Properties
        public bool Inverted {
            get => inverted;
            set => inverted = value;
        }

        public bool Not {
            get => not;
            set => not = value;
        }
        #endregion Properties

        #region Private Methods
        private object VisibilityToBool(object value) {
            if (!(value is Visibility))
                return DependencyProperty.UnsetValue;

            return (((Visibility)value) == Visibility.Visible) ^ Not;
        }

        private object BoolToVisibility(object value) {
            if (!(value is bool))
                return DependencyProperty.UnsetValue;

            return ((bool)value ^ Not) ? Visibility.Visible : Visibility.Collapsed;
        }
        #endregion Private Methods

        #region IValueConverter
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            return Inverted ? BoolToVisibility(value) : VisibilityToBool(value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return Inverted ? VisibilityToBool(value) : BoolToVisibility(value);
        }
        #endregion IValueConverter
    }
}
