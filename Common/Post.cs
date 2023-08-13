using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace WeChatCrawler {
  public class Post : INotifyPropertyChanged {
    public int Id { get; set; }
    public int VendorId { get; set; }
    public string Content { get; set; }
    public int CountOfPictures { get; set; }
    public int CountOfVideos { get; set; }
    private int krwPrice;
    public int KrwPrice {
      get { return krwPrice; }
      set {
        krwPrice = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(KrwPrice)));
      }
    }
    private int twdPrice;
    public int TwdPrice {
      get { return twdPrice; }
      set {
        twdPrice = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TwdPrice)));
      }
    }
    private string spec;
    public string Spec {
      get { return spec; }
      set {
        spec = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Spec)));
      }
    }
    private string color;
    public string Color {
      get { return color; }
      set {
        color = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Color)));
      }
    }
    private string material;
    public string Material {
      get { return material; }
      set {
        material = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Material)));
      }
    }
    public PostStatus Status { get; set; }
    public DateTime Date { get; set; }
    public IList<Picture> Pictures { get; set; }

    public event PropertyChangedEventHandler PropertyChanged;

    public class Picture {
      public string Name { get; set; }
      public bool IsFirst { get; set; }
    }
  }
  [TypeConverter(typeof(EnumDescriptionTypeConverter))]
  public enum PostStatus {
    [Description("待處理")]
    New = 0,
    [Description("已編輯")]
    Edited = 1,
    [Description("已匯出")]
    Exported = 2,
    [Description("已刪除")]
    Deleted = 3
  }
}
