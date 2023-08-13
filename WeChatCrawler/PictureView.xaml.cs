using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using WeChatCrawler;

namespace WeChatCrawler {
  /// <summary>
  /// Interaction logic for PictureView.xaml
  /// </summary>
  public partial class PictureView : Window {
    BitmapImage image;
    public PictureView() {
      InitializeComponent();
    }
    public Post.Picture picture;
    public Post.Picture SelectedPicture {
      get { return picture; }
      set {
        picture = value;
        var file = new FileInfo($"{Constant.PictureFolder}/{picture.Name}");
        if (!file.Exists)
          WebApi.DownloadPicture(picture.Name);
        image = new BitmapImage(new Uri(file.FullName, UriKind.Absolute));
        image.Freeze();
        ViewedPicture.Width = image.Width;
        ViewedPicture.Height = image.Height;
        ViewedPicture.Source = image;
      }
    }

    private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
      image = null;
      GC.Collect();
      this.Hide();
      e.Cancel = true;
    }
  }
}
