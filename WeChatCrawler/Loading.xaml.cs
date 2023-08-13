using System;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace WeChatCrawler {
  /// <summary>
  /// Interaction logic for Loading.xaml
  /// </summary>
  public partial class Loading : Window {
    Config config;
    BackgroundWorker worker;
    public Loading(Config config) {
      this.config = config;
      InitializeComponent();
    }
    private void Window_ContentRendered(object sender, EventArgs e) {
      worker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true };
      worker.DoWork += Worker_DoWork;
      worker.ProgressChanged += Worker_ProgressChanged;
      worker.RunWorkerCompleted += Worker_RunWorkerCompleted;
      worker.RunWorkerAsync();
    }
    private void Worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
      if (e.Error != null) {
        MessageBox.Show($"發生錯誤: {e.Error.Message}");
      }
      this.Close();
    }
    private void Worker_ProgressChanged(object sender, ProgressChangedEventArgs e) {
      LoadingProgressBar.Value = e.ProgressPercentage;
      StatusTextBlock.Text = $"{e.ProgressPercentage}%";
    }
    // 清除過期的貼文＆圖片
    private void Clear(int days) {
      var thisDay = DateTime.Today.AddDays(-days);
      using (var db = new Database()) {
        var postIds = db.GetPostIdsAfterDate(thisDay);
        var total = postIds.Count;
        var count = 0;
        foreach (var id in postIds) {
          var pictures = db.GetPicturesByPost(id);
          foreach (var picture in pictures) {
            var filename = $"{Constant.ThumbnailFolder}/{picture.Name}";
            if (File.Exists(filename))
              File.Delete(filename);
            filename = $"{Constant.PictureFolder}/{picture.Name}";
            if (File.Exists(filename)) File.Delete(filename);
          }
          db.DeletePost(id);
          worker.ReportProgress(++count * 10 / total);
        }
      }
    }
    // 更新本地快取
    private void Worker_DoWork(object sender, DoWorkEventArgs e) {
      Clear(config.CacheLifeTime);
      var vendors = WebApi.GetAllVendors();
      using (var db = new Database()) {
        foreach (var vendor in vendors) {
          if (db.IsExist(vendor))
            db.Update(vendor);
          else
            db.Insert(vendor);
        }
        var lastPostId = db.GetLastPostId();
        var total = WebApi.GetPostsCountAfter(lastPostId);
        int count = 0;
        if (total > 0) {
          vendors = db.GetVendors(true);
          foreach (var vendor in vendors) {
            lastPostId = db.GetLastPostIdByVendor(vendor.VendorId);
            var posts = WebApi.GetPostsByVendorAfterPost(vendor.VendorId, lastPostId);
            foreach (var post in posts) {
              db.Insert(post);
              foreach (var file in post.Pictures) {
                WebApi.DownloadThumbnail(file.Name);
              }
              if (worker.CancellationPending == true) {
                e.Cancel = true;
                return;
              }
              worker.ReportProgress(++count * 90 / total + 10);
            }
          }
        }
      }
    }
    private void Close(object sender, RoutedEventArgs e) {
      worker.CancelAsync();
    }

    private void Window_Closing(object sender, CancelEventArgs e) {
      worker.CancelAsync();
      if (worker.IsBusy) e.Cancel = true;
    }
  }
}
