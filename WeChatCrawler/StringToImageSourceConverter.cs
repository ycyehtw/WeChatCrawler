using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using WeChatCrawler;

namespace WeChatCrawler {
  public class StringToImageSourceConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      string strValue = value as string;
      if (strValue != null) {
        Uri uri = new Uri($"{Directory.GetCurrentDirectory()}/{Constant.ThumbnailFolder}/{strValue}", UriKind.Absolute);
        var image = new BitmapImage(uri);
        //var image = new BitmapImage(new Uri($"/Thumbnail/{strValue}", UriKind.Relative));
        return image;
      }
      throw new InvalidOperationException("Unexpected value in converter");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}
