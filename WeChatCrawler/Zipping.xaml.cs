using System;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading;
using System.Windows;
using Microsoft.Win32;

namespace WeChatCrawler {
  /// <summary>
  /// Interaction logic for Zipping.xaml
  /// </summary>
  public partial class Zipping : Window {
    private readonly ImageProcessor imageProcessor;
    private BackgroundWorker worker;
    private readonly Vendor vendor;
    private readonly IList posts;
    private string prefix;
    private string zipFilename;
    public Zipping(Vendor vendor, IList posts) {
      InitializeComponent();
      this.vendor = vendor;
      this.posts = posts;
      imageProcessor = new ImageProcessor();
    }

    private System.Drawing.Image GetImage(string filename) {
      var file = new FileInfo($"{Constant.PictureFolder}/{filename}");
      System.Drawing.Image image = null;
      var count = 0;
      do {
        if (!file.Exists)
          WebApi.DownloadPicture(filename);
        try {
          image = System.Drawing.Image.FromFile(file.FullName);
        } catch (OutOfMemoryException) {
          file.Delete();
        }
      } while (image == null && count++ < 3);

      return image;
    }
    private void Window_ContentRendered(object sender, System.EventArgs e) {
      var today = DateTime.Today;
      if (posts.Count > 0) {
        prefix = $"{vendor.Code}{today:MMdd}";
        zipFilename = $"{prefix}-{posts.Count}款.zip";
      } else {
        MessageBox.Show("沒有選擇要匯出的項目");
        return;
      }
      var saveFileDialog = new SaveFileDialog {
        FileName = zipFilename
      };
      if (saveFileDialog.ShowDialog() == true) {
        this.Show();
        // 檢查並清空暫存檔
        var directory = new DirectoryInfo(Constant.TempFolder);
        if (directory.Exists) {
          var files = directory.GetFiles();
          foreach (var file in files)
            file.Delete();
        } else {
          directory.Create();
        }
        zipFilename = saveFileDialog.FileName;
        worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
        worker.DoWork += Worker_DoWork;
        worker.ProgressChanged += Worker_ProgressChanged;
        worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
        worker.RunWorkerAsync();
      }
    }
    private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
      if (e.Error == null) {
        Thread.Sleep(500);
        this.Close();
      }
    }

    private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
      StatusProgressBar.Value = e.ProgressPercentage;
    }

    private void Worker_DoWork(object sender, DoWorkEventArgs e) {
      using (var db = new Database()) {
        for (var i = 0; i < posts.Count; i++) {
          var post = (Post)posts[i];
          int count = 0;
          for (var j = 0; j < post.Pictures.Count; j++) {
            var picture = post.Pictures[j];
            var text = $"{prefix} ({j + 1})";
            using (var image = GetImage(picture.Name)) {
              if (image == null) continue;
              imageProcessor.DrawLogoAndText(image, text);
              var sb = new StringBuilder($"{Constant.TempFolder}/{i + 1}-{post.KrwPrice / 1000}");
              if (post.Spec != null && post.Spec.Length > 0) sb.Append($"-{post.Spec}");
              if (post.Material != null && post.Material.Length > 0) sb.Append($"-{post.Material}");
              if (post.Color != null && post.Color.Length > 0) sb.Append($"-{post.Color}");
              sb.Append($"-{post.TwdPrice} ({++count}).jpg");
              image.Save(sb.ToString());
            }
          }
          post.Status = PostStatus.Exported;
          db.Update(post);
          WebApi.Update(post);
          worker.ReportProgress(i * 100 / posts.Count);
        }
      }

      // 加入壓縮檔
      var zipFile = new FileInfo(zipFilename);
      if (zipFile.Exists) zipFile.Delete();
      ZipFile.CreateFromDirectory(Constant.TempFolder, zipFile.FullName, CompressionLevel.NoCompression, false);
    }

    private void Close(object sender, RoutedEventArgs e) {
      worker.CancelAsync();
    }

    private void Window_Closing(object sender, CancelEventArgs e) {
      worker.CancelAsync();
    }
  }
}
