using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Data;

namespace TranslatorApk.Logic.Converters
{
    public abstract class ConverterBase<TValueType> : IValueConverter
    {
        protected virtual TValueType NullForward { get; } = default;
        protected virtual object NullBackward { get; } = default;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return NullForward;

            if (value is TValueType typeVal)
                return Convert(typeVal, targetType, parameter, culture);

            ThrowInvalidTypeException(value);

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return NullBackward;

            if (value is TValueType typeVal)
                return ConvertBackObj(typeVal, targetType, parameter, culture);

            ThrowInvalidTypeException(value);

            return null;
        }

        private void ThrowInvalidTypeException(object value)
        {
            Trace.WriteLine(Environment.StackTrace);

            throw new ArgumentException($"Derived type: '{GetType().Name}'\n'{nameof(value)}' must be of type '{typeof(TValueType).FullName}' and actually is '{value.GetType().FullName}'");
        }

        protected abstract object Convert(TValueType value, Type targetType, object parameter, CultureInfo culture);

        protected virtual TValueType ConvertBackObj(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException($"'{GetType().FullName}' converter can't handle back conversation");
        }
    }
}
