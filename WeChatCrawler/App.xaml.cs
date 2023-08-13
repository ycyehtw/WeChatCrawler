using System.Windows;
using System.Windows.Threading;
using NLog;

namespace WeChatCrawler {
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application {
    private static readonly Logger Log = LogManager.GetCurrentClassLogger();
    private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
      if (Log.IsErrorEnabled) Log.Error(e.Exception, e.Exception.Message);
      MessageBox.Show($"程式發生錯誤: {e.Exception.Message}");
      e.Handled = true;
      this.Shutdown();
    }
  }
}
