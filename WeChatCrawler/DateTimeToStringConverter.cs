using System;
using System.Globalization;
using System.Windows.Data;

namespace WeChatCrawler {
  class DateTimeToStringConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
      DateTime dt = (DateTime)value;
      return $"{dt:g}";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
      throw new NotImplementedException();
    }
  }
}
