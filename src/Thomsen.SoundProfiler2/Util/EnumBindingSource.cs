using System;
using System.Windows.Markup;

namespace Thomsen.WpfTools.Util {
    public class EnumBindingSource : MarkupExtension {
        #region Private Fields
        private Type _enumType = null!;
        #endregion Private Fields

        #region Public Properties
        public Type EnumType {
            get => _enumType;
            set {
                if (value != _enumType) {
                    if (value is not null) {
                        Type enumType = Nullable.GetUnderlyingType(value) ?? value;

                        if (!enumType.IsEnum) {
                            throw new ArgumentException("Type must be for an Enum.");
                        }

                        _enumType = value;
                    }
                }
            }
        }
        #endregion Public Properties

        #region Constructors
        public EnumBindingSource() {
        }

        public EnumBindingSource(Type enumType) {
            EnumType = enumType;
        }
        #endregion Constructors

        #region MarkupExtension
        public override object? ProvideValue(IServiceProvider serviceProvider) {
            if (_enumType is null) {
                throw new InvalidOperationException("The EnumType must be specified.");
            }

            Type actualEnumType = Nullable.GetUnderlyingType(_enumType) ?? _enumType;
            Array enumValues = Enum.GetValues(actualEnumType);

            if (actualEnumType == _enumType) {
                return enumValues;
            }

            Array tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
            enumValues.CopyTo(tempArray, 1);
            return tempArray;
        }
        #endregion MarkupExtension
    }
}