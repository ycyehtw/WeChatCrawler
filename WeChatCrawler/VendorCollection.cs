using System.Collections.ObjectModel;

namespace WeChatCrawler {
  public class VendorCollection : ObservableCollection<Vendor> {
    public void Load() {
      Clear();
      using (var db = new Database()) {
        var vendors = db.GetVendors(true);
        foreach (var vendor in vendors)
          Add(vendor);
      }
    }
  }
}
