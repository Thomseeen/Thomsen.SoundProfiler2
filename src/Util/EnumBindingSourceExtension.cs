using System;
using System.Windows.Markup;

namespace Util {
    public class EnumBindingSourceExtension : MarkupExtension {
        #region Private Fields
        private Type enumType;
        #endregion Private Fields

        #region Public Properties
        public Type EnumType {
            get => enumType;
            set => enumType = value;
        }
        #endregion Public Properties

        #region Constructors
        public EnumBindingSourceExtension() { }

        public EnumBindingSourceExtension(Type enumType) {
            EnumType = enumType;
        }
        #endregion Constructors

        #region MarkupExtension
        public override object ProvideValue(IServiceProvider serviceProvider) {
            if (enumType is not null) {
                return Enum.GetValues(enumType);
            } else {
                return default(object);
            }
        }
        #endregion MarkupExtension
    }
}