using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Newtonsoft.Json;

namespace WeChatCrawler {
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window {
    public VendorCollection MyVendors;
    public PostCollection Posts;
    public VendorSetup wVendorSetup;
    public PictureView wPictureView;

    public MainWindow() {
      InitializeComponent();
      // 讀取設定檔
      var config = JsonConvert.DeserializeObject<Config>(File.ReadAllText("Config.json"));
      WebApi.WebApiServer = config.WebApiServer;
      WebApi.User = config.User;
      // 不檢查 SSL 憑證合法性
      ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(delegate { return true; });
      var loadingWindow = new Loading(config);
      loadingWindow.ShowDialog();
      MyVendors = (VendorCollection)(Application.Current.Resources["MyVendors"] as ObjectDataProvider)?.Data;
      MyVendors.Load();
      Posts = (PostCollection)(Application.Current.Resources["Posts"] as ObjectDataProvider)?.Data;
      var today = DateTime.Today;
      DateFromDatePicker.SelectedDate = today.AddDays(-3);
      DateToDatePicker.SelectedDate = today;
    }

    private void UpdatePostListView() {
      if (VendorsListBox != null && VendorsListBox.SelectedItem != null) {
        Posts.Status = (PostStatus)PostStutusComboBox.SelectedItem;
        Posts.From = (DateTime)DateFromDatePicker.SelectedDate;
        Posts.To = (DateTime)DateToDatePicker.SelectedDate;
        Posts.Vendor = (Vendor)VendorsListBox.SelectedItem;
        if (Posts.Count > 0) PostsListBox.SelectedItem = Posts[0];
        SelectAllButton.Content = SelectAll;
      }
    }
    private void VendorList_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      UpdatePostListView();
    }

    private void PostStutusComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      var status = (PostStatus)PostStutusComboBox.SelectedItem;
      switch (status) {
        case PostStatus.New:
          if (DeleteButton != null) DeleteButton.IsEnabled = true;
          if (RestoreButton != null) RestoreButton.IsEnabled = false;
          break;
        case PostStatus.Edited:
          if (DeleteButton != null) DeleteButton.IsEnabled = true;
          if (RestoreButton != null) RestoreButton.IsEnabled = false;
          break;
        case PostStatus.Exported:
          if (DeleteButton != null) DeleteButton.IsEnabled = true;
          if (RestoreButton != null) RestoreButton.IsEnabled = false;
          break;
        case PostStatus.Deleted:
          if (DeleteButton != null) DeleteButton.IsEnabled = false;
          if (RestoreButton != null) RestoreButton.IsEnabled = true;
          break;
      }
      UpdatePostListView();
    }

    private void ListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      var picture = (Post.Picture)((ListBox)sender).SelectedItem;
      if (wPictureView == null) wPictureView = new PictureView { Owner = this };
      wPictureView.SelectedPicture = picture;
      wPictureView.ShowDialog();
    }

    private void SetupButton_Click(object sender, RoutedEventArgs e) {
      var VendorSetupWindow = new VendorSetup { Owner = this };
      VendorSetupWindow.ShowDialog();
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e) {
      SetPostStatus(PostsListBox.SelectedItems, PostStatus.Deleted);
    }

    private void RestoreButton_Click(object sender, RoutedEventArgs e) {
      SetPostStatus(PostsListBox.SelectedItems, PostStatus.New);
    }
    public void SetPostStatus(IList items, PostStatus status) {
      var posts = new Post[items.Count];
      items.CopyTo(posts, 0);
      using (var db = new Database()) {
        foreach (var post in posts) {
          post.Status = status;
          if ((PostStatus)PostStutusComboBox.SelectedItem != status)
            Posts.Remove(post);
          db.Update(post);
          WebApi.Update(post);
        }
      }
    }

    private void ExportButton_Click(object sender, RoutedEventArgs e) {
      var zippingWindow = new Zipping((Vendor)VendorsListBox.SelectedItem, PostsListBox.SelectedItems) {
        Owner = this
      };
      zippingWindow.ShowDialog();
    }

    private void SubmitButton_Click(object sender, RoutedEventArgs e) {
      SetPostStatus(PostsListBox.SelectedItems, PostStatus.Edited);
    }
    public void RestorePost(Post post) {
      using (var db = new Database()) {
        var orgPost = db.GetPost(post.Id);
        post.KrwPrice = orgPost.KrwPrice;
        post.TwdPrice = orgPost.TwdPrice;
        post.Color = orgPost.Color;
        post.Spec = orgPost.Spec;
        post.Material = orgPost.Material;
      }
    }
    private void CancelButton_Click(object sender, RoutedEventArgs e) {
      RestorePost((Post)PostsListBox.SelectedItem);
    }
    private const string SelectAll = "全選";
    private const string UnSelectAll = "全不選";
    private void SelectAllButton_Click(object sender, RoutedEventArgs e) {
      switch (SelectAllButton.Content) {
        case SelectAll:
          PostsListBox.SelectAll();
          SelectAllButton.Content = UnSelectAll;
          break;
        case UnSelectAll:
          PostsListBox.UnselectAll();
          SelectAllButton.Content = SelectAll;
          break;
      }
    }

    private void DatePicker_SelectedDateChanged(object sender, SelectionChangedEventArgs e) {
      UpdatePostListView();
    }

    private void EditPostButton_Click(object sender, RoutedEventArgs e) {
      var postEditorWindow = new PostEditor((Post)((Button)sender).DataContext) {
        Owner = this
      };
      postEditorWindow.ShowDialog();
    }

    private void PostsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      SummaryTextBlock.Text = $"({PostsListBox.SelectedItems.Count}/{PostsListBox.Items.Count})";
    }
  }
}
