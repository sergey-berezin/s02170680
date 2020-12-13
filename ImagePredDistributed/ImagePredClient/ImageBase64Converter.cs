
using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

namespace ImagePredClient
{
    public class ImageBase64Converter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if (value is string base64str && targetType == typeof(IBitmap))
            {
                return new Bitmap(new MemoryStream(System.Convert.FromBase64String(base64str)));
            }
            throw new NotSupportedException();
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }      
}