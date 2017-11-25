using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;


namespace WpfFileExplorer
{

    [ValueConversion(typeof(string), typeof(BitmapImage))]

    class HeaderToImageConverter : IValueConverter
    {

        public static HeaderToImageConverter Instance = new HeaderToImageConverter();
        //COnvert a full path to a specific image type of a drive; folder or file

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = (string)value;

            if (path == null)
            {
                return null;
            }

            var name = MainWindow.GetFileFolderName(path);

            var image = "file.png";

            if (string.IsNullOrEmpty(name))
            {
                image = "drive.png";
            }
            else if (new FileInfo(path).Attributes.HasFlag(FileAttributes.Directory))
            {
                image = "folder.png";
            }

            return new BitmapImage(new Uri($"pack://application:,,,/Image/{image}"));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
