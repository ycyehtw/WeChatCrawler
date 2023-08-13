namespace WeChatCrawler {
  public class Vendor {
    public int VendorId { get; set; }
    public string WeChatId { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public bool IsEnabled { get; set; }
    public override string ToString() {
      return $"[{Code}] {Name}";
    }
  }
}
