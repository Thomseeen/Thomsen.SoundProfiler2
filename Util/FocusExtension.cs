using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Util {
    public static class FocusExtension {
        #region Properties
        public static readonly DependencyProperty IsFocusedProperty = DependencyProperty.RegisterAttached(nameof(FrameworkElement.IsFocused), typeof(bool?), typeof(FocusExtension), new FrameworkPropertyMetadata(IsFocusedChanged) { BindsTwoWayByDefault = true });
        #endregion Properties

        #region Public Methods
        public static bool? GetIsFocused(DependencyObject element) {
            if (element == null) {
                throw new ArgumentNullException(nameof(element));
            }

            return (bool?)element.GetValue(IsFocusedProperty);
        }

        public static void SetIsFocused(DependencyObject element, bool? value) {
            if (element == null) {
                throw new ArgumentNullException(nameof(element));
            }

            element.SetValue(IsFocusedProperty, value);
        }
        #endregion Public Methods

        #region Private Methods
        private static void IsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var frameworkElement = (FrameworkElement)d;

            if (e.OldValue == null) {
                frameworkElement.GotFocus += FrameworkElement_GotFocus;
                frameworkElement.LostFocus += FrameworkElement_LostFocus;
            }

            if (!frameworkElement.IsVisible) {
                frameworkElement.IsVisibleChanged += new DependencyPropertyChangedEventHandler(FrameworkElement_IsVisibleChanged);
            }

            if (e.NewValue != null && (bool)e.NewValue) {
                frameworkElement.Focus();
            }
        }
        #endregion Private Methods


        #region EventHandler
        private static void FrameworkElement_GotFocus(object sender, RoutedEventArgs e) {
            ((FrameworkElement)sender).SetValue(IsFocusedProperty, true);
        }

        private static void FrameworkElement_LostFocus(object sender, RoutedEventArgs e) {
            ((FrameworkElement)sender).SetValue(IsFocusedProperty, false);
        }

        private static void FrameworkElement_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e) {
            var frameworkElement = (FrameworkElement)sender;
            if (frameworkElement.IsVisible && (bool)frameworkElement.GetValue(IsFocusedProperty)) {
                frameworkElement.IsVisibleChanged -= FrameworkElement_IsVisibleChanged;
                frameworkElement.Focus();
            }
        }
        #endregion EventHandler
    }
}
