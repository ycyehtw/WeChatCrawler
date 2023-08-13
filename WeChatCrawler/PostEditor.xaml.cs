using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace WeChatCrawler {
  /// <summary>
  /// Interaction logic for PostEditor.xaml
  /// </summary>
  public partial class PostEditor : Window {
    private Post post;
    public PictureView wPictureView;
    public PostEditor(Post post) {
      InitializeComponent();
      this.post = post;
      DateTextBlock.Text = post.Date.ToString("yyyy/MM/dd HH:mm");
      ContentTextBox.Text = post.Content;
      BindingOperations.SetBinding(PicturesListBox, ListBox.ItemsSourceProperty, new Binding() { Source = post, Path = new PropertyPath("Pictures") });
      BindingOperations.SetBinding(KrwPriceTextBox, TextBox.TextProperty, new Binding() { Source = post, Path = new PropertyPath("KrwPrice") });
      BindingOperations.SetBinding(TwdPriceTextBox, TextBox.TextProperty, new Binding() { Source = post, Path = new PropertyPath("TwdPrice") });
      BindingOperations.SetBinding(ColorTextBox, TextBox.TextProperty, new Binding() { Source = post, Path = new PropertyPath("Color") });
      BindingOperations.SetBinding(SpecTextBox, TextBox.TextProperty, new Binding() { Source = post, Path = new PropertyPath("Spec") });
      BindingOperations.SetBinding(MaterialTextBox, TextBox.TextProperty, new Binding() { Source = post, Path = new PropertyPath("Material") });
    }

    private void SubmitButton_Click(object sender, RoutedEventArgs e) {
      ((MainWindow)Owner).SetPostStatus(new ArrayList { post }, PostStatus.Edited);
      Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e) {
      ((MainWindow)Owner).RestorePost(post);
      Close();
    }

    private void PicturesListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e) {
      var picture = (Post.Picture)((ListBox)sender).SelectedItem;
      if (wPictureView == null) wPictureView = new PictureView { Owner = this };
      wPictureView.SelectedPicture = picture;
      wPictureView.ShowDialog();
    }
  }
}
