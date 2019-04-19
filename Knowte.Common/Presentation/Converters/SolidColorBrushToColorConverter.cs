﻿using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Knowte.Common.Presentation.Converters
{
    public class SolidColorBrushToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            if (value.GetType() != typeof(SolidColorBrush))
            {
                throw new InvalidOperationException("Unsupported type [" + value.GetType().Name + "], SolidColorBrushToColorConverter.Convert()");
            }

            return ((SolidColorBrush)value).Color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
