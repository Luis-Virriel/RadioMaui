using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace mauiApp1Prueba.Converters
{
    public class WidthToHeightConverter : IValueConverter
    {
        // Convierte el ancho de la página en altura 16:9 para la imagen
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double width)
                return width * 9 / 16; // ratio 16:9
            return 180; // fallback
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
