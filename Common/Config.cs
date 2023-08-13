namespace WeChatCrawler {
  public class Config {
    public string WebApiServer { set; get; }
    public string User { set; get; }
    public string Password { set; get; }
    public int CacheLifeTime { set; get; }
    public bool IsDebug { set; get; }
    public bool EnableAppiumLog { set; get; }
    public string StartTime { set; get; }
    public int Duration { set; get; }
  }
}
