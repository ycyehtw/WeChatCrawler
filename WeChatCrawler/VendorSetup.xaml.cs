using System.Windows;
using System.Windows.Data;

namespace WeChatCrawler {
  /// <summary>
  /// Interaction logic for VendorSetup.xaml
  /// </summary>
  public partial class VendorSetup : Window {
    public VendorCollection MyVendors;
    public VendorSetup() {
      InitializeComponent();
      MyVendors = (VendorCollection)(TryFindResource("MyVendors") as ObjectDataProvider)?.Data;
      MyVendors.Load();
    }

    private void SubmitButtonClick(object sender, RoutedEventArgs e) {
      using (var db = new Database()) {
        foreach (var vendor in MyVendors) {
          db.Update(vendor);
          WebApi.Update(vendor);
        }
      }
      this.Close();
    }

    private void CancelButtonClick(object sender, RoutedEventArgs e) {
      this.Close();
    }
  }
}
