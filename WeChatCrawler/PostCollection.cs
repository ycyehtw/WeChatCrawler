using System;
using System.Collections.ObjectModel;

namespace WeChatCrawler {
  public class PostCollection : ObservableCollection<Post> {
    private Vendor vendor;
    public PostCollection() { }
    public PostCollection(Vendor vendor) {
      this.vendor = vendor;
      Update();
    }

    public Vendor Vendor {
      set {
        vendor = value;
        Update();
      }
      get { return vendor; }
    }
    public PostStatus Status { get; set; }
    public DateTime From { set; get; }
    public DateTime To { set; get; }
    private void Update() {
      Clear();
      using (var db = new Database()) {
        var posts = db.GetPostsByVendorAndStatusAndDate(vendor.VendorId, (int)Status, From, To);
        foreach (var post in posts) {
          Add(post);
        }
      }
    }
  }
}
