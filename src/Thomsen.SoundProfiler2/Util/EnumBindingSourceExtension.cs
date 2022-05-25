using System;
using System.Windows.Markup;

namespace Thomsen.WpfTools.Util {
    public class EnumBindingSourceExtension<T> : MarkupExtension {
        #region Private Fields
        private Type _enumType;
        #endregion Private Fields

        #region Public Properties
        public Type EnumType {
            set => _enumType = value;
        }
        #endregion Public Properties

        #region Constructors
        public EnumBindingSourceExtension() {
            _enumType = typeof(T);
        }

        public EnumBindingSourceExtension(T _) {
            _enumType = typeof(T);
        }
        #endregion Constructors

        #region MarkupExtension
        public override object? ProvideValue(IServiceProvider serviceProvider) {
            if (_enumType is not null) {
                return Enum.GetValues(_enumType);
            } else {
                return default;
            }
        }
        #endregion MarkupExtension
    }
}